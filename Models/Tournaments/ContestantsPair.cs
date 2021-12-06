using System;

namespace TableTennis.Models.Tournaments
{
    /// <summary>
    /// Пара мест для спортсменов в турнирной таблице. Места могут быть незаняты.
    /// </summary>
    public sealed class ContestantsPair
    {
        public Guid FirstContestantGuid { get; }
        public Guid SecondContestantGuid { get; }
        
        /// <exception cref="ArgumentException"></exception>
        public ContestantsPair(Guid firstContestantGuid, Guid secondContestantGuid)
        {
            if (firstContestantGuid == secondContestantGuid && firstContestantGuid != default)
                throw new ArgumentException(string.Empty, nameof(secondContestantGuid));
            
            FirstContestantGuid = firstContestantGuid;
            SecondContestantGuid = secondContestantGuid;
        }

        public bool IsFull() => FirstContestantGuid != default && SecondContestantGuid != default;
    }
}