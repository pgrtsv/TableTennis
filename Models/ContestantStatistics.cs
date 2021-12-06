using System;
using System.Linq;

namespace TableTennis.Models
{
    /// <summary>
    /// Статистика матчей спортсмена.
    /// </summary>
    public sealed class ContestantStatistics
    {
        private ContestantStatistics(
            Guid contestantGuid, 
            int gamesTotal, 
            int wins, 
            int losses, 
            bool isCalibrated,
            double winTotalRatio,
            double winTotalRatioPercentage)
        {
            ContestantGuid = contestantGuid;
            GamesTotal = gamesTotal;
            Wins = wins;
            Losses = losses;
            IsCalibrated = isCalibrated;
            if (!IsCalibrated && winTotalRatio != 0 && winTotalRatioPercentage != 0)
                throw new Exception();
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
        /// Если <see cref="IsCalibrated"/> == false, <see cref="WinTotalRatio"/> == 0.
        /// </summary>
        public double WinTotalRatio { get; }

        /// <summary>
        /// Соотношение побед и всех матчей спортсмена (в процентах).
        /// Если <see cref="IsCalibrated"/> == false, <see cref="WinTotalRatioPercentage"/> == 0.
        /// </summary>
        public double WinTotalRatioPercentage { get; }
        
        /// <summary>
        /// true, если <see cref="GamesTotal"/> >= 10.
        /// </summary>
        public bool IsCalibrated { get; }
        
        /// <summary>
        /// Собирает и возвращает статистику для выбранного спортсмена.
        /// </summary>
        /// <param name="gamesDb">БД матчей.</param>
        /// <param name="contestantGuid">Guid спортсмена.</param>
        public static ContestantStatistics GetForContestant(GamesDb gamesDb, Guid contestantGuid)
        {
            var gamesWithContestant = gamesDb.GamesResults.Where(result => result.DidContestantTakePart(contestantGuid))
                .ToArray();
            var gamesTotal = gamesWithContestant.Length;
            var wins = gamesWithContestant.Count(result => result.GetWinnerGuid() == contestantGuid);
            var losses = gamesWithContestant.Length - wins;
            var isCalibrated = gamesTotal >= 10;
            var winTotalRatio = isCalibrated ? 1.0 * wins / gamesTotal : 0;
            var winTotalRatioPercentage = winTotalRatio * 100;
            return new ContestantStatistics(
                contestantGuid,
                gamesTotal,
                wins,
                losses,
                isCalibrated,
                winTotalRatio,
                winTotalRatioPercentage);
        }

        public static ContestantStatistics GetDefault(Guid contestantGuid) => new(
            contestantGuid,
            0,
            0,
            0,
            false,
            0,
            0);
    }
}