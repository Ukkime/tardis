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

// Función para recorrer los grupos y limpiar nodos y grupos inactivos
async function cleanInactiveNodesAndGroups() {
  let groupRef = db.ref("groups");
  let snapshot = await groupRef.once("value");
  let groups = snapshot.val();
  let currentTime = moment().tz('Europe/Madrid').valueOf();

  for (let groupId in groups) {
    let group = groups[groupId];
    let nodeCount = group.NeighborNodes.length;

    for (let i = 0; i < group.NeighborNodes.length; i++) {
      let nodeTime = moment.tz(group.NeighborNodes[i].Updatetime, 'Europe/Madrid').valueOf();
      let diff = currentTime - nodeTime;
      if (diff > 5 * 60 * 1000) { // 5 minutos
        // Elimina el nodo si es inactivo
        await groupRef.child(groupId).child('NeighborNodes').child(i).remove();
        nodeCount--;
      }
    }

    if (nodeCount === 0) {
      // Elimina el grupo si no tiene nodos activos
      await groupRef.child(groupId).remove();
    }
  }
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