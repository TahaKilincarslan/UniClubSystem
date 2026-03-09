document
    .addEventListener("submit", async function (e) {
        e.preventDefault()
        console.log("Registration started...");

        try {
            const user = {
                firstName: document.getElementById("firstName").value,
                lastName: document.getElementById("lastName").value,
                email: document.getElementById("email").value,
                password: document.getElementById("password").value
            }
            console.log("Sending user data:", user);

            const response = await fetch("http://localhost:5149/api/auth/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(user)
            })

            console.log("Response received:", response.status, response.statusText);

            if (response.ok) {
                const userData = await response.json()
                console.log("Registration successful, user data:", userData);

                localStorage.setItem("user", JSON.stringify(userData))
                alert("Registration successful! You are being logged in.")
                window.location.href = "index.html"
            } else {
                let errorMessage = "Register failed";
                const responseText = await response.text();
                console.error("Registration failed. Status:", response.status, "Body:", responseText);

                try {
                    const errorData = JSON.parse(responseText);
                    errorMessage = errorData.message || errorData.title || JSON.stringify(errorData);
                } catch (e) {
                    errorMessage = responseText || "Register failed";
                }
                alert("Error: " + errorMessage)
            }
        } catch (error) {
            console.error("CRITICAL ERROR during registration:", error);
            alert("Critical Error: " + error.message);
        }
    })