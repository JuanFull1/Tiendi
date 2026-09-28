document.addEventListener(
    "DOMContentLoaded",
    () => {

        const form =
            document.getElementById("login-form");

        const message =
            document.getElementById("login-message");

        const button =
            document.getElementById("login-button");

        form.addEventListener(
            "submit",
            async event => {

                event.preventDefault();

                message.textContent = "";

                const email =
                    document
                        .getElementById("email")
                        .value
                        .trim();

                const password =
                    document
                        .getElementById("password")
                        .value;

                button.disabled = true;

                button.textContent =
                    "Ingresando...";

                try {

                    const response =
                        await apiRequest(
                            "/auth/login",
                            {
                                method: "POST",

                                body: JSON.stringify({
                                    email,
                                    password
                                })
                            }
                        );

                    localStorage.setItem(
                        "tiendi_token",
                        response.token
                    );

                    localStorage.setItem(
                        "tiendi_user",
                        JSON.stringify(
                            response.usuario
                        )
                    );

                    window.location.href =
                        "index.html";

                } catch (error) {

                    message.textContent =
                        error.message;

                } finally {

                    button.disabled = false;

                    button.textContent =
                        "Ingresar";
                }
            }
        );
    }
);