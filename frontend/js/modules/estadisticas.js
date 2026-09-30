let graficoVentas = null;
let graficoProductos = null;


document.addEventListener("DOMContentLoaded", async () => {

    await cargarEstadisticas();

});


async function cargarEstadisticas() {

    try {

        ocultarError();

        const [
            resumen,
            productos,
            ventasPorFecha
        ] = await Promise.all([

            apiRequest("/estadisticas/resumen"),

            apiRequest("/estadisticas/productos-mas-vendidos"),

            apiRequest("/estadisticas/ventas-por-fecha")

        ]);

        // Si apiRequest redirigió por sesión expirada
        if (!resumen) {
            return;
        }

        mostrarResumen(resumen);

        mostrarProductos(productos || []);

        mostrarVentasPorFecha(ventasPorFecha || []);

        crearGraficoProductos(productos || []);

        crearGraficoVentas(ventasPorFecha || []);

    } catch (error) {

        console.error(
            "Error cargando estadísticas:",
            error
        );

        mostrarError(error.message);

    }

}


/* ========================================
   RESUMEN
======================================== */

function mostrarResumen(resumen) {

    document.getElementById("total-ventas").textContent =
        formatoDinero(resumen.totalVentas);

    document.getElementById("cantidad-ventas").textContent =
        formatoNumero(resumen.cantidadVentas);

    document.getElementById("promedio-venta").textContent =
        formatoDinero(resumen.promedioVenta);

    document.getElementById("venta-mayor").textContent =
        formatoDinero(resumen.ventaMayor);

}


/* ========================================
   PRODUCTOS
======================================== */

function mostrarProductos(productos) {

    const tbody =
        document.getElementById("tabla-productos");

    tbody.innerHTML = "";

    if (productos.length === 0) {

        tbody.innerHTML = `
            <tr>
                <td
                    colspan="4"
                    class="tabla-vacia">
                    No existen productos vendidos.
                </td>
            </tr>
        `;

        return;
    }

    productos.forEach((producto, index) => {

        const fila =
            document.createElement("tr");

        fila.innerHTML = `
            <td>
                ${crearPosicion(index + 1)}
            </td>

            <td>
                <div class="producto-info">
                    <div class="producto-avatar">
                        ${obtenerInicial(producto.nombreProducto)}
                    </div>

                    <span>
                        ${escaparHtml(producto.nombreProducto)}
                    </span>
                </div>
            </td>

            <td class="text-end">
                <strong>
                    ${formatoCantidad(producto.cantidadVendida)}
                </strong>
            </td>

            <td class="text-end">
                ${formatoDinero(producto.totalVendido)}
            </td>
        `;

        tbody.appendChild(fila);

    });

}


/* ========================================
   VENTAS POR FECHA
======================================== */

function mostrarVentasPorFecha(ventas) {

    const tbody =
        document.getElementById("tabla-fechas");

    tbody.innerHTML = "";

    if (ventas.length === 0) {

        tbody.innerHTML = `
            <tr>
                <td
                    colspan="3"
                    class="tabla-vacia">
                    No existen ventas registradas.
                </td>
            </tr>
        `;

        return;
    }

    /*
        La API entrega las fechas ordenadas.
        Para la tabla mostramos primero
        los registros más recientes.
    */
    const ventasOrdenadas =
        [...ventas].reverse();

    ventasOrdenadas.forEach(venta => {

        const fila =
            document.createElement("tr");

        fila.innerHTML = `
            <td>
                <div class="fecha-tabla">
                    ${formatoFecha(venta.fecha)}
                </div>
            </td>

            <td class="text-center">
                <span class="badge-ventas">
                    ${formatoNumero(venta.cantidadVentas)}
                </span>
            </td>

            <td class="text-end">
                <strong>
                    ${formatoDinero(venta.totalVentas)}
                </strong>
            </td>
        `;

        tbody.appendChild(fila);

    });

}


/* ========================================
   GRÁFICO DE VENTAS
======================================== */

function crearGraficoVentas(ventas) {

    const canvas =
        document.getElementById("grafico-ventas");

    const sinDatos =
        document.getElementById("sin-datos-ventas");

    if (ventas.length === 0) {

        canvas.classList.add("d-none");

        sinDatos.classList.remove("d-none");

        return;
    }

    canvas.classList.remove("d-none");

    sinDatos.classList.add("d-none");

    const etiquetas =
        ventas.map(
            venta => formatoFechaCorta(venta.fecha)
        );

    const valores =
        ventas.map(
            venta => Number(venta.totalVentas)
        );

    if (graficoVentas) {
        graficoVentas.destroy();
    }

    graficoVentas = new Chart(
        canvas,
        {
            type: "line",

            data: {

                labels: etiquetas,

                datasets: [
                    {
                        label: "Total vendido",

                        data: valores,

                        borderColor: "#4f46e5",

                        backgroundColor:
                            "rgba(79, 70, 229, 0.10)",

                        borderWidth: 3,

                        fill: true,

                        tension: 0.35,

                        pointRadius: 4,

                        pointHoverRadius: 6,

                        pointBackgroundColor:
                            "#4f46e5",

                        pointBorderColor:
                            "#ffffff",

                        pointBorderWidth: 2
                    }
                ]
            },

            options: {

                responsive: true,

                maintainAspectRatio: false,

                interaction: {
                    intersect: false,
                    mode: "index"
                },

                plugins: {

                    legend: {
                        display: false
                    },

                    tooltip: {

                        callbacks: {

                            label: function (context) {

                                return (
                                    " Ventas: " +
                                    formatoDinero(context.raw)
                                );

                            }

                        }

                    }

                },

                scales: {

                    x: {

                        grid: {
                            display: false
                        },

                        ticks: {
                            color: "#6b7280"
                        }

                    },

                    y: {

                        beginAtZero: true,

                        border: {
                            display: false
                        },

                        grid: {
                            color:
                                "rgba(148, 163, 184, 0.15)"
                        },

                        ticks: {

                            color: "#6b7280",

                            callback: function (value) {
                                return "$" + value;
                            }

                        }

                    }

                }

            }
        }
    );

}


/* ========================================
   GRÁFICO PRODUCTOS
======================================== */

function crearGraficoProductos(productos) {

    const canvas =
        document.getElementById(
            "grafico-productos"
        );

    const sinDatos =
        document.getElementById(
            "sin-datos-productos"
        );

    if (productos.length === 0) {

        canvas.classList.add("d-none");

        sinDatos.classList.remove("d-none");

        return;
    }

    canvas.classList.remove("d-none");

    sinDatos.classList.add("d-none");

    /*
        Mostramos máximo 5 productos
        para mantener legible el gráfico.
    */
    const principales =
        productos.slice(0, 5);

    const etiquetas =
        principales.map(
            producto => producto.nombreProducto
        );

    const valores =
        principales.map(
            producto =>
                Number(producto.cantidadVendida)
        );

    if (graficoProductos) {
        graficoProductos.destroy();
    }

    graficoProductos = new Chart(
        canvas,
        {
            type: "bar",

            data: {

                labels: etiquetas,

                datasets: [
                    {
                        label: "Cantidad vendida",

                        data: valores,

                        backgroundColor: [
                            "#4f46e5",
                            "#6366f1",
                            "#818cf8",
                            "#a5b4fc",
                            "#c7d2fe"
                        ],

                        borderRadius: 7,

                        borderSkipped: false,

                        barThickness: 20
                    }
                ]
            },

            options: {

                indexAxis: "y",

                responsive: true,

                maintainAspectRatio: false,

                plugins: {

                    legend: {
                        display: false
                    },

                    tooltip: {

                        callbacks: {

                            label: function (context) {

                                return (
                                    " Cantidad: " +
                                    formatoCantidad(context.raw)
                                );

                            }

                        }

                    }

                },

                scales: {

                    x: {

                        beginAtZero: true,

                        border: {
                            display: false
                        },

                        grid: {
                            color:
                                "rgba(148, 163, 184, 0.15)"
                        },

                        ticks: {
                            color: "#6b7280"
                        }

                    },

                    y: {

                        border: {
                            display: false
                        },

                        grid: {
                            display: false
                        },

                        ticks: {

                            color: "#374151",

                            font: {
                                weight: "500"
                            }

                        }

                    }

                }

            }
        }
    );

}


/* ========================================
   FORMATOS
======================================== */

function formatoDinero(valor) {

    const numero =
        Number(valor || 0);

    return new Intl.NumberFormat(
        "es-EC",
        {
            style: "currency",
            currency: "USD",
            minimumFractionDigits: 2
        }
    ).format(numero);

}


function formatoNumero(valor) {

    return new Intl.NumberFormat(
        "es-EC"
    ).format(Number(valor || 0));

}


function formatoCantidad(valor) {

    return new Intl.NumberFormat(
        "es-EC",
        {
            maximumFractionDigits: 2
        }
    ).format(Number(valor || 0));

}


function formatoFecha(fecha) {

    const fechaLocal =
        crearFechaLocal(fecha);

    return new Intl.DateTimeFormat(
        "es-EC",
        {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }
    ).format(fechaLocal);

}


function formatoFechaCorta(fecha) {

    const fechaLocal =
        crearFechaLocal(fecha);

    return new Intl.DateTimeFormat(
        "es-EC",
        {
            day: "2-digit",
            month: "short"
        }
    ).format(fechaLocal);

}


/*
    Evita desplazamientos de fecha causados
    por interpretaciones de zona horaria.
*/
function crearFechaLocal(fecha) {

    const texto =
        String(fecha).substring(0, 10);

    const partes =
        texto.split("-");

    if (partes.length !== 3) {
        return new Date(fecha);
    }

    return new Date(
        Number(partes[0]),
        Number(partes[1]) - 1,
        Number(partes[2])
    );

}


/* ========================================
   UTILIDADES
======================================== */

function crearPosicion(posicion) {

    if (posicion <= 3) {

        return `
            <span class="ranking ranking-${posicion}">
                ${posicion}
            </span>
        `;

    }

    return `
        <span class="ranking ranking-normal">
            ${posicion}
        </span>
    `;

}


function obtenerInicial(nombre) {

    if (!nombre) {
        return "?";
    }

    return nombre
        .trim()
        .charAt(0)
        .toUpperCase();

}


function escaparHtml(texto) {

    const elemento =
        document.createElement("div");

    elemento.textContent =
        texto ?? "";

    return elemento.innerHTML;

}


function mostrarError(mensaje) {

    const alerta =
        document.getElementById(
            "mensaje-error"
        );

    alerta.textContent =
        "No se pudieron cargar las estadísticas. " +
        mensaje;

    alerta.classList.remove("d-none");

}


function ocultarError() {

    const alerta =
        document.getElementById(
            "mensaje-error"
        );

    alerta.classList.add("d-none");

}