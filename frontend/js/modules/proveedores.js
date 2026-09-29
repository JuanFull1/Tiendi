document.addEventListener("DOMContentLoaded", () => {

    const contenedor =
        document.getElementById("proveedores-content");

    let proveedores = [];

    renderizar();

    cargarProveedores();

    async function cargarProveedores() {

        try {

            contenedor.innerHTML = `
                <div class="text-center p-4">
                    <div class="spinner-border" role="status"></div>
                    <p class="mt-2">Cargando proveedores...</p>
                </div>
            `;

            proveedores =
                await apiRequest("/proveedores");

            renderizar();

        } catch (error) {

            contenedor.innerHTML = `
                <div class="alert alert-danger">
                    ${error.message}
                </div>
            `;
        }
    }

    function renderizar() {

        contenedor.innerHTML = `

            <div class="card shadow-sm mb-4">

                <div class="card-header d-flex justify-content-between align-items-center">

                    <h5 class="mb-0">
                        Lista de proveedores
                    </h5>

                    <button
                        type="button"
                        class="btn btn-primary"
                        id="btnNuevoProveedor">
                        + Nuevo proveedor
                    </button>

                </div>

                <div class="card-body">

                    <div class="table-responsive">

                        <table class="table table-hover align-middle">

                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Nombre</th>
                                    <th>Identificación</th>
                                    <th>Teléfono</th>
                                    <th>Email</th>
                                    <th>Dirección</th>
                                    <th>Estado</th>
                                    <th>Acciones</th>
                                </tr>
                            </thead>

                            <tbody id="tablaProveedores">
                                ${generarFilas()}
                            </tbody>

                        </table>

                    </div>

                </div>

            </div>

            <div id="formularioProveedor"></div>
        `;

        document
            .getElementById("btnNuevoProveedor")
            .addEventListener(
                "click",
                () => mostrarFormulario()
            );

        document
            .querySelectorAll(".btn-editar")
            .forEach(boton => {

                boton.addEventListener(
                    "click",
                    () => {

                        const id =
                            Number(
                                boton.dataset.id
                            );

                        const proveedor =
                            proveedores.find(
                                p =>
                                    p.idProveedor === id
                            );

                        if (proveedor) {
                            mostrarFormulario(proveedor);
                        }
                    }
                );
            });

        document
            .querySelectorAll(".btn-desactivar")
            .forEach(boton => {

                boton.addEventListener(
                    "click",
                    () => {

                        const id =
                            Number(
                                boton.dataset.id
                            );

                        desactivarProveedor(id);
                    }
                );
            });
    }

    function generarFilas() {

        if (!proveedores || proveedores.length === 0) {

            return `
                <tr>
                    <td
                        colspan="8"
                        class="text-center text-muted p-4">
                        No existen proveedores registrados.
                    </td>
                </tr>
            `;
        }

        return proveedores
            .map(proveedor => {

                const estado =
                    proveedor.activo
                        ? `<span class="badge bg-success">
                            Activo
                           </span>`
                        : `<span class="badge bg-secondary">
                            Inactivo
                           </span>`;

                const botonDesactivar =
                    proveedor.activo
                        ? `
                            <button
                                type="button"
                                class="btn btn-sm btn-danger btn-desactivar"
                                data-id="${proveedor.idProveedor}">
                                Desactivar
                            </button>
                          `
                        : "";

                return `
                    <tr>

                        <td>
                            ${proveedor.idProveedor}
                        </td>

                        <td>
                            ${proveedor.nombre}
                        </td>

                        <td>
                            ${proveedor.identificacion}
                        </td>

                        <td>
                            ${proveedor.telefono || "-"}
                        </td>

                        <td>
                            ${proveedor.email || "-"}
                        </td>

                        <td>
                            ${proveedor.direccion || "-"}
                        </td>

                        <td>
                            ${estado}
                        </td>

                        <td>

                            <div class="d-flex gap-1">

                                <button
                                    type="button"
                                    class="btn btn-sm btn-warning btn-editar"
                                    data-id="${proveedor.idProveedor}">
                                    Editar
                                </button>

                                ${botonDesactivar}

                            </div>

                        </td>

                    </tr>
                `;
            })
            .join("");
    }

    function mostrarFormulario(proveedor = null) {

        const esEdicion =
            proveedor !== null;

        const formulario =
            document.getElementById(
                "formularioProveedor"
            );

        formulario.innerHTML = `

            <div class="card shadow-sm">

                <div class="card-header">

                    <h5 class="mb-0">
                        ${esEdicion
                            ? "Editar proveedor"
                            : "Nuevo proveedor"}
                    </h5>

                </div>

                <div class="card-body">

                    <form id="formProveedor">

                        <div class="row g-3">

                            <div class="col-md-6">

                                <label
                                    for="nombre"
                                    class="form-label">
                                    Nombre
                                </label>

                                <input
                                    type="text"
                                    class="form-control"
                                    id="nombre"
                                    required
                                    value="${esEdicion
                                        ? proveedor.nombre
                                        : ""}">
                            </div>

                            <div class="col-md-6">

                                <label
                                    for="identificacion"
                                    class="form-label">
                                    Identificación
                                </label>

                                <input
                                    type="text"
                                    class="form-control"
                                    id="identificacion"
                                    required
                                    value="${esEdicion
                                        ? proveedor.identificacion
                                        : ""}">
                            </div>

                            <div class="col-md-6">

                                <label
                                    for="telefono"
                                    class="form-label">
                                    Teléfono
                                </label>

                                <input
                                    type="text"
                                    class="form-control"
                                    id="telefono"
                                    value="${esEdicion
                                        ? proveedor.telefono || ""
                                        : ""}">
                            </div>

                            <div class="col-md-6">

                                <label
                                    for="email"
                                    class="form-label">
                                    Email
                                </label>

                                <input
                                    type="email"
                                    class="form-control"
                                    id="email"
                                    value="${esEdicion
                                        ? proveedor.email || ""
                                        : ""}">
                            </div>

                            <div class="col-12">

                                <label
                                    for="direccion"
                                    class="form-label">
                                    Dirección
                                </label>

                                <input
                                    type="text"
                                    class="form-control"
                                    id="direccion"
                                    value="${esEdicion
                                        ? proveedor.direccion || ""
                                        : ""}">
                            </div>

                        </div>

                        <div
                            id="mensajeFormulario"
                            class="mt-3">
                        </div>

                        <div class="mt-4">

                            <button
                                type="submit"
                                class="btn btn-success">
                                ${esEdicion
                                    ? "Actualizar"
                                    : "Guardar"}
                            </button>

                            <button
                                type="button"
                                class="btn btn-secondary"
                                id="btnCancelar">
                                Cancelar
                            </button>

                        </div>

                    </form>

                </div>

            </div>
        `;

        document
            .getElementById("formProveedor")
            .addEventListener(
                "submit",
                async event => {

                    event.preventDefault();

                    await guardarProveedor(
                        proveedor
                    );
                }
            );

        document
            .getElementById("btnCancelar")
            .addEventListener(
                "click",
                () => {

                    formulario.innerHTML = "";
                }
            );
    }

    async function guardarProveedor(
        proveedor
    ) {

        const mensaje =
            document.getElementById(
                "mensajeFormulario"
            );

        const datos = {

            nombre:
                document
                    .getElementById("nombre")
                    .value
                    .trim(),

            identificacion:
                document
                    .getElementById("identificacion")
                    .value
                    .trim(),

            telefono:
                document
                    .getElementById("telefono")
                    .value
                    .trim(),

            email:
                document
                    .getElementById("email")
                    .value
                    .trim(),

            direccion:
                document
                    .getElementById("direccion")
                    .value
                    .trim()
        };

        if (
            !datos.nombre ||
            !datos.identificacion
        ) {

            mensaje.innerHTML = `
                <div class="alert alert-warning">
                    Nombre e identificación son obligatorios.
                </div>
            `;

            return;
        }

        try {

            if (proveedor) {

                await apiRequest(
                    `/proveedores/${proveedor.idProveedor}`,
                    {
                        method: "PUT",
                        body: JSON.stringify(datos)
                    }
                );

                alert(
                    "Proveedor actualizado correctamente."
                );

            } else {

                await apiRequest(
                    "/proveedores",
                    {
                        method: "POST",
                        body: JSON.stringify(datos)
                    }
                );

                alert(
                    "Proveedor registrado correctamente."
                );
            }

            await cargarProveedores();

        } catch (error) {

            mensaje.innerHTML = `
                <div class="alert alert-danger">
                    ${error.message}
                </div>
            `;
        }
    }

    async function desactivarProveedor(id) {

        const confirmar =
            confirm(
                "¿Está seguro de desactivar este proveedor?"
            );

        if (!confirmar) {
            return;
        }

        try {

            await apiRequest(
                `/proveedores/${id}`,
                {
                    method: "DELETE"
                }
            );

            alert(
                "Proveedor desactivado correctamente."
            );

            await cargarProveedores();

        } catch (error) {

            alert(
                `No se pudo desactivar el proveedor: ${error.message}`
            );
        }
    }

});