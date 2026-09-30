// Módulo Balance: ingresos, gastos y balance por periodo.
// Consume: GET /balance/resumen, GET /balance/gastos,
//          POST /balance/gastos, DELETE /balance/gastos/{id}

(() => {

    const moneda = new Intl.NumberFormat("es-EC", {
        style: "currency",
        currency: "USD"
    });

    const MESES = [
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    ];

    const NOMBRES_METODO = {
        EFECTIVO: "Efectivo",
        TRANSFERENCIA: "Transferencia",
        TARJETA: "Tarjeta"
    };

    // Estado de la pantalla
    const estado = {
        periodo: "mes",
        referencia: hoy(),
        desde: null,
        hasta: null,
        gastoAAnular: null
    };

    let modalGasto;
    let modalAnular;
    let toast;

    // ---------- Utilidades de fechas ----------

    function hoy() {
        const d = new Date();
        return new Date(d.getFullYear(), d.getMonth(), d.getDate());
    }

    function aTextoISO(fecha) {
        const y = fecha.getFullYear();
        const m = String(fecha.getMonth() + 1).padStart(2, "0");
        const d = String(fecha.getDate()).padStart(2, "0");
        return `${y}-${m}-${d}`;
    }

    function desdeTextoISO(texto) {
        const [y, m, d] = texto.split("-").map(Number);
        return new Date(y, m - 1, d);
    }

    function sumarDias(fecha, dias) {
        const r = new Date(fecha);
        r.setDate(r.getDate() + dias);
        return r;
    }

    function formatoCorto(fecha) {
        return `${fecha.getDate()} ${MESES[fecha.getMonth()].slice(0, 3)}`;
    }

    function formatoLargo(fecha) {
        return `${fecha.getDate()} de ${MESES[fecha.getMonth()]} de ${fecha.getFullYear()}`;
    }

    function formatoFechaHora(texto) {
        const f = new Date(texto);
        const dia = String(f.getDate()).padStart(2, "0");
        const mes = String(f.getMonth() + 1).padStart(2, "0");
        const hora = String(f.getHours()).padStart(2, "0");
        const min = String(f.getMinutes()).padStart(2, "0");
        return `${dia}/${mes}/${f.getFullYear()} ${hora}:${min}`;
    }

    // Calcula desde/hasta según el periodo elegido y la fecha de referencia.
    function calcularRango() {
        const ref = estado.referencia;

        switch (estado.periodo) {
            case "dia":
                return { desde: ref, hasta: ref };

            case "semana": {
                // Semana de lunes a domingo
                const diaSemana = (ref.getDay() + 6) % 7;
                const lunes = sumarDias(ref, -diaSemana);
                return { desde: lunes, hasta: sumarDias(lunes, 6) };
            }

            case "anio":
                return {
                    desde: new Date(ref.getFullYear(), 0, 1),
                    hasta: new Date(ref.getFullYear(), 11, 31)
                };

            case "personalizado":
                return { desde: estado.desde, hasta: estado.hasta };

            case "mes":
            default:
                return {
                    desde: new Date(ref.getFullYear(), ref.getMonth(), 1),
                    hasta: new Date(ref.getFullYear(), ref.getMonth() + 1, 0)
                };
        }
    }

    function etiquetaPeriodo(desde, hasta) {
        switch (estado.periodo) {
            case "dia":
                return aTextoISO(desde) === aTextoISO(hoy())
                    ? `Hoy, ${formatoCorto(desde)}`
                    : formatoLargo(desde);

            case "semana":
                return `${formatoCorto(desde)} – ${formatoCorto(hasta)} ${hasta.getFullYear()}`;

            case "anio":
                return String(desde.getFullYear());

            case "mes": {
                const nombre = MESES[desde.getMonth()];
                return `${nombre.charAt(0).toUpperCase()}${nombre.slice(1)} ${desde.getFullYear()}`;
            }

            default:
                return `${formatoCorto(desde)} – ${formatoCorto(hasta)}`;
        }
    }

    // Mueve el periodo hacia atrás (-1) o adelante (+1).
    function moverPeriodo(direccion) {
        const ref = estado.referencia;

        switch (estado.periodo) {
            case "dia":
                estado.referencia = sumarDias(ref, direccion);
                break;
            case "semana":
                estado.referencia = sumarDias(ref, 7 * direccion);
                break;
            case "anio":
                estado.referencia = new Date(ref.getFullYear() + direccion, 0, 1);
                break;
            case "mes":
                estado.referencia = new Date(ref.getFullYear(), ref.getMonth() + direccion, 1);
                break;
        }

        cargarBalance();
    }

    // ---------- Utilidades de interfaz ----------

    function escaparHtml(texto) {
        return String(texto ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function plural(n, singular, pluralTexto) {
        return `${n} ${n === 1 ? singular : pluralTexto}`;
    }

    function mostrarError(idAlerta, mensaje) {
        const alerta = document.getElementById(idAlerta);
        alerta.textContent = mensaje;
        alerta.classList.remove("d-none");
    }

    function ocultarError(idAlerta) {
        document.getElementById(idAlerta).classList.add("d-none");
    }

    function mostrarToast(mensaje) {
        document.getElementById("toast-mensaje").textContent = mensaje;
        toast.show();
    }

    // ---------- Pintar datos ----------

    function pintarResumen(resumen) {
        document.getElementById("total-ingresos").textContent =
            moneda.format(resumen.totalIngresos);

        document.getElementById("detalle-ingresos").textContent =
            plural(resumen.cantidadVentas, "venta", "ventas");

        document.getElementById("total-gastos").textContent =
            moneda.format(resumen.totalGastos);

        document.getElementById("detalle-gastos").textContent =
            plural(resumen.cantidadGastos, "gasto", "gastos");

        const balance = resumen.balance;
        const valor = document.getElementById("total-balance");
        const tarjeta = document.getElementById("card-balance");

        valor.textContent = moneda.format(balance);
        valor.classList.toggle("text-danger", balance < 0);
        valor.classList.toggle("text-primary", balance >= 0);
        tarjeta.classList.toggle("negativo", balance < 0);

        document.getElementById("detalle-balance").textContent =
            balance < 0
                ? "Los gastos superan a los ingresos"
                : "Ingresos − gastos";

        pintarMetodos(resumen.ingresosPorMetodoPago, resumen.totalIngresos);
    }

    function pintarMetodos(metodos, totalIngresos) {
        const contenedor = document.getElementById("lista-metodos");

        if (!metodos || metodos.length === 0) {
            contenedor.innerHTML = `
                <div class="text-center text-muted py-4">
                    <i class="bi bi-cash-coin fs-2 d-block mb-2"></i>
                    Aún no hay ventas en este periodo.
                </div>
            `;
            return;
        }

        const totalPagos =
            metodos.reduce((suma, m) => suma + m.total, 0) || totalIngresos || 1;

        contenedor.innerHTML = metodos.map(m => {
            const porcentaje = Math.round((m.total / totalPagos) * 100);
            const nombre = NOMBRES_METODO[m.metodoPago] ?? m.metodoPago;

            return `
                <div class="metodo-item">
                    <div class="d-flex justify-content-between mb-1">
                        <span>${escaparHtml(nombre)}</span>
                        <span class="fw-semibold">${moneda.format(m.total)}</span>
                    </div>
                    <div class="progress" role="progressbar"
                         aria-label="${escaparHtml(nombre)}"
                         aria-valuenow="${porcentaje}" aria-valuemin="0" aria-valuemax="100">
                        <div class="progress-bar bg-success" style="width: ${porcentaje}%"></div>
                    </div>
                    <small class="text-muted">${porcentaje}% de lo cobrado</small>
                </div>
            `;
        }).join("");
    }

    function pintarGastos(gastos) {
        const tbody = document.getElementById("tabla-gastos");

        document.getElementById("contador-gastos").textContent = gastos.length;

        if (gastos.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted py-5">
                        <i class="bi bi-receipt fs-2 d-block mb-2"></i>
                        No hay gastos registrados en este periodo.
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = gastos.map(g => `
            <tr>
                <td class="text-nowrap">${formatoFechaHora(g.fecha)}</td>
                <td>${escaparHtml(g.descripcion)}</td>
                <td>
                    ${g.categoria
                        ? `<span class="badge badge-categoria">${escaparHtml(g.categoria)}</span>`
                        : `<span class="text-muted">—</span>`}
                </td>
                <td class="d-none d-md-table-cell">${escaparHtml(g.registradoPor)}</td>
                <td class="text-end text-danger fw-semibold text-nowrap">
                    −${moneda.format(g.monto)}
                </td>
                <td class="text-end">
                    <button
                        type="button"
                        class="btn btn-sm btn-outline-secondary btn-anular"
                        data-id="${g.idGasto}"
                        data-descripcion="${escaparHtml(g.descripcion)}"
                        title="Anular gasto"
                        aria-label="Anular gasto ${escaparHtml(g.descripcion)}"
                    >
                        <i class="bi bi-x-lg"></i>
                    </button>
                </td>
            </tr>
        `).join("");
    }

    // ---------- Cargar datos de la API ----------

    async function cargarBalance() {
        const { desde, hasta } = calcularRango();

        document.getElementById("etiqueta-periodo").textContent =
            etiquetaPeriodo(desde, hasta);

        // No permitir avanzar a periodos futuros
        document.getElementById("btn-siguiente").disabled =
            sumarDias(hasta, 1) > hoy();

        const query = `?desde=${aTextoISO(desde)}&hasta=${aTextoISO(hasta)}`;

        const contenido = document.getElementById("balance-content");
        contenido.classList.add("cargando");
        ocultarError("alerta-balance");

        try {
            const [resumen, gastos] = await Promise.all([
                apiRequest(`/balance/resumen${query}`),
                apiRequest(`/balance/gastos${query}`)
            ]);

            if (!resumen || !gastos) {
                return;
            }

            pintarResumen(resumen);
            pintarGastos(gastos);
        } catch (error) {
            mostrarError(
                "alerta-balance",
                `No se pudo cargar el balance: ${error.message}`
            );
        } finally {
            contenido.classList.remove("cargando");
        }
    }

    // ---------- Registrar gasto ----------

    function prepararFormularioGasto() {
        const form = document.getElementById("form-gasto");
        const fecha = document.getElementById("gasto-fecha");

        form.reset();
        form.classList.remove("was-validated");
        ocultarError("alerta-gasto");

        fecha.value = aTextoISO(hoy());
        fecha.max = aTextoISO(hoy());
    }

    async function guardarGasto(evento) {
        evento.preventDefault();

        const form = evento.target;
        const boton = document.getElementById("btn-guardar-gasto");

        const descripcion = document.getElementById("gasto-descripcion");
        descripcion.setCustomValidity(
            descripcion.value.trim() === "" ? "vacío" : ""
        );

        form.classList.add("was-validated");

        if (!form.checkValidity()) {
            return;
        }

        const gasto = {
            descripcion: descripcion.value.trim(),
            categoria: document.getElementById("gasto-categoria").value || null,
            monto: Number(document.getElementById("gasto-monto").value),
            fecha: document.getElementById("gasto-fecha").value
        };

        boton.disabled = true;
        boton.textContent = "Guardando…";
        ocultarError("alerta-gasto");

        try {
            await apiRequest("/balance/gastos", {
                method: "POST",
                body: JSON.stringify(gasto)
            });

            modalGasto.hide();
            mostrarToast("Gasto registrado correctamente.");

            // Si el gasto cae fuera del periodo visible, saltar a ese día
            const { desde, hasta } = calcularRango();
            const fechaGasto = desdeTextoISO(gasto.fecha);

            if (fechaGasto < desde || fechaGasto > hasta) {
                estado.referencia = fechaGasto;
                if (estado.periodo === "personalizado") {
                    seleccionarPeriodo("mes");
                    return;
                }
            }

            await cargarBalance();
        } catch (error) {
            mostrarError("alerta-gasto", error.message);
        } finally {
            boton.disabled = false;
            boton.textContent = "Guardar gasto";
        }
    }

    // ---------- Anular gasto ----------

    function pedirConfirmacionAnular(boton) {
        estado.gastoAAnular = Number(boton.dataset.id);
        document.getElementById("anular-descripcion").textContent =
            boton.dataset.descripcion;
        modalAnular.show();
    }

    async function anularGasto() {
        const boton = document.getElementById("btn-confirmar-anular");
        boton.disabled = true;

        try {
            await apiRequest(`/balance/gastos/${estado.gastoAAnular}`, {
                method: "DELETE"
            });

            modalAnular.hide();
            mostrarToast("Gasto anulado.");
            await cargarBalance();
        } catch (error) {
            modalAnular.hide();
            mostrarError("alerta-balance", error.message);
        } finally {
            boton.disabled = false;
        }
    }

    // ---------- Selector de periodo ----------

    function seleccionarPeriodo(periodo) {
        estado.periodo = periodo;

        document.querySelectorAll("[data-periodo]").forEach(b => {
            b.classList.toggle("active", b.dataset.periodo === periodo);
        });

        const esPersonalizado = periodo === "personalizado";

        document.getElementById("navegacion-periodo")
            .classList.toggle("d-none", esPersonalizado);

        const formPersonalizado = document.getElementById("form-personalizado");
        formPersonalizado.classList.toggle("d-none", !esPersonalizado);
        formPersonalizado.classList.toggle("d-flex", esPersonalizado);

        if (esPersonalizado) {
            if (!estado.desde) {
                estado.desde = sumarDias(hoy(), -29);
                estado.hasta = hoy();
            }

            document.getElementById("fecha-desde").value = aTextoISO(estado.desde);
            document.getElementById("fecha-hasta").value = aTextoISO(estado.hasta);
        }

        cargarBalance();
    }

    function aplicarRangoPersonalizado(evento) {
        evento.preventDefault();

        const desde = document.getElementById("fecha-desde").value;
        const hasta = document.getElementById("fecha-hasta").value;

        if (!desde || !hasta) {
            return;
        }

        if (desde > hasta) {
            mostrarError(
                "alerta-balance",
                "La fecha 'desde' no puede ser mayor que la fecha 'hasta'."
            );
            return;
        }

        estado.desde = desdeTextoISO(desde);
        estado.hasta = desdeTextoISO(hasta);
        cargarBalance();
    }

    // ---------- Inicio ----------

    document.addEventListener("DOMContentLoaded", () => {

        modalGasto = new bootstrap.Modal("#modalGasto");
        modalAnular = new bootstrap.Modal("#modalAnular");
        toast = new bootstrap.Toast("#toast-balance", { delay: 2500 });

        document.querySelectorAll("[data-periodo]").forEach(boton => {
            boton.addEventListener("click", () => {
                estado.referencia = hoy();
                seleccionarPeriodo(boton.dataset.periodo);
            });
        });

        document.getElementById("btn-anterior")
            .addEventListener("click", () => moverPeriodo(-1));

        document.getElementById("btn-siguiente")
            .addEventListener("click", () => moverPeriodo(1));

        document.getElementById("form-personalizado")
            .addEventListener("submit", aplicarRangoPersonalizado);

        document.getElementById("modalGasto")
            .addEventListener("show.bs.modal", prepararFormularioGasto);

        document.getElementById("modalGasto")
            .addEventListener("shown.bs.modal", () =>
                document.getElementById("gasto-descripcion").focus()
            );

        document.getElementById("form-gasto")
            .addEventListener("submit", guardarGasto);

        document.getElementById("tabla-gastos")
            .addEventListener("click", evento => {
                const boton = evento.target.closest(".btn-anular");
                if (boton) {
                    pedirConfirmacionAnular(boton);
                }
            });

        document.getElementById("btn-confirmar-anular")
            .addEventListener("click", anularGasto);

        cargarBalance();
    });

})();
