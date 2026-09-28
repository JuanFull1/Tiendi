# Guía de Contribución

Este documento establece las reglas de colaboración utilizadas durante el desarrollo del proyecto Tiendi.

El proyecto utiliza Git, GitHub y el modelo GitFlow para organizar el desarrollo colaborativo.

## Modelo de ramas

El repositorio utiliza las siguientes ramas:

```text
main
develop
feature/*
release/*
hotfix/*
```

## Rama main

La rama `main` contiene las versiones estables del proyecto.

No se deben desarrollar funcionalidades directamente sobre esta rama.

Los cambios deben llegar a `main` mediante el flujo establecido por GitFlow.

## Rama develop

La rama `develop` funciona como rama de integración durante el desarrollo.

Las nuevas funcionalidades deben partir desde esta rama.

No se recomienda realizar directamente el desarrollo de funcionalidades sobre `develop`.

## Ramas feature

Las ramas `feature/*` se utilizan para desarrollar nuevas funcionalidades.

Toda rama feature debe crearse desde `develop`.

Ejemplos:

```text
feature/ventas-api
feature/ventas-ui
feature/inventario-api
feature/clientes-ui
```

Antes de crear una rama feature se debe actualizar `develop`.

```bash
git checkout develop
git pull origin develop
```

Después se crea la nueva rama:

```bash
git branch feature/nombre-funcionalidad
git checkout feature/nombre-funcionalidad
```

El desarrollo debe realizarse únicamente dentro de la rama correspondiente.

## Ramas release

Las ramas `release/*` se utilizan para preparar una versión estable del proyecto.

Se crean desde `develop`.

Ejemplo:

```bash
git checkout develop
git pull origin develop
git branch release/1.0.0
git checkout release/1.0.0
```

Una vez preparada la versión, la rama release puede integrarse en `main` y `develop`.

## Ramas hotfix

Las ramas `hotfix/*` se utilizan para solucionar errores importantes encontrados en una versión estable.

Se crean desde `main`.

Ejemplo:

```bash
git checkout main
git pull origin main
git branch hotfix/nombre-correccion
git checkout hotfix/nombre-correccion
```

Una vez corregido el problema, el hotfix debe integrarse nuevamente en `main` y `develop`.

## Commits

Los commits deben representar cambios concretos y comprensibles.

Antes de realizar un commit se debe comprobar el estado del repositorio:

```bash
git status
```

Agregar los archivos correspondientes:

```bash
git add .
```

Crear el commit:

```bash
git commit -m "tipo: descripcion del cambio"
```

Se recomienda utilizar los siguientes tipos:

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de un error |
| `docs` | Cambios de documentación |
| `style` | Cambios visuales o de formato |
| `refactor` | Reorganización del código |
| `test` | Pruebas |
| `chore` | Configuración o mantenimiento |

Ejemplos:

```text
feat: agrega registro de clientes
feat: implementa consulta de productos
fix: corrige calculo del total de venta
docs: actualiza instrucciones del proyecto
style: mejora diseño del menu lateral
```

Los mensajes deben ser breves y describir claramente el cambio realizado.

## Publicación de una rama

Después de realizar los commits correspondientes, la rama debe publicarse en el repositorio remoto.

```bash
git push -u origin feature/nombre-funcionalidad
```

Los cambios no deben integrarse directamente mediante un push hacia `develop` o `main`.

## Pull Requests

Las funcionalidades desarrolladas en ramas `feature/*` deben integrarse a `develop` mediante Pull Requests.

Cada integrante debe realizar al menos dos Pull Requests durante el desarrollo del proyecto.

Cada Pull Request debe contener:

- Un título claro.
- Una descripción de los cambios realizados.
- La rama de origen correcta.
- La rama de destino correcta.
- Código relacionado únicamente con la funcionalidad desarrollada.
- Un proyecto que compile correctamente.
- Ausencia de archivos innecesarios o información sensible.

Ejemplo de título:

```text
feat: agrega API para gestión de clientes
```

Ejemplo de descripción:

```text
## Cambios realizados

- Se agregó el controlador de clientes.
- Se implementó la lógica de consulta.
- Se agregaron los DTO necesarios.
- Se verificó la compilación del backend.
```

## Revisión de código

Los Pull Requests deben ser revisados por otros integrantes del equipo antes de realizar el merge.

Durante la revisión se debe comprobar:

- Que los cambios correspondan a la funcionalidad indicada.
- Que el proyecto compile correctamente.
- Que no existan conflictos pendientes.
- Que no se hayan modificado archivos sin relación con la funcionalidad.
- Que no existan credenciales o información sensible.
- Que los nombres de archivos y clases sean comprensibles.
- Que no se agreguen archivos generados automáticamente.

Los revisores pueden realizar comentarios y sugerencias antes de aprobar el Pull Request.

## Actualización antes de comenzar un trabajo

Antes de crear una nueva rama se debe trabajar con la versión más reciente de `develop`.

```bash
git checkout develop
git pull origin develop
```

Después se crea la rama correspondiente.

Esto reduce la posibilidad de trabajar sobre versiones antiguas del proyecto.

## Resolución de conflictos

Si existen conflictos durante una integración, estos deben resolverse antes de completar el Pull Request.

Después de resolverlos se debe comprobar nuevamente:

```bash
git status
```

y verificar que el proyecto continúe funcionando correctamente.

Los conflictos no deben resolverse eliminando código de otros integrantes sin revisar previamente los cambios.

## Organización del backend

El backend se encuentra en:

```text
backend/Tiendi.Api/
```

Su estructura principal es:

```text
Controllers/
Services/
DTOs/
Models/
Data/
```

### Controllers

Contienen los endpoints de la API.

### Services

Contienen la lógica correspondiente a las funcionalidades del sistema.

### DTOs

Contienen los objetos utilizados para transportar información entre el frontend y backend.

### Models

Contienen las entidades correspondientes a las tablas de la base de datos.

### Data

Contiene el contexto utilizado por Entity Framework Core.

Cada integrante debe modificar únicamente los archivos necesarios para desarrollar la funcionalidad correspondiente.

## Organización del frontend

Las páginas de los módulos se encuentran en:

```text
frontend/pages/
```

La lógica JavaScript de cada módulo se encuentra en:

```text
frontend/js/modules/
```

Las llamadas generales hacia la API se encuentran en:

```text
frontend/js/services/api.js
```

Los elementos compartidos se encuentran en:

```text
frontend/js/shared/
```

Los estilos generales se encuentran en:

```text
frontend/css/
```

Se debe evitar modificar archivos compartidos si el cambio no es necesario para la funcionalidad desarrollada.

## Base de datos

La estructura de la base de datos se encuentra definida mediante:

```text
database/Tiendi.sql
```

Los integrantes deben utilizar este script para crear la base de datos local del proyecto.

No se deben subir al repositorio archivos físicos de SQL Server como:

```text
*.mdf
*.ldf
*.bak
```

Si se requiere modificar la estructura de la base de datos, el cambio debe reflejarse también en el script `Tiendi.sql`.

## Archivos ignorados

El archivo `.gitignore` evita subir archivos que no deben formar parte del repositorio.

Entre ellos se encuentran:

```text
bin/
obj/
.vs/
node_modules/
dist/
*.mdf
*.ldf
*.bak
.env
*.log
```

Los directorios `bin` y `obj`, por ejemplo, son generados nuevamente por .NET al restaurar y compilar el proyecto.

Por esta razón, los integrantes no necesitan recibir estos archivos mediante Git.

## Comprobación del backend

Antes de realizar un Pull Request que modifique el backend se debe comprobar que el proyecto pueda restaurarse y compilarse.

Desde:

```text
backend/Tiendi.Api/
```

ejecutar:

```bash
dotnet restore
dotnet build
```

Si se necesita ejecutar la API:

```bash
dotnet run
```

No se debe enviar un Pull Request con errores de compilación conocidos.

## Comprobación del frontend

Antes de realizar un Pull Request relacionado con el frontend se debe comprobar que las páginas modificadas puedan ejecutarse correctamente.

Desde la raíz del repositorio se puede utilizar:

```bash
npx serve frontend -l 5500
```

Después se puede abrir:

```text
http://localhost:5500
```

Las páginas deben mantener la navegación y estructura general del sistema.

## Seguridad

Está prohibido subir al repositorio información sensible.

No se deben versionar:

- Contraseñas.
- Tokens de acceso.
- Claves privadas.
- Credenciales personales.
- Archivos `.env`.
- Cadenas de conexión que contengan contraseñas.
- Información privada de los integrantes.

Antes de ejecutar:

```bash
git add .
```

se recomienda comprobar los archivos modificados mediante:

```bash
git status
```

## Reglas generales

Durante el desarrollo se deben respetar las siguientes reglas:

1. No desarrollar directamente sobre `main`.
2. Crear las funcionalidades desde `develop`.
3. Utilizar ramas con nombres descriptivos.
4. Realizar commits relacionados con cambios concretos.
5. Mantener actualizada la rama local antes de iniciar un nuevo trabajo.
6. Utilizar Pull Requests para integrar funcionalidades.
7. Realizar revisión de código a los compañeros.
8. No subir archivos generados automáticamente.
9. No subir información sensible.
10. Comprobar el funcionamiento del proyecto antes de solicitar un merge.

## Flujo resumido

El flujo habitual para desarrollar una funcionalidad es:

```bash
git checkout develop
git pull origin develop

git branch feature/nombre-funcionalidad
git checkout feature/nombre-funcionalidad
```

Después de realizar los cambios:

```bash
git status
git add .
git commit -m "feat: descripcion del cambio"
git push -u origin feature/nombre-funcionalidad
```

Finalmente se crea un Pull Request:

```text
feature/nombre-funcionalidad
            |
            v
         develop
```

El Pull Request debe ser revisado por otro integrante antes de completar el merge.