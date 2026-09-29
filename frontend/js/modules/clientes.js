const API_CLIENTES = "/clientes";
let clientesCargados = [];

document.addEventListener("DOMContentLoaded", async () => {
    // Verificar sesión
    const token = localStorage.getItem("tiendi_token");

    if (!token) {
        window.location.href = "../login.html";
        return;
    }

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
    const totalClientes = clientes.length;

    document.getElementById("total-clientes").textContent =
        totalClientes;

    document.getElementById("total-por-cobrar").textContent =
        "$0.00";
}