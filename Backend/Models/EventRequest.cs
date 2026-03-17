using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityClubSystem.Models
{
    // Kullanıcıların etkinliklere katılım isteklerini temsil eder
    public class EventRequest
    {
        public int Id { get; set; }

        // İsteği gönderen kullanıcı
        public int UserId { get; set; }

        // Hangi etkinliğe katılmak istiyor?
        public int EventId { get; set; }

        // İstek durumu (Varsayılan: Pending)
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        // İstek tarihi
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;
    }
}
