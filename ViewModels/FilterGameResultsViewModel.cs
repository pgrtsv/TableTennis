using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class FilterGameResultsViewModel : ReactiveObject
    {
        public FilterGameResultsViewModel(IObservable<GamesDb> gamesDb)
        {
            gamesDb.Select(x => x.ContestantsConnect())
                .Switch()
                .Bind(out var contestants)
                .Subscribe();
            SelectedContestant = contestants.FirstOrDefault();
            Contestants = contestants;
        }

        public ReadOnlyObservableCollection<Contestant> Contestants { get; }
        [Reactive] public Contestant? SelectedContestant { get; set; }
        [Reactive] public bool IsFilteringByContestant { get; set; }

        public IObservable<Func<GameResultReadViewModel, bool>> GetFilter() => this.WhenAnyValue(
            x => x.IsFilteringByContestant,
            x => x.SelectedContestant,
            (isFilteringByContestant, contestant) => isFilteringByContestant
                ? (Func<GameResultReadViewModel, bool>) (result =>
                    contestant == null ||
                    result.FirstContestantGuid == contestant.Guid ||
                    result.SecondContestantGuid == contestant.Guid)
                : (Func<GameResultReadViewModel, bool>) (_ => true)
        );
    }
}