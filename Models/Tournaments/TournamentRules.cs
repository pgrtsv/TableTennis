using System;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace TableTennis.Models.Tournaments
{
    /// <summary>
    /// Правила турнира.
    /// </summary>
    public sealed class TournamentRules
    {
        public TournamentRules()
        {
        }

        [JsonConstructor, UsedImplicitly]
        public TournamentRules(int winsCount, TimeSpan maxPreparationTime, TimeSpan maxGameTime)
        {
            _winsCount = new BehaviorSubject<int>(winsCount);
            _maxPreparationTime = new BehaviorSubject<TimeSpan>(maxPreparationTime);
            _maxGameTime = new BehaviorSubject<TimeSpan>(maxGameTime);
        }

        private readonly BehaviorSubject<int> _winsCount;
        
        /// <summary>
        /// Число побед, которое должен одержать спортсмен для победы в паре, в матчах. 
        /// </summary>
        public int WinsCount => _winsCount.Value;
        
        private readonly BehaviorSubject<TimeSpan> _maxPreparationTime;
        private readonly BehaviorSubject<TimeSpan> _maxGameTime;

        /// <summary>
        /// Наибольшее время для подготовки пары спортсменов перед матчами.
        /// </summary>
        public TimeSpan MaxPreparationTime => _maxPreparationTime.Value;

        /// <summary>
        /// Наибольшее время, которое может длиться матч.
        /// </summary>
        public TimeSpan MaxGameTime => _maxGameTime.Value;

        /// <summary>
        /// Наибольшее количество спортсменов, которые могут участвовать в турнире.
        /// </summary>
        public int MaxContestantsCount() => 16;

        public int MinContestantsCount() => 4;

        /// <summary>
        /// Наименьшее количество спортсменов, которое необходимо для проведения матчей первой волны вместо
        /// автоперехода на следующую.
        /// </summary>
        public int FirstWaveMinCount() => MaxContestantsCount() / 2;
    }
}