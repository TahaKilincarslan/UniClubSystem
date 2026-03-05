document.addEventListener("DOMContentLoaded", function () {

    /* =========================
       GENERATE UNIVERSITY CARDS
    ========================== */

    const universityList = document.getElementById("universityList");

    Object.keys(universityData).forEach(function (key) {

        const university = universityData[key];

        const col = document.createElement("div");
        col.className = "col-md-4 university-card";
        col.setAttribute("data-city", university.city);

        col.innerHTML = `
            <div class="card shadow-sm p-3 h-100">
                <img src="${university.image}" class="card-img-top uni-img">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title">${key} University</h5>
                    <p class="text-muted">
                        ${university.clubs.length} Clubs • ${university.students} Students
                    </p>
                    <a href="university.html?name=${key}" 
                       class="btn btn-outline-dark mt-auto w-100">
                       View Clubs
                    </a>
                </div>
            </div>
        `;

        universityList.appendChild(col);
    });


    /* =========================
       SEARCH + CITY FILTER
    ========================== */

    const searchInput = document.getElementById("searchInput");
    const cityFilter = document.getElementById("cityFilter");
    const cards = document.querySelectorAll(".university-card");
    const noResultMessage = document.getElementById("noResultMessage");

    function filterUniversities() {

        const searchValue = searchInput.value.toLowerCase();
        const selectedCity = cityFilter.value;
        let visibleCount = 0;

        cards.forEach(function (card) {

            const text = card.innerText.toLowerCase();
            const cardCity = card.getAttribute("data-city");

            const matchesSearch = text.includes(searchValue);
            const matchesCity =
                selectedCity === "all" ||
                cardCity.toLowerCase() === selectedCity.toLowerCase();

            if (matchesSearch && matchesCity) {
                card.style.display = "block";
                visibleCount++;
            } else {
                card.style.display = "none";
            }
        });

        if (visibleCount === 0) {
            noResultMessage.classList.remove("d-none");
        } else {
            noResultMessage.classList.add("d-none");
        }
    }

    searchInput.addEventListener("keyup", filterUniversities);
    cityFilter.addEventListener("change", filterUniversities);


    /* =========================
       STATS CALCULATION
    ========================== */

    const totalUniversities = document.getElementById("totalUniversities");
    const totalClubs = document.getElementById("totalClubs");
    const totalMembers = document.getElementById("totalMembers");

    let clubCount = 0;
    let memberCount = 0;

    Object.values(universityData).forEach(function (university) {
        clubCount += university.clubs.length;
        memberCount += university.students;
    });


    /* =========================
       ANIMATED COUNTER
    ========================== */

    function animateCounter(element, target, duration = 1500) {
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

    animateCounter(totalUniversities, Object.keys(universityData).length);
    animateCounter(totalClubs, clubCount);
    animateCounter(totalMembers, memberCount);

});