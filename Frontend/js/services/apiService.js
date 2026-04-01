const API_BASE_URL = "http://localhost:5149/api";
const MEDIA_BASE_URL = "http://localhost:5149";

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

    // Upload Image
    async uploadImage(folder, file) {
        try {
            const formData = new FormData();
            formData.append("file", file);
            const response = await fetch(`${API_BASE_URL}/media/upload/${folder}`, {
                method: "POST",
                headers: getAuthHeaders(false),
                body: formData
            });
            if (!response.ok) return null;
            const data = await response.json();
            return data.url;
        } catch (error) {
            console.error("Error uploading image:", error);
            return null;
        }
    },

    // Create University
    async createUniversity(universityData) {
        try {
            const response = await fetch(`${API_BASE_URL}/universities`, {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(universityData)
            });
            return response.ok;
        } catch (error) {
            console.error("Error creating university:", error);
            return false;
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

    // Create Club
    async createClub(clubData) {
        try {
            const response = await fetch(`${API_BASE_URL}/clubs`, {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(clubData)
            });
            if (!response.ok) {
                const err = await response.text();
                console.error("Create club error:", response.status, err);
                return { ok: false, message: `${response.status}: ${err}` };
            }
            return { ok: true };
        } catch (error) {
            console.error("Error creating club:", error);
            return { ok: false, message: error.message };
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

    async getMyMemberships() {
        try {
            const response = await fetch(`${API_BASE_URL}/memberships/my`, {
                headers: getAuthHeaders()
            });
            if (!response.ok) return [];
            return await response.json();
        } catch (error) {
            console.error("Error fetching memberships:", error);
            return [];
        }
    },

    async applyToClub(clubId) {
        try {
            const response = await fetch(`${API_BASE_URL}/memberships/apply`, {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify({ clubId })
            });
            const data = await response.json();
            return { ok: response.ok, message: data.message };
        } catch (error) {
            console.error("Error applying to club:", error);
            return { ok: false, message: "Bir hata oluştu." };
        }
    },

    async getMyEventRequests() {
        try {
            const response = await fetch(`${API_BASE_URL}/events/my-requests`, {
                headers: getAuthHeaders()
            });
            if (!response.ok) return [];
            return await response.json();
        } catch (error) {
            console.error("Error fetching my requests:", error);
            return [];
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
            return { ok: false, message: "Bir hata oluştu." };
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
