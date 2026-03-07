using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityClubSystem.Models
{
    // Kulüp üyelik başvurularını ve durumlarını takip eden model
    public class ClubMembership
    {
        public int Id { get; set; }

        // Başvuran öğrenci (Foreign Key)
        public int UserId { get; set; }

        // Başvurulan kulüp (Foreign Key)
        public int ClubId { get; set; }

        // Başvuru durumu: Pending, Approved, Rejected
        public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

        // Başvuru tarihi
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(ClubId))]
        public Club Club { get; set; } = null!;
    }
}
