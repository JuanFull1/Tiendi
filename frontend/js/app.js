document.addEventListener("DOMContentLoaded", async () => {
    const status = document.getElementById("api-status");

    try {
        const data = await getApiStatus();

        status.textContent = `${data.estado} - Base de datos ${data.baseDatos}`;
        status.className = "badge bg-success";
    } catch {
        status.textContent = "API sin conexión";
        status.className = "badge bg-danger";
    }
});