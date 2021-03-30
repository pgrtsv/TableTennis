using System;

namespace TableTennis.Models
{
    public sealed class MonthlyRatingForContestant
    {
        public MonthlyRatingForContestant(Guid contestantGuid)
        {
            ContestantGuid = contestantGuid;
            Score = 0;
        }

        public Guid ContestantGuid { get; }
        
        public int Score { get; private set; }

        public void UpdateScore(int delta)
        {
            var newScore = Score + delta;
            if (newScore < 0) newScore = 0;
            Score = newScore;
        }
    }
}