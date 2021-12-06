using System;
using FluentValidation;

namespace TableTennis.Models.Tournaments.Validators
{
    public sealed class TournamentRulesValidator : AbstractValidator<TournamentRules>
    {
        public TournamentRulesValidator()
        {
            RuleFor(x => x.WinsCount)
                .GreaterThan(0)
                .LessThan(10);
            RuleFor(x => x.MaxGameTime)
                .GreaterThan(TimeSpan.Zero)
                .LessThanOrEqualTo(TimeSpan.FromMinutes(15));
            RuleFor(x => x.MaxPreparationTime)
                .GreaterThan(TimeSpan.Zero)
                .LessThanOrEqualTo(TimeSpan.FromMinutes(30));
        }
    }
}