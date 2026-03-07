document.addEventListener("DOMContentLoaded", async function () {

    const universityList = document.getElementById("universityList");
    const totalUniversities = document.getElementById("totalUniversities");
    const totalClubs = document.getElementById("totalClubs");
    const totalMembers = document.getElementById("totalMembers");
    const searchInput = document.getElementById("searchInput");
    const cityFilter = document.getElementById("cityFilter");
    const noResultMessage = document.getElementById("noResultMessage");

    // Fetch data from API
    const universities = await ApiService.getUniversities();

    /* =========================
       GENERATE UNIVERSITY CARDS
    ========================== */

    function renderUniversities(data) {
        universityList.innerHTML = "";
        data.forEach(function (uni) {
            const col = document.createElement("div");
            col.className = "col-md-4 university-card";
            col.setAttribute("data-city", uni.city);

            col.innerHTML = `
                <div class="card shadow-sm p-3 h-100">
                    <img src="${uni.logoUrl || 'images/universities/default.png'}" class="card-img-top uni-img" onerror="this.src='images/universities/default.png'">
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title">${uni.name}</h5>
                        <p class="text-muted">
                            ${uni.clubCount || 0} Clubs • ${uni.city}
                        </p>
                        <a href="university.html?id=${uni.id}&name=${encodeURIComponent(uni.name)}" 
                           class="btn btn-outline-dark mt-auto w-100">
                           View Clubs
                        </a>
                    </div>
                </div>
            `;
            universityList.appendChild(col);
        });

        if (data.length === 0) {
            noResultMessage.classList.remove("d-none");
        } else {
            noResultMessage.classList.add("d-none");
        }
    }

    renderUniversities(universities);

    /* =========================
       SEARCH + CITY FILTER
    ========================== */

    async function filterUniversities() {
        const searchValue = searchInput.value.toLowerCase();
        const selectedCity = cityFilter.value;

        // Filter locally for performance, or call API again if preferred
        // Let's filter locally since we have all data
        const filtered = universities.filter(uni => {
            const matchesSearch = uni.name.toLowerCase().includes(searchValue);
            const matchesCity = selectedCity === "all" || uni.city.toLowerCase() === selectedCity.toLowerCase();
            return matchesSearch && matchesCity;
        });

        renderUniversities(filtered);
    }

    searchInput.addEventListener("keyup", filterUniversities);
    cityFilter.addEventListener("change", filterUniversities);

    /* =========================
       STATS CALCULATION
    ========================== */

    let clubCount = 0;
    universities.forEach(uni => {
        clubCount += uni.clubCount || 0;
    });

    /* =========================
       ANIMATED COUNTER
    ========================== */

    function animateCounter(element, target, duration = 1500) {
        if (!element) return;
        let start = 0;
        const increment = target / (duration / 16);

        const timer = setInterval(() => {
            start += increment;
            if (start >= target) {
                element.textContent = target;
                clearInterval(timer);
            } else {
                element.textContent = Math.floor(start);
            }
        }, 16);
    }

    animateCounter(totalUniversities, universities.length);
    animateCounter(totalClubs, clubCount);
    animateCounter(totalMembers, 0); // No student count in base API yet

});