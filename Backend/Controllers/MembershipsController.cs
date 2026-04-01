using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.Models;
using Microsoft.AspNetCore.Authorization;
using UniversityClubSystem.DTOs;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembershipsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembershipsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Öğrencinin bir kulübe katılma başvurusu yapması.
        /// Başvuru varsayılan olarak "Pending" durumuyla oluşturulur.
        /// POST /api/memberships/apply
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] MembershipApplyDto dto)
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            // Kulübün var olup olmadığını kontrol et
            var club = await _context.Clubs.FindAsync(dto.ClubId);
            if (club == null)
                return NotFound(new { message = "Kulüp bulunamadı." });

            // Üniversite kısıtlaması
            if (user.UniversityId != null && user.UniversityId != club.UniversityId)
                return BadRequest(new { message = "Yalnızca kendi üniversitenizin kulüplerine başvurabilirsiniz." });

            // Daha önce başvuru yapılmış mı kontrol et
            var existingMembership = await _context.ClubMemberships
                .FirstOrDefaultAsync(m => m.UserId == userId && m.ClubId == dto.ClubId);

            if (existingMembership != null)
            {
                if (existingMembership.Status == MembershipStatus.Pending)
                    return BadRequest(new { message = "Başvurunuz zaten inceleniyor." });
                if (existingMembership.Status == MembershipStatus.Approved)
                    return BadRequest(new { message = "Bu kulübün zaten üyesisiniz." });

                // Reddedilmişse mevcut kaydı güncelle (yeniden başvuru)
                existingMembership.Status = MembershipStatus.Pending;
                existingMembership.ApplicationDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Yeniden başvurunuz alındı. Onay bekleniyor." });
            }

            // Yeni başvuru oluştur
            var membership = new ClubMembership
            {
                UserId = userId,
                ClubId = dto.ClubId,
                Status = MembershipStatus.Pending,
                ApplicationDate = DateTime.UtcNow
            };

            _context.ClubMemberships.Add(membership);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Başvurunuz başarıyla alındı. Kulüp yöneticisinin onayı bekleniyor.",
                membershipId = membership.Id,
                status = membership.Status.ToString()
            });
        }

        /// <summary>
        /// Giriş yapmış kullanıcının tüm kulüp üyelik durumlarını döndürür.
        /// GET /api/memberships/my
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyMemberships()
        {
            var userIdClaim = User.FindFirst("nameid")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var memberships = await _context.ClubMemberships
                .Where(m => m.UserId == userId)
                .Select(m => new
                {
                    m.ClubId,
                    ClubName = m.Club.Name,
                    ClubCategory = m.Club.Category,
                    ClubImageUrl = m.Club.ImageUrl,
                    UniversityName = m.Club.University.Name,
                    UniversityId = m.Club.UniversityId,
                    Status = m.Status.ToString(),
                    m.ApplicationDate
                })
                .OrderByDescending(m => m.ApplicationDate)
                .ToListAsync();

            return Ok(memberships);
        }

        /// <summary>
        /// Kulüp Yöneticisinin başvuru talebini onaylaması (Approved) veya reddetmesi (Rejected).
        /// PUT /api/memberships/respond
        /// </summary>
        [HttpPut("respond")]
        [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.SystemAdmin)}")]
        public async Task<IActionResult> Respond([FromBody] MembershipResponseDto dto)
        {
            // Sadece Approved veya Rejected kabul edilir
            if (dto.Status != MembershipStatus.Approved && dto.Status != MembershipStatus.Rejected)
                return BadRequest(new { message = "Geçersiz durum. Sadece 'Approved' veya 'Rejected' kabul edilir." });

            var membership = await _context.ClubMemberships
                .Include(m => m.User)
                .Include(m => m.Club)
                .FirstOrDefaultAsync(m => m.Id == dto.MembershipId);

            if (membership == null)
                return NotFound(new { message = "Başvuru bulunamadı." });

            // Zaten yanıtlanmış bir başvuruyu tekrar güncellemesini engelle
            if (membership.Status != MembershipStatus.Pending)
                return BadRequest(new { message = $"Bu başvuru zaten '{membership.Status}' durumunda." });

            // Durumu güncelle
            membership.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.Status == MembershipStatus.Approved
                    ? $"{membership.User.FirstName} {membership.User.LastName} kullanıcısının '{membership.Club.Name}' kulübüne üyeliği onaylandı."
                    : $"{membership.User.FirstName} {membership.User.LastName} kullanıcısının '{membership.Club.Name}' kulübüne başvurusu reddedildi.",
                membershipId = membership.Id,
                status = membership.Status.ToString()
            });
        }

        /// <summary>
        /// Bir kulübe yapılmış tüm başvuruları listeler (Yönetici paneli için).
        /// GET /api/memberships/club/{clubId}
        /// </summary>
        [HttpGet("club/{clubId:int}")]
        [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.SystemAdmin)}")]
        public async Task<IActionResult> GetMembershipsByClub(int clubId)
        {
            var memberships = await _context.ClubMemberships
                .Where(m => m.ClubId == clubId)
                .Include(m => m.User)
                .OrderByDescending(m => m.ApplicationDate)
                .Select(m => new
                {
                    m.Id,
                    UserFullName = m.User.FirstName + " " + m.User.LastName,
                    m.User.Email,
                    Status = m.Status.ToString(),
                    m.ApplicationDate
                })
                .ToListAsync();

            return Ok(memberships);
        }
    }
}
