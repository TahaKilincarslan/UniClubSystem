using FluentValidation;
using UniversityClubSystem.Models;

namespace UniversityClubSystem.Validators
{
    public class ClubValidator : AbstractValidator<Club>
    {
        public ClubValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
            RuleFor(x => x.UniversityId).GreaterThan(0);
            RuleFor(x => x.ManagerId).GreaterThan(0);
        }
    }

    public class EventValidator : AbstractValidator<Event>
    {
        public EventValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.UtcNow);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
            RuleFor(x => x.ClubId).GreaterThan(0);
        }
    }
}
