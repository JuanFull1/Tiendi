const path =
    window.location.pathname.replaceAll("\\", "/");

const inPages =
    path.includes("/pages/");

const base =
    inPages ? "../" : "./";

const sidebar =
    document.getElementById("sidebar");

const topbar =
    document.getElementById("topbar");

const usuario =
    getStoredUser();

if (sidebar) {

    sidebar.innerHTML = `

        <div class="sidebar-brand">
            TIENDI
        </div>

        <nav class="nav flex-column">

            <a
                class="nav-link"
                href="${base}index.html"
                data-page="index"
            >
                Inicio
            </a>

            <a
                class="nav-link"
                href="${base}pages/ventas.html"
                data-page="ventas"
            >
                Vender
            </a>

            <a
                class="nav-link"
                href="${base}pages/balance.html"
                data-page="balance"
            >
                Balance
            </a>

            <a
                class="nav-link"
                href="${base}pages/inventario.html"
                data-page="inventario"
            >
                Inventario
            </a>

            <a
                class="nav-link"
                href="${base}pages/estadisticas.html"
                data-page="estadisticas"
            >
                Estadísticas
            </a>

            <a
                class="nav-link"
                href="${base}pages/clientes.html"
                data-page="clientes"
            >
                Clientes
            </a>

            <a
                class="nav-link"
                href="${base}pages/proveedores.html"
                data-page="proveedores"
            >
                Proveedores
            </a>

        </nav>
    `;
}

if (topbar) {

    topbar.innerHTML = `

        <nav
            class="navbar bg-white border-bottom px-4"
        >

            <span class="navbar-brand mb-0">
                Sistema de Inventario y Ventas
            </span>

            <div
                class="d-flex align-items-center gap-3"
            >

                <div class="text-end">

                    <div class="fw-semibold">
                        ${usuario?.nombre ?? "Usuario"}
                    </div>

                    <small class="text-muted">
                        ${usuario?.rol ?? ""}
                    </small>

                </div>

                <button
                    type="button"
                    class="btn btn-outline-secondary btn-sm"
                    onclick="logout()"
                >
                    Salir
                </button>

            </div>

        </nav>
    `;
}

const fileName =
    path.split("/").pop();

const currentPage =
    fileName.replace(".html", "") || "index";

document
    .querySelectorAll("#sidebar .nav-link")
    .forEach(link => {

        if (
            link.dataset.page === currentPage
        ) {
            link.classList.add("active");
        }
    });