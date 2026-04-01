using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bilgilerini döndürür.
        /// GET /api/users/me
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.ToString(),
                user.UniversityId,
                UniversityName = user.University?.Name,
                user.Department,
                user.Year
            });
        }

        /// <summary>
        /// Giriş yapmış kullanıcının bilgilerini günceller.
        /// PUT /api/users/me
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // E-posta başkası tarafından kullanılıyor mu?
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
                if (emailExists)
                    return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (dto.UniversityId.HasValue)
                user.UniversityId = dto.UniversityId.Value == 0 ? null : dto.UniversityId;

            if (dto.Department != null)
                user.Department = string.IsNullOrWhiteSpace(dto.Department) ? null : dto.Department;

            if (dto.Year.HasValue)
                user.Year = dto.Year.Value == 0 ? null : dto.Year;

            // Şifre değişikliği isteğe bağlı
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                    return BadRequest(new { message = "Mevcut şifre yanlış." });
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                Role = user.Role.ToString(),
                message = "Bilgileriniz güncellendi."
            });
        }
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? UniversityId { get; set; }
        public string? Department { get; set; }
        public int? Year { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
