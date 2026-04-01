document.addEventListener("DOMContentLoaded", async function () {

    const params = new URLSearchParams(window.location.search);
    const universityId = params.get("id");
    const universityName = params.get("name");
    const universityImageUrl = params.get("image");

    const universityTitle = document.getElementById("universityTitle");
    const universityDescription = document.getElementById("universityDescription");
    const studentCount = document.getElementById("studentCount");
    const clubCountElement = document.getElementById("clubCount");
    const universityImage = document.getElementById("universityImage");
    const clubList = document.getElementById("clubList");
    const clubFoundCount = document.getElementById("clubFoundCount");
    const clubSearch = document.getElementById("clubSearch");
    const categoryFilter = document.getElementById("categoryFilter");

    if (!universityId) {
        if (universityTitle) universityTitle.textContent = "University not selected";
        return;
    }

    // Set initial info from query params
    if (universityTitle) universityTitle.textContent = universityName + " University";
    if (universityDescription) universityDescription.textContent = "Welcome to " + universityName + ". Explore our student clubs and organizations.";
    if (universityImage) {
        const resolvedUrl = universityImageUrl
            ? (universityImageUrl.startsWith('http') ? universityImageUrl
                : universityImageUrl.startsWith('/uploads/') ? MEDIA_BASE_URL + universityImageUrl
                : universityImageUrl)
            : 'images/universities/default.png';
        universityImage.src = resolvedUrl;
    }

    /* =========================
       NAVBAR USER CONTROL
    ========================== */

    function updateNavbar() {
        if (typeof isLoggedIn === 'function' && isLoggedIn()) {
            const user = JSON.parse(localStorage.getItem("user"));
            const navButtons = document.getElementById("navButtons");
            if (navButtons) {
                let adminBtn = "";
                if (isAdmin()) {
                    adminBtn = `<a href="admin.html" class="btn btn-warning me-2">Admin Panel</a>`;
                }
                navButtons.innerHTML = `
                    ${adminBtn}
                    <button class="btn btn-dark" onclick="logout()">Logout</button>
                `;
            }
        }
    }

    updateNavbar();

    // Admin ise kulüp ekleme formunu göster
    if (typeof isAdmin === 'function' && isAdmin()) {
        document.getElementById("addClubSection").classList.remove("d-none");
    }

    // Kulüp ekleme formu
    document.getElementById("addClubForm").addEventListener("submit", async function(e) {
        e.preventDefault();
        const msgEl = document.getElementById("addClubMessage");
        msgEl.innerHTML = `<div class="alert alert-info py-1">Kaydediliyor...</div>`;

        try {
            let imageUrl = null;
            const fileInput = document.getElementById("newClubImageFile");
            if (fileInput.files.length > 0) {
                imageUrl = await ApiService.uploadImage("clubs", fileInput.files[0]);
                if (!imageUrl) {
                    msgEl.innerHTML = `<div class="alert alert-danger py-1">Resim yüklenirken hata oluştu.</div>`;
                    return;
                }
            }

            const clubData = {
                name: document.getElementById("newClubName").value.trim(),
                category: document.getElementById("newClubCategory").value,
                description: document.getElementById("newClubDescription").value.trim(),
                imageUrl: imageUrl,
                universityId: parseInt(universityId)
            };

            const result = await ApiService.createClub(clubData);
            if (result.ok) {
                msgEl.innerHTML = `<div class="alert alert-success py-1">"${clubData.name}" başarıyla kaydedildi!</div>`;
                this.reset();
                setTimeout(() => location.reload(), 2000);
            } else {
                alert("HATA: " + result.message);
                msgEl.innerHTML = `<div class="alert alert-danger py-1">Kulüp kaydedilemedi: ${result.message}</div>`;
            }
        } catch (err) {
            msgEl.innerHTML = `<div class="alert alert-danger py-1">Hata: ${err.message}</div>`;
        }
    });

    // Fetch clubs from API
    const clubs = await ApiService.getClubsByUniversity(universityId);

    /* =========================
       ÜST BİLGİ ALANI GÜNCELLEME
    ========================== */

    if (clubCountElement) clubCountElement.textContent = clubs.length;

    // Üniversiteye kayıtlı öğrenci sayısını çek
    if (studentCount) {
        try {
            const uniResponse = await fetch(`http://localhost:5149/api/universities`);
            const unis = await uniResponse.json();
            const thisUni = unis.find(u => u.id === parseInt(universityId));
            studentCount.textContent = thisUni ? thisUni.studentCount : 0;
        } catch { studentCount.textContent = 0; }
    }

    /* =========================
       KULÜP RENDER FONKSİYONU
    ========================== */

    function renderClubs(data) {
        if (!clubList) return;
        clubList.innerHTML = "";

        data.forEach(function (club) {
            const col = document.createElement("div");
            col.className = "col-md-4";

            // Admin kontrolü: Eğer adminse Sil butonu göster
            let adminActions = "";
            if (typeof isAdmin === 'function' && isAdmin()) {
                const escapedDesc = (club.description || "").replace(/'/g, "\\'");
                adminActions = `
                    <div class="mt-2 d-flex gap-2">
                        <button class="btn btn-sm btn-outline-primary w-100" onclick="handleClubEdit(${club.id}, '${club.name}', '${club.category}', '${escapedDesc}', '${club.imageUrl || ''}')">Edit</button>
                        <button class="btn btn-sm btn-outline-danger w-100" onclick="handleClubDelete(${club.id})">Delete</button>
                    </div>
                `;
            }

            col.innerHTML = `
                <div class="card shadow-sm h-100">
                    <img src="${club.imageUrl ? (club.imageUrl.startsWith('http') ? club.imageUrl : club.imageUrl.startsWith('/uploads/') ? MEDIA_BASE_URL + club.imageUrl : club.imageUrl) : 'images/clubs/default.jpg'}"
                         class="card-img-top"
                         style="height:180px; object-fit:cover;"
                         onerror="this.src='images/clubs/default.jpg'">
                    <div class="card-body d-flex flex-column">
                        <h5 class="fw-bold">${club.name}</h5>
                        <p class="text-muted">${club.category}</p>
                        <a href="club.html?id=${club.id}" class="btn btn-dark mt-auto">
                            View Details
                        </a>
                        ${adminActions}
                    </div>
                </div>
            `;
            clubList.appendChild(col);
        });

        if (clubFoundCount)
            clubFoundCount.textContent = data.length + " clubs found";
    }

    renderClubs(clubs);

    /* =========================
       ADMIN ACTIONS (EDIT/DELETE)
    ========================== */

    window.handleClubEdit = function(id, name, category, description, imageUrl) {
        document.getElementById("editClubId").value = id;
        document.getElementById("editClubName").value = name;
        document.getElementById("editClubCategory").value = category;
        document.getElementById("editClubDescription").value = description;
        document.getElementById("editClubImage").value = imageUrl;
        
        const modal = new bootstrap.Modal(document.getElementById('editClubModal'));
        modal.show();
    }

    const editClubForm = document.getElementById("editClubForm");
    if (editClubForm) {
        editClubForm.addEventListener("submit", async function(e) {
            e.preventDefault();
            const id = document.getElementById("editClubId").value;
            const clubData = {
                id: parseInt(id),
                name: document.getElementById("editClubName").value,
                category: document.getElementById("editClubCategory").value,
                description: document.getElementById("editClubDescription").value,
                imageUrl: document.getElementById("editClubImage").value
            };

            const success = await ApiService.updateClub(id, clubData);
            if (success) {
                alert("Club updated successfully!");
                location.reload();
            } else {
                alert("Error updating club.");
            }
        });
    }

    window.handleClubDelete = async function(id) {
        if (confirm("Are you sure you want to delete this club?")) {
            const success = await ApiService.deleteClub(id);
            if (success) {
                alert("Club deleted!");
                location.reload();
            } else {
                alert("Error deleting club.");
            }
        }
    };


    /* =========================
       FİLTRE SİSTEMİ
    ========================== */

    function filterClubs() {
        if (!clubSearch || !categoryFilter) return;

        const searchValue = clubSearch.value.toLowerCase();
        const selectedCategory = categoryFilter.value;

        const filtered = clubs.filter(function (club) {
            const matchesSearch = club.name.toLowerCase().includes(searchValue);
            const matchesCategory = selectedCategory === "all" || club.category === selectedCategory;
            return matchesSearch && matchesCategory;
        });

        renderClubs(filtered);
    }

    if (clubSearch) clubSearch.addEventListener("keyup", filterClubs);
    if (categoryFilter) categoryFilter.addEventListener("change", filterClubs);

});