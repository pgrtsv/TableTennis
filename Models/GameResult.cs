using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TableTennis.Models
{
    /// <summary>
    /// Результаты матча.
    /// </summary>
    public sealed class GameResult
    {
        /// <summary>
        /// Guid матча.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Счёт первого спортсмена. 
        /// </summary>
        public ContestantScore FirstContestantScore { get; }
        
        /// <summary>
        /// Счёт второго спортсмена.
        /// </summary>
        public ContestantScore SecondContestantScore { get; }
        
        /// <summary>
        /// Дата и время матча.
        /// </summary>
        public DateTime DateTime { get; }

        public GameResult(ContestantScore first, ContestantScore second)
        {
            Guid = Guid.NewGuid();
            FirstContestantScore = first;
            SecondContestantScore = second;
            DateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public GameResult(Guid guid, 
            ContestantScore firstContestantScore, 
            ContestantScore secondContestantScore, 
            DateTime dateTime)
        {
            Guid = guid;
            FirstContestantScore = firstContestantScore;
            SecondContestantScore = secondContestantScore;
            DateTime = dateTime;
        }

        public Guid GetWinnerGuid() => FirstContestantScore.Score > SecondContestantScore.Score
            ? FirstContestantScore.ContestantGuid
            : SecondContestantScore.ContestantGuid;

        public bool IsFirstContestantWinner() => FirstContestantScore.Score == 11;
            
        public bool DidContestantTakePart(Guid contestantGuid) =>
            FirstContestantScore.ContestantGuid == contestantGuid ||
            SecondContestantScore.ContestantGuid == contestantGuid;
    }
}