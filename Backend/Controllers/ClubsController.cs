using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using UniversityClubSystem.DTOs;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClubsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClubsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Bir üniversiteye ait tüm kulüpleri listeler.
        /// GET /api/clubs/{universityId}
        /// </summary>
        [HttpGet("{universityId:int}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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

            // AutoMapper kullanarak DTO'ya dönüştür
            var dto = _mapper.Map<ClubDetailDto>(club);

            return Ok(dto);
        }

        /// <summary>
        /// Yeni bir kulüp oluşturur.
        /// POST /api/clubs
        /// </summary>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))] // Sadece Admin kulüp oluşturabilir
        public async Task<IActionResult> CreateClub([FromBody] Club club)
        {
            club.CreatedDate = DateTime.UtcNow;
            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetClubDetail), new { id = club.Id }, club);
        }
    }
}
