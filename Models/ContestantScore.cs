using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TableTennis.Models
{
    /// <summary>
    /// Счёт спортсмена в игре.
    /// </summary>
    public sealed class ContestantScore
    {
        /// <summary>
        /// Guid спортсмена.
        /// </summary>
        public Guid ContestantGuid { get; }
        
        /// <summary>
        /// Счёт спортсмена.
        /// </summary>
        public int Score { get; }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contestant"></param>
        /// <param name="score"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ContestantScore(Contestant contestant, int score)
        {
            ContestantGuid = contestant.Guid;
            if (score < 0 || score > 11)
                throw new ArgumentOutOfRangeException(nameof(score));
            Score = score;
        }

        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public ContestantScore(Guid contestantGuid, int score)
        {
            ContestantGuid = contestantGuid;
            Score = score;
        }
    }
}