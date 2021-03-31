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
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;
        public GamesDb GamesDb => _gamesDb.Value;

        [Reactive] public string Name { get; set; }

        [Reactive] public MilitaryRank Rank { get; set; }

        public IEnumerable<MilitaryRank> MilitaryRanks { get; } = MilitaryRank.GetAll();

        public ReactiveCommand<Unit, Unit> Add { get; }

        public AddContestantViewModel(IObservable<GamesDb> gamesDb)
        {
            Name = string.Empty;
            Rank = MilitaryRank.None;
            _gamesDb = gamesDb
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, nameof(GamesDb));
            Add = ReactiveCommand.Create(() => GamesDb.AddContestant(new Contestant(Rank, FullName.Parse(Name))),
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.Name),
                    gamesDb.Select(x => x.ContestantsConnect().QueryWhenChanged()).Switch(),
                    (name, contestants) =>
                        FullName.TryParse(out var fullName, name) &&
                        contestants.Items.All(x => !x.Name.Equals(fullName))));
        }
    }
}