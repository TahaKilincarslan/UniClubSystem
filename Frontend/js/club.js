document.addEventListener("DOMContentLoaded", async function () {
    const params = new URLSearchParams(window.location.search);
    const clubId = params.get("id");

    if (!clubId) {
        alert("Club not selected.");
        window.location.href = "index.html";
        return;
    }

    /* =========================
       NAVBAR USER CONTROL
    ========================== */
    function updateNavbar() {
        const navButtons = document.getElementById("navButtons");
        if (!navButtons) return;

        if (typeof isLoggedIn === 'function' && isLoggedIn()) {
            const user = JSON.parse(localStorage.getItem("user"));
            let adminBtn = "";
            // Check if user is SystemAdmin or if they are the manager of this club (logic for this can be added)
            if (user.role === "SystemAdmin" || user.role === "ClubManager") {
                adminBtn = `<a href="admin.html" class="btn btn-warning me-2">Admin Panel</a>`;
            }
            navButtons.innerHTML = `
                ${adminBtn}
                <button class="btn btn-dark" onclick="logout()">Logout</button>
            `;
        } else {
            navButtons.innerHTML = `
                <a href="login.html" class="btn btn-outline-dark me-2">Login</a>
                <a href="register.html" class="btn btn-dark">Register</a>
            `;
        }
    }

    updateNavbar();

    /* =========================
       LOAD CLUB DATA
    ========================== */
    const clubNameElement = document.getElementById("clubName");
    const clubCategoryElement = document.getElementById("clubCategory");
    const clubDescriptionElement = document.getElementById("clubDescription");
    const clubManagerElement = document.getElementById("clubManager");
    const clubHeader = document.getElementById("clubHeader");

    try {
        const response = await fetch(`http://localhost:5149/api/clubs/detail/${clubId}`);
        if (!response.ok) throw new Error("Club not found");
        const club = await response.json();

        clubNameElement.textContent = club.name;
        clubCategoryElement.textContent = club.category;
        clubDescriptionElement.textContent = club.description || "No description available.";
        clubManagerElement.textContent = club.managerName || "Not assigned";
        
        if (club.imageUrl) {
            const imgUrl = club.imageUrl.startsWith('http') ? club.imageUrl
                : club.imageUrl.startsWith('/uploads/') ? MEDIA_BASE_URL + club.imageUrl
                : club.imageUrl;
            clubHeader.style.backgroundImage = `linear-gradient(rgba(0,0,0,0.6), rgba(0,0,0,0.6)), url('${imgUrl}')`;
        }

        // Admin ise etkinlik ekleme formunu göster
        if (typeof isAdmin === 'function' && isAdmin()) {
            document.getElementById("addEventSection").classList.remove("d-none");
        }

        // Kulübe katılma butonu
        await renderMembershipButton(clubId);

        loadEvents(clubId);

    } catch (error) {
        console.error("Error loading club details:", error);
        clubNameElement.textContent = "Error loading club";
    }

    /* =========================
       KULÜP ÜYELİK BUTONU
    ========================== */
    async function renderMembershipButton(clubId) {
        const container = document.getElementById("membershipContainer");
        if (!container) return;

        if (!isLoggedIn()) {
            container.innerHTML = `<a href="login.html" class="btn btn-outline-dark w-100">Katılmak için giriş yap</a>`;
            return;
        }

        const user = JSON.parse(localStorage.getItem("user"));
        if (user.role === "SystemAdmin") return; // Admin için gösterme

        const memberships = await ApiService.getMyMemberships();
        const existing = memberships.find(m => m.clubId === parseInt(clubId));

        if (existing) {
            if (existing.status === "Pending") {
                container.innerHTML = `<button class="btn btn-warning w-100" disabled>⏳ Başvuru Bekleniyor</button>`;
            } else if (existing.status === "Approved") {
                container.innerHTML = `<button class="btn btn-success w-100" disabled>✓ Üyesiniz</button>`;
            } else if (existing.status === "Rejected") {
                container.innerHTML = `
                    <div class="text-danger small mb-2">✗ Başvurunuz reddedildi.</div>
                    <button id="applyBtn" class="btn btn-outline-dark w-100">Tekrar Başvur</button>
                `;
                document.getElementById("applyBtn").addEventListener("click", async () => {
                    const result = await ApiService.applyToClub(parseInt(clubId));
                    alert(result.message);
                    if (result.ok) await renderMembershipButton(clubId);
                });
            }
        } else {
            container.innerHTML = `<button id="applyBtn" class="btn btn-dark w-100">Kulübe Katılmak İstiyorum</button>`;
            document.getElementById("applyBtn").addEventListener("click", async () => {
                const result = await ApiService.applyToClub(parseInt(clubId));
                alert(result.message);
                if (result.ok) await renderMembershipButton(clubId);
            });
        }
    }

    /* =========================
       ETKİNLİK EKLEME FORMU
    ========================== */
    document.getElementById("addEventForm").addEventListener("submit", async function(e) {
        e.preventDefault();
        const msgEl = document.getElementById("addEventMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        try {
            const eventData = {
                title: document.getElementById("newEventTitle").value.trim(),
                description: document.getElementById("newEventDescription").value.trim(),
                date: document.getElementById("newEventDate").value,
                location: document.getElementById("newEventLocation").value.trim(),
                clubId: parseInt(clubId)
            };

            const response = await fetch(`http://localhost:5149/api/events/create`, {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(eventData)
            });

            if (response.ok) {
                msgEl.innerHTML = `<div class="alert alert-success py-1">"${eventData.title}" etkinliği eklendi!</div>`;
                this.reset();
                loadEvents(clubId);
            } else {
                const err = await response.text();
                msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${response.status} - ${err}</div>`;
            }
        } catch (err) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err.message}</div>`;
        }
    });

    /* =========================
       LOAD EVENTS
    ========================== */
    async function loadEvents(id) {
        const eventList = document.getElementById("eventList");
        const noEvents = document.getElementById("noEvents");
        eventList.innerHTML = "";

        const events = await ApiService.getClubEvents(id);
        const myRequests = isLoggedIn() ? await ApiService.getMyEventRequests() : [];
        const requestMap = {};
        myRequests.forEach(r => { requestMap[r.eventId] = r.status; });

        if (events.length === 0) {
            noEvents.classList.remove("d-none");
            return;
        }

        noEvents.classList.add("d-none");

        const now = new Date();
        const upcoming = events.filter(e => new Date(e.date) >= now);
        const past = events.filter(e => new Date(e.date) < now);

        function createEventCard(event, isPast) {
            const eventDate = new Date(event.date).toLocaleDateString("tr-TR", {
                day: "2-digit", month: "long", year: "numeric", hour: "2-digit", minute: "2-digit"
            });

            let actionBtn;
            if (isPast) {
                actionBtn = `<button class="btn btn-secondary w-100" disabled>Etkinlik Sona Erdi</button>`;
            } else {
                const status = requestMap[event.id];
                if (status === "Pending") {
                    actionBtn = `<button class="btn btn-warning w-100" disabled>⏳ İstek Gönderildi</button>`;
                } else if (status === "Approved") {
                    actionBtn = `<button class="btn btn-success w-100" disabled>✓ Onaylandı</button>`;
                } else if (status === "Rejected") {
                    actionBtn = `<button class="btn btn-danger w-100" disabled>✗ Reddedildi</button>`;
                } else {
                    actionBtn = `<button class="btn btn-primary w-100 join-event-btn" data-event-id="${event.id}">Katılmak İstiyorum</button>`;
                }
            }

            const col = document.createElement("div");
            col.className = "col-md-6";
            col.innerHTML = `
                <div class="card event-card h-100 p-3 ${isPast ? 'opacity-75' : ''}">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <h5 class="fw-bold">${event.title}</h5>
                            <span class="badge ${isPast ? 'bg-secondary' : 'bg-primary'}">${eventDate}</span>
                        </div>
                        <p class="text-muted small">${event.description}</p>
                        <div class="mb-3">
                            <strong>Konum:</strong> ${event.location}
                        </div>
                        ${actionBtn}
                    </div>
                </div>
            `;
            return col;
        }

        if (upcoming.length > 0) {
            const heading = document.createElement("div");
            heading.className = "col-12";
            heading.innerHTML = `<h5 class="fw-bold text-primary mb-2">Yaklaşan Etkinlikler</h5>`;
            eventList.appendChild(heading);
            upcoming.forEach(e => eventList.appendChild(createEventCard(e, false)));
        }

        if (past.length > 0) {
            const heading = document.createElement("div");
            heading.className = "col-12 mt-4";
            heading.innerHTML = `<h5 class="fw-bold text-secondary mb-2">Geçmiş Etkinlikler</h5>`;
            eventList.appendChild(heading);
            past.forEach(e => eventList.appendChild(createEventCard(e, true)));
        }

        if (upcoming.length === 0 && past.length === 0) {
            noEvents.classList.remove("d-none");
        }

        document.querySelectorAll(".join-event-btn").forEach(btn => {
            btn.addEventListener("click", handleJoinRequest);
        });
    }

    /* =========================
       HANDLE JOIN REQUEST
    ========================== */
    async function handleJoinRequest(e) {
        if (!isLoggedIn()) {
            alert("You must be logged in to join an event!");
            window.location.href = "login.html";
            return;
        }

        const btn = e.target;
        const eventId = btn.getAttribute("data-event-id");
        
        btn.disabled = true;
        btn.textContent = "Sending...";

        const result = await ApiService.joinEvent(eventId);
        
        if (result.ok) {
            alert(result.message || "İstek başarıyla gönderildi!");
            btn.textContent = "İstek Gönderildi";
            btn.classList.replace("btn-primary", "btn-success");
        } else {
            alert(result.message || "İstek gönderilemedi.");
            btn.disabled = false;
            btn.textContent = "Katılmak İstiyorum";
        }
    }
});
