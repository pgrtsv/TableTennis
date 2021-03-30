using System;

namespace TableTennis.ViewModels
{
    public sealed class GameResultReadViewModel
    {
        public GameResultReadViewModel(
            Guid guid, 
            DateTime dateTime,
            Guid firstContestantGuid,
            string firstContestantName, 
            Guid secondContestantGuid,
            string secondContestantName, 
            int firstContestantScore, 
            int secondContestantScore, 
            int firstContestantDelta, 
            int secondContestantDelta, 
            int firstContestantInitialScore, 
            int secondContestantInitialScore)
        {
            Guid = guid;
            DateTime = dateTime;
            FirstContestantGuid = firstContestantGuid;
            FirstContestantName = firstContestantName;
            SecondContestantGuid = secondContestantGuid;
            SecondContestantName = secondContestantName;
            FirstContestantScore = firstContestantScore;
            SecondContestantScore = secondContestantScore;
            FirstContestantInitialScore = firstContestantInitialScore;
            SecondContestantInitialScore = secondContestantInitialScore;
            IsFirstContestantWinner = firstContestantScore > secondContestantScore;
            FirstContestantDelta = firstContestantDelta;
            SecondContestantDelta = secondContestantDelta;
            FirstContestantInfo = $"{firstContestantName} ({firstContestantInitialScore} {(IsFirstContestantWinner ? "+" : "-")}{Math.Abs(firstContestantDelta)})";
            SecondContestantInfo = $"{secondContestantName} ({secondContestantInitialScore} {(IsFirstContestantWinner ? "-" : "+")}{Math.Abs(secondContestantDelta)})";
        }

        public string SecondContestantInfo { get; }

        public string FirstContestantInfo { get; }

        public Guid Guid { get; }
        
        public DateTime DateTime { get; }
        public Guid FirstContestantGuid { get; }

        public string FirstContestantName { get; }
        public Guid SecondContestantGuid { get; }

        public string SecondContestantName { get; }
        
        public int FirstContestantScore { get; }
        
        public int SecondContestantScore { get; }
        
        public int FirstContestantInitialScore { get; }
        
        public int SecondContestantInitialScore { get; }
        
        public int FirstContestantDelta { get; }
        
        public int SecondContestantDelta { get; }
        
        public bool IsFirstContestantWinner { get; }
    }
}