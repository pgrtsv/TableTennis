using System;
using System.Linq;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class ContestantReadViewModel : ReactiveObject, IDisposable
    {
        public ContestantReadViewModel(GamesDb gamesDb, MonthlyRating monthlyRating, Contestant contestant)
        {
            if (gamesDb == null) throw new ArgumentNullException(nameof(gamesDb));
            if (monthlyRating == null) throw new ArgumentNullException(nameof(monthlyRating));

            Contestant = contestant ?? throw new ArgumentNullException(nameof(contestant));
            _statistics = gamesDb.StatisticsConnect(contestant.Guid)
                .ToProperty(this, nameof(Statistics));
            _monthlyScore = this.WhenAnyValue(x => x.Statistics,
                    selector: _ => monthlyRating.RatingsForContestants.First(x => x.ContestantGuid == contestant.Guid).Score)
                .ToProperty(this, nameof(contestant));
        }

        public Contestant Contestant { get; }

        public ContestantStatistics Statistics => _statistics.Value;

        public double MonthlyScore => _monthlyScore.Value;

        private readonly ObservableAsPropertyHelper<int> _monthlyScore;

        private readonly ObservableAsPropertyHelper<ContestantStatistics> _statistics;

        public void Dispose() => _statistics.Dispose();
    }
}