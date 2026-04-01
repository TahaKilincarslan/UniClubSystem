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
        /// Yeni bir kulüp ekler.
        /// POST /api/clubs
        /// </summary>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Category))
                return BadRequest("Name and Category are required.");

            var universityExists = await _context.Universities.AnyAsync(u => u.Id == dto.UniversityId);
            if (!universityExists)
                return NotFound(new { message = "Üniversite bulunamadı." });

            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int managerId))
                return Unauthorized(new { message = "Token'dan kullanıcı ID okunamadı.", claim = userIdClaim });

            var club = new Club
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                UniversityId = dto.UniversityId,
                ManagerId = managerId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetClubsByUniversity), new { universityId = club.UniversityId }, club);
        }

        /// <summary>
        /// Mevcut bir kulübü günceller.
        /// PUT /api/clubs/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async Task<IActionResult> UpdateClub(int id, [FromBody] Club club)
        {
            if (id != club.Id) return BadRequest("ID mismatch");

            var existingClub = await _context.Clubs.FindAsync(id);
            if (existingClub == null) return NotFound();

            existingClub.Name = club.Name;
            existingClub.Description = club.Description;
            existingClub.Category = club.Category;
            existingClub.ImageUrl = club.ImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Bir kulübü siler.
        /// DELETE /api/clubs/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null) return NotFound();

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kulüp başarıyla silindi." });
        }
    }
}
