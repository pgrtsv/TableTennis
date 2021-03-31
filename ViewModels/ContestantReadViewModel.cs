using System;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class ContestantReadViewModel : ReactiveObject, IDisposable
    {
        public ContestantReadViewModel(GamesDb gamesDb, Contestant contestant)
        {
            if (gamesDb == null) throw new ArgumentNullException(nameof(gamesDb));
            Contestant = contestant ?? throw new ArgumentNullException(nameof(contestant));
            _statistics = gamesDb.StatisticsConnect(contestant.Guid)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(Statistics));
            _monthlyScore = this.WhenAnyValue(x => x.Statistics,
                    selector: _ =>
                        gamesDb.GetMonthlyScoresDb().RatingsForContestants
                            .First(x => x.ContestantGuid == contestant.Guid).Score)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(contestant));
            _scorePosition = gamesDb.GetMonthlyScoresDb().RatingsForContestantsConnect()
                .QueryWhenChanged(ratings => ratings.Items
                    .OrderByDescending(x => x.Score)
                    .Count(x => x.Score > gamesDb.GetMonthlyScoresDb().RatingsForContestants
                        .First(y => y.ContestantGuid == contestant.Guid).Score) + 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(ScorePosition));
        }

        public Contestant Contestant { get; }
        public ContestantStatistics Statistics => _statistics.Value;
        public double MonthlyScore => _monthlyScore.Value;
        public int ScorePosition => _scorePosition.Value;

        private readonly ObservableAsPropertyHelper<int> _monthlyScore;
        private readonly ObservableAsPropertyHelper<ContestantStatistics> _statistics;
        private readonly ObservableAsPropertyHelper<int> _scorePosition;

        public void Dispose()
        {
            _statistics.Dispose();
            _monthlyScore.Dispose();
            _scorePosition.Dispose();
        }
    }
}