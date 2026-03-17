using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Sadece SystemAdmin rolüne sahip kullanıcılar bu controller'a erişebilir
    [Authorize(Roles = "SystemAdmin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetAdminDashboardData()
        {
            return Ok(new 
            { 
                Message = "Hoş geldiniz Yönetici. Bu veriyi sadece 'SystemAdmin' rolüne sahip kullanıcılar görebilir.",
                AdminRoleActive = true
            });
        }
    }
}
