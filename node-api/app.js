const express = require("express");
const admin = require("firebase-admin");
const moment = require('moment-timezone');

// Inicializa Firebase
var serviceAccount = require("./serviceAccountKey.json");
admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://tardis-c6ed2-default-rtdb.europe-west1.firebasedatabase.app",
});

const db = admin.database();
const app = express();

// Función para manejar la creación de grupos y nodos
function handleGroupAndNode(groupId, nodeName, status = null) {
  let groupRef = db.ref("groups/" + groupId);
  return groupRef.once("value").then((snapshot) => {
    let group = snapshot.val();
    if (group === null) {
      // Crea un nuevo grupo si no existe
      group = {
        NodeCount: 0,
        Id: groupId,
        NeighborNodes: [],
      };
    }

    let nodeIndex = group.NeighborNodes.findIndex(
      (node) => node.Name === nodeName
    );
    if (nodeIndex === -1) {
      // Crea un nuevo nodo si no existe
      const newNode = {
        Name: nodeName,
        Updatetime: moment().tz('Europe/Madrid').format(),
        Datetime: moment().tz('Europe/Madrid').format(),
        Status: "Disponible",
      };
      group.NeighborNodes.push(newNode);
      group.NodeCount += 1;
    } else {
      // Actualiza el estado del nodo si existe
      if (status !== null) {
        group.NeighborNodes[nodeIndex].Status = status;
        group.NeighborNodes[nodeIndex].Datetime = moment().tz('Europe/Madrid').format(); // Agrega datetime
      }
      group.NeighborNodes[nodeIndex].Updatetime = moment().tz('Europe/Madrid').format();
    }

    return groupRef.set(group);
  });
}

// Función para limpiar nodos y grupos inactivos
function cleanInactiveNodesAndGroups() {
  let groupRef = db.ref("groups");
  groupRef.once("value").then((snapshot) => {
    let groups = snapshot.val();
    let currentTime = moment().tz('Europe/Madrid').format();

    for (let groupId in groups) {
      let group = groups[groupId];
      let activeNodes = group.NeighborNodes.filter((node) => {
        let nodeTime = moment.tz(node.Updatetime, 'Europe/Madrid').valueOf();
        return currentTime - nodeTime <= 5 * 60 * 1000; // 5 minutos
      });

      if (activeNodes.length > 0) {
        // Actualiza los nodos activos del grupo
        group.NeighborNodes = activeNodes;
        group.NodeCount = activeNodes.length;
        groupRef.child(groupId).set(group);
      } else {
        // Elimina el grupo si no tiene nodos activos
        groupRef.child(groupId).remove();
      }
    }
  });
}

// Ejecuta la función de limpieza cada 1 minuto
setInterval(cleanInactiveNodesAndGroups, 60 * 1000); // 1 minuto

app.get("/group/:groupId/node/:nodeName", (req, res) => {
  const groupId = req.params.groupId;
  const nodeName = req.params.nodeName;

  handleGroupAndNode(groupId, nodeName).then(() => {
    let groupRef = db.ref("groups/" + groupId);
    groupRef.once("value", (snapshot) => {
      let group = snapshot.val();
      res.json(group);
    });
  });
});

app.post("/group/:groupId/node/:nodeName/status/:status", (req, res) => {
  const groupId = req.params.groupId;
  const nodeName = req.params.nodeName;
  const status = req.params.status;

  handleGroupAndNode(groupId, nodeName, status).then(() => {
    res.send("Estado actualizado con éxito");
  });
});

app.listen(3000, () => console.log("App escuchando en el puerto 3000!"));