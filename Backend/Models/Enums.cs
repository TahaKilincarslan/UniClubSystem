namespace UniversityClubSystem.Models
{
    // Kullanıcı rolleri: Sistemdeki yetki seviyelerini tanımlar
    public enum UserRole
    {
        Student,      // Öğrenci
        ClubManager,  // Kulüp Yöneticisi / Başkanı
        SystemAdmin   // Sistem Yöneticisi
    }

    // Üyelik başvurusu durumu
    public enum MembershipStatus
    {
        Pending,   // Beklemede
        Approved,  // Onaylandı
        Rejected   // Reddedildi
    }
}
