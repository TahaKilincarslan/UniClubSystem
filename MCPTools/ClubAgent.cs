using System;
using System.Collections.Generic;
using System.Linq;
using UniversityClubSystem.Models;
using UniversityClubSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversityClubSystem.McpTools
{
    public class ClubAgent
    {
        private readonly AppDbContext _context;

        public ClubAgent(AppDbContext context)
        {
            _context = context;
        }

        // Aktif kulüpleri veritabanından çeker (üniversite bilgisiyle birlikte)
        public async Task<string> GetActiveClubs()
        {
            var clubs = await _context.Clubs
                .Include(c => c.University)
                .Select(c => $"{c.Name} ({c.University.Name})")
                .ToListAsync();

            if (!clubs.Any())
                return "Henüz sistemde kayıtlı aktif kulüp bulunmamaktadır.";

            return "Aktif Kulüpler: " + string.Join(", ", clubs) + ".";
        }

        // Etkinlik çakışması kontrolü (Aynı gün, aynı mekan ve aktif etkinlik)
        public async Task<bool> CheckEventConflict(DateTime eventDate, string location)
        {
            var conflict = await _context.Events.AnyAsync(e =>
                e.Date.Date == eventDate.Date &&
                e.Location.ToLower() == location.ToLower() &&
                e.IsActive);

            return conflict;
        }
    }
}
