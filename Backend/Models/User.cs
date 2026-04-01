using System.ComponentModel.DataAnnotations;

namespace UniversityClubSystem.Models
{
    // Sistemdeki tüm kullanıcıları temsil eder (Öğrenci, Kulüp Yöneticisi, Admin)
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Kullanıcının rolü: Student, ClubManager, SystemAdmin
        public UserRole Role { get; set; } = UserRole.Student;

        // Kayıtlı olduğu üniversite (opsiyonel — admin için null)
        public int? UniversityId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? Department { get; set; }

        public int? Year { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(UniversityId))]
        public University? University { get; set; }

        // --- Navigation Properties ---
        // Bu kullanıcının yönettiği kulüpler
        public ICollection<Club> ManagedClubs { get; set; } = new List<Club>();

        // Bu kullanıcının kulüp üyelik başvuruları
        public ICollection<ClubMembership> Memberships { get; set; } = new List<ClubMembership>();

        // Bu kullanıcının etkinlik katılım istekleri
        public ICollection<EventRequest> EventRequests { get; set; } = new List<EventRequest>();
    }
}
