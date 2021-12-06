using System;

namespace TableTennis.Models
{
    public sealed class RatingDeltaForGameResult
    {
        public RatingDeltaForGameResult(
            Guid gameResultGuid, 
            int firstContestantDelta, 
            int secondContestantDelta, 
            int firstContestantInitialScore, 
            int secondContestantInitialScore)
        {
            GameResultGuid = gameResultGuid;
            FirstContestantDelta = firstContestantDelta;
            SecondContestantDelta = secondContestantDelta;
            FirstContestantInitialScore = firstContestantInitialScore;
            SecondContestantInitialScore = secondContestantInitialScore;
        }

        public Guid GameResultGuid { get; }
        
        public int FirstContestantInitialScore { get; }
        
        public int SecondContestantInitialScore { get; }
        
        public int FirstContestantDelta { get; }
        
        public int SecondContestantDelta { get; }
    }
}