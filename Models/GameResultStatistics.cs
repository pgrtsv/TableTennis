using System;

namespace TableTennis.Models
{
    public sealed class GameResultStatistics
    {
        public GameResultStatistics(Guid gameResultGuid, int contestantsGamesCount, int firstContestantWins,
            int secondContestantWins, int firstContestantScore, int secondContestantScore)
        {
            GameResultGuid = gameResultGuid;
            ContestantsGamesCount = contestantsGamesCount;
            FirstContestantWins = firstContestantWins;
            SecondContestantWins = secondContestantWins;
            FirstContestantScore = firstContestantScore;
            SecondContestantScore = secondContestantScore;
            if (firstContestantWins + secondContestantWins != contestantsGamesCount)
                throw new InvalidOperationException();
        }

        public Guid GameResultGuid { get; }

        /// <summary>
        /// Количество матчей соперников до этого.
        /// </summary>
        public int ContestantsGamesCount { get; }

        public int FirstContestantWins { get; }

        public int SecondContestantWins { get; }

        public int FirstContestantScore { get; }

        public int SecondContestantScore { get; }
    }
}