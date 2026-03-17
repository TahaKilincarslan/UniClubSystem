namespace UniversityClubSystem.DTOs
{
    // Kulübe ait etkinlikleri listelemek için
    public class EventListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ClubId { get; set; }
    }

    // Etkinlik detay sayfası için (kulüp bilgisiyle birlikte)
    public class EventDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int ApprovedParticipants { get; set; }
    }

    // Kullanıcının katılma isteği göndermesi için
    public class CreateEventRequestDto
    {
        // EventId URL'den alındığı için burada sadece ek bilgiler gelebilir (şimdilik boş)
    }

    // Admin'e gösterilecek istek listesi için
    public class EventRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }

    // Admin'in istek durumunu güncellemesi için
    public class UpdateEventRequestStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
