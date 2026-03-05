using System.ComponentModel.DataAnnotations;

namespace UniversityClubSystem.Models
{
    // Üniversite bilgilerini tutan model
    public class University
    {
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        // --- Navigation Property ---
        // Bu üniversiteye ait kulüpler
        public ICollection<Club> Clubs { get; set; } = new List<Club>();
    }
}
