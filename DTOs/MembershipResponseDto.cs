using UniversityClubSystem.Models;

namespace UniversityClubSystem.DTOs
{
    // Kulüp yöneticisinin başvuruya cevap vermesi için istek modeli
    public class MembershipResponseDto
    {
        public int MembershipId { get; set; }
        public MembershipStatus Status { get; set; } // Approved veya Rejected
    }
}
