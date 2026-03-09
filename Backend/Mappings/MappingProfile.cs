using AutoMapper;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Club -> ClubDetailDto
            CreateMap<Club, ClubDetailDto>()
                .ForMember(dest => dest.UniversityName, opt => opt.MapFrom(src => src.University.Name))
                .ForMember(dest => dest.ManagerFullName, opt => opt.MapFrom(src => $"{src.Manager.FirstName} {src.Manager.LastName}"))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Memberships.Count(m => m.Status == MembershipStatus.Approved)))
                .ForMember(dest => dest.UpcomingEvents, opt => opt.MapFrom(src => src.Events.Where(e => e.IsActive && e.Date >= DateTime.UtcNow).OrderBy(e => e.Date)));

            // Event -> EventDto
            CreateMap<Event, EventDto>();
            
            // RegisterDto -> User
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // PasswordHash manual set edilecek (BCrypt)
        }
    }
}
