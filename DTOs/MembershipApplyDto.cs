namespace UniversityClubSystem.DTOs
{
    // Kulübe katılma başvurusu için istek modeli
    public class MembershipApplyDto
    {
        public int UserId { get; set; }
        public int ClubId { get; set; }
    }
}
