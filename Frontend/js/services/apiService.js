const API_BASE_URL = "http://localhost:5149/api";

const ApiService = {
    // Universities
    async getUniversities(city = "") {
        let url = `${API_BASE_URL}/universities`;
        if (city && city !== "all") {
            url += `?city=${encodeURIComponent(city)}`;
        }
        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching universities:", error);
            return [];
        }
    },

    // Clubs
    async getClubsByUniversity(universityId) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs/${universityId}`);
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching clubs:", error);
            return [];
        }
    },

    async getClubDetail(clubId) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs/detail/${clubId}`);
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching club detail:", error);
            return null;
        }
    },

    // Mock data conversion (optional, if you want to keep data.js structure temporarily)
    async getUniversityData() {
        // This converts the Flat API results back to the Object format expected by existing main.js/university.js
        // better to update main.js directly though.
        const universities = await this.getUniversities();
        const data = {};
        for (const uni of universities) {
            data[uni.name] = {
                id: uni.id,
                city: uni.city,
                students: 0, // Need to add this to model if needed
                image: uni.logoUrl || "images/universities/default.png",
                clubs: [] // We fetch clubs separately now
            };
        }
        return data;
    }
};
