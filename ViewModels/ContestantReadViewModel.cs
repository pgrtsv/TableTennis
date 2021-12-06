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
        public ContestantReadViewModel(
            RatingSystem ratingSystem, 
            Contestant contestant)
        {
            if (ratingSystem == null) throw new ArgumentNullException(nameof(ratingSystem));
            var gamesDb = ratingSystem.GamesDb;
            var contestantsDb = ratingSystem.ContestantsDb;
            Contestant = contestant ?? throw new ArgumentNullException(nameof(contestant));
            _statistics = gamesDb.StatisticsConnect(contestant.Guid, contestantsDb)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(Statistics), ContestantStatistics.GetDefault(contestant.Guid));
            _rating = ratingSystem
                .RatingsForContestantsConnect()
                .Filter(rating => rating.ContestantGuid == contestant.Guid)
                .QueryWhenChanged(set => set.Items.First().Score)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(Rating));
            _winTotalRatioPercentage = this.WhenAnyValue(x => x.Statistics,
                statistics =>
                    statistics.IsCalibrated 
                        ? $"{statistics.WinTotalRatioPercentage:F1}%"
                        : "-")
                .ToProperty(this, nameof(WinTotalRatioPercentage));
            _scorePosition = ratingSystem
                .RatingsForContestantsConnect()
                .QueryWhenChanged(ratings => ratings.Items
                    .OrderByDescending(x => x.Score)
                    .Count(x => x.Score > ratingSystem.RatingsForContestants
                        .First(y => y.ContestantGuid == contestant.Guid).Score) + 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(Rating));
        }

        public Contestant Contestant { get; }

        public ContestantStatistics Statistics => _statistics.Value;

        private readonly ObservableAsPropertyHelper<string> _winTotalRatioPercentage;
        public string WinTotalRatioPercentage => _winTotalRatioPercentage.Value;
        
        private readonly ObservableAsPropertyHelper<int> _rating;
        public int Rating => _rating.Value;
        public int ScorePosition => _scorePosition.Value;

        private readonly ObservableAsPropertyHelper<ContestantStatistics> _statistics;
        private readonly ObservableAsPropertyHelper<int> _scorePosition;

        public void Dispose()
        {
            _statistics.Dispose();
            _rating.Dispose();
            _scorePosition.Dispose();
        }
    }
}