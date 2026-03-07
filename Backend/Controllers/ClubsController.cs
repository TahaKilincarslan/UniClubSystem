using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClubsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClubsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Bir üniversiteye ait tüm kulüpleri listeler.
        /// GET /api/clubs/{universityId}
        /// </summary>
        [HttpGet("{universityId:int}")]
        public async Task<IActionResult> GetClubsByUniversity(int universityId)
        {
            // Üniversitenin var olup olmadığını kontrol et
            var universityExists = await _context.Universities.AnyAsync(u => u.Id == universityId);
            if (!universityExists)
                return NotFound(new { message = "Üniversite bulunamadı." });

            var clubs = await _context.Clubs
                .Where(c => c.UniversityId == universityId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Category,
                    c.ImageUrl,
                    c.CreatedDate,
                    ManagerName = c.Manager.FirstName + " " + c.Manager.LastName,
                    // Onaylanmış üye sayısı
                    MemberCount = c.Memberships.Count(m => m.Status == MembershipStatus.Approved)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(clubs);
        }

        /// <summary>
        /// Kulüp detaylarını, üye sayısını ve yaklaşan etkinlikleri döndürür.
        /// GET /api/clubs/detail/{id}
        /// </summary>
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetClubDetail(int id)
        {
            var club = await _context.Clubs
                .Include(c => c.University)
                .Include(c => c.Manager)
                .Include(c => c.Memberships)
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
                return NotFound(new { message = "Kulüp bulunamadı." });

            // Yanıtı DTO'ya dönüştür
            var dto = new ClubDetailDto
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                Category = club.Category,
                ImageUrl = club.ImageUrl,
                UniversityName = club.University.Name,
                ManagerFullName = $"{club.Manager.FirstName} {club.Manager.LastName}",
                CreatedDate = club.CreatedDate,
                // Sadece onaylanmış üyeleri say
                MemberCount = club.Memberships.Count(m => m.Status == MembershipStatus.Approved),
                // Bugünden sonraki aktif etkinlikleri getir
                UpcomingEvents = club.Events
                    .Where(e => e.IsActive && e.Date >= DateTime.UtcNow)
                    .OrderBy(e => e.Date)
                    .Select(e => new EventDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        Date = e.Date,
                        Location = e.Location,
                        IsActive = e.IsActive
                    })
                    .ToList()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Yeni bir kulüp oluşturur.
        /// POST /api/clubs
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] Club club)
        {
            club.CreatedDate = DateTime.UtcNow;
            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetClubDetail), new { id = club.Id }, club);
        }
    }
}
