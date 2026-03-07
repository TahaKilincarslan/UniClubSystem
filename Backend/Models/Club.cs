using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityClubSystem.Models
{
    // Kulüp bilgilerini temsil eden ana model
    public class Club
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Kulüp kategorisi (örn: "Kültür & Sanat", "Teknoloji", "Spor")
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Hangi üniversiteye ait? (Foreign Key)
        public int UniversityId { get; set; }

        // Kulüp yöneticisi/başkanı (Foreign Key)
        public int ManagerId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(UniversityId))]
        public University University { get; set; } = null!;

        [ForeignKey(nameof(ManagerId))]
        public User Manager { get; set; } = null!;

        // Kulübe ait etkinlikler
        public ICollection<Event> Events { get; set; } = new List<Event>();

        // Kulübe gelen üyelik başvuruları
        public ICollection<ClubMembership> Memberships { get; set; } = new List<ClubMembership>();
    }
}
