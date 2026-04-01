using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Data;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email.ToLower()))
                throw new Exception("Email already exists.");

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = UserRole.Student,
                UniversityId = registerDto.UniversityId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var university = user.UniversityId.HasValue
                ? await _context.Universities.FindAsync(user.UniversityId.Value)
                : null;

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                UniversityId = user.UniversityId,
                UniversityName = university?.Name
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                UniversityId = user.UniversityId,
                UniversityName = user.University?.Name
            };
        }
    }
}
