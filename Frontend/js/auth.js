// js/auth.js

// Kullanıcının giriş yapıp yapmadığını kontrol eder
function isLoggedIn() {
    return localStorage.getItem("user") !== null;
}

// Kullanıcının "SystemAdmin" rolüne sahip olup olmadığını kontrol eder
function isAdmin() {
    const user = JSON.parse(localStorage.getItem("user"));
    return user && user.role === "SystemAdmin";
}

// Sadece SystemAdmin rolü varsa ve token varsa geçerli sayılır.
function requireAdmin() {
    if (!isAdmin()) {
        alert("Bu sayfayı/özelliği görüntüleme yetkiniz yok!");
        window.location.href = "index.html";
    }
}

// API istekleri için Authorization başlığını (Header) döndürür
// includeContentType=false: FormData yüklemelerinde kullan (tarayıcı otomatik ayarlar)
function getAuthHeaders(includeContentType = true) {
    const user = JSON.parse(localStorage.getItem("user"));
    const headers = {};

    if (includeContentType) {
        headers["Content-Type"] = "application/json";
    }

    if (user && user.token) {
        headers["Authorization"] = `Bearer ${user.token}`;
    }

    return headers;
}

// Kullanıcı çıkışı
function logout() {
    localStorage.removeItem("user");
    window.location.href = "login.html";
}
