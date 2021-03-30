using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class GamesViewModel : ChildViewModelBase, IHasSorting
    {
        private bool _isAddingGameResult;
        private bool _isSorting;
        private SortViewModel<GameResultReadViewModel> _selectedSortViewModel;
        private bool _isFiltering;

        public GamesViewModel(IObservable<GamesDb> gamesDb)
        {
            _selectedSortViewModel = SortViewModels.First();
            _selectedSortViewModel.IsDescending = true;
            FilterViewModel = new FilterGameResultsViewModel(gamesDb);
            gamesDb.Select(db => db.GamesResultsConnect()
                    .Transform(result =>
                    {
                        var scoreDeltas = db.GetMonthlyScoresDb().RatingsForGameResults
                            .First(x => x.GameResultGuid == result.Guid);
                        return new GameResultReadViewModel(
                            result.Guid,
                            result.DateTime,
                            result.FirstContestantResult.ContestantGuid,
                            db.Contestants.First(y => y.Guid == result.FirstContestantResult.ContestantGuid).Name
                                .ToString(),
                            result.SecondContestantResult.ContestantGuid,
                            db.Contestants.First(y => y.Guid == result.SecondContestantResult.ContestantGuid).Name
                                .ToString(),
                            result.FirstContestantResult.Score,
                            result.SecondContestantResult.Score,
                            scoreDeltas.FirstContestantDelta,
                            scoreDeltas.SecondContestantDelta,
                            scoreDeltas.FirstContestantInitialScore,
                            scoreDeltas.SecondContestantInitialScore);
                    }))
                .Switch()
                .Filter(FilterViewModel.GetFilter())
                .Sort(this.WhenAnyValue(x => x.SelectedSortViewModel)
                    .Select(x => x.GetObservable())
                    .Switch())
                .Bind(out var gameResults)
                .Subscribe();
            GameResults = gameResults;
            AddGameResultViewModel = new AddGameResultViewModel(gamesDb);

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

        public ReadOnlyObservableCollection<GameResultReadViewModel> GameResults { get; }

        public FilterGameResultsViewModel FilterViewModel { get; }

        public bool IsAddingGameResult
        {
            get => _isAddingGameResult;
            set => this.RaiseAndSetIfChanged(ref _isAddingGameResult, value);
        }

        public bool IsSorting
        {
            get => _isSorting;
            set => this.RaiseAndSetIfChanged(ref _isSorting, value);
        }

        public bool IsFiltering
        {
            get => _isFiltering;
            set => this.RaiseAndSetIfChanged(ref _isFiltering, value);
        }

        public override string Name { get; } = "Матчи";

        public IEnumerable<SortViewModel<GameResultReadViewModel>> SortViewModels { get; } = new[]
        {
            new SortViewModel<GameResultReadViewModel>("дате/времени матча", x => x.DateTime),
            new SortViewModel<GameResultReadViewModel>("ФИО первого спортсмена", x => x.FirstContestantName),
            new SortViewModel<GameResultReadViewModel>("ФИО второго спортсмена", x => x.SecondContestantName),
        };

        IEnumerable<ISortViewModel> IHasSorting.SortViewModels => SortViewModels;

        public SortViewModel<GameResultReadViewModel> SelectedSortViewModel
        {
            get => _selectedSortViewModel;
            set => this.RaiseAndSetIfChanged(ref _selectedSortViewModel, value);
        }

        ISortViewModel IHasSorting.SelectedSortViewModel
        {
            get => SelectedSortViewModel;
            set => SelectedSortViewModel = (SortViewModel<GameResultReadViewModel>) value;
        }
    }
}