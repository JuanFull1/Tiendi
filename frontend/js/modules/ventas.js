const ventasState = {
    clientes: [],
    productos: [],
    ventas: [],
    productosVenta: [],
    enviando: false
};

document.addEventListener("DOMContentLoaded", () => {
    inicializarVentas();
});

function inicializarVentas() {
    const clienteSelect = document.getElementById("clienteSelect");
    const productoSelect = document.getElementById("productoSelect");

    if (!clienteSelect || !productoSelect) {
        return;
    }

    document
        .getElementById("btnAgregarProducto")
        .addEventListener("click", agregarProductoAVenta);

    document
        .getElementById("btnConfirmarVenta")
        .addEventListener("click", confirmarVenta);

    document
        .getElementById("productoSelect")
        .addEventListener("change", actualizarInfoProducto);

    document
        .getElementById("descuentoGeneral")
        .addEventListener("input", actualizarTotales);

    document
        .getElementById("detalleVentaBody")
        .addEventListener("click", (event) => {
            const boton = event.target.closest("[data-remove-item]");

            if (!boton) {
                return;
            }

            const index = Number(boton.dataset.removeItem);

            if (Number.isNaN(index)) {
                return;
            }

            ventasState.productosVenta.splice(index, 1);
            renderDetalleVenta();
            actualizarTotales();
        });

    cargarClientes();
    cargarProductos();
    cargarVentas();
}

async function cargarClientes() {
    try {
        const clientes = await apiRequest("/clientes") || [];
        ventasState.clientes = clientes;

        const clienteSelect = document.getElementById("clienteSelect");

        clienteSelect.innerHTML = `
            <option value="">Consumidor final / Sin cliente</option>
            ${clientes
                .filter(cliente => cliente && cliente.activo !== false)
                .map(cliente => `
                    <option value="${cliente.idCliente}">
                        ${escapeHtml(cliente.nombre)} ${escapeHtml(cliente.apellido ?? "")}
                    </option>
                `)
                .join("")}
        `;
    } catch (error) {
        console.error("Error al cargar clientes:", error);
        mostrarToast("No se pudieron cargar los clientes.", "danger");
    }
}

async function cargarProductos() {
    try {
        const productos = await apiRequest("/productos") || [];
        ventasState.productos = productos.filter(producto => producto && producto.activo !== false);

        const productoSelect = document.getElementById("productoSelect");

        productoSelect.innerHTML = `
            <option value="">Seleccionar producto</option>
            ${ventasState.productos
                .map(producto => `
                    <option value="${producto.idProducto}">
                        ${escapeHtml(producto.nombre)} - ${formatCurrency(producto.precioVenta)}
                    </option>
                `)
                .join("")}
        `;

        if (ventasState.productos.length === 0) {
            productoSelect.disabled = true;
        } else {
            productoSelect.disabled = false;
        }

        actualizarInfoProducto();
    } catch (error) {
        console.error("Error al cargar productos:", error);
        mostrarToast("No se pudieron cargar los productos.", "danger");
    }
}

async function cargarVentas() {
    try {
        const ventas = await apiRequest("/ventas") || [];
        ventasState.ventas = ventas;
        renderVentasList();
    } catch (error) {
        console.error("Error al cargar ventas:", error);
        mostrarToast("No se pudieron cargar las ventas recientes.", "danger");
    }
}

function actualizarInfoProducto() {
    const productoSelect = document.getElementById("productoSelect");
    const producto = ventasState.productos.find(item => item.idProducto === Number(productoSelect.value));
    const productoInfo = document.getElementById("productoInfo");
    const stockInfo = document.getElementById("stockInfo");

    if (!producto) {
        productoInfo.textContent = "Selecciona un producto para ver su stock y precio.";
        stockInfo.textContent = "";
        return;
    }

    productoInfo.textContent = `${producto.nombre} - ${formatCurrency(producto.precioVenta)}`;
    stockInfo.textContent = `Stock disponible: ${Number(producto.stock)}`;
}

function agregarProductoAVenta() {
    const productoSelect = document.getElementById("productoSelect");
    const cantidadInput = document.getElementById("cantidadProducto");
    const descuentoInput = document.getElementById("descuentoProducto");

    const idProducto = Number(productoSelect.value);
    const producto = ventasState.productos.find(item => item.idProducto === idProducto);

    if (!producto) {
        mostrarAlerta("Debes seleccionar un producto válido.");
        return;
    }

    const cantidad = Number(cantidadInput.value);

    if (!Number.isFinite(cantidad) || cantidad <= 0) {
        mostrarAlerta("La cantidad debe ser mayor que cero.");
        return;
    }

    if (cantidad > Number(producto.stock)) {
        mostrarAlerta(`Stock insuficiente para ${producto.nombre}. Disponible: ${producto.stock}.`);
        return;
    }

    const descuento = Number(descuentoInput.value || 0);

    if (!Number.isFinite(descuento) || descuento < 0) {
        mostrarAlerta("El descuento del producto no puede ser negativo.");
        return;
    }

    if (descuento > producto.precioVenta * cantidad) {
        mostrarAlerta("El descuento del producto no puede superar el importe total del producto.");
        return;
    }

    const existente = ventasState.productosVenta.find(item => item.idProducto === idProducto);

    if (existente) {
        existente.cantidad = Number((existente.cantidad + cantidad).toFixed(2));
        existente.descuento = Number((existente.descuento + descuento).toFixed(2));
    } else {
        ventasState.productosVenta.push({
            idProducto: producto.idProducto,
            nombre: producto.nombre,
            cantidad: Number(cantidad.toFixed(2)),
            precioUnitario: Number(producto.precioVenta),
            descuento: Number(descuento.toFixed(2))
        });
    }

    ocultarAlerta();
    document.getElementById("cantidadProducto").value = "1";
    document.getElementById("descuentoProducto").value = "0";
    productoSelect.selectedIndex = 0;
    actualizarInfoProducto();
    renderDetalleVenta();
    actualizarTotales();
}

function renderDetalleVenta() {
    const tbody = document.getElementById("detalleVentaBody");

    if (!ventasState.productosVenta.length) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    Aún no has agregado productos.
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = ventasState.productosVenta
        .map((producto, index) => {
            const subtotalProducto = (producto.cantidad * producto.precioUnitario) - producto.descuento;

            return `
                <tr>
                    <td>${escapeHtml(producto.nombre)}</td>
                    <td>${Number(producto.cantidad)}</td>
                    <td>${formatCurrency(producto.precioUnitario)}</td>
                    <td>${formatCurrency(producto.descuento)}</td>
                    <td>${formatCurrency(Math.max(subtotalProducto, 0))}</td>
                    <td class="text-end">
                        <button
                            type="button"
                            class="btn btn-sm btn-outline-danger"
                            data-remove-item="${index}"
                        >
                            Quitar
                        </button>
                    </td>
                </tr>
            `;
        })
        .join("");
}

function actualizarTotales() {
    const subtotal = ventasState.productosVenta.reduce((total, producto) => {
        return total + (producto.cantidad * producto.precioUnitario);
    }, 0);

    const descuentoProductos = ventasState.productosVenta.reduce((total, producto) => {
        return total + producto.descuento;
    }, 0);

    const descuentoGeneral = Number(document.getElementById("descuentoGeneral").value || 0);
    const total = Math.max(subtotal - descuentoProductos - descuentoGeneral, 0);

    document.getElementById("subtotalVenta").textContent = formatCurrency(subtotal);
    document.getElementById("descuentoVenta").textContent = formatCurrency(descuentoProductos + descuentoGeneral);
    document.getElementById("totalVenta").textContent = formatCurrency(total);
}

async function confirmarVenta() {
    if (ventasState.enviando) {
        return;
    }

    if (!ventasState.productosVenta.length) {
        mostrarAlerta("Debes agregar al menos un producto antes de confirmar la venta.");
        return;
    }

    const metodoPago = document.getElementById("metodoPago").value;

    if (!metodoPago) {
        mostrarAlerta("Debes seleccionar un método de pago.");
        return;
    }

    const descuentoGeneral = Number(document.getElementById("descuentoGeneral").value || 0);

    if (!Number.isFinite(descuentoGeneral) || descuentoGeneral < 0) {
        mostrarAlerta("El descuento general no puede ser negativo.");
        return;
    }

    const subtotal = ventasState.productosVenta.reduce((total, producto) => {
        return total + (producto.cantidad * producto.precioUnitario);
    }, 0);

    if (descuentoGeneral > subtotal) {
        mostrarAlerta("El descuento general no puede superar el subtotal de la venta.");
        return;
    }

    const clienteSelect = document.getElementById("clienteSelect");
    const idCliente = clienteSelect.value ? Number(clienteSelect.value) : null;

    const payload = {
        idCliente,
        descuento: Number(descuentoGeneral.toFixed(2)),
        metodoPago,
        detalles: ventasState.productosVenta.map(producto => ({
            idProducto: Number(producto.idProducto),
            cantidad: Number(producto.cantidad),
            descuento: Number(producto.descuento.toFixed(2))
        }))
    };

    try {
        ventasState.enviando = true;
        const boton = document.getElementById("btnConfirmarVenta");
        boton.disabled = true;
        boton.textContent = "Procesando...";
        ocultarAlerta();

        await apiRequest("/ventas", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        mostrarToast("Venta registrada correctamente.", "success");
        ventasState.productosVenta = [];
        renderDetalleVenta();
        document.getElementById("descuentoGeneral").value = "0";
        document.getElementById("metodoPago").selectedIndex = 0;
        document.getElementById("clienteSelect").selectedIndex = 0;
        document.getElementById("productoSelect").selectedIndex = 0;
        document.getElementById("cantidadProducto").value = "1";
        document.getElementById("descuentoProducto").value = "0";
        actualizarInfoProducto();
        actualizarTotales();
        await cargarProductos();
        await cargarVentas();
    } catch (error) {
        console.error("Error al registrar la venta:", error);
        mostrarAlerta(error.message || "No se pudo registrar la venta.");
        mostrarToast(error.message || "Error al registrar la venta.", "danger");
    } finally {
        ventasState.enviando = false;
        const boton = document.getElementById("btnConfirmarVenta");
        boton.disabled = false;
        boton.textContent = "Confirmar venta";
    }
}

function renderVentasList() {
    const contenedor = document.getElementById("ventasList");

    if (!contenedor) {
        return;
    }

    if (!ventasState.ventas.length) {
        contenedor.innerHTML = `
            <div class="alert alert-light border mb-0 text-center text-muted">
                No hay ventas recientes.
            </div>
        `;
        return;
    }

    contenedor.innerHTML = ventasState.ventas
        .slice(0, 6)
        .map(venta => {
            const fecha = venta.fecha
                ? new Date(venta.fecha).toLocaleDateString("es-ES", {
                    day: "2-digit",
                    month: "2-digit",
                    year: "numeric"
                })
                : "Sin fecha";

            return `
                <div class="border rounded p-3 bg-light">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <strong>${escapeHtml(venta.numeroVenta || "Venta")}</strong>
                        <span class="badge text-bg-success">${escapeHtml(venta.estado || "COMPLETADA")}</span>
                    </div>

                    <div class="small text-muted mb-1">
                        ${escapeHtml(venta.nombreCliente || "Consumidor final")}
                    </div>

                    <div class="d-flex justify-content-between align-items-center small text-muted">
                        <span>${fecha}</span>
                        <strong class="text-dark">${formatCurrency(venta.total ?? 0)}</strong>
                    </div>
                </div>
            `;
        })
        .join("");
}

function mostrarAlerta(mensaje) {
    const alerta = document.getElementById("ventaAlert");
    alerta.textContent = mensaje;
    alerta.className = "alert alert-danger";
}

function ocultarAlerta() {
    const alerta = document.getElementById("ventaAlert");
    alerta.textContent = "";
    alerta.className = "alert alert-danger d-none";
}

function mostrarToast(mensaje, tipo = "success") {
    const container = document.getElementById("toastContainer");

    if (!container || !window.bootstrap) {
        return;
    }

    const toast = document.createElement("div");

    toast.className = `toast border-0 text-white ${tipo === "danger" ? "bg-danger" : "bg-success"}`;
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${escapeHtml(mensaje)}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Cerrar"></button>
        </div>
    `;

    container.appendChild(toast);

    const instancia = new bootstrap.Toast(toast, { delay: 3000 });
    instancia.show();

    toast.addEventListener("hidden.bs.toast", () => toast.remove());
}

function formatCurrency(value) {
    return new Intl.NumberFormat("es-ES", {
        style: "currency",
        currency: "USD",
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(Number(value || 0));
}

function escapeHtml(value) {
    return String(value ?? "").replace(/[&<>\"']/g, (char) => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#039;"
    })[char]);
}
