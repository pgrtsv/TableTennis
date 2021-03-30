using System;
using System.Linq;

namespace TableTennis.Models
{
    /// <summary>
    /// Статистика матчей спортсмена.
    /// </summary>
    public sealed class ContestantStatistics
    {
        private ContestantStatistics(Guid contestantGuid, int gamesTotal, int wins, int losses, double winLossRatio,
            double winLossRationPercentage)
        {
            ContestantGuid = contestantGuid;
            GamesTotal = gamesTotal;
            Wins = wins;
            Losses = losses;
            WinLossRatio = winLossRatio;
            WinLossRatioPercentage = winLossRationPercentage;
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
        /// Соотношение побед и поражений спортсмена.
        /// </summary>
        public double WinLossRatio { get; }
        
        /// <summary>
        /// Соотношение побед и поражений спортсмена (в процентах).
        /// </summary>
        public double WinLossRatioPercentage { get; }
        
        public static ContestantStatistics GetForContestant(GamesDb gamesDb, Guid contestantGuid)
        {
            var gamesWithContestant = gamesDb.GamesResults.Where(result => result.DidContestantTakePart(contestantGuid))
                .ToArray();
            var gamesTotal = gamesWithContestant.Length;
            var wins = gamesWithContestant.Count(result => result.GetWinnerGuid() == contestantGuid);
            var losses = gamesWithContestant.Length - wins;
            var winLossRatio = losses == 0 ? wins : 1.0 * wins / losses;
            var winLossRationPercentage = winLossRatio * 100;
            return new ContestantStatistics(
                contestantGuid,
                gamesTotal,
                wins,
                losses,
                winLossRatio,
                winLossRationPercentage);
        }

        public override string ToString() => $"Матчей: {GamesTotal}, побед: {Wins}, поражений: {Losses}, W/L: {WinLossRatio:F}";
    }
}