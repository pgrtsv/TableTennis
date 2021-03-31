using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
            SelectedSortViewModel.IsDescending = true;
            gamesDb.Select(db => db.ContestantsConnect()
                    .Transform(contestant => new ContestantReadViewModel(db, contestant))
                    .AutoRefresh()
                    .DisposeMany())
                .Switch()
                .Filter(x => x.Statistics?.GamesTotal > 0)
                .Sort(this.WhenAnyValue(x => x.SelectedSortViewModel)
                    .StartWith(SelectedSortViewModel)
                    .Select(sortViewModel => sortViewModel.GetObservable())
                    .Switch())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var contestants)
                .Subscribe();
            Contestants = contestants;

            this.WhenActivated(cleanUp =>
            {
                AddContestantViewModel.Add
                    .ObserveOn(RxApp.MainThreadScheduler)
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

        public ReadOnlyObservableCollection<ContestantReadViewModel> Contestants { get; }
        public AddContestantViewModel AddContestantViewModel { get; }
        public IEnumerable<SortViewModel<ContestantReadViewModel>> SortViewModels { get; }
        IEnumerable<ISortViewModel> IHasSorting.SortViewModels => SortViewModels;
        [Reactive] public SortViewModel<ContestantReadViewModel> SelectedSortViewModel { get; set; }

        ISortViewModel IHasSorting.SelectedSortViewModel
        {
            get => SelectedSortViewModel;
            set => SelectedSortViewModel = (SortViewModel<ContestantReadViewModel>) value;
        }

        [Reactive] public bool IsAddingContestant { get; set; }
        [Reactive] public bool IsShowingSortPanel { get; set; }


        public override string Name { get; } = "Спортсмены";
    }
}