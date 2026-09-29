const INVENTARIO_MEDIA_URL =
    API_URL.replace(/\/api\/?$/, "");

let productos = [];
let categorias = [];
let stockBajo = [];
let movimientos = [];

let productoEditando = null;
let imagenSeleccionada = null;

let modalProducto;
let modalAjuste;

document.addEventListener(
    "DOMContentLoaded",
    async () => {

        modalProducto =
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById(
                    "modalProducto"
                )
            );

        modalAjuste =
            bootstrap.Modal.getOrCreateInstance(
                document.getElementById(
                    "modalAjuste"
                )
            );

        configurarEventos();

        await cargarTodo();
    }
);

function configurarEventos() {

    document
        .querySelectorAll(".inventario-tab")
        .forEach(boton => {
            boton.addEventListener(
                "click",
                () => cambiarPanel(
                    boton.dataset.tab
                )
            );
        });

    document
        .getElementById("btnNuevoProducto")
        .addEventListener(
            "click",
            abrirNuevoProducto
        );

    document
        .getElementById("btnAbrirAjuste")
        .addEventListener(
            "click",
            () => abrirAjuste()
        );

    document
        .getElementById("buscarProducto")
        .addEventListener(
            "input",
            renderizarProductos
        );

    document
        .getElementById("filtroCategoria")
        .addEventListener(
            "change",
            renderizarProductos
        );

    document
        .getElementById("filtroStock")
        .addEventListener(
            "change",
            renderizarProductos
        );

    document
        .getElementById("formProducto")
        .addEventListener(
            "submit",
            guardarProducto
        );

    document
        .getElementById("formAjuste")
        .addEventListener(
            "submit",
            guardarAjuste
        );

    document
        .getElementById("productoImagen")
        .addEventListener(
            "change",
            seleccionarImagen
        );

    document
        .getElementById("btnEliminarImagen")
        .addEventListener(
            "click",
            eliminarImagen
        );

    document
        .getElementById("btnActualizarStock")
        .addEventListener(
            "click",
            cargarStockBajo
        );

    document
        .getElementById("btnActualizarMovimientos")
        .addEventListener(
            "click",
            cargarMovimientos
        );
}

async function cargarTodo() {

    try {
        await cargarCategorias();

        await Promise.all([
            cargarProductos(),
            cargarStockBajo(),
            cargarMovimientos()
        ]);

        actualizarResumen();
    } catch (error) {
        mostrarToast(
            error.message ||
            "No se pudo cargar el inventario.",
            "danger"
        );
    }
}

async function cargarProductos() {

    productos =
        await apiRequest("/productos")
        || [];

    renderizarProductos();

    cargarSelectAjuste();

    actualizarResumen();
}

async function cargarCategorias() {

    categorias =
        await apiRequest(
            "/inventario/categorias"
        )
        || [];

    const filtro =
        document.getElementById(
            "filtroCategoria"
        );

    const formulario =
        document.getElementById(
            "productoCategoria"
        );

    filtro.innerHTML =
        `<option value="">
            Todas las categorías
        </option>`;

    formulario.innerHTML =
        `<option value="">
            Sin categoría
        </option>`;

    categorias.forEach(categoria => {

        filtro.insertAdjacentHTML(
            "beforeend",
            `
            <option value="${categoria.idCategoria}">
                ${escapeHtml(categoria.nombre)}
            </option>
            `
        );

        formulario.insertAdjacentHTML(
            "beforeend",
            `
            <option value="${categoria.idCategoria}">
                ${escapeHtml(categoria.nombre)}
            </option>
            `
        );
    });
}

async function cargarStockBajo() {

    try {
        stockBajo =
            await apiRequest(
                "/inventario/stock-bajo"
            )
            || [];

        renderizarStockBajo();

        actualizarResumen();
    } catch (error) {
        mostrarToast(
            error.message,
            "danger"
        );
    }
}

async function cargarMovimientos() {

    try {
        movimientos =
            await apiRequest(
                "/inventario/movimientos"
            )
            || [];

        renderizarMovimientos();
    } catch (error) {
        mostrarToast(
            error.message,
            "danger"
        );
    }
}

function cambiarPanel(tab) {

    document
        .querySelectorAll(".inventario-tab")
        .forEach(boton => {
            boton.classList.toggle(
                "active",
                boton.dataset.tab === tab
            );
        });

    document
        .getElementById("panelProductos")
        .classList.toggle(
            "d-none",
            tab !== "productos"
        );

    document
        .getElementById("panelStock")
        .classList.toggle(
            "d-none",
            tab !== "stock"
        );

    document
        .getElementById("panelMovimientos")
        .classList.toggle(
            "d-none",
            tab !== "movimientos"
        );
}

function renderizarProductos() {

    const tbody =
        document.getElementById(
            "tablaProductos"
        );

    const vacio =
        document.getElementById(
            "productosVacio"
        );

    const busqueda =
        document
            .getElementById(
                "buscarProducto"
            )
            .value
            .trim()
            .toLowerCase();

    const categoria =
        document.getElementById(
            "filtroCategoria"
        ).value;

    const stock =
        document.getElementById(
            "filtroStock"
        ).value;

    const lista =
        productos.filter(producto => {

            const texto =
                `${producto.nombre || ""} ${producto.codigo || ""}`
                    .toLowerCase();

            if (
                busqueda &&
                !texto.includes(busqueda)
            ) {
                return false;
            }

            if (
                categoria &&
                String(
                    producto.idCategoria
                ) !== categoria
            ) {
                return false;
            }

            const actual =
                Number(producto.stock);

            const minimo =
                Number(
                    producto.stockMinimo
                );

            if (
                stock === "disponible" &&
                actual <= minimo
            ) {
                return false;
            }

            if (
                stock === "bajo" &&
                (
                    actual <= 0 ||
                    actual > minimo
                )
            ) {
                return false;
            }

            if (
                stock === "agotado" &&
                actual > 0
            ) {
                return false;
            }

            return true;
        });

    tbody.innerHTML = "";

    if (lista.length === 0) {
        vacio.classList.remove(
            "d-none"
        );

        return;
    }

    vacio.classList.add("d-none");

    lista.forEach(producto => {

        const fila =
            document.createElement("tr");

        fila.innerHTML = `
            <td>
                <div class="producto-info">

                    ${miniaturaProducto(producto)}

                    <div>
                        <div class="producto-nombre">
                            ${escapeHtml(producto.nombre)}
                        </div>

                        <div class="producto-descripcion">
                            ${
                                escapeHtml(
                                    producto.descripcion ||
                                    "Sin descripción"
                                )
                            }
                        </div>
                    </div>

                </div>
            </td>

            <td>
                <span class="categoria-badge">
                    ${
                        escapeHtml(
                            producto.categoria ||
                            "Sin categoría"
                        )
                    }
                </span>
            </td>

            <td>
                ${
                    escapeHtml(
                        producto.codigo ||
                        "—"
                    )
                }
            </td>

            <td>
                ${badgeStock(producto)}
            </td>

            <td>
                <strong>
                    ${dinero(
                        producto.precioVenta
                    )}
                </strong>
            </td>

            <td>
                <div class="producto-actions">

                    <button
                        type="button"
                        class="btn btn-light"
                        title="Ajustar stock"
                        onclick="abrirAjuste(${producto.idProducto})"
                    >
                        <i class="bi bi-arrow-left-right"></i>
                    </button>

                    <button
                        type="button"
                        class="btn btn-light"
                        title="Editar"
                        onclick="editarProducto(${producto.idProducto})"
                    >
                        <i class="bi bi-pencil"></i>
                    </button>

                    <button
                        type="button"
                        class="btn btn-light text-danger"
                        title="Desactivar"
                        onclick="desactivarProducto(${producto.idProducto})"
                    >
                        <i class="bi bi-trash3"></i>
                    </button>

                </div>
            </td>
        `;

        tbody.appendChild(fila);
    });
}

function renderizarStockBajo() {

    const contenedor =
        document.getElementById(
            "listaStockBajo"
        );

    const vacio =
        document.getElementById(
            "stockVacio"
        );

    contenedor.innerHTML = "";

    if (stockBajo.length === 0) {

        contenedor.classList.add(
            "d-none"
        );

        vacio.classList.remove(
            "d-none"
        );

        return;
    }

    contenedor.classList.remove(
        "d-none"
    );

    vacio.classList.add("d-none");

    stockBajo.forEach(producto => {

        const columna =
            document.createElement("div");

        columna.className =
            "col-md-6 col-xl-4";

        columna.innerHTML = `
            <div class="stock-card">

                <div class="stock-card-image">
                    ${
                        producto.imagenUrl
                            ? `
                                <img
                                    src="${urlImagen(producto.imagenUrl)}"
                                    alt="${escapeHtml(producto.nombre)}"
                                >
                              `
                            : `
                                <i class="bi bi-image"></i>
                              `
                    }
                </div>

                <div class="flex-grow-1">
                    <h6>
                        ${escapeHtml(producto.nombre)}
                    </h6>

                    <small>
                        ${
                            escapeHtml(
                                producto.categoria ||
                                "Sin categoría"
                            )
                        }
                    </small>

                    <div class="stock-value">
                        Stock:
                        ${numero(producto.stock)}
                        ·
                        Mínimo:
                        ${numero(producto.stockMinimo)}
                    </div>

                    <button
                        type="button"
                        class="btn btn-sm btn-outline-success mt-2"
                        onclick="abrirAjuste(${producto.idProducto})"
                    >
                        Ajustar
                    </button>
                </div>

            </div>
        `;

        contenedor.appendChild(
            columna
        );
    });
}

function renderizarMovimientos() {

    const tbody =
        document.getElementById(
            "tablaMovimientos"
        );

    const vacio =
        document.getElementById(
            "movimientosVacio"
        );

    tbody.innerHTML = "";

    if (
        movimientos.length === 0
    ) {
        vacio.classList.remove(
            "d-none"
        );

        return;
    }

    vacio.classList.add("d-none");

    movimientos.forEach(
        movimiento => {

            const entrada =
                movimiento.tipoMovimiento ===
                    "AJUSTE_ENTRADA" ||
                movimiento.tipoMovimiento ===
                    "COMPRA";

            const fila =
                document.createElement(
                    "tr"
                );

            fila.innerHTML = `
                <td>
                    <strong>
                        ${escapeHtml(
                            movimiento.producto
                        )}
                    </strong>

                    ${
                        movimiento.observacion
                            ? `
                                <div class="small text-muted">
                                    ${escapeHtml(
                                        movimiento.observacion
                                    )}
                                </div>
                              `
                            : ""
                    }
                </td>

                <td>
                    <span class="${
                        entrada
                            ? "movimiento-entrada"
                            : "movimiento-salida"
                    }">
                        ${
                            nombreMovimiento(
                                movimiento.tipoMovimiento
                            )
                        }
                    </span>
                </td>

                <td>
                    ${
                        entrada ? "+" : "-"
                    }${numero(
                        movimiento.cantidad
                    )}
                </td>

                <td>
                    ${numero(
                        movimiento.stockAnterior
                    )}
                </td>

                <td>
                    <strong>
                        ${numero(
                            movimiento.stockNuevo
                        )}
                    </strong>
                </td>

                <td>
                    ${fecha(
                        movimiento.fecha
                    )}
                </td>
            `;

            tbody.appendChild(
                fila
            );
        }
    );
}

function actualizarResumen() {

    document.getElementById(
        "totalProductos"
    ).textContent =
        productos.length;

    const valor =
        productos.reduce(
            (
                total,
                producto
            ) =>
                total +
                Number(
                    producto.costo || 0
                ) *
                Number(
                    producto.stock || 0
                ),
            0
        );

    document.getElementById(
        "valorInventario"
    ).textContent =
        dinero(valor);

    document.getElementById(
        "cantidadStockBajo"
    ).textContent =
        stockBajo.length;

    document.getElementById(
        "badgeStockBajo"
    ).textContent =
        stockBajo.length;

    document.getElementById(
        "cantidadSinImagen"
    ).textContent =
        productos.filter(
            producto =>
                !producto.imagenUrl
        ).length;
}

function abrirNuevoProducto() {

    productoEditando = null;
    imagenSeleccionada = null;

    document
        .getElementById(
            "formProducto"
        )
        .reset();

    document.getElementById(
        "tituloModalProducto"
    ).textContent =
        "Nuevo producto";

    document.getElementById(
        "grupoStockInicial"
    ).classList.remove(
        "d-none"
    );

    document.getElementById(
        "productoCosto"
    ).value = 0;

    document.getElementById(
        "productoPrecio"
    ).value = 0;

    document.getElementById(
        "productoStockInicial"
    ).value = 0;

    document.getElementById(
        "productoStockMinimo"
    ).value = 0;

    limpiarPreview();

    document.getElementById(
        "btnEliminarImagen"
    ).classList.add(
        "d-none"
    );

    ocultarAlerta(
        "alertaProducto"
    );

    modalProducto.show();
}

async function editarProducto(
    idProducto
) {

    try {

        const producto =
            await apiRequest(
                `/productos/${idProducto}`
            );

        productoEditando =
            producto;

        imagenSeleccionada =
            null;

        document.getElementById(
            "productoNombre"
        ).value =
            producto.nombre || "";

        document.getElementById(
            "productoCodigo"
        ).value =
            producto.codigo || "";

        document.getElementById(
            "productoCategoria"
        ).value =
            producto.idCategoria ?? "";

        document.getElementById(
            "productoDescripcion"
        ).value =
            producto.descripcion || "";

        document.getElementById(
            "productoCosto"
        ).value =
            producto.costo || 0;

        document.getElementById(
            "productoPrecio"
        ).value =
            producto.precioVenta || 0;

        document.getElementById(
            "productoStockMinimo"
        ).value =
            producto.stockMinimo || 0;

        document.getElementById(
            "tituloModalProducto"
        ).textContent =
            "Editar producto";

        document.getElementById(
            "grupoStockInicial"
        ).classList.add(
            "d-none"
        );

        if (
            producto.imagenUrl
        ) {
            mostrarPreview(
                urlImagen(
                    producto.imagenUrl
                )
            );

            document.getElementById(
                "btnEliminarImagen"
            ).classList.remove(
                "d-none"
            );
        } else {
            limpiarPreview();

            document.getElementById(
                "btnEliminarImagen"
            ).classList.add(
                "d-none"
            );
        }

        document.getElementById(
            "productoImagen"
        ).value = "";

        ocultarAlerta(
            "alertaProducto"
        );

        modalProducto.show();

    } catch (error) {
        mostrarToast(
            error.message,
            "danger"
        );
    }
}

async function guardarProducto(
    event
) {

    event.preventDefault();

    const idCategoria =
        document.getElementById(
            "productoCategoria"
        ).value;

    const datos = {

        idCategoria:
            idCategoria
                ? Number(
                    idCategoria
                )
                : null,

        codigo:
            valorOpcional(
                "productoCodigo"
            ),

        nombre:
            document.getElementById(
                "productoNombre"
            ).value.trim(),

        descripcion:
            valorOpcional(
                "productoDescripcion"
            ),

        costo:
            valorNumero(
                "productoCosto"
            ),

        precioVenta:
            valorNumero(
                "productoPrecio"
            ),

        stockMinimo:
            valorNumero(
                "productoStockMinimo"
            )
    };

    if (!datos.nombre) {

        mostrarAlerta(
            "alertaProducto",
            "El nombre es obligatorio."
        );

        return;
    }

    try {

        let guardado;

        if (
            productoEditando
        ) {

            guardado =
                await apiRequest(
                    `/productos/${productoEditando.idProducto}`,
                    {
                        method: "PUT",
                        body:
                            JSON.stringify(
                                datos
                            )
                    }
                );

        } else {

            datos.stockInicial =
                valorNumero(
                    "productoStockInicial"
                );

            guardado =
                await apiRequest(
                    "/productos",
                    {
                        method: "POST",
                        body:
                            JSON.stringify(
                                datos
                            )
                    }
                );
        }

        if (
            imagenSeleccionada
        ) {
            await subirImagen(
                guardado.idProducto,
                imagenSeleccionada
            );
        }

        modalProducto.hide();

        mostrarToast(
            productoEditando
                ? "Producto actualizado."
                : "Producto registrado.",
            "success"
        );

        await refrescarDatos();

    } catch (error) {

        mostrarAlerta(
            "alertaProducto",
            error.message
        );
    }
}

async function desactivarProducto(
    idProducto
) {

    const producto =
        productos.find(
            p =>
                p.idProducto ===
                idProducto
        );

    if (!producto) {
        return;
    }

    if (
        !confirm(
            `¿Desactivar "${producto.nombre}"?`
        )
    ) {
        return;
    }

    try {

        await apiRequest(
            `/productos/${idProducto}`,
            {
                method: "DELETE"
            }
        );

        mostrarToast(
            "Producto desactivado.",
            "success"
        );

        await refrescarDatos();

    } catch (error) {
        mostrarToast(
            error.message,
            "danger"
        );
    }
}

function abrirAjuste(
    idProducto = null
) {

    document
        .getElementById(
            "formAjuste"
        )
        .reset();

    cargarSelectAjuste();

    if (idProducto) {
        document.getElementById(
            "ajusteProducto"
        ).value =
            String(idProducto);
    }

    ocultarAlerta(
        "alertaAjuste"
    );

    modalAjuste.show();
}

function cargarSelectAjuste() {

    const select =
        document.getElementById(
            "ajusteProducto"
        );

    select.innerHTML =
        `
        <option value="">
            Seleccionar producto
        </option>
        `;

    productos.forEach(
        producto => {

            select.insertAdjacentHTML(
                "beforeend",
                `
                <option value="${producto.idProducto}">
                    ${escapeHtml(producto.nombre)}
                    · Stock ${numero(producto.stock)}
                </option>
                `
            );
        }
    );
}

async function guardarAjuste(
    event
) {

    event.preventDefault();

    const datos = {

        idProducto:
            Number(
                document.getElementById(
                    "ajusteProducto"
                ).value
            ),

        tipoMovimiento:
            document.getElementById(
                "ajusteTipo"
            ).value,

        cantidad:
            valorNumero(
                "ajusteCantidad"
            ),

        observacion:
            valorOpcional(
                "ajusteObservacion"
            )
    };

    if (
        !datos.idProducto ||
        datos.cantidad <= 0
    ) {

        mostrarAlerta(
            "alertaAjuste",
            "Ingresa un producto y una cantidad válida."
        );

        return;
    }

    try {

        await apiRequest(
            "/inventario/ajuste",
            {
                method: "POST",
                body:
                    JSON.stringify(
                        datos
                    )
            }
        );

        modalAjuste.hide();

        mostrarToast(
            "Stock actualizado.",
            "success"
        );

        await refrescarDatos();

    } catch (error) {

        mostrarAlerta(
            "alertaAjuste",
            error.message
        );
    }
}

function seleccionarImagen(
    event
) {

    const archivo =
        event.target.files[0];

    if (!archivo) {
        return;
    }

    const permitidos = [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    if (
        !permitidos.includes(
            archivo.type
        )
    ) {

        mostrarAlerta(
            "alertaProducto",
            "Solo se permiten JPG, PNG o WEBP."
        );

        event.target.value = "";

        return;
    }

    if (
        archivo.size >
        5 * 1024 * 1024
    ) {

        mostrarAlerta(
            "alertaProducto",
            "La imagen no puede superar los 5 MB."
        );

        event.target.value = "";

        return;
    }

    imagenSeleccionada =
        archivo;

    mostrarPreview(
        URL.createObjectURL(
            archivo
        )
    );
}

async function subirImagen(
    idProducto,
    archivo
) {

    const formData =
        new FormData();

    formData.append(
        "imagen",
        archivo
    );

    const response =
        await fetch(
            `${API_URL}/productos/${idProducto}/imagen`,
            {
                method: "POST",

                headers: {
                    Authorization:
                        `Bearer ${getAuthToken()}`
                },

                body:
                    formData
            }
        );

    if (!response.ok) {

        let mensaje =
            "No se pudo subir la imagen.";

        try {

            const error =
                await response.json();

            mensaje =
                error.mensaje ||
                mensaje;

        } catch {
        }

        throw new Error(
            mensaje
        );
    }

    return await response.json();
}

async function eliminarImagen() {

    if (
        !productoEditando ||
        !productoEditando.imagenUrl
    ) {
        return;
    }

    if (
        !confirm(
            "¿Eliminar la imagen del producto?"
        )
    ) {
        return;
    }

    try {

        await apiRequest(
            `/productos/${productoEditando.idProducto}/imagen`,
            {
                method: "DELETE"
            }
        );

        productoEditando.imagenUrl =
            null;

        imagenSeleccionada =
            null;

        limpiarPreview();

        document.getElementById(
            "btnEliminarImagen"
        ).classList.add(
            "d-none"
        );

        mostrarToast(
            "Imagen eliminada.",
            "success"
        );

        await cargarProductos();

    } catch (error) {

        mostrarAlerta(
            "alertaProducto",
            error.message
        );
    }
}

async function refrescarDatos() {

    await Promise.all([
        cargarProductos(),
        cargarStockBajo(),
        cargarMovimientos()
    ]);

    actualizarResumen();
}

function miniaturaProducto(
    producto
) {

    if (
        !producto.imagenUrl
    ) {
        return `
            <div class="producto-miniatura">
                <i class="bi bi-image"></i>
            </div>
        `;
    }

    return `
        <div class="producto-miniatura">
            <img
                src="${urlImagen(producto.imagenUrl)}"
                alt="${escapeHtml(producto.nombre)}"
            >
        </div>
    `;
}

function badgeStock(
    producto
) {

    const stock =
        Number(producto.stock);

    const minimo =
        Number(
            producto.stockMinimo
        );

    if (stock <= 0) {
        return `
            <span class="stock-badge stock-agotado">
                Agotado
            </span>
        `;
    }

    if (stock <= minimo) {
        return `
            <span class="stock-badge stock-bajo">
                ${numero(stock)}
            </span>
        `;
    }

    return `
        <span class="stock-badge stock-ok">
            ${numero(stock)}
        </span>
    `;
}

function mostrarPreview(url) {

    const imagen =
        document.getElementById(
            "imagenPreview"
        );

    const placeholder =
        document.getElementById(
            "imagenPlaceholder"
        );

    imagen.src = url;

    imagen.classList.remove(
        "d-none"
    );

    placeholder.classList.add(
        "d-none"
    );
}

function limpiarPreview() {

    const imagen =
        document.getElementById(
            "imagenPreview"
        );

    imagen.removeAttribute(
        "src"
    );

    imagen.classList.add(
        "d-none"
    );

    document.getElementById(
        "imagenPlaceholder"
    ).classList.remove(
        "d-none"
    );
}

function urlImagen(ruta) {

    if (!ruta) {
        return "";
    }

    if (
        ruta.startsWith(
            "http"
        )
    ) {
        return ruta;
    }

    return (
        INVENTARIO_MEDIA_URL +
        (
            ruta.startsWith("/")
                ? ruta
                : `/${ruta}`
        )
    );
}

function nombreMovimiento(
    tipo
) {

    const nombres = {
        AJUSTE_ENTRADA:
            "Entrada",
        AJUSTE_SALIDA:
            "Salida",
        COMPRA:
            "Compra",
        VENTA:
            "Venta"
    };

    return nombres[tipo] || tipo;
}

function dinero(valor) {

    return new Intl.NumberFormat(
        "es-EC",
        {
            style:
                "currency",

            currency:
                "USD"
        }
    ).format(
        Number(valor || 0)
    );
}

function numero(valor) {

    return new Intl.NumberFormat(
        "es-EC",
        {
            maximumFractionDigits:
                2
        }
    ).format(
        Number(valor || 0)
    );
}

function fecha(valor) {

    if (!valor) {
        return "—";
    }

    return new Intl.DateTimeFormat(
        "es-EC",
        {
            dateStyle:
                "short",

            timeStyle:
                "short"
        }
    ).format(
        new Date(valor)
    );
}

function valorNumero(id) {

    const valor =
        Number(
            document.getElementById(
                id
            ).value
        );

    return Number.isFinite(
        valor
    )
        ? valor
        : 0;
}

function valorOpcional(id) {

    const valor =
        document
            .getElementById(id)
            .value
            .trim();

    return valor || null;
}

function mostrarAlerta(
    id,
    mensaje
) {

    const alerta =
        document.getElementById(
            id
        );

    alerta.className =
        "alert alert-danger";

    alerta.textContent =
        mensaje;
}

function ocultarAlerta(id) {

    const alerta =
        document.getElementById(
            id
        );

    alerta.className =
        "alert d-none";

    alerta.textContent = "";
}

function mostrarToast(
    mensaje,
    tipo
) {

    const toast =
        document.createElement(
            "div"
        );

    toast.className =
        `toast border-0 text-white ${
            tipo === "danger"
                ? "bg-danger"
                : "bg-success"
        }`;

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${escapeHtml(mensaje)}
            </div>

            <button
                type="button"
                class="btn-close btn-close-white me-2 m-auto"
                data-bs-dismiss="toast"
            ></button>
        </div>
    `;

    document.getElementById(
        "toastContainer"
    ).appendChild(
        toast
    );

    const instancia =
        new bootstrap.Toast(
            toast,
            {
                delay: 3000
            }
        );

    toast.addEventListener(
        "hidden.bs.toast",
        () => toast.remove()
    );

    instancia.show();
}

function escapeHtml(valor) {

    return String(
        valor ?? ""
    )
        .replaceAll(
            "&",
            "&amp;"
        )
        .replaceAll(
            "<",
            "&lt;"
        )
        .replaceAll(
            ">",
            "&gt;"
        )
        .replaceAll(
            '"',
            "&quot;"
        )
        .replaceAll(
            "'",
            "&#039;"
        );
}