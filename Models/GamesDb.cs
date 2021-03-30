using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DynamicData;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace TableTennis.Models
{
    public sealed class GamesDb
    {
        private readonly SourceCache<GameResult, Guid> _gamesResults = new SourceCache<GameResult, Guid>(x => x.Guid);
        private readonly SourceCache<Contestant, Guid> _contestants = new SourceCache<Contestant, Guid>(x => x.Guid);

        public IEnumerable<GameResult> GamesResults => _gamesResults.Items;
        public IEnumerable<Contestant> Contestants => _contestants.Items;

        public IObservable<IChangeSet<GameResult, Guid>> GamesResultsConnect() => _gamesResults.Connect();
        public IObservable<IChangeSet<GameResult, Guid>> GamesResultsPreview() => _gamesResults.Preview();
        public IObservable<IChangeSet<Contestant, Guid>> ContestantsConnect() => _contestants.Connect();
        public IObservable<IChangeSet<Contestant, Guid>> ContestantsPreview() => _contestants.Preview();


        public GamesDb()
        {
            _monthlyRating = new MonthlyRating(this);
        }
        
        [JsonConstructor]
        public GamesDb(IEnumerable<GameResult> gamesResults, IEnumerable<Contestant> contestants)
        {
            _gamesResults.AddOrUpdate(gamesResults);
            _contestants.AddOrUpdate(contestants);
            _monthlyRating = new MonthlyRating(this);
        }

        public void AddContestant(Contestant contestant)
        {
            _contestants.AddOrUpdate(contestant);
        }

        public void AddGameResult(GameResult gameResult)
        {
            if (gameResult.FirstContestantResult.ContestantGuid == gameResult.SecondContestantResult.ContestantGuid)
                throw new Exception();
            if (Contestants.All(x => x.Guid != gameResult.FirstContestantResult.ContestantGuid))
                throw new Exception();
            if (Contestants.All(x => x.Guid != gameResult.SecondContestantResult.ContestantGuid))
                throw new Exception();
            _gamesResults.AddOrUpdate(gameResult);
        }

        public IObservable<ContestantStatistics> StatisticsConnect(Guid contestantGuid)
        {
            if (Contestants.All(x => x.Guid != contestantGuid))
                throw new ArgumentException();
            return GamesResultsConnect()
                .Where(x => x.Any(change => change.Current.DidContestantTakePart(contestantGuid)))
                .StartWith(new ChangeSet<GameResult, Guid>())
                .Select(_ => ContestantStatistics.GetForContestant(this, contestantGuid));
        }

        public ContestantStatistics GetStatisticsForContestant(Guid contestantGuid)
        {
            if (Contestants.All(x => x.Guid != contestantGuid))
                throw new ArgumentException();
            return ContestantStatistics.GetForContestant(this, contestantGuid);
        }

        private readonly MonthlyRating _monthlyRating;
        public MonthlyRating GetMonthlyScoresDb() => _monthlyRating;
    }
}