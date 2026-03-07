using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm aktif etkinlikleri listeler. Opsiyonel olarak kulübe göre filtreleme yapar.
        /// GET /api/events?clubId=1
        /// </summary>
        [HttpGet]
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
        /// Yeni bir etkinlik oluşturur.
        /// POST /api/events
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event @event)
        {
            // Kulübün var olup olmadığını kontrol et
            var clubExists = await _context.Clubs.AnyAsync(c => c.Id == @event.ClubId);
            if (!clubExists)
                return NotFound(new { message = "Kulüp bulunamadı." });

            @event.IsActive = true;
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvents), new { id = @event.Id }, @event);
        }

        /// <summary>
        /// Etkinliğin aktiflik durumunu günceller.
        /// PUT /api/events/{id}/toggle
        /// </summary>
        [HttpPut("{id:int}/toggle")]
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
