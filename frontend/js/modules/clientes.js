const API_CLIENTES = "/clientes";
let clientesCargados = [];
let modalCliente = null;

document.addEventListener("DOMContentLoaded", async () => {
    // Verificar sesión
    const token = localStorage.getItem("tiendi_token");

    if (!token) {
        window.location.href = "../login.html";
        return;
    }

    // Inicializar modal
    modalCliente = new bootstrap.Modal(
        document.getElementById("modalCliente")
    );

    document
        .getElementById("btn-nuevo")
        .addEventListener("click", abrirModalNuevo);

    document
        .getElementById("btn-guardar")
        .addEventListener("click", guardarCliente);

    document
        .getElementById("buscar")
        .addEventListener("input", debounce(buscarClientes, 300));

    // Cargar clientes al iniciar
    await cargarClientes();
});

async function cargarClientes() {
    try {
        const clientes = await apiRequest(API_CLIENTES);

        clientesCargados = clientes ?? [];

        renderizarTabla(clientesCargados);
        actualizarResumen(clientesCargados);
    } catch (error) {
        console.error("Error al cargar clientes:", error);

        document.getElementById("tabla-clientes").innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-danger py-4">
                    No se pudieron cargar los clientes.
                </td>
            </tr>
        `;
    }
}

async function buscarClientes() {
    const termino = document
        .getElementById("buscar")
        .value
        .trim();

    if (!termino) {
        renderizarTabla(clientesCargados);
        actualizarResumen(clientesCargados);
        return;
    }

    try {
        const resultados = await apiRequest(
            `${API_CLIENTES}/buscar?termino=${encodeURIComponent(termino)}`
        );

        renderizarTabla(resultados ?? []);
    } catch (error) {
        console.error("Error al buscar clientes:", error);
    }
}

function renderizarTabla(clientes) {
    const tbody = document.getElementById("tabla-clientes");

    if (!clientes || clientes.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No hay clientes registrados.
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = clientes
        .map(cliente => {
            const nombreCompleto =
                `${cliente.nombre ?? ""} ${cliente.apellido ?? ""}`.trim();

            return `
                <tr>
                    <td>${nombreCompleto || "—"}</td>
                    <td>${cliente.telefono ?? "—"}</td>
                    <td>${cliente.identificacion ?? "—"}</td>
                    <td class="text-end">—</td>
                    <td class="text-center">
                        <span class="badge bg-secondary">Sin deuda</span>
                    </td>
                    <td class="text-end">
                        <button
                            class="btn btn-sm btn-outline-primary"
                            onclick="editarCliente(${cliente.idCliente})"
                        >
                            Editar
                        </button>

                        <button
                            class="btn btn-sm btn-outline-danger"
                            onclick="desactivarCliente(${cliente.idCliente})"
                        >
                            Desactivar
                        </button>
                    </td>
                </tr>
            `;
        })
        .join("");
}

function actualizarResumen(clientes) {
    document.getElementById("total-clientes").textContent =
        clientes.length;

    document.getElementById("total-por-cobrar").textContent =
        "$0.00";
}

function abrirModalNuevo() {
    document.getElementById("modalTitulo").textContent = "Nuevo cliente";
    document.getElementById("form-cliente").reset();
    document.getElementById("cliente-id").value = "";

    modalCliente.show();
}

async function editarCliente(id) {
    try {
        const cliente = await apiRequest(`${API_CLIENTES}/${id}`);

        document.getElementById("modalTitulo").textContent = "Editar cliente";
        document.getElementById("cliente-id").value = cliente.idCliente;
        document.getElementById("nombre").value = cliente.nombre ?? "";
        document.getElementById("apellido").value = cliente.apellido ?? "";
        document.getElementById("identificacion").value =
            cliente.identificacion ?? "";
        document.getElementById("telefono").value = cliente.telefono ?? "";
        document.getElementById("email").value = cliente.email ?? "";
        document.getElementById("direccion").value = cliente.direccion ?? "";

        modalCliente.show();
    } catch (error) {
        console.error("Error al cargar cliente:", error);
        alert("No se pudo cargar el cliente.");
    }
}

async function guardarCliente() {
    const id = document.getElementById("cliente-id").value;

    const payload = {
        nombre: document.getElementById("nombre").value.trim(),
        apellido: document.getElementById("apellido").value.trim(),
        identificacion: document.getElementById("identificacion").value.trim(),
        telefono: document.getElementById("telefono").value.trim(),
        email: document.getElementById("email").value.trim(),
        direccion: document.getElementById("direccion").value.trim()
    };

    if (!payload.nombre) {
        alert("El nombre es obligatorio.");
        return;
    }

    try {
        if (id) {
            payload.activo = true;

            await apiRequest(`${API_CLIENTES}/${id}`, {
                method: "PUT",
                body: JSON.stringify(payload)
            });
        } else {
            await apiRequest(API_CLIENTES, {
                method: "POST",
                body: JSON.stringify(payload)
            });
        }

        modalCliente.hide();
        await cargarClientes();
    } catch (error) {
        console.error("Error al guardar cliente:", error);
        alert(error.message || "No se pudo guardar el cliente.");
    }
}

async function desactivarCliente(id) {
    if (!confirm("¿Desactivar este cliente?")) {
        return;
    }

    try {
        await apiRequest(`${API_CLIENTES}/${id}`, {
            method: "DELETE"
        });

        await cargarClientes();
    } catch (error) {
        console.error("Error al desactivar cliente:", error);
        alert(error.message || "No se pudo desactivar el cliente.");
    }
}

function debounce(fn, ms) {
    let timeout;

    return (...args) => {
        clearTimeout(timeout);
        timeout = setTimeout(() => fn(...args), ms);
    };
}