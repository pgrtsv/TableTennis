using System;
using System.Linq;

namespace TableTennis.Models
{
    /// <summary>
    /// Статистика матчей спортсмена.
    /// </summary>
    public sealed class ContestantStatistics
    {
        private ContestantStatistics(Guid contestantGuid, int gamesTotal, int wins, int losses, double winTotalRatio,
            double winTotalRatioPercentage)
        {
            ContestantGuid = contestantGuid;
            GamesTotal = gamesTotal;
            Wins = wins;
            Losses = losses;
            WinTotalRatio = winTotalRatio;
            WinTotalRatioPercentage = winTotalRatioPercentage;
        }

        /// <summary>
        /// Guid спортсмена.
        /// </summary>
        public Guid ContestantGuid { get; }
        
        /// <summary>
        /// Количество сыгранных спортсменом матчей.
        /// </summary>
        public int GamesTotal { get; }
        
        /// <summary>
        /// Количество побед спортсмена.
        /// </summary>
        public int Wins { get; }
        
        /// <summary>
        /// Количество поражений спортсмена.
        /// </summary>
        public int Losses { get; }
        
        /// <summary>
        /// Соотношение побед и всех матчей спортсмена.
        /// </summary>
        public double WinTotalRatio { get; }
        
        /// <summary>
        /// Соотношение побед и всех матчей спортсмена (в процентах).
        /// </summary>
        public double WinTotalRatioPercentage { get; }
        
        public static ContestantStatistics GetForContestant(GamesDb gamesDb, Guid contestantGuid)
        {
            var gamesWithContestant = gamesDb.GamesResults.Where(result => result.DidContestantTakePart(contestantGuid))
                .ToArray();
            var gamesTotal = gamesWithContestant.Length;
            var wins = gamesWithContestant.Count(result => result.GetWinnerGuid() == contestantGuid);
            var losses = gamesWithContestant.Length - wins;
            var winTotalRatio = gamesTotal == 0 ? 0 : 1.0 * wins / gamesTotal;
            var winTotalRatioPercentage = winTotalRatio * 100;
            return new ContestantStatistics(
                contestantGuid,
                gamesTotal,
                wins,
                losses,
                winTotalRatio,
                winTotalRatioPercentage);
        }

        public override string ToString() => $"Матчей: {GamesTotal}, побед: {Wins}, поражений: {Losses}, W/T: {WinTotalRatio:F}";
    }
}