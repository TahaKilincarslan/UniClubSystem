using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Varsayılan olarak tüm metodlar giriş yapmış kullanıcı gerektirir
    public class UniversitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UniversitiesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm üniversiteleri listeler. Opsiyonel olarak şehre göre filtreleme yapar.
        /// GET /api/universities?city=İstanbul
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Herkes görebilir
        public async Task<IActionResult> GetUniversities([FromQuery] string? city)
        {
            var query = _context.Universities.AsQueryable();

            // Şehir parametresi varsa filtrele
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(u => u.City.ToLower().Contains(city.ToLower()));
            }

            var universities = await query
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.City,
                    u.LogoUrl,
                    ClubCount = u.Clubs.Count // Her üniversitenin kaç kulübü olduğunu da döndür
                })
                .ToListAsync();

            return Ok(universities);
        }

        /// <summary>
        /// Mevcut bir üniversiteyi günceller.
        /// PUT /api/universities/{id}
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async Task<IActionResult> UpdateUniversity(int id, [FromBody] University university)
        {
            if (id != university.Id) return BadRequest("ID mismatch");

            var existingUni = await _context.Universities.FindAsync(id);
            if (existingUni == null) return NotFound();

            existingUni.Name = university.Name;
            existingUni.City = university.City;
            existingUni.LogoUrl = university.LogoUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Bir üniversiteyi ve ona bağlı tüm kulüpleri siler.
        /// DELETE /api/universities/{id}
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var university = await _context.Universities
                .Include(u => u.Clubs)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (university == null) return NotFound();

            _context.Universities.Remove(university);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Üniversite ve bağlı tüm kulüpler başarıyla silindi." });
        }
    }
}
