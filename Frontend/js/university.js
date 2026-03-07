document.addEventListener("DOMContentLoaded", async function () {

    const params = new URLSearchParams(window.location.search);
    const universityId = params.get("id");
    const universityName = params.get("name");

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

    // Fetch clubs from API
    const clubs = await ApiService.getClubsByUniversity(universityId);

    /* =========================
       ÜST BİLGİ ALANI GÜNCELLEME
    ========================== */

    if (clubCountElement) clubCountElement.textContent = clubs.length;
    if (studentCount) studentCount.textContent = "..."; // We don't have student count in API yet

    /* =========================
       KULÜP RENDER FONKSİYONU
    ========================== */

    function renderClubs(data) {
        if (!clubList) return;
        clubList.innerHTML = "";

        data.forEach(function (club) {
            const col = document.createElement("div");
            col.className = "col-md-4";

            col.innerHTML = `
                <div class="card shadow-sm h-100">
                    <img src="${club.imageUrl || 'images/clubs/default.jpg'}" 
                         class="card-img-top"
                         style="height:180px; object-fit:cover;"
                         onerror="this.src='images/clubs/default.jpg'">
                    <div class="card-body d-flex flex-column">
                        <h5 class="fw-bold">${club.name}</h5>
                        <p class="text-muted">${club.category}</p>
                        <p class="small text-secondary">${club.managerName || 'No Manager'}</p>
                        <button class="btn btn-dark mt-auto">
                            View Details (${club.memberCount} members)
                        </button>
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