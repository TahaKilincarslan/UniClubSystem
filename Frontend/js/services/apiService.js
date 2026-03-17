const API_BASE_URL = "http://localhost:5149/api";

const ApiService = {
    // Universities
    async getUniversities(city = "") {
        let url = `${API_BASE_URL}/universities`;
        if (city && city !== "all") {
            url += `?city=${encodeURIComponent(city)}`;
        }
        try {
            const response = await fetch(url, {
                headers: typeof getAuthHeaders === 'function' ? getAuthHeaders() : {}
            });
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
            const response = await fetch(`${API_BASE_URL}/clubs/${universityId}`, {
                headers: typeof getAuthHeaders === 'function' ? getAuthHeaders() : {}
            });
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching clubs:", error);
            return [];
        }
    },

    // Update University
    async updateUniversity(id, universityData) {
        try {
            const response = await fetch(`${API_BASE_URL}/universities/${id}`, {
                method: "PUT",
                headers: getAuthHeaders(),
                body: JSON.stringify(universityData)
            });
            return response.ok;
        } catch (error) {
            console.error("Error updating university:", error);
            return false;
        }
    },

    // Delete University
    async deleteUniversity(id) {
        try {
            const response = await fetch(`${API_BASE_URL}/universities/${id}`, {
                method: "DELETE",
                headers: getAuthHeaders()
            });
            return response.ok;
        } catch (error) {
            console.error("Error deleting university:", error);
            return false;
        }
    },

    // Update Club
    async updateClub(id, clubData) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs/${id}`, {
                method: "PUT",
                headers: getAuthHeaders(),
                body: JSON.stringify(clubData)
            });
            return response.ok;
        } catch (error) {
            console.error("Error updating club:", error);
            return false;
        }
    },

    // Delete Club
    async deleteClub(id) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs/${id}`, {
                method: "DELETE",
                headers: getAuthHeaders()
            });
            return response.ok;
        } catch (error) {
            console.error("Error deleting club:", error);
            return false;
        }
    },

    // Mock data conversion (optional, if you want to keep data.js structure temporarily)
    async getUniversityData() {
        const universities = await this.getUniversities();
        const data = {};
        for (const uni of universities) {
            data[uni.name] = {
                id: uni.id,
                city: uni.city,
                students: 0,
                image: uni.logoUrl || "images/universities/default.png",
                clubs: []
            };
        }
        return data;
    },

    // Events
    async getClubEvents(clubId) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs/${clubId}/events`);
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching club events:", error);
            return [];
        }
    },

    async getEventDetail(eventId) {
        try {
            const response = await fetch(`${API_BASE_URL}/events/${eventId}`);
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching event detail:", error);
            return null;
        }
    },

    async joinEvent(eventId) {
        try {
            const response = await fetch(`${API_BASE_URL}/events/${eventId}/join`, {
                method: "POST",
                headers: getAuthHeaders()
            });
            const data = await response.json();
            return { ok: response.ok, message: data.message };
        } catch (error) {
            console.error("Error joining event:", error);
            return { ok: false, message: "An error occurred." };
        }
    },

    // Admin - Event Requests
    async getEventRequests(eventId) {
        try {
            const response = await fetch(`${API_BASE_URL}/admin/events/${eventId}/requests`, {
                headers: getAuthHeaders()
            });
            if (!response.ok) throw new Error("Network response was not ok");
            return await response.json();
        } catch (error) {
            console.error("Error fetching event requests:", error);
            return [];
        }
    },

    async updateRequestStatus(requestId, status) {
        try {
            const response = await fetch(`${API_BASE_URL}/admin/event-requests/${requestId}/status`, {
                method: "PUT",
                headers: {
                    ...getAuthHeaders(),
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ status })
            });
            const data = await response.json();
            return { ok: response.ok, message: data.message };
        } catch (error) {
            console.error("Error updating request status:", error);
            return { ok: false, message: "An error occurred." };
        }
    }
};
