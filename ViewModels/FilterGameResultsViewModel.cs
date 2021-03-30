using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class FilterGameResultsViewModel : ReactiveObject
    {
        private Contestant? _selectedContestant;
        private bool _isFilteringByContestant;

        public FilterGameResultsViewModel(IObservable<GamesDb> gamesDb)
        {
            gamesDb.Select(x => x.ContestantsConnect())
                .Switch()
                .Bind(out var contestants)
                .Subscribe();
            _selectedContestant = contestants.FirstOrDefault();
            Contestants = contestants;
        }

        public ReadOnlyObservableCollection<Contestant> Contestants { get; }

        public Contestant? SelectedContestant
        {
            get => _selectedContestant;
            set => this.RaiseAndSetIfChanged(ref _selectedContestant, value);
        }

        public bool IsFilteringByContestant
        {
            get => _isFilteringByContestant;
            set => this.RaiseAndSetIfChanged(ref _isFilteringByContestant, value);
        }

        public IObservable<Func<GameResultReadViewModel, bool>> GetFilter() => this.WhenAnyValue(
            x => x.IsFilteringByContestant,
            x => x.SelectedContestant,
            (isFilteringByContestant, contestant) => _isFilteringByContestant
                ? (Func<GameResultReadViewModel, bool>) (result =>
                    contestant == null ||
                    result.FirstContestantGuid == contestant.Guid ||
                    result.SecondContestantGuid == contestant.Guid)
                : (Func<GameResultReadViewModel, bool>) (_ => true)
        );
    }
}