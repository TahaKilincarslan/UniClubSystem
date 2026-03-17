using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.DTOs;

namespace UniversityClubSystem.Services
{
    public class EventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Belirli bir kulübe ait tüm aktif etkinlikleri listeler.
        /// </summary>
        public async Task<List<EventListDto>> GetEventsByClubIdAsync(int clubId)
        {
            return await _context.Events
                .Where(e => e.ClubId == clubId && e.IsActive)
                .OrderBy(e => e.Date)
                .Select(e => new EventListDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    IsActive = e.IsActive,
                    ClubId = e.ClubId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Belirli bir etkinliğin detaylarını getirir (kulüp adı ve katılımcı sayısı dahil).
        /// </summary>
        public async Task<EventDetailDto?> GetEventByIdAsync(int eventId)
        {
            return await _context.Events
                .Where(e => e.Id == eventId)
                .Select(e => new EventDetailDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    IsActive = e.IsActive,
                    ClubId = e.ClubId,
                    ClubName = e.Club.Name,
                    TotalRequests = e.EventRequests.Count,
                    ApprovedParticipants = e.EventRequests.Count(r => r.Status == Models.RequestStatus.Approved)
                })
                .FirstOrDefaultAsync();
        }
    }
}
