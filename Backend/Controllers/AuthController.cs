using Microsoft.AspNetCore.Mvc;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Services;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur.
        /// </summary>
        /// <param name="registerDto">Kayıt bilgileri</param>
        /// <returns>Auth token ve kullanıcı bilgileri</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            try 
            {
                var response = await _authService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Exceptions shouldn't typically be 400 always but for simplicity here
                if (ex.Message == "Email already exists.")
                    return BadRequest(ex.Message);
                    
                throw; 
            }
        }

        /// <summary>
        /// Kullanıcı girişi yaparak JWT token döndürür.
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri (Email veya Username, Password)</param>
        /// <returns>Auth token ve kullanıcı bilgileri</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
