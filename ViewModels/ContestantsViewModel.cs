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
    public sealed class ContestantsViewModel : ChildViewModelBase, IHasSorting
    {
        public AppCommands AppCommands { get; }

        public ContestantsViewModel(IObservable<GamesDb> gamesDb, AppCommands appCommands)
        {
            AppCommands = appCommands;
            AddContestantViewModel = new AddContestantViewModel(gamesDb);
            SortViewModels = new[]
            {
                new SortViewModel<ContestantReadViewModel>("званию", x => x.Contestant.Rank),
                new SortViewModel<ContestantReadViewModel>("ФИО", x => x.Contestant.Name),
                new SortViewModel<ContestantReadViewModel>("количеству матчей", x => x.Statistics.GamesTotal),
                new SortViewModel<ContestantReadViewModel>("количеству побед", x => x.Statistics.Wins),
                new SortViewModel<ContestantReadViewModel>("количеству поражений", x => x.Statistics.Losses),
                new SortViewModel<ContestantReadViewModel>("соотношению W/T", x => x.Statistics.WinTotalRatio),
                new SortViewModel<ContestantReadViewModel>("рейтингу", x => x.MonthlyScore)
            };
            SelectedSortViewModel = SortViewModels.Last();
            this.WhenActivated(cleanUp =>
            {
                gamesDb.Select(db => db.ContestantsConnect()
                        .Transform(contestant => new ContestantReadViewModel(db, db.GetMonthlyScoresDb(), contestant)))
                    .Switch()
                    .Sort(this.WhenAnyValue(x => x.SelectedSortViewModel)
                        .StartWith(SelectedSortViewModel)
                        .Select(sortViewModel => sortViewModel.GetObservable())
                        .Switch())
                    .Bind(out var contestants)
                    .Subscribe()
                    .DisposeWith(cleanUp);
                Contestants = contestants;
                AddContestantViewModel.Add
                    .Subscribe(_ => IsAddingContestant = false)
                    .DisposeWith(cleanUp);

                this.WhenAnyValue(x => x.IsAddingContestant)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingSortPanel = false)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.IsShowingSortPanel)
                    .Where(x => x)
                    .Subscribe(_ => IsAddingContestant = false)
                    .DisposeWith(cleanUp);
            });
        }

        public ReadOnlyObservableCollection<ContestantReadViewModel> Contestants { get; private set; }

        public AddContestantViewModel AddContestantViewModel { get; }

        public IEnumerable<SortViewModel<ContestantReadViewModel>> SortViewModels { get; }

        IEnumerable<ISortViewModel> IHasSorting.SortViewModels => SortViewModels;

        public SortViewModel<ContestantReadViewModel> SelectedSortViewModel
        {
            get => _selectedSortViewModel;
            set => this.RaiseAndSetIfChanged(ref _selectedSortViewModel, value);
        }

        ISortViewModel IHasSorting.SelectedSortViewModel
        {
            get => SelectedSortViewModel;
            set => SelectedSortViewModel = (SortViewModel<ContestantReadViewModel>) value;
        }

        private bool _isAddingContestant;
        private SortViewModel<ContestantReadViewModel> _selectedSortViewModel;
        private bool _isShowingSortPanel;

        public bool IsAddingContestant
        {
            get => _isAddingContestant;
            set => this.RaiseAndSetIfChanged(ref _isAddingContestant, value);
        }

        public bool IsShowingSortPanel
        {
            get => _isShowingSortPanel;
            set => this.RaiseAndSetIfChanged(ref _isShowingSortPanel, value);
        }


        public override string Name { get; } = "Спортсмены";
    }
}