using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class AddContestantViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<ContestantsDb> _contestantsDb;
        public ContestantsDb ContestantsDb => _contestantsDb.Value;

        [Reactive] public string Name { get; set; }

        [Reactive] public MilitaryRank Rank { get; set; }

        public IEnumerable<MilitaryRank> MilitaryRanks { get; } = MilitaryRank.GetAll();

        public ReactiveCommand<Unit, Unit> Add { get; }

        public AddContestantViewModel(IObservable<ContestantsDb> contestantsDb, IObservable<GamesDb> gamesDb)
        {
            Name = string.Empty;
            Rank = MilitaryRank.None;
            _contestantsDb = contestantsDb
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(ContestantsDb));
            Add = ReactiveCommand.Create(() => ContestantsDb.AddContestant(new Contestant(Rank, FullName.Parse(Name))),
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.Name),
                    contestantsDb.Select(x => x.ContestantsConnect().QueryWhenChanged(query => query.Items).StartWith(x.Contestants)).Switch(),
                    (name, contestants) =>
                        FullName.TryParse(out var fullName, name) &&
                        contestants.All(x => !x.Name.Equals(fullName))));
        }
    }
}