using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public IActionResult GetAdminDashboardData()
        {
            return Ok(new
            {
                Message = "Hoş geldiniz Yönetici.",
                AdminRoleActive = true
            });
        }

        /// <summary>
        /// Tüm üniversite → kulüp → üyelik & etkinlik isteklerini organize döndürür.
        /// GET /api/admin/requests
        /// </summary>
        [HttpGet("requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var universities = await _context.Universities
                .Include(u => u.Clubs)
                    .ThenInclude(c => c.Memberships)
                        .ThenInclude(m => m.User)
                .Include(u => u.Clubs)
                    .ThenInclude(c => c.Events)
                        .ThenInclude(e => e.EventRequests)
                            .ThenInclude(r => r.User)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var result = universities.Select(u => new
            {
                u.Id,
                u.Name,
                Clubs = u.Clubs.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Category,
                    MembershipRequests = c.Memberships.Select(m => new
                    {
                        m.Id,
                        UserFullName = m.User.FirstName + " " + m.User.LastName,
                        m.User.Email,
                        Status = m.Status.ToString(),
                        m.ApplicationDate
                    }).OrderByDescending(m => m.ApplicationDate),
                    Events = c.Events.Select(e => new
                    {
                        e.Id,
                        e.Title,
                        e.Date,
                        EventRequests = e.EventRequests.Select(r => new
                        {
                            r.Id,
                            UserFullName = r.User.FirstName + " " + r.User.LastName,
                            r.User.Email,
                            Status = r.Status.ToString(),
                            r.RequestDate
                        }).OrderByDescending(r => r.RequestDate)
                    }).OrderBy(e => e.Date)
                }).OrderBy(c => c.Name)
            });

            return Ok(result);
        }

        /// <summary>
        /// Üyelik başvurusunu onayla veya reddet.
        /// PUT /api/admin/membership-requests/{id}/status
        /// </summary>
        [HttpPut("membership-requests/{id:int}/status")]
        public async Task<IActionResult> UpdateMembershipStatus(int id, [FromBody] StatusDto dto)
        {
            var membership = await _context.ClubMemberships.FindAsync(id);
            if (membership == null) return NotFound();

            if (!Enum.TryParse<MembershipStatus>(dto.Status, ignoreCase: true, out var newStatus))
                return BadRequest(new { message = "Geçersiz statü." });

            membership.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Üyelik başvurusu '{newStatus}' olarak güncellendi." });
        }
    }

    public class StatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
