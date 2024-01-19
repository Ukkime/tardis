# Tareas Asignadas y Recursos Disponibles Inteligentemente Sincronizados

## Descripción

Este proyecto es una aplicación en C# con .NET Core que ayuda a asignar tareas y gestionar recursos de manera inteligente.

## Requisitos

- .NET Core 3.1 o superior
- Visual Studio 2019 o superior

## Instalación

1. Clona este repositorio en tu máquina local.
2. Abre el archivo de solución (.sln) en Visual Studio.
3. Restaura los paquetes NuGet (esto debería hacerse automáticamente).
4. Compila y ejecuta la aplicación.

## Uso

Configura el fichero config.json para crear tu grupo de trabajo e identificar el nodo
Especificar, si es necesario la url para trabajar en SERVIDOR o el modo de trabajo UDP

Si usas la API que proporcionamos, necesitaras crear una Realtimedatabase de Firebase y generar credenciales para la api.
Para generar el archivo serviceAccountKey.json, necesitas crear un proyecto de Firebase y generar una nueva clave privada. Aquí te explico cómo hacerlo:

1. Ve al Firebase console.
2. Haz clic en “Añadir proyecto” y sigue las instrucciones para crear un nuevo proyecto de Firebase.
3. Una vez que tu proyecto esté creado, haz clic en el icono de configuración (el icono del engranaje) en el menú lateral y selecciona “Configuración del proyecto”.
4. En la pestaña “Cuentas de servicio”, haz clic en el botón “Generar nueva clave privada”.
5. Confirma la acción y tu navegador descargará un archivo JSON que contiene tu clave de servicio.
6. Este archivo JSON es tu serviceAccountKey.json y contiene toda la información necesaria para autenticar tu servidor con Firebase. Deberías mantener este archivo seguro y no compartirlo públicamente, ya que contiene información sensible.

La URL de tu base de datos puedes encontrarla en la consola
1. Crea o accede a una realtimedatabase
2. En la pestaña Datos copia la URL de la cabecera (estilo https://tardis-asdsad-aadasd-daas.europe-west1.firebasedatabase.app)

Compilación del proyecto:
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true -o output

## Contribución

Las contribuciones son bienvenidas. Por favor, abre un problema para discutir la contribución antes de hacer un pull request.

## Licencia

GNU Affero General Public License v3.0

## Contacto

Si tienes alguna pregunta o comentario, por favor, abre un problema en este repositorio.
