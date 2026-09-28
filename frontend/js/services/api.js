const API_URL = "http://localhost:5145/api";

async function apiRequest(endpoint, options = {}) {

    const token = localStorage.getItem("tiendi_token");

    const headers = {
        "Content-Type": "application/json",
        ...(token
            ? {
                Authorization: `Bearer ${token}`
            }
            : {}),
        ...(options.headers || {})
    };

    const response = await fetch(
        `${API_URL}${endpoint}`,
        {
            ...options,
            headers
        }
    );

    if (
        response.status === 401 &&
        endpoint !== "/auth/login"
    ) {
        localStorage.removeItem("tiendi_token");
        localStorage.removeItem("tiendi_user");

        window.location.href = "/login.html";

        return null;
    }

    if (!response.ok) {

        let mensaje = `Error ${response.status}`;

        try {
            const error = await response.json();

            if (error.mensaje) {
                mensaje = error.mensaje;
            }
        } catch {
        }

        throw new Error(mensaje);
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json();
}