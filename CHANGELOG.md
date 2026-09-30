# Changelog

Todos los cambios importantes de Tiendi se registran en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el proyecto usa [Versionado Semántico](https://semver.org/lang/es/).

## [1.0.0] - 2026-09-29

Primera versión estable de Tiendi, preparada en la rama `release/1.0.0`.

### Agregado

- Inicio de sesión con JWT y menú lateral compartido.
- Módulo **Balance**: resumen de ingresos, gastos y balance por periodo; registro y anulación de gastos.
- Módulo **Clientes**: listado, búsqueda, registro, edición y desactivación.
- Módulo **Proveedores**: gestión de proveedores y registro de compras.
- Módulo **Inventario**: productos, categorías, ajuste de stock y stock bajo.
- Módulo **Estadísticas**: resumen de ventas, productos más vendidos y ventas por fecha.
- Módulo **Vender**: registro de ventas con productos, método de pago y descuento automático de stock.

### Corregido

- Los estilos de la pantalla de inicio de sesión estaban dentro de un `@media` y no se aplicaban en computadoras de escritorio.
- La página de Inicio mostraba el error `Cannot set properties of null` en la consola por buscar un elemento `#api-status` que no existe.
- No se podía registrar ni editar un cliente sin correo: el formulario enviaba `""` y la API respondía 400.
