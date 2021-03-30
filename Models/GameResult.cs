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
        public ContestantResult FirstContestantResult { get; }
        
        /// <summary>
        /// Счёт второго спортсмена.
        /// </summary>
        public ContestantResult SecondContestantResult { get; }
        
        /// <summary>
        /// Дата и время матча.
        /// </summary>
        public DateTime DateTime { get; }

        public GameResult(ContestantResult first, ContestantResult second)
        {
            Guid = Guid.NewGuid();
            FirstContestantResult = first;
            SecondContestantResult = second;
            DateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public GameResult(Guid guid, 
            ContestantResult firstContestantResult, 
            ContestantResult secondContestantResult, 
            DateTime dateTime)
        {
            Guid = guid;
            FirstContestantResult = firstContestantResult;
            SecondContestantResult = secondContestantResult;
            DateTime = dateTime;
        }

        public Guid GetWinnerGuid() => FirstContestantResult.Score > SecondContestantResult.Score
            ? FirstContestantResult.ContestantGuid
            : SecondContestantResult.ContestantGuid;

        public bool IsFirstContestantWinner() => FirstContestantResult.Score == 11;
            
        public bool DidContestantTakePart(Guid contestantGuid) =>
            FirstContestantResult.ContestantGuid == contestantGuid ||
            SecondContestantResult.ContestantGuid == contestantGuid;
    }
}