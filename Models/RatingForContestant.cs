using System;

namespace TableTennis.Models
{
    /// <summary>
    /// Актуальный рейтинг спортсмена.
    /// </summary>
    public sealed class RatingForContestant
    {
        public RatingForContestant(Guid contestantGuid, int score)
        {
            ContestantGuid = contestantGuid;
            Score = score;
        }

        /// <summary>
        /// Guid спортсмена.
        /// </summary>
        public Guid ContestantGuid { get; }
        
        /// <summary>
        /// Счёт спортсмена в рейтинге.
        /// </summary>
        public int Score { get; }
    }
}