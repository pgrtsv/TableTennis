using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;
        public GamesDb GamesDb => _gamesDb.Value;

        public GamesDbProvider GamesDbProvider { get; }

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


        public MainWindowViewModel()
        {
            GamesDbProvider = new GamesDbProvider();
            GamesDbProvider.LoadOrNew();
            _gamesDb = GamesDbProvider.GamesDb
                .ToProperty(this, nameof(GamesDb))!;
            var gamesViewModel = new GamesViewModel(GamesDbProvider.GamesDb!);
            var contestantViewModel = new ContestantsViewModel(GamesDbProvider.GamesDb!, new AppCommands(
                ReactiveCommand.Create<Guid>(contestantGuid =>
                {
                    SelectedViewModel = gamesViewModel;
                    gamesViewModel.IsFiltering = true;
                    gamesViewModel.FilterViewModel.IsFilteringByContestant = true;
                    gamesViewModel.FilterViewModel.SelectedContestant =
                        gamesViewModel.FilterViewModel.Contestants.First(x => x.Guid == contestantGuid);
                })));
            _viewModels = new ChildViewModelBase[]
            {
                contestantViewModel,
                gamesViewModel
            };
            _selectedViewModel = contestantViewModel;
            _dateTime = Observable.Interval(TimeSpan.FromSeconds(1))
                .StartWith(0)
                .Select(_ => DateTime.Now)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(DateTime));
            _breakfastTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetBreakfastTime())
                .ToProperty(this, nameof(BreakfastTime));
            _dinnerTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetDinnerTime())
                .ToProperty(this, nameof(DinnerTime));
            _supperTime = Observable.Interval(TimeSpan.FromMinutes(30))
                .StartWith(0)
                .Select(_ => EatingService.GetSupperTime())
                .ToProperty(this, nameof(SupperTime));
            _eatingTimeInfo = this.WhenAnyValue(
                x => x.BreakfastTime,
                x => x.DinnerTime,
                x => x.SupperTime,
                (breakfast, dinner, supper) =>
                    $"Завтрак: {breakfast:HH:mm}, обед: {dinner:HH:mm}, ужин: {supper:HH:mm}")
                .ToProperty(this, nameof(EatingTimeInfo));
            this.WhenActivated(cleanUp =>
            {
                GamesDbProvider.EnableAutoSaving(TimeSpan.FromSeconds(1))
                    .DisposeWith(cleanUp);
                GamesDbProvider.EnableAutoLoading(TimeSpan.FromSeconds(1))
                    .DisposeWith(cleanUp);
            });
        }
    }
}