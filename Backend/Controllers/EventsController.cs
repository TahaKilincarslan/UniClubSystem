using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;
using UniversityClubSystem.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UniversityClubSystem.DTOs;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EventService _eventService;
        private readonly EventRequestService _eventRequestService;

        public EventsController(AppDbContext context, EventService eventService, EventRequestService eventRequestService)
        {
            _context = context;
            _eventService = eventService;
            _eventRequestService = eventRequestService;
        }

        /// <summary>
        /// Tüm aktif etkinlikleri listeler. Opsiyonel olarak kulübe göre filtreleme yapar.
        /// GET /api/events?clubId=1
        /// </summary>
        [HttpGet("api/events")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents([FromQuery] int? clubId)
        {
            var query = _context.Events
                .Include(e => e.Club)
                .AsQueryable();

            if (clubId.HasValue)
            {
                query = query.Where(e => e.ClubId == clubId.Value);
            }

            var events = await query
                .OrderBy(e => e.Date)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Date,
                    e.Location,
                    e.IsActive,
                    ClubName = e.Club.Name,
                    e.ClubId
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Bir kulübe ait aktif etkinlikleri listeler.
        /// GET /api/clubs/{clubId}/events
        /// </summary>
        [HttpGet("api/clubs/{clubId:int}/events")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventsByClub(int clubId)
        {
            var events = await _eventService.GetEventsByClubIdAsync(clubId);
            return Ok(events);
        }

        /// <summary>
        /// Belirli bir etkinliğin detaylarını getirir.
        /// GET /api/events/{id}
        /// </summary>
        [HttpGet("api/events/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound(new { message = "Etkinlik bulunamadı." });

            return Ok(ev);
        }

        /// <summary>
        /// Yeni bir etkinlik oluşturur.
        /// POST /api/events/create
        /// </summary>
        [HttpPost("api/events/create")]
        [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.SystemAdmin)}")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var clubExists = await _context.Clubs.AnyAsync(c => c.Id == dto.ClubId);
            if (!clubExists)
                return NotFound(new { message = "Kulüp bulunamadı." });

            var @event = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date.ToUniversalTime(),
                Location = dto.Location,
                ClubId = dto.ClubId,
                IsActive = true
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvents), new { id = @event.Id }, @event);
        }

        /// <summary>
        /// Giriş yapmış kullanıcının tüm etkinlik isteklerini döndürür.
        /// GET /api/events/my-requests
        /// </summary>
        [HttpGet("api/events/my-requests")]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var requests = await _context.EventRequests
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.EventId,
                    EventTitle = r.Event.Title,
                    EventDate = r.Event.Date,
                    EventLocation = r.Event.Location,
                    ClubId = r.Event.ClubId,
                    ClubName = r.Event.Club.Name,
                    UniversityName = r.Event.Club.University.Name,
                    Status = r.Status.ToString(),
                    r.RequestDate
                })
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bir etkinliğe katılma isteği göndermesini sağlar.
        /// POST /api/events/{id}/join
        /// </summary>
        [HttpPost("api/events/{id:int}/join")]
        [Authorize]
        public async Task<IActionResult> JoinEvent(int id)
        {
            // JWT token'dan UserId'yi oku
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Geçersiz token." });

            var (success, message) = await _eventRequestService.CreateRequestAsync(userId, id);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        /// <summary>
        /// Etkinliğin aktiflik durumunu günceller.
        /// PUT /api/events/{id}/toggle
        /// </summary>
        [HttpPut("api/events/{id:int}/toggle")]
        [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.SystemAdmin)}")]
        public async Task<IActionResult> ToggleEventStatus(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
                return NotFound(new { message = "Etkinlik bulunamadı." });

            @event.IsActive = !@event.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = @event.IsActive ? "Etkinlik aktifleştirildi." : "Etkinlik pasifleştirildi.",
                eventId = @event.Id,
                isActive = @event.IsActive
            });
        }
    }
}
