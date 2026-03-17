using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Services
{
    public class EventRequestService
    {
        private readonly AppDbContext _context;

        public EventRequestService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kullanıcının bir etkinliğe katılma isteği oluşturur.
        /// Aynı kullanıcının aynı etkinliğe birden fazla Pending veya Approved isteği olması engellenir.
        /// </summary>
        public async Task<(bool Success, string Message)> CreateRequestAsync(int userId, int eventId)
        {
            // Etkinlik var mı?
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId && e.IsActive);
            if (!eventExists)
                return (false, "Etkinlik bulunamadı veya aktif değil.");

            // Kullanıcının mevcut aktif isteği var mı?
            var existingRequest = await _context.EventRequests
                .AnyAsync(r => r.UserId == userId
                            && r.EventId == eventId
                            && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Approved));

            if (existingRequest)
                return (false, "Bu etkinliğe zaten bir katılma isteğiniz bulunmaktadır.");

            var request = new EventRequest
            {
                UserId = userId,
                EventId = eventId,
                Status = RequestStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.EventRequests.Add(request);
            await _context.SaveChangesAsync();

            return (true, "Katılma isteğiniz başarıyla gönderildi.");
        }

        /// <summary>
        /// Belirli bir etkinliğe gelen tüm istekleri listeler (Admin için).
        /// </summary>
        public async Task<List<EventRequestDto>> GetRequestsByEventIdAsync(int eventId)
        {
            return await _context.EventRequests
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new EventRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserFullName = r.User.FirstName + " " + r.User.LastName,
                    UserEmail = r.User.Email,
                    EventId = r.EventId,
                    EventTitle = r.Event.Title,
                    Status = r.Status.ToString(),
                    RequestDate = r.RequestDate
                })
                .ToListAsync();
        }

        /// <summary>
        /// Bir isteğin statüsünü Approved veya Rejected olarak günceller (Admin için).
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateRequestStatusAsync(int requestId, string statusString)
        {
            if (!Enum.TryParse<RequestStatus>(statusString, ignoreCase: true, out var newStatus))
                return (false, $"Geçersiz statü: '{statusString}'. Geçerli değerler: Pending, Approved, Rejected.");

            var request = await _context.EventRequests.FindAsync(requestId);
            if (request == null)
                return (false, "İstek bulunamadı.");

            request.Status = newStatus;
            await _context.SaveChangesAsync();

            return (true, $"İstek başarıyla '{newStatus}' olarak güncellendi.");
        }
    }
}
