// Pantalla de inicio.
// El estado de la API solo se muestra si la página tiene el elemento #api-status
// y está cargada la función getApiStatus(); si no, no se hace nada.
document.addEventListener("DOMContentLoaded", async () => {
    const status = document.getElementById("api-status");

    if (!status || typeof getApiStatus !== "function") {
        return;
    }

    try {
        const data = await getApiStatus();

        status.textContent = `${data.estado} - Base de datos ${data.baseDatos}`;
        status.className = "badge bg-success";
    } catch {
        status.textContent = "API sin conexión";
        status.className = "badge bg-danger";
    }
});