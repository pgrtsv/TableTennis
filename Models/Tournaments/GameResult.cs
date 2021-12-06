using System;

namespace TableTennis.Models.Tournaments
{
    /// <summary>
    /// Результат матча турнира.
    /// </summary>
    public sealed class GameResult
    {
        public ContestantsPair Pair { get; }
        
        public int FirstContestantScore { get; }
        public int SecondContestantScore { get; }

        public GameResult(ContestantsPair pair, int firstContestantScore, int secondContestantScore)
        {
            if (!pair.IsFull())
                throw new ArgumentException(string.Empty, nameof(pair));
            Pair = pair;
            FirstContestantScore = firstContestantScore;
            SecondContestantScore = secondContestantScore;
        }
    }
}