window.auth = {
    login: async function (email, password) {
        const formData = new URLSearchParams();
        formData.append("email", email);
        formData.append("password", password);

        const response = await fetch("/api/auth/login", {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: formData.toString()
        });

        return response.ok;
    },

    logout: async function () {
        const response = await fetch("/api/auth/logout", {
            method: "POST",
            credentials: "include"
        });

        return response.ok;
    }
};
