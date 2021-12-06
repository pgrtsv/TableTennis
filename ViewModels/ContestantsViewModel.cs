using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public sealed class SortViewModelsCollection
        {
            public SortViewModel<ContestantReadViewModel> Rank { get; } = new("званию", x => x.Contestant.Rank);
            public SortViewModel<ContestantReadViewModel> Name { get; } = new("ФИО", x => x.Contestant.Name);

            public SortViewModel<ContestantReadViewModel> GamesTotal { get; } =
                new("количеству матчей", x => x.Statistics.GamesTotal);

            public SortViewModel<ContestantReadViewModel> Wins { get; } =
                new("количеству побед", x => x.Statistics.Wins);

            public SortViewModel<ContestantReadViewModel> Losses { get; } =
                new("количеству поражений", x => x.Statistics.Losses);

            public SortViewModel<ContestantReadViewModel> WinTotalRatio { get; } =
                new("соотношению W/T", x => x.Statistics.WinTotalRatio);

            public SortViewModel<ContestantReadViewModel> MonthlyScore { get; } = new("рейтингу", x => x.Rating);

            public IEnumerable<SortViewModel<ContestantReadViewModel>> GetAll() => new[]
            {
                Rank,
                Name,
                GamesTotal,
                Wins,
                Losses,
                WinTotalRatio,
                MonthlyScore
            };
        }

        public AppCommands AppCommands { get; }

        public ContestantsViewModel(
            IObservable<RatingSystem> ratingSystemObservable,
            AppCommands appCommands)
        {
            AppCommands = appCommands;
            AddContestantViewModel = new AddContestantViewModel(
                ratingSystemObservable.Select(x => x.ContestantsDb),
                ratingSystemObservable.Select(x => x.GamesDb));
            SortViewModels = new();
            SelectedSortViewModel = SortViewModels.MonthlyScore;
            SelectedSortViewModel.IsDescending = true;
            ratingSystemObservable.Select(ratingSystem => ratingSystem.ContestantsDb.ContestantsConnect()
                    .Transform(contestant => new ContestantReadViewModel(ratingSystem, contestant))
                    .AutoRefresh()
                    .DisposeMany())
                .Switch()
                // .Filter(x => x.Statistics.GamesTotal > 0)
                .Sort(this.WhenAnyValue(x => x.SelectedSortViewModel)
                    .StartWith(SelectedSortViewModel)
                    .Select(sortViewModel => sortViewModel.GetObservable())
                    .Switch())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var contestants)
                .Subscribe();
            Contestants = contestants;
            _canShowDateTimePicker = this.WhenAnyValue(x => x.SelectedContestant)
                .Select(contestant => contestant != null &&
                                      (contestant.Contestant.Rank == MilitaryRank.Private ||
                                       contestant.Contestant.Rank == MilitaryRank.LanceCorporal))
                .ToProperty(this, nameof(CanShowDateTimePicker));

            this.WhenActivated(cleanUp =>
            {
                AddContestantViewModel.Add
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => IsAddingContestant = false)
                    .DisposeWith(cleanUp);

                this.WhenAnyValue(x => x.IsAddingContestant)
                    .Where(x => x)
                    .Subscribe(_ =>
                    {
                        IsShowingSortPanel = false;
                        IsShowingDateTimePicker = false;
                    })
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.IsShowingSortPanel)
                    .Where(x => x)
                    .Subscribe(_ => IsAddingContestant = false)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.IsShowingDateTimePicker)
                    .Where(x => x)
                    .Subscribe(_ => IsAddingContestant = false)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.CanShowDateTimePicker)
                    .Where(x => !x)
                    .Subscribe(_ => IsShowingDateTimePicker = false)
                    .DisposeWith(cleanUp);
            });
        }

        public ReadOnlyObservableCollection<ContestantReadViewModel> Contestants { get; }
        public AddContestantViewModel AddContestantViewModel { get; }
        public SortViewModelsCollection SortViewModels { get; }
        IEnumerable<ISortViewModel> IHasSorting.SortViewModels => SortViewModels.GetAll();
        [Reactive] public SortViewModel<ContestantReadViewModel> SelectedSortViewModel { get; set; }

        ISortViewModel IHasSorting.SelectedSortViewModel
        {
            get => SelectedSortViewModel;
            set => SelectedSortViewModel = (SortViewModel<ContestantReadViewModel>) value;
        }

        [Reactive] public bool IsAddingContestant { get; set; }
        [Reactive] public bool IsShowingSortPanel { get; set; }
        [Reactive] public bool IsShowingDateTimePicker { get; set; }
        private readonly ObservableAsPropertyHelper<bool> _canShowDateTimePicker;
        public bool CanShowDateTimePicker => _canShowDateTimePicker.Value;

        [Reactive] public ContestantReadViewModel? SelectedContestant { get; set; }

        public override string Name { get; } = "Спортсмены";
    }
}