document.addEventListener("DOMContentLoaded", () => {
    requireAdmin();

    const user = JSON.parse(localStorage.getItem("user"));
    if (user) {
        document.getElementById("adminNameDisplay").innerText = `Merhaba, ${user.firstName} ${user.lastName}`;
    }

    // Tab navigasyon
    document.querySelectorAll("[data-tab]").forEach(btn => {
        btn.addEventListener("click", function () {
            document.querySelectorAll("[data-tab]").forEach(b => b.classList.remove("active"));
            this.classList.add("active");
            document.querySelectorAll("[id^='tab-']").forEach(t => t.classList.add("d-none"));
            document.getElementById("tab-" + this.dataset.tab).classList.remove("d-none");
        });
    });

    // İstekleri yükle
    loadRequests();

    // Üniversite listesini kulüp ve etkinlik formları için yükle
    loadUniversitySelects();

    // Logo önizleme
    document.getElementById("newUniLogoFile").addEventListener("change", function () {
        const file = this.files[0];
        if (!file) return;
        document.getElementById("logoPreviewImg").src = URL.createObjectURL(file);
        document.getElementById("logoPreview").classList.remove("d-none");
    });

    // Kulüp ekleme formu
    document.getElementById("addClubFormAdmin").addEventListener("submit", async function (e) {
        e.preventDefault();
        const msgEl = document.getElementById("addClubMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        try {
            let imageUrl = null;
            const fileInput = document.getElementById("adminClubImageFile");
            if (fileInput.files.length > 0) {
                imageUrl = await ApiService.uploadImage("clubs", fileInput.files[0]);
                if (!imageUrl) {
                    msgEl.innerHTML = `<div class="alert alert-danger py-1">Resim yüklenemedi.</div>`;
                    return;
                }
            }

            const result = await ApiService.createClub({
                name: document.getElementById("adminClubName").value.trim(),
                category: document.getElementById("adminClubCategory").value,
                description: document.getElementById("adminClubDescription").value.trim(),
                imageUrl: imageUrl,
                universityId: parseInt(document.getElementById("clubUniSelect").value)
            });

            if (result.ok) {
                msgEl.innerHTML = `<div class="alert alert-success py-1">Kulüp başarıyla eklendi!</div>`;
                this.reset();
            } else {
                msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${result.message}</div>`;
            }
        } catch (err) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err.message}</div>`;
        }
    });

    // Etkinlik — üniversite seçince kulüpleri yükle
    document.getElementById("eventUniSelect").addEventListener("change", async function () {
        const clubSelect = document.getElementById("eventClubSelect");
        clubSelect.innerHTML = `<option value="">Yükleniyor...</option>`;
        if (!this.value) return;
        const clubs = await ApiService.getClubsByUniversity(this.value);
        clubSelect.innerHTML = `<option value="">Seçiniz...</option>`;
        clubs.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c.id;
            opt.textContent = c.name;
            clubSelect.appendChild(opt);
        });
    });

    // Etkinlik ekleme formu
    document.getElementById("addEventFormAdmin").addEventListener("submit", async function (e) {
        e.preventDefault();
        const msgEl = document.getElementById("addEventMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        try {
            const response = await fetch("http://localhost:5149/api/events/create", {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify({
                    title: document.getElementById("adminEventTitle").value.trim(),
                    description: document.getElementById("adminEventDescription").value.trim(),
                    date: document.getElementById("adminEventDate").value,
                    location: document.getElementById("adminEventLocation").value.trim(),
                    clubId: parseInt(document.getElementById("eventClubSelect").value)
                })
            });

            if (response.ok) {
                msgEl.innerHTML = `<div class="alert alert-success py-1">Etkinlik başarıyla eklendi!</div>`;
                this.reset();
                document.getElementById("eventClubSelect").innerHTML = `<option value="">Önce üniversite seçin</option>`;
            } else {
                const err = await response.text();
                msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err}</div>`;
            }
        } catch (err) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err.message}</div>`;
        }
    });

    // Üniversite ekleme formu
    document.getElementById("addUniversityForm").addEventListener("submit", async function (e) {
        e.preventDefault();
        const msgEl = document.getElementById("addUniMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        try {
            let logoUrl = null;
            const fileInput = document.getElementById("newUniLogoFile");
            if (fileInput.files.length > 0) {
                logoUrl = await ApiService.uploadImage("universities", fileInput.files[0]);
                if (!logoUrl) {
                    msgEl.innerHTML = `<div class="alert alert-danger py-1">Resim yüklenirken hata oluştu.</div>`;
                    return;
                }
            }

            const success = await ApiService.createUniversity({
                name: document.getElementById("newUniName").value.trim(),
                city: document.getElementById("newUniCity").value.trim(),
                logoUrl: logoUrl
            });

            if (success) {
                msgEl.innerHTML = `<div class="alert alert-success py-1">Üniversite başarıyla kaydedildi!</div>`;
                this.reset();
                document.getElementById("logoPreview").classList.add("d-none");
            } else {
                msgEl.innerHTML = `<div class="alert alert-danger py-1">Üniversite kaydedilemedi.</div>`;
            }
        } catch (err) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err.message}</div>`;
        }
    });
});

// =============================================
// ÜNİVERSİTE SEÇİMLERİNİ DOLDUR
// =============================================
async function loadUniversitySelects() {
    const universities = await ApiService.getUniversities();
    ["clubUniSelect", "eventUniSelect"].forEach(id => {
        const select = document.getElementById(id);
        if (!select) return;
        universities.forEach(u => {
            const opt = document.createElement("option");
            opt.value = u.id;
            opt.textContent = u.name + " (" + u.city + ")";
            select.appendChild(opt);
        });
    });
}

// =============================================
// TÜM İSTEKLERİ YÜKLE
// =============================================
async function loadRequests() {
    const container = document.getElementById("requestsContainer");
    container.innerHTML = `<p class="text-muted">Yükleniyor...</p>`;

    try {
        const response = await fetch("http://localhost:5149/api/admin/requests", {
            headers: getAuthHeaders()
        });
        const universities = await response.json();

        if (universities.length === 0) {
            container.innerHTML = `<p class="text-muted">Henüz veri yok.</p>`;
            return;
        }

        container.innerHTML = "";

        universities.forEach(uni => {
            const uniBlock = document.createElement("div");
            uniBlock.className = "card shadow-sm mb-4 university-block";
            uniBlock.innerHTML = `
                <div class="card-header bg-dark text-white d-flex align-items-center gap-2">
                    <strong>${uni.name}</strong>
                </div>
                <div class="card-body p-3" id="uni-${uni.id}"></div>
            `;
            container.appendChild(uniBlock);

            const uniBody = document.getElementById(`uni-${uni.id}`);

            if (uni.clubs.length === 0) {
                uniBody.innerHTML = `<p class="text-muted small">Bu üniversiteye ait kulüp yok.</p>`;
                return;
            }

            uni.clubs.forEach(club => {
                const clubBlock = document.createElement("div");
                clubBlock.className = "mb-4 p-3 bg-white rounded border club-block";
                clubBlock.innerHTML = `
                    <h6 class="fw-bold mb-3">
                        <span class="badge bg-primary me-2">${club.category}</span>${club.name}
                    </h6>
                    <div id="club-content-${club.id}"></div>
                `;
                uniBody.appendChild(clubBlock);

                const clubContent = document.getElementById(`club-content-${club.id}`);

                // ÜYELİK İSTEKLERİ
                const memberRows = club.membershipRequests.map(m => `
                    <tr>
                        <td>
                            <div class="fw-bold">${m.userFullName}</div>
                            <small class="text-muted">${m.email}</small>
                        </td>
                        <td><span class="badge badge-${m.status.toLowerCase()}">${statusLabel(m.status)}</span></td>
                        <td>${new Date(m.applicationDate).toLocaleDateString("tr-TR")}</td>
                        <td>
                            ${m.status === "Pending" ? `
                                <button onclick="handleMembership(${m.id}, 'Approved')" class="btn btn-sm btn-success me-1">Onayla</button>
                                <button onclick="handleMembership(${m.id}, 'Rejected')" class="btn btn-sm btn-danger">Reddet</button>
                            ` : "—"}
                        </td>
                    </tr>
                `).join("") || `<tr><td colspan="4" class="text-muted text-center">Üyelik isteği yok.</td></tr>`;

                // ETKİNLİK İSTEKLERİ
                let eventHtml = "";
                club.events.forEach(event => {
                    const eventRows = event.eventRequests.map(r => `
                        <tr>
                            <td>
                                <div class="fw-bold">${r.userFullName}</div>
                                <small class="text-muted">${r.email}</small>
                            </td>
                            <td><span class="badge badge-${r.status.toLowerCase()}">${statusLabel(r.status)}</span></td>
                            <td>${new Date(r.requestDate).toLocaleDateString("tr-TR")}</td>
                            <td>
                                ${r.status === "Pending" ? `
                                    <button onclick="handleEventRequest(${r.id}, 'Approved')" class="btn btn-sm btn-success me-1">Onayla</button>
                                    <button onclick="handleEventRequest(${r.id}, 'Rejected')" class="btn btn-sm btn-danger">Reddet</button>
                                ` : "—"}
                            </td>
                        </tr>
                    `).join("") || `<tr><td colspan="4" class="text-muted text-center">Katılım isteği yok.</td></tr>`;

                    eventHtml += `
                        <div class="mt-3">
                            <p class="section-title text-secondary mb-1">
                                Etkinlik: <strong>${event.title}</strong>
                                <span class="text-muted">(${new Date(event.date).toLocaleDateString("tr-TR")})</span>
                            </p>
                            ${buildTable(eventRows)}
                        </div>
                    `;
                });

                clubContent.innerHTML = `
                    <p class="section-title text-secondary mb-1">Kulüp Üyelik İstekleri</p>
                    ${buildTable(memberRows)}
                    ${eventHtml}
                `;
            });
        });

    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">İstekler yüklenemedi: ${err.message}</div>`;
    }
}

function buildTable(rows) {
    return `
        <table class="table table-sm table-bordered mb-2">
            <thead class="table-light">
                <tr><th>Kullanıcı</th><th>Durum</th><th>Tarih</th><th>İşlem</th></tr>
            </thead>
            <tbody>${rows}</tbody>
        </table>
    `;
}

function statusLabel(status) {
    return { Pending: "Bekliyor", Approved: "Onaylandı", Rejected: "Reddedildi" }[status] || status;
}

window.handleMembership = async function (id, status) {
    const response = await fetch(`http://localhost:5149/api/admin/membership-requests/${id}/status`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify({ status })
    });
    const data = await response.json();
    alert(data.message || "İşlem tamamlandı.");
    loadRequests();
};

window.handleEventRequest = async function (id, status) {
    const result = await ApiService.updateRequestStatus(id, status);
    alert(result.message || "İşlem tamamlandı.");
    loadRequests();
};
