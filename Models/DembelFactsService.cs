using System;
using System.Linq;
using System.Text;

namespace TableTennis.Models
{
    public static class DembelFactsService
    {
        public static string GetRandomFact(ContestantsDb contestantsDb)
        {
            var contestant = contestantsDb.GetRandomContestantWithRequiredData();
            if (contestant == null) return "Не хватает данных!";
            var dembelDateTime = contestant.BecamePrivateDateTime.AddYears(1);
            var timeLeft = dembelDateTime - DateTime.Now;
            if (timeLeft < TimeSpan.Zero) return $"{contestant.Rank.Name} {contestant.Name} уже дембельнулся!";
            var builder = new StringBuilder();
            builder.Append(
                $"{contestant.Rank.Name} {contestant.Name.LastName} {contestant.Name.FirstName}, до дембеля тебе осталось ");
            builder.Append(GetRandomFactType() switch
            {
                FactType.Days => $"{(int)timeLeft.TotalDays} дней!",
                FactType.Pelmens => $"{(int) timeLeft.TotalDays / 7 * 2} тарелок пельменей!",
                FactType.WearChanges => $"{(int) timeLeft.TotalDays / 7} смен постельного белья!",
                FactType.VitruskiOdeyal => $"{(int) timeLeft.TotalDays / 7} вытрусок одеял!",
                FactType.Eatings => $"{(int) (timeLeft.TotalDays / 7 * 3)} приёмов пищи!",
                _ => throw new ArgumentOutOfRangeException()
            });
            return builder.ToString();
        }
        
        private enum FactType
        {
            Days,
            Pelmens,
            WearChanges,
            VitruskiOdeyal,
            Eatings,
        }

        private static FactType GetRandomFactType() => (FactType) new Random().Next(0, 4);
        
        private static Contestant? GetRandomContestantWithRequiredData(this ContestantsDb contestantsDb)
        {
            var contestants = contestantsDb.Contestants.Where(x => (x.Rank == MilitaryRank.Private
                                                              || x.Rank == MilitaryRank.LanceCorporal) &&
                                                             x.BecamePrivateDateTime != default &&
                                                             x.BecamePrivateDateTime != new DateTime(2020, 1, 1))
                .ToArray();
            return contestants.Length == 0 ? null : contestants[new Random().Next(0, contestants.Length)];
        }
    }
}