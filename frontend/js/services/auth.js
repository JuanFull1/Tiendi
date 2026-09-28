const AUTH_TOKEN_KEY = "tiendi_token";
const AUTH_USER_KEY = "tiendi_user";

function getStoredUser() {

    const value =
        localStorage.getItem(AUTH_USER_KEY);

    if (!value) {
        return null;
    }

    try {
        return JSON.parse(value);
    } catch {
        return null;
    }
}

function getAuthToken() {
    return localStorage.getItem(AUTH_TOKEN_KEY);
}

function isAuthenticated() {
    return Boolean(getAuthToken());
}

function logout() {

    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_USER_KEY);

    const path =
        window.location.pathname.replaceAll("\\", "/");

    const inPages =
        path.includes("/pages/");

    window.location.href =
        inPages
            ? "../login.html"
            : "./login.html";
}

function requireAuth() {

    if (isAuthenticated()) {
        return;
    }

    const path =
        window.location.pathname.replaceAll("\\", "/");

    if (path.endsWith("/login.html")) {
        return;
    }

    const inPages =
        path.includes("/pages/");

    window.location.href =
        inPages
            ? "../login.html"
            : "./login.html";
}

requireAuth();