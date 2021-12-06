using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;
        public GamesDb GamesDb => _gamesDb.Value;

        private readonly ObservableAsPropertyHelper<ContestantsDb> _contestantsDb;
        public ContestantsDb ContestantsDb => _contestantsDb.Value;

        public DbProvider DbProvider { get; }

        private readonly ChildViewModelBase[] _viewModels;

        public IEnumerable<ChildViewModelBase> ViewModels => _viewModels;

        private ChildViewModelBase _selectedViewModel;

        public ChildViewModelBase SelectedViewModel
        {
            get => _selectedViewModel;
            set => this.RaiseAndSetIfChanged(ref _selectedViewModel, value);
        }

        private readonly ObservableAsPropertyHelper<DateTime> _dateTime;
        public DateTime DateTime => _dateTime.Value;

        private readonly ObservableAsPropertyHelper<DateTime> _breakfastTime;
        public DateTime BreakfastTime => _breakfastTime.Value;

        private readonly ObservableAsPropertyHelper<DateTime> _dinnerTime;
        public DateTime DinnerTime => _dinnerTime.Value;

        private readonly ObservableAsPropertyHelper<DateTime> _supperTime;
        public DateTime SupperTime => _supperTime.Value;

        private readonly ObservableAsPropertyHelper<string> _eatingTimeInfo;
        public string EatingTimeInfo => _eatingTimeInfo.Value;

        private readonly ObservableAsPropertyHelper<string> _dembelFact;
        public string DembelFact => _dembelFact.Value;

        [Reactive] public bool IsShowingDembelFact { get; set; }

        public ReactiveCommand<Unit, Unit> CloseDembelFact { get; }

        public SettingsViewModel SettingsViewModel { get; }

        public MainWindowViewModel()
        {
            DbProvider = new DbProvider();
            DbProvider.LoadOrNew();
            // DbProvider.Bench();
            var ratingSystem = Observable.CombineLatest(
                    DbProvider.ContestantsDb,
                    DbProvider.GamesDb,
                    (contestantsDb, gamesDb) => contestantsDb == null || gamesDb == null
                        ? throw new InvalidOperationException()
                        : new RatingSystem(
                            new BasicRatingMethod(),
                            gamesDb,
                            contestantsDb))
                .Publish();
            _gamesDb = DbProvider.GamesDb
                .ToProperty(this, nameof(GamesDb))!;
            _contestantsDb = DbProvider.ContestantsDb
                .ToProperty(this, nameof(ContestantsDb))!;
            var gamesViewModel = new GamesViewModel(ratingSystem);
            var contestantViewModel = new ContestantsViewModel(
                ratingSystem,
                new AppCommands(
                    ReactiveCommand.Create<Guid>(contestantGuid =>
                    {
                        SelectedViewModel = gamesViewModel;
                        gamesViewModel.IsFiltering = true;
                        gamesViewModel.FilterViewModel.IsFilteringByContestant = true;
                        gamesViewModel.FilterViewModel.SelectedContestant =
                            gamesViewModel.FilterViewModel.Contestants.First(x => x.Guid == contestantGuid);
                    })));
            SettingsViewModel = new SettingsViewModel();
            _viewModels = new ChildViewModelBase[]
            {
                contestantViewModel,
                gamesViewModel,
                SettingsViewModel,
                new TournamentViewModel(),
                // new StatisticsViewModel(DbProvider.GamesDb!)
            };
            _selectedViewModel = contestantViewModel;
            _dateTime = Observable.Interval(TimeSpan.FromSeconds(1))
                .StartWith(0)
                .Select(_ => DateTime.Now)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(DateTime));
            _breakfastTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetBreakfastTime(DateTime.Today).Subtract(TimeSpan.FromMinutes(10)))
                .ToProperty(this, nameof(BreakfastTime));
            _dinnerTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetDinnerTime(DateTime.Today).Subtract(TimeSpan.FromMinutes(10)))
                .ToProperty(this, nameof(DinnerTime));
            _supperTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetSupperTime(DateTime.Today).Subtract(TimeSpan.FromMinutes(10)))
                .ToProperty(this, nameof(SupperTime));
            _eatingTimeInfo = this.WhenAnyValue(
                    x => x.BreakfastTime,
                    x => x.DinnerTime,
                    x => x.SupperTime,
                    (breakfast, dinner, supper) =>
                        $"Построение на завтрак: {breakfast:HH:mm}, обед: {dinner:HH:mm}, ужин: {supper:HH:mm}")
                .ToProperty(this, nameof(EatingTimeInfo));
            _dembelFact = Observable.Interval(TimeSpan.FromMinutes(5))
                .StartWith(0)
                .Select(_ => DembelFactsService.GetRandomFact(ContestantsDb))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(DembelFact));
            IsShowingDembelFact = true;
            CloseDembelFact = ReactiveCommand.Create(() => { IsShowingDembelFact = false; });
            ratingSystem.Connect();
            this.WhenActivated(cleanUp =>
            {
                DbProvider.EnableAutoSaving(TimeSpan.FromSeconds(1))
                    .DisposeWith(cleanUp);
                DbProvider.EnableAutoLoading(TimeSpan.FromSeconds(1))
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(x => x.DembelFact)
                    .Subscribe(_ => IsShowingDembelFact = true)
                    .DisposeWith(cleanUp);
            });
        }
    }
}