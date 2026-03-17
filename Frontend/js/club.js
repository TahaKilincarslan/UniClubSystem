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
        // Fetch specific club details (Wait, we need a getClubById in ApiService)
        // For now, let's look for the club in the list or assume we have the detail endpoint.
        // Actually, let's use the getClubsByUniversity and find it, or I should have added getClubDetail.
        // I added getEventDetail but not specifically getClubDetail. 
        // Let's check ClubsController. It has GetClubDetail(int id).
        
        // I'll add getClubDetail to ApiService if it's missing (I only added event ones).
        // Wait, I saw GetClubDetail in ClubsController earlier.
        
        // Let's use fetch directly or update ApiService. 
        // I'll update ApiService to include getClubDetail.
        
        const response = await fetch(`http://localhost:5149/api/clubs/detail/${clubId}`);
        if (!response.ok) throw new Error("Club not found");
        const club = await response.json();

        clubNameElement.textContent = club.name;
        clubCategoryElement.textContent = club.category;
        clubDescriptionElement.textContent = club.description || "No description available.";
        clubManagerElement.textContent = club.managerName || "Not assigned";
        
        if (club.imageUrl) {
            clubHeader.style.backgroundImage = `linear-gradient(rgba(0,0,0,0.6), rgba(0,0,0,0.6)), url('${club.imageUrl}')`;
        }

        loadEvents(clubId);

    } catch (error) {
        console.error("Error loading club details:", error);
        clubNameElement.textContent = "Error loading club";
    }

    /* =========================
       LOAD EVENTS
    ========================== */
    async function loadEvents(id) {
        const eventList = document.getElementById("eventList");
        const noEvents = document.getElementById("noEvents");
        eventList.innerHTML = "";

        const events = await ApiService.getClubEvents(id);

        if (events.length === 0) {
            noEvents.classList.remove("d-none");
            return;
        }

        noEvents.classList.add("d-none");

        events.forEach(event => {
            const eventDate = new Date(event.date).toLocaleDateString();
            const col = document.createElement("div");
            col.className = "col-md-6";
            col.innerHTML = `
                <div class="card event-card h-100 p-3">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <h5 class="fw-bold">${event.title}</h5>
                            <span class="badge bg-secondary">${eventDate}</span>
                        </div>
                        <p class="text-muted small">${event.description}</p>
                        <div class="mb-3">
                            <i class="bi bi-geo-alt"></i> <strong>Location:</strong> ${event.location}
                        </div>
                        <button class="btn btn-primary w-100 join-event-btn" data-event-id="${event.id}">
                            Join Event
                        </button>
                    </div>
                </div>
            `;
            eventList.appendChild(col);
        });

        // Add event listeners for Join buttons
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
            alert(result.message || "Join request sent successfully!");
            btn.textContent = "Request Sent";
            btn.classList.replace("btn-primary", "btn-success");
        } else {
            alert(result.message || "Failed to send join request.");
            btn.disabled = false;
            btn.textContent = "Join Event";
        }
    }
});
