using Microsoft.EntityFrameworkCore;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Veritabanı tabloları
        public DbSet<User> Users { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMembership> ClubMemberships { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRequest> EventRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- User Yapılandırması ---
            modelBuilder.Entity<User>(entity =>
            {
                // E-posta alanı benzersiz olmalı
                entity.HasIndex(u => u.Email).IsUnique();

                // Rol enum'ını veritabanında string olarak sakla
                entity.Property(u => u.Role)
                      .HasConversion<string>()
                      .HasMaxLength(50);
            });

            // --- University Yapılandırması ---
            modelBuilder.Entity<University>(entity =>
            {
                // Şehir alanına index ekle (filtreleme performansı için)
                entity.HasIndex(u => u.City);
            });

            // --- Club Yapılandırması ---
            modelBuilder.Entity<Club>(entity =>
            {
                // Club → University ilişkisi (Bir üniversitenin birden fazla kulübü olabilir)
                entity.HasOne(c => c.University)
                      .WithMany(u => u.Clubs)
                      .HasForeignKey(c => c.UniversityId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Club → Manager (User) ilişkisi (Bir kullanıcı birden fazla kulüp yönetebilir)
                entity.HasOne(c => c.Manager)
                      .WithMany(u => u.ManagedClubs)
                      .HasForeignKey(c => c.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict); // Yönetici silindiğinde kulüp silinmesin
            });

            // --- ClubMembership Yapılandırması ---
            modelBuilder.Entity<ClubMembership>(entity =>
            {
                // Durum enum'ını veritabanında string olarak sakla
                entity.Property(m => m.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // ClubMembership → User ilişkisi
                entity.HasOne(m => m.User)
                      .WithMany(u => u.Memberships)
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ClubMembership → Club ilişkisi
                entity.HasOne(m => m.Club)
                      .WithMany(c => c.Memberships)
                      .HasForeignKey(m => m.ClubId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Aynı kullanıcı aynı kulübe birden fazla başvuru yapmasın
                entity.HasIndex(m => new { m.UserId, m.ClubId }).IsUnique();
            });

            // --- Event Yapılandırması ---
            modelBuilder.Entity<Event>(entity =>
            {
                // Event → Club ilişkisi (Bir kulübün birden fazla etkinliği olabilir)
                entity.HasOne(e => e.Club)
                      .WithMany(c => c.Events)
                      .HasForeignKey(e => e.ClubId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- EventRequest Yapılandırması ---
            modelBuilder.Entity<EventRequest>(entity =>
            {
                // Durum enum'ını veritabanında string olarak sakla
                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50);

                // EventRequest → User ilişkisi
                entity.HasOne(r => r.User)
                      .WithMany(u => u.EventRequests)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // EventRequest → Event ilişkisi
                entity.HasOne(r => r.Event)
                      .WithMany(e => e.EventRequests)
                      .HasForeignKey(r => r.EventId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Aynı kullanıcı aynı etkinliğe birden fazla "Pending" veya "Approved" istek atmasını engellemek için
                // NOT: Bu veritabanı seviyesinde zordur (duruma bağlı), kod tarafında kontrol edeceğiz.
                // Ancak hızlı filtreleme için index ekleyebiliriz.
                entity.HasIndex(r => new { r.UserId, r.EventId });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
