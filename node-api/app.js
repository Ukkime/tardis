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

// Función para manejar el envío de contenido del portapapeles a un nodo
function handleClipboard(groupId, nodeName, sender, clipboardContent) {
  let groupRef = db.ref("groups/" + groupId);
  return groupRef.once("value").then((snapshot) => {
    let group = snapshot.val();
    if (group === null) {
      throw new Error("El grupo no existe");
    }

    let nodeIndex = group.NeighborNodes.findIndex(
      (node) => node.Name === nodeName
    );
    if (nodeIndex === -1) {
      throw new Error("El nodo no existe");
    }

    // Agrega el contenido del portapapeles y el remitente a la clave Clipboards si se proporcionan
    if (clipboardContent !== null && sender !== null) {
      if (!group.NeighborNodes[nodeIndex].Clipboards) {
        group.NeighborNodes[nodeIndex].Clipboards = [];
      }
      group.NeighborNodes[nodeIndex].Clipboards.push({ content: clipboardContent, sender: sender });
    }

    return groupRef.set(group);
  });
}

// Ruta POST para enviar contenido del portapapeles a un nodo
app.post("/group/:groupId/node/:nodeName/clipboard", express.json(), (req, res) => {
  const groupId = req.params.groupId;
  const nodeName = req.params.nodeName;
  const sender = req.body.sender;
  const clipboardContent = req.body.content;

  handleClipboard(groupId, nodeName, sender, clipboardContent).then(() => {
    res.send("Contenido del portapapeles enviado con éxito");
  }).catch((error) => {
    res.status(400).send(error.message);
  });
});

// Función para manejar la recuperación de contenido del portapapeles
function handleClipboardRetrieval(groupId, nodeName) {
  let groupRef = db.ref("groups/" + groupId);
  return groupRef.once("value").then((snapshot) => {
    let group = snapshot.val();
    if (group === null) {
      throw new Error("El grupo no existe");
    }

    let nodeIndex = group.NeighborNodes.findIndex(
      (node) => node.Name === nodeName
    );
    if (nodeIndex === -1) {
      throw new Error("El nodo no existe");
    }

    // Verifica si hay contenido del portapapeles pendiente de recuperar
    if (group.NeighborNodes[nodeIndex].Clipboards && group.NeighborNodes[nodeIndex].Clipboards.length > 0) {
      // Recupera el contenido del portapapeles y lo elimina de la base de datos
      let clipboardContent = group.NeighborNodes[nodeIndex].Clipboards.shift();
      return groupRef.set(group).then(() => clipboardContent);
    } else {
      return null;
    }
  });
}

// Ruta GET para recuperar contenido del portapapeles
app.get("/group/:groupId/node/:nodeName/clipboard", (req, res) => {
  const groupId = req.params.groupId;
  const nodeName = req.params.nodeName;

  handleClipboardRetrieval(groupId, nodeName).then((clipboardContent) => {
    if (clipboardContent !== null) {
      // Devuelve el contenido del portapapeles y el remitente
      res.json({
        sender: clipboardContent.sender,
        clipboardContent: clipboardContent.content
      });
    } else {
      res.send("-1");
    }
  }).catch((error) => {
    res.status(400).send(error.message);
  });
});


app.listen(3000, () => console.log("App escuchando en el puerto 3000!"));