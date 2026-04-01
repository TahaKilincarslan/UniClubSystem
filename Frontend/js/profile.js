document.addEventListener("DOMContentLoaded", async function () {

    if (!isLoggedIn()) {
        window.location.href = "login.html";
        return;
    }

    // Üniversiteleri dropdown'a doldur
    async function loadUniversities(selectedId) {
        const select = document.getElementById("universityId");
        try {
            const response = await fetch("http://localhost:5149/api/universities");
            const universities = await response.json();
            universities.forEach(u => {
                const option = document.createElement("option");
                option.value = u.id;
                option.textContent = u.name + " (" + u.city + ")";
                if (u.id === selectedId) option.selected = true;
                select.appendChild(option);
            });
        } catch (e) { console.error(e); }
    }

    // Kullanıcı bilgilerini API'den çek
    async function loadProfile() {
        const response = await fetch("http://localhost:5149/api/users/me", {
            headers: getAuthHeaders()
        });
        if (!response.ok) { logout(); return; }
        const user = await response.json();

        document.getElementById("firstName").value = user.firstName;
        document.getElementById("lastName").value = user.lastName;
        document.getElementById("email").value = user.email;
        document.getElementById("department").value = user.department || "";
        document.getElementById("year").value = user.year != null ? user.year : "";
        document.getElementById("profileFullName").textContent = user.firstName + " " + user.lastName;
        document.getElementById("avatarInitials").textContent = user.firstName[0] + user.lastName[0];
        document.getElementById("profileRole").textContent =
            user.role === "SystemAdmin" ? "Admin" :
            user.role === "ClubManager" ? "Kulüp Yöneticisi" : "Öğrenci";

        // Üniversite dropdown'ını doldur
        document.getElementById("universityId").innerHTML = '<option value="0">Seçiniz...</option>';
        await loadUniversities(user.universityId);
    }

    await loadProfile();

    // Kişisel bilgi güncelleme
    document.getElementById("profileForm").addEventListener("submit", async function (e) {
        e.preventDefault();
        const msgEl = document.getElementById("profileMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        const response = await fetch("http://localhost:5149/api/users/me", {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify({
                firstName: document.getElementById("firstName").value.trim(),
                lastName: document.getElementById("lastName").value.trim(),
                email: document.getElementById("email").value.trim(),
                universityId: parseInt(document.getElementById("universityId").value) || 0,
                department: document.getElementById("department").value.trim(),
                year: document.getElementById("year").value !== "" ? parseInt(document.getElementById("year").value) : null
            })
        });

        const data = await response.json();
        if (response.ok) {
            // localStorage'ı da güncelle
            const stored = JSON.parse(localStorage.getItem("user"));
            stored.firstName = data.firstName;
            stored.lastName = data.lastName;
            stored.email = data.email;
            localStorage.setItem("user", JSON.stringify(stored));

            msgEl.innerHTML = `<div class="alert alert-success py-1">Bilgileriniz güncellendi!</div>`;
            await loadProfile();
        } else {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">${data.message || "Hata oluştu."}</div>`;
        }
    });

    // Şifre değiştirme
    document.getElementById("passwordForm").addEventListener("submit", async function (e) {
        e.preventDefault();
        const msgEl = document.getElementById("passwordMessage");

        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        if (newPassword !== confirmPassword) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Yeni şifreler eşleşmiyor.</div>`;
            return;
        }

        msgEl.innerHTML = `<div class="alert alert-info py-1">Güncelleniyor...</div>`;

        const response = await fetch("http://localhost:5149/api/users/me", {
            method: "PUT",
            headers: getAuthHeaders(),
            body: JSON.stringify({
                currentPassword: document.getElementById("currentPassword").value,
                newPassword: newPassword
            })
        });

        const data = await response.json();
        if (response.ok) {
            msgEl.innerHTML = `<div class="alert alert-success py-1">Şifreniz güncellendi!</div>`;
            this.reset();
        } else {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">${data.message || "Hata oluştu."}</div>`;
        }
    });
});
