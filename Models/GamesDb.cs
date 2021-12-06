using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using System.Reactive.Linq;
using System.Text.Json.Serialization;

namespace TableTennis.Models
{
    /// <summary>
    /// БД матчей.
    /// </summary>
    public sealed class GamesDb
    {
        private readonly SourceCache<GameResult, Guid> _gamesResults = new(x => x.Guid);

        /// <summary>
        /// Теннисные матчи.
        /// </summary>
        public IEnumerable<GameResult> GamesResults => _gamesResults.Items;

        public GameResult GetGameResult(Guid guid) => _gamesResults.Lookup(guid).Value;

        public IObservable<IChangeSet<GameResult, Guid>> GamesResultsConnect() => _gamesResults.Connect();
        
        public IObservable<IChangeSet<GameResult, Guid>> GamesResultsPreview() => _gamesResults.Preview();

        public GamesDb()
        {
        }
        
        [JsonConstructor]
        public GamesDb(IEnumerable<GameResult> gamesResults)
        {
            _gamesResults.AddOrUpdate(gamesResults);
        }

        public void AddGameResult(GameResult gameResult, ContestantsDb contestantsDb)
        {
            if (gameResult.FirstContestantScore.ContestantGuid == gameResult.SecondContestantScore.ContestantGuid)
                throw new Exception();
            if (contestantsDb == null)
                throw new InvalidOperationException();
            if (contestantsDb.Contestants.All(x => x.Guid != gameResult.FirstContestantScore.ContestantGuid))
                throw new Exception();
            if (contestantsDb.Contestants.All(x => x.Guid != gameResult.SecondContestantScore.ContestantGuid))
                throw new Exception();
            _gamesResults.AddOrUpdate(gameResult);
        }

        public void RemoveGameResult(GameResult gameResult)
        {
            if (!_gamesResults.Lookup(gameResult.Guid).HasValue)
                throw new ArgumentException(string.Empty, nameof(gameResult));
            _gamesResults.Remove(gameResult);
        }

        public IObservable<ContestantStatistics> StatisticsConnect(Guid contestantGuid, ContestantsDb contestantsDb)
        {
            if (contestantsDb == null)
                throw new ArgumentNullException(string.Empty, nameof(contestantsDb));
            if (contestantsDb.Contestants.All(x => x.Guid != contestantGuid))
                throw new ArgumentException(string.Empty, nameof(contestantGuid));
            return GamesResultsConnect()
                .Where(x => x.Any(change => change.Current.DidContestantTakePart(contestantGuid)))
                .StartWith(new ChangeSet<GameResult, Guid>())
                .Select(_ => ContestantStatistics.GetForContestant(this, contestantGuid));
        }

        public ContestantStatistics GetStatisticsForContestant(Guid contestantGuid, ContestantsDb contestantsDb)
        {
            if (contestantsDb == null)
                throw new ArgumentNullException(string.Empty, nameof(contestantsDb));
            if (contestantsDb.Contestants.All(x => x.Guid != contestantGuid))
                throw new ArgumentException(string.Empty, nameof(contestantGuid));
            return ContestantStatistics.GetForContestant(this, contestantGuid);
        }
    }
}