document.addEventListener("DOMContentLoaded", async function () {

    if (!isLoggedIn()) {
        window.location.href = "login.html";
        return;
    }

    // Sekme navigasyonu
    document.querySelectorAll("[data-tab]").forEach(btn => {
        btn.addEventListener("click", function () {
            document.querySelectorAll("[data-tab]").forEach(b => b.classList.remove("active"));
            this.classList.add("active");
            document.querySelectorAll("[id^='tab-']").forEach(t => t.classList.add("d-none"));
            document.getElementById("tab-" + this.dataset.tab).classList.remove("d-none");
        });
    });

    loadClubs();
    loadEvents();
});

async function loadClubs() {
    const container = document.getElementById("clubsContainer");

    const memberships = await ApiService.getMyMemberships();

    if (memberships.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <p class="text-muted">Henüz hiçbir kulübe başvurmadınız.</p>
                <a href="index.html" class="btn btn-dark">Kulüpleri Keşfet</a>
            </div>`;
        return;
    }

    container.innerHTML = `<div class="row g-4" id="clubCards"></div>`;
    const row = document.getElementById("clubCards");

    memberships.forEach(m => {
        const statusInfo = {
            "Pending":  { cls: "warning",  label: "⏳ Başvuru Bekleniyor" },
            "Approved": { cls: "success",  label: "✓ Üyesiniz" },
            "Rejected": { cls: "danger",   label: "✗ Reddedildi" }
        }[m.status] || { cls: "secondary", label: m.status };

        const imgSrc = m.clubImageUrl
            ? (m.clubImageUrl.startsWith("/uploads/") ? MEDIA_BASE_URL + m.clubImageUrl : m.clubImageUrl)
            : "images/clubs/default.jpg";

        const col = document.createElement("div");
        col.className = "col-md-4";
        col.innerHTML = `
            <div class="card shadow-sm h-100">
                <img src="${imgSrc}" class="card-img-top"
                     style="height:160px; object-fit:cover;"
                     onerror="this.src='images/clubs/default.jpg'">
                <div class="card-body d-flex flex-column">
                    <h5 class="fw-bold">${m.clubName}</h5>
                    <p class="text-muted small mb-1">${m.clubCategory}</p>
                    <p class="text-muted small mb-3">${m.universityName}</p>
                    <span class="badge bg-${statusInfo.cls} mb-3">${statusInfo.label}</span>
                    <small class="text-muted mt-auto">Başvuru: ${new Date(m.applicationDate).toLocaleDateString("tr-TR")}</small>
                    <a href="club.html?id=${m.clubId}" class="btn btn-outline-dark btn-sm mt-2">Kulübe Git</a>
                </div>
            </div>
        `;
        row.appendChild(col);
    });
}

async function loadEvents() {
    const container = document.getElementById("eventsContainer");

    const requests = await ApiService.getMyEventRequests();

    if (requests.length === 0) {
        container.innerHTML = `
            <div class="text-center py-5">
                <p class="text-muted">Henüz hiçbir etkinliğe başvurmadınız.</p>
                <a href="index.html" class="btn btn-dark">Etkinlikleri Keşfet</a>
            </div>`;
        return;
    }

    const now = new Date();
    const upcoming = requests.filter(r => new Date(r.eventDate) >= now);
    const past = requests.filter(r => new Date(r.eventDate) < now);

    container.innerHTML = "";

    function renderSection(title, items) {
        if (items.length === 0) return;

        const section = document.createElement("div");
        section.className = "mb-5";
        section.innerHTML = `<h5 class="fw-bold mb-3 text-${title === "Yaklaşan Etkinlikler" ? "primary" : "secondary"}">${title}</h5>
            <div class="row g-4" id="events-${title}"></div>`;
        container.appendChild(section);

        const row = section.querySelector(".row");
        items.forEach(r => {
            const statusInfo = {
                "Pending":  { cls: "warning", label: "⏳ Bekliyor" },
                "Approved": { cls: "success", label: "✓ Onaylandı" },
                "Rejected": { cls: "danger",  label: "✗ Reddedildi" }
            }[r.status] || { cls: "secondary", label: r.status };

            const col = document.createElement("div");
            col.className = "col-md-4";
            col.innerHTML = `
                <div class="card shadow-sm h-100 p-3">
                    <div class="card-body d-flex flex-column">
                        <h5 class="fw-bold">${r.eventTitle}</h5>
                        <p class="text-muted small mb-1">${r.clubName} — ${r.universityName}</p>
                        <p class="text-muted small mb-1">📅 ${new Date(r.eventDate).toLocaleDateString("tr-TR", { day:"2-digit", month:"long", year:"numeric", hour:"2-digit", minute:"2-digit" })}</p>
                        <p class="text-muted small mb-3">📍 ${r.eventLocation}</p>
                        <span class="badge bg-${statusInfo.cls} mb-3">${statusInfo.label}</span>
                        <small class="text-muted mt-auto">Başvuru: ${new Date(r.requestDate).toLocaleDateString("tr-TR")}</small>
                    </div>
                </div>
            `;
            row.appendChild(col);
        });
    }

    renderSection("Yaklaşan Etkinlikler", upcoming);
    renderSection("Geçmiş Etkinlikler", past);
}
