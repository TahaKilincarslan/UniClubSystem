using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;
using UniversityClubSystem.Services;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur.
        /// </summary>
        /// <param name="registerDto">Kayıt bilgileri</param>
        /// <returns>Auth token ve kullanıcı bilgileri</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            Console.WriteLine($"Registration attempt for: {registerDto.Email}");
            
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email.ToLower()))
            {
                Console.WriteLine("Register failed: Email already exists.");
                return BadRequest("Email already exists.");
            }

            try 
            {
                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email.ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = UserRole.Student // Default role
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                Console.WriteLine($"User saved to database: {user.Email}, ID: {user.Id}");

                var response = new AuthResponseDto
                {
                    Token = _tokenService.CreateToken(user),
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString()
                };
                Console.WriteLine("Token created successfully.");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR during registration: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw; // Rethrow to let ExceptionMiddleware handle it
            }
        }

        /// <summary>
        /// Kullanıcı girişi yaparak JWT token döndürür.
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri (Email, Password)</param>
        /// <returns>Auth token ve kullanıcı bilgileri</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            };
        }
    }
}
