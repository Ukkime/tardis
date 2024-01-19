# Tareas Asignadas y Recursos Disponibles Inteligentemente Sincronizados

Esta aplicación de escritorio proporciona una manera visual y eficiente de compartir tu estado de trabajo con tus compañeros. Aquí están sus principales características:

## Indicador de Estado
La aplicación crea un indicador visual en forma de una pequeña esfera en tu pantalla que muestra tu estado de trabajo actual. Este indicador se adhiere a los bordes de la pantalla para una visibilidad constante sin obstruir tu espacio de trabajo.

## Detección de Compañeros
La aplicación puede detectar a tus compañeros ya sea a través de UDP o de un servidor. Esto te permite ver el estado de trabajo de tus compañeros en tiempo real, fomentando una comunicación más eficiente y consciente.

## Compartir Portapapeles
Una característica única de esta aplicación es su capacidad para compartir el contenido de tu portapapeles con tus compañeros. Esto puede ser útil para compartir rápidamente fragmentos de código, enlaces o cualquier otra información que estés utilizando.

## Notificaciones de Pomodoro
Para promover una gestión saludable del tiempo, la aplicación te notificará al estilo Pomodoro si has estado concentrado durante un largo periodo de tiempo. Esto te ayuda a recordar tomar descansos regulares, lo que puede mejorar tu productividad y bienestar.

## Personalización
La aplicación es altamente personalizable, permitiéndote cambiar tu estado de trabajo, ajustar la frecuencia de las actualizaciones y las notificaciones, y mucho más.

## Requisitos

- .NET Core 8.0 o superior
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

* Recuerda hacer un npm install antes de arrancar la api con node .\app.js

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
