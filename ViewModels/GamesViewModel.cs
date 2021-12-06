using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class GamesViewModel : ChildViewModelBase, IHasSorting
    {
        public sealed class SortViewModelsCollection
        {
            public SortViewModel<GameResultReadViewModel> DateTime = new("дате/времени матча", x => x.DateTime);

            public SortViewModel<GameResultReadViewModel> FirstContestant =
                new("ФИО первого спортсмена", x => x.FirstContestantName);

            public SortViewModel<GameResultReadViewModel> SecondContestant =
                new("ФИО второго спортсмена", x => x.SecondContestantName);

            public IEnumerable<SortViewModel<GameResultReadViewModel>> SortViewModels => new[]
            {
                DateTime,
                FirstContestant,
                SecondContestant
            };
        }

        public GamesViewModel(IObservable<RatingSystem> ratingSystemObservable)
        {
            SelectedSortViewModel = SortViewModels.DateTime;
            SelectedSortViewModel.IsDescending = true;
            FilterViewModel =
                new FilterGameResultsViewModel(
                    ratingSystemObservable.Select(ratingSystem => ratingSystem.ContestantsDb));
            var gameResultsShared = ratingSystemObservable
                .Select(ratingSystem => ratingSystem.GamesDb.GamesResultsConnect()
                    .Transform(result =>
                    {
                        var scoreDeltas = ratingSystem.GetRatingDeltaForGameResult(result.Guid);
                        var gameStatistics = ratingSystem.GetStatisticsForGameResult(result.Guid);
                        return new GameResultReadViewModel(
                            result.Guid,
                            result.DateTime,
                            result.FirstContestantScore.ContestantGuid,
                            ratingSystem.ContestantsDb.Contestants
                                .First(y => y.Guid == result.FirstContestantScore.ContestantGuid).Name
                                .ToString(),
                            result.SecondContestantScore.ContestantGuid,
                            ratingSystem.ContestantsDb.Contestants
                                .First(y => y.Guid == result.SecondContestantScore.ContestantGuid).Name
                                .ToString(),
                            result.FirstContestantScore.Score,
                            result.SecondContestantScore.Score,
                            scoreDeltas.FirstContestantDelta,
                            scoreDeltas.SecondContestantDelta,
                            scoreDeltas.FirstContestantInitialScore,
                            scoreDeltas.SecondContestantInitialScore,
                            gameStatistics.FirstContestantWins,
                            gameStatistics.SecondContestantWins,
                            gameStatistics.FirstContestantScore,
                            gameStatistics.SecondContestantScore);
                    }))
                .Switch()
                .Filter(FilterViewModel.GetFilter())
                .Sort(this.WhenAnyValue(x => x.SelectedSortViewModel)
                    .Select(x => x.GetObservable())
                    .Switch())
                .Publish();

            _pagesCount = CountEx.Count(gameResultsShared)
                .Select(count =>
                {
                    var pages = count / GameResultsOnPage;
                    if (pages == 0) return 1;
                    return count % GameResultsOnPage != 0
                        ? pages + 1
                        : pages;
                })
                .ToProperty(this, nameof(PagesCount));
            CurrentPage = 1;

            NextPage = ReactiveCommand.Create(() => { CurrentPage += 1; },
                this.WhenAnyValue(
                    x => x.CurrentPage,
                    x => x.PagesCount,
                    (page, count) => page < count - 1));

            PreviousPage = ReactiveCommand.Create(() => { CurrentPage -= 1; },
                this.WhenAnyValue(
                    x => x.CurrentPage,
                    page => page > 1));

            gameResultsShared
                .Page(this.WhenAnyValue(x => x.CurrentPage)
                    .Select(page => new PageRequest(page, GameResultsOnPage)))
                .Bind(out var gameResults)
                .Subscribe();
            GameResults = gameResults;
            AddGameResultViewModel = new AddGameResultViewModel(
                ratingSystemObservable.Select(x => x.GamesDb),
                ratingSystemObservable.Select(x => x.ContestantsDb));

            gameResultsShared.Connect();

            this.WhenActivated(cleanUp =>
            {
                this.WhenAnyValue(x => x.IsSorting)
                    .Where(x => x)
                    .Subscribe(_ => IsAddingGameResult = false)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.IsFiltering)
                    .Where(x => x)
                    .Subscribe(_ => IsAddingGameResult = false)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.IsAddingGameResult)
                    .Where(x => x)
                    .Subscribe(_ =>
                    {
                        IsSorting = false;
                        IsFiltering = false;
                    })
                    .DisposeWith(cleanUp);
                AddGameResultViewModel.AddGameResult
                    .Subscribe(_ => IsAddingGameResult = false)
                    .DisposeWith(cleanUp);
            });
        }

        public AddGameResultViewModel AddGameResultViewModel { get; }

        private readonly ObservableAsPropertyHelper<int> _pagesCount;

        public int PagesCount => _pagesCount.Value;

        [Reactive] public int CurrentPage { get; private set; }

        public ReactiveCommand<Unit, Unit> NextPage { get; }
        public ReactiveCommand<Unit, Unit> PreviousPage { get; }

        public const int GameResultsOnPage = 50;

        public ReadOnlyObservableCollection<GameResultReadViewModel> GameResults { get; }

        public FilterGameResultsViewModel FilterViewModel { get; }

        [Reactive] public bool IsAddingGameResult { get; set; }

        [Reactive] public bool IsSorting { get; set; }

        [Reactive] public bool IsFiltering { get; set; }

        public override string Name { get; } = "Матчи";

        public SortViewModelsCollection SortViewModels { get; } = new();

        IEnumerable<ISortViewModel> IHasSorting.SortViewModels => SortViewModels.SortViewModels;

        [Reactive] public SortViewModel<GameResultReadViewModel> SelectedSortViewModel { get; set; }

        ISortViewModel IHasSorting.SelectedSortViewModel
        {
            get => SelectedSortViewModel;
            set => SelectedSortViewModel = (SortViewModel<GameResultReadViewModel>) value;
        }
    }
}