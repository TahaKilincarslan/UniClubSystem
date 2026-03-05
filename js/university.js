document.addEventListener("DOMContentLoaded", function () {

    const params = new URLSearchParams(window.location.search);
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

    // Güvenlik kontrolü
    if (!universityName || !universityData || !universityData[universityName]) {
        if (universityTitle) {
            universityTitle.textContent = "University not found";
        }
        return;
    }

    const university = universityData[universityName];

    /* =========================
       ÜST BİLGİ ALANI
    ========================== */

    if (universityTitle)
        universityTitle.textContent = universityName + " University";

    if (universityDescription)
        universityDescription.textContent = "Welcome to " + universityName + ".";

    if (studentCount)
        studentCount.textContent = university.students;

    if (clubCountElement)
        clubCountElement.textContent = university.clubs.length;

    if (universityImage)
        universityImage.src = university.image;


    /* =========================
       KULÜP RENDER FONKSİYONU
    ========================== */

    function renderClubs(clubs) {

        if (!clubList) return;

        clubList.innerHTML = "";

        clubs.forEach(function (club) {

            const col = document.createElement("div");
            col.className = "col-md-4";

            col.innerHTML = `
                <div class="card shadow-sm h-100">
                    <img src="${club.image}" 
                         class="card-img-top"
                         style="height:180px; object-fit:cover;">
                    <div class="card-body d-flex flex-column">
                        <h5 class="fw-bold">${club.name}</h5>
                        <p class="text-muted">${club.category}</p>
                        <button class="btn btn-dark mt-auto">
                            View Details
                        </button>
                    </div>
                </div>
            `;

            clubList.appendChild(col);
        });

        if (clubFoundCount)
            clubFoundCount.textContent = clubs.length + " clubs found";
    }

    // İlk yüklemede tüm kulüpler
    renderClubs(university.clubs);


    /* =========================
       FİLTRE SİSTEMİ
    ========================== */

    function filterClubs() {

        if (!clubSearch || !categoryFilter) return;

        const searchValue = clubSearch.value.toLowerCase();
        const selectedCategory = categoryFilter.value;

        const filtered = university.clubs.filter(function (club) {

            const matchesSearch =
                club.name.toLowerCase().includes(searchValue);

            const matchesCategory =
                selectedCategory === "all" ||
                club.category === selectedCategory;

            return matchesSearch && matchesCategory;
        });

        renderClubs(filtered);
    }

    if (clubSearch)
        clubSearch.addEventListener("keyup", filterClubs);

    if (categoryFilter)
        categoryFilter.addEventListener("change", filterClubs);

});