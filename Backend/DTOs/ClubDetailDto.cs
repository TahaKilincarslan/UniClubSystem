namespace UniversityClubSystem.DTOs
{
    // Kulüp detay sayfası için zenginleştirilmiş yanıt modeli
    public class ClubDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public string ManagerFullName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Onaylanmış üye sayısı
        public int MemberCount { get; set; }

        // Yaklaşan aktif etkinlikler listesi
        public List<EventDto> UpcomingEvents { get; set; } = new();
    }

    // Etkinlik bilgisi DTO
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
