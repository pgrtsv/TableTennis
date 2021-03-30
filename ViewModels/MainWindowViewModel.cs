﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
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