// js/admin.js

document.addEventListener("DOMContentLoaded", () => {
    // 1. Sayfa yüklendiğinde Admin kontrolü yap
    requireAdmin();

    // 2. Kullanıcı adını ekrana yazdır
    const user = JSON.parse(localStorage.getItem("user"));
    if (user) {
        document.getElementById("adminNameDisplay").innerText = `Merhaba, ${user.firstName} ${user.lastName}`;
    }

    // 3. Test verisini çek
    fetchDashboardData();

    // 4. İstekleri yükle
    loadAllEventRequests();
});

// Mocking logic to get requests for all events managed by the user
// Ideally, the backend would have a "GET /api/admin/requests" for the logged-in admin.
// For now, let's assume we list all events first, then fetch requests for each.
// Or better: update the backend if needed. But let's work with what we have.
// Since we don't have "get all clubs/events for manager", let's use a simpler approach for the demo.
async function loadAllEventRequests() {
    const tableBody = document.getElementById("eventRequestsTable");
    if (!tableBody) return;

    // Fetch all events first (Public endpoint)
    const events = await ApiService.getUniversities(); // Wait, this gets universities. 
    // Let's use getClubsByUniversity or similar. 
    // Actually, I'll just look for events.
    
    // Simplification for testing: We'll fetch requests for a few IDs or just use the API we built.
    // In a real scenario, the admin would select an event. 
    // Let's fetch all events and then their requests.
    
    // Fetch all events (I'll add this to ApiService if not there)
    try {
        const response = await fetch("http://localhost:5149/api/events");
        const events = await response.json();
        
        tableBody.innerHTML = "";
        
        if (events.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="3" class="text-center py-4">Etkinlik bulunamadı.</td></tr>';
            return;
        }

        for (const event of events) {
            const requests = await ApiService.getEventRequests(event.id);
            requests.forEach(req => {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                    <td>
                        <div class="fw-bold">${req.userFullName}</div>
                        <small class="text-muted">${req.userEmail}</small>
                    </td>
                    <td>${req.eventTitle}</td>
                    <td>
                        <div class="d-flex gap-1">
                            ${req.status === "Pending" ? `
                                <button onclick="handleRequestStatus(${req.id}, 'Approved')" class="btn btn-sm btn-success">Onayla</button>
                                <button onclick="handleRequestStatus(${req.id}, 'Rejected')" class="btn btn-sm btn-danger">Reddet</button>
                            ` : `
                                <span class="badge ${req.status === 'Approved' ? 'bg-success' : 'bg-danger'}">${req.status}</span>
                            `}
                        </div>
                    </td>
                `;
                tableBody.appendChild(tr);
            });
        }

        if (tableBody.innerHTML === "") {
            tableBody.innerHTML = '<tr><td colspan="3" class="text-center py-4">Bekleyen istek yok.</td></tr>';
        }

    } catch (err) {
        tableBody.innerHTML = '<tr><td colspan="3" class="text-center py-4 text-danger">İstekler yüklenirken hata oluştu.</td></tr>';
    }
}

window.handleRequestStatus = async function(requestId, status) {
    const result = await ApiService.updateRequestStatus(requestId, status);
    if (result.ok) {
        alert(result.message || "İşlem başarılı.");
        loadAllEventRequests();
    } else {
        alert(result.message || "Hata oluştu.");
    }
}

async function fetchDashboardData() {
    const messageEl = document.getElementById("dashboardMessage");
    messageEl.innerText = "Veri yükleniyor...";
    messageEl.className = "card-text text-warning";

    try {
        const response = await fetch("http://localhost:5149/api/admin/dashboard", {
            method: "GET",
            headers: getAuthHeaders() // Token ekleniyor
        });

        if (response.ok) {
            const data = await response.json();
            messageEl.innerText = data.message;
            messageEl.className = "card-text text-success fw-bold";
        } else if (response.status === 401 || response.status === 403) {
            messageEl.innerText = "Hata: Yetkiniz reddedildi! Lütfen tekrar giriş yapın.";
            messageEl.className = "card-text text-danger";
            logout(); // Yetki sorunu varsa dışarı at
        } else {
            messageEl.innerText = "Sunucu hatası oluştu.";
            messageEl.className = "card-text text-danger";
        }
    } catch (error) {
        messageEl.innerText = "Bağlantı hatası: Sunucuya ulaşılamıyor.";
        messageEl.className = "card-text text-danger";
    }
}
