using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityClubSystem.Models
{
    // Kulüplere ait etkinlikleri temsil eder
    public class Event
    {
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Etkinlik tarihi
        public DateTime Date { get; set; }

        // Etkinlik yeri
        [Required, MaxLength(300)]
        public string Location { get; set; } = string.Empty;

        // Bu etkinlik hangi kulübe ait? (Foreign Key)
        public int ClubId { get; set; }

        // Etkinlik aktif mi?
        public bool IsActive { get; set; } = true;

        // --- Navigation Property ---
        [ForeignKey(nameof(ClubId))]
        public Club Club { get; set; } = null!;
    }
}
