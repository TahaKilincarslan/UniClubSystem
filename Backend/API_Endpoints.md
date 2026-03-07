# UniClub System API Endpoints

This project is separated into **Backend** (ASP.NET Core API) and **Frontend** (HTML/JS/CSS).

## Base URL
Default: `http://localhost:5149/api`

## Endpoints

### Universities
- `GET /api/universities`: List all universities.
  - Query Param: `city` (optional)
- `POST /api/universities`: Add a new university.

### Clubs
- `GET /api/clubs/{universityId}`: List all clubs for a specific university.
- `GET /api/clubs/detail/{clubId}`: Get detailed info for a club.
- `POST /api/clubs`: Create a new club.

### Events
- `GET /api/events/{clubId}`: List events for a club.
- `POST /api/events`: Create a new event.

### Memberships
- `POST /api/memberships/apply`: Apply for a club.
- `POST /api/memberships/approve/{id}`: Approve a membership.

## How to Run
1. Go to `Backend` folder.
2. Run `dotnet run`.
3. Open `Frontend/index.html` in a browser (or use Live Server).
