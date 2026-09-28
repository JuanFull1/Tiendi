# Tiendi

Sistema web de control de inventario y ventas orientado a pequeños negocios.

El proyecto permite gestionar información relacionada con productos, inventario, ventas, clientes, proveedores, compras, gastos y estadísticas generales del negocio.

Además del desarrollo de la aplicación, el proyecto tiene como objetivo aplicar buenas prácticas de control de versiones y trabajo colaborativo mediante Git, GitHub y el modelo GitFlow.

## Objetivo

Desarrollar una aplicación web sencilla para simular un entorno real de desarrollo colaborativo, utilizando Git como sistema de control de versiones y GitHub como plataforma de repositorio remoto.

El proyecto aplica el modelo GitFlow para organizar el desarrollo mediante ramas y Pull Requests.

## Tecnologías utilizadas

### Frontend

- HTML5
- CSS3
- JavaScript
- Bootstrap 5

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core

### Base de datos

- Microsoft SQL Server

### Control de versiones

- Git
- GitHub
- GitFlow

## Arquitectura del proyecto

El sistema se encuentra dividido en tres partes principales:

- Frontend
- Backend
- Base de datos

El frontend contiene las interfaces que utiliza el usuario y la lógica JavaScript correspondiente a cada módulo.

El backend proporciona una API REST desarrollada con .NET que recibe las solicitudes del frontend, procesa la información y utiliza Entity Framework Core para comunicarse con SQL Server.

La comunicación entre frontend y backend se realiza mediante solicitudes HTTP y datos en formato JSON.

El flujo general es:

```text
Frontend
HTML + CSS + JavaScript + Bootstrap
        |
        | HTTP / JSON
        v
Backend
.NET Web API
        |
        v
Entity Framework Core
        |
        v
Microsoft SQL Server
```

## Módulos del sistema

El sistema está compuesto por los siguientes módulos:

### Vender

Permite gestionar el proceso de venta de productos y registrar la información asociada a cada venta.

### Balance

Permite consultar información relacionada con ingresos, gastos y movimientos económicos del negocio.

### Inventario

Permite administrar productos, categorías, existencias y movimientos de inventario.

### Estadísticas

Permite consultar información resumida y estadísticas generadas a partir de los datos registrados en el sistema.

### Clientes

Permite administrar la información de los clientes y relacionarlos con sus ventas.

### Proveedores

Permite administrar proveedores y registrar información relacionada con las compras de productos.

## Estructura del proyecto

```text
Tiendi/
│
├── backend/
│   └── Tiendi.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Models/
│       ├── Services/
│       ├── Program.cs
│       └── Tiendi.Api.csproj
│
├── database/
│   └── Tiendi.sql
│
├── frontend/
│   ├── assets/
│   │   └── img/
│   │
│   ├── css/
│   │   └── styles.css
│   │
│   ├── js/
│   │   ├── modules/
│   │   │   ├── balance.js
│   │   │   ├── clientes.js
│   │   │   ├── estadisticas.js
│   │   │   ├── inventario.js
│   │   │   ├── proveedores.js
│   │   │   └── ventas.js
│   │   │
│   │   ├── services/
│   │   │   └── api.js
│   │   │
│   │   ├── shared/
│   │   │   └── layout.js
│   │   │
│   │   └── app.js
│   │
│   ├── pages/
│   │   ├── balance.html
│   │   ├── clientes.html
│   │   ├── estadisticas.html
│   │   ├── inventario.html
│   │   ├── proveedores.html
│   │   └── ventas.html
│   │
│   └── index.html
│
├── .gitignore
├── CONTRIBUTING.md
└── README.md
```

## Organización del frontend

El frontend se encuentra dentro de la carpeta:

```text
frontend/
```

Las páginas correspondientes a cada módulo se encuentran en:

```text
frontend/pages/
```

La lógica JavaScript específica de cada módulo se encuentra en:

```text
frontend/js/modules/
```

Las solicitudes realizadas hacia la API se centralizan en:

```text
frontend/js/services/api.js
```

Los elementos compartidos de la interfaz se encuentran en:

```text
frontend/js/shared/
```

Los estilos generales se encuentran en:

```text
frontend/css/
```

## Organización del backend

El backend se encuentra dentro de:

```text
backend/Tiendi.Api/
```

La estructura principal está organizada de la siguiente manera:

```text
Controllers/
Services/
DTOs/
Models/
Data/
```

### Controllers

Contienen los endpoints de la API REST y reciben las solicitudes realizadas desde el frontend.

### Services

Contienen la lógica correspondiente a las funcionalidades del sistema.

### DTOs

Contienen los objetos utilizados para recibir o devolver información mediante la API.

### Models

Contienen las entidades generadas a partir de las tablas de SQL Server.

### Data

Contiene el contexto de Entity Framework Core utilizado para acceder a la base de datos.

## Base de datos

El proyecto utiliza una base de datos de Microsoft SQL Server denominada:

```text
Tiendi_DB
```

El script necesario para crear la base de datos y sus tablas se encuentra en:

```text
database/Tiendi.sql
```

La base contiene información relacionada con:

- Usuarios
- Categorías
- Productos
- Clientes
- Proveedores
- Ventas
- Detalles de ventas
- Pagos
- Gastos
- Compras
- Detalles de compras
- Movimientos de inventario

Los módulos de Balance y Estadísticas utilizan información obtenida de las tablas existentes y no requieren tablas independientes.

## Configuración de la base de datos

Para crear la base de datos:

1. Abrir Microsoft SQL Server Management Studio.
2. Conectarse al servidor SQL Server.
3. Abrir el archivo:

```text
database/Tiendi.sql
```

4. Ejecutar el script completo.
5. Verificar que exista la base de datos:

```text
Tiendi_DB
```

La cadena de conexión del backend debe corresponder al servidor SQL Server utilizado en cada equipo.

## Ejecución del backend

Ingresar a la carpeta del proyecto backend:

```bash
cd backend/Tiendi.Api
```

Restaurar las dependencias:

```bash
dotnet restore
```

Compilar el proyecto:

```bash
dotnet build
```

Ejecutar la API:

```bash
dotnet run
```

ASP.NET Core mostrará en la terminal la dirección local en la cual se encuentra ejecutándose la API.

## Ejecución del frontend

Desde la raíz del repositorio se puede utilizar un servidor web local.

Por ejemplo:

```bash
npx serve frontend -l 5500
```

Después se puede ingresar desde el navegador a:

```text
http://localhost:5500
```

## Bootstrap

Bootstrap se utiliza mediante CDN, por lo que no es necesario instalarlo localmente.

Se utiliza para elementos como:

- Sistema de cuadrícula.
- Tarjetas.
- Botones.
- Barras de navegación.
- Espaciados.
- Diseño adaptable.

Los estilos personalizados del proyecto se encuentran en:

```text
frontend/css/styles.css
```

## GitFlow

El proyecto utiliza GitFlow como estrategia de organización de ramas.

Las ramas utilizadas son:

```text
main
develop
feature/*
release/*
hotfix/*
```

### main

Contiene versiones estables del proyecto.

### develop

Es la rama principal de integración durante el desarrollo.

### feature

Se utiliza para desarrollar nuevas funcionalidades.

Las ramas `feature/*` se crean desde `develop` y posteriormente se integran nuevamente en `develop` mediante Pull Request.

### release

Se utiliza para preparar una nueva versión estable del proyecto.

Las ramas `release/*` se crean desde `develop` y posteriormente pueden integrarse en `main` y `develop`.

### hotfix

Se utiliza para realizar correcciones urgentes sobre una versión estable.

Las ramas `hotfix/*` se crean desde `main` y posteriormente se integran en `main` y `develop`.

## Flujo general de trabajo

```text
main
  |
  +---- develop
          |
          +---- feature/*
          |
          +---- release/*
                    |
                    +---- main
```

Cada funcionalidad debe desarrollarse de manera independiente en una rama creada desde `develop`.

Las integraciones deben realizarse mediante Pull Requests y revisión de código.

Las reglas completas de colaboración se encuentran en:

```text
CONTRIBUTING.md
```

## Archivo .gitignore

El repositorio utiliza un archivo `.gitignore` para evitar versionar archivos generados automáticamente o archivos que no deben formar parte del código fuente.

Se consideraron las siguientes tecnologías:

| Tecnología | Archivos o directorios ignorados | Justificación |
|---|---|---|
| .NET | `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo` | Contienen archivos de compilación y configuraciones locales que pueden generarse nuevamente. |
| Node.js | `node_modules/`, `dist/`, `npm-debug.log*` | Las dependencias pueden instalarse nuevamente y los archivos generados no necesitan almacenarse en Git. |
| SQL Server | `*.mdf`, `*.ldf`, `*.bak` | Son archivos físicos o respaldos locales de la base de datos. La estructura se comparte mediante el script SQL. |

También se ignoran archivos locales de editores, logs y archivos que podrían contener configuración sensible.

## Repositorio remoto

El proyecto se encuentra alojado en GitHub:

```text
https://github.com/JuanFull1/Tiendi
```

## Estado del proyecto

El proyecto se encuentra actualmente en desarrollo.

La estructura base del frontend, backend y base de datos se encuentra preparada para continuar con el desarrollo colaborativo de los diferentes módulos.