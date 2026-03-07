using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Yeni bir üniversite ekler.
        /// POST /api/universities
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUniversity([FromBody] University university)
        {
            _context.Universities.Add(university);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUniversities), new { id = university.Id }, university);
        }
    }
}
