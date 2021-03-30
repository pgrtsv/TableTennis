using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using JetBrains.Annotations;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class AddContestantViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;

        public GamesDb GamesDb => _gamesDb.Value;

        public AddContestantViewModel(IObservable<GamesDb> gamesDb)
        {
            _name = string.Empty;
            _rank = MilitaryRank.None;
            _gamesDb = gamesDb.ToProperty(this, nameof(GamesDb));
            Add = ReactiveCommand.Create(() =>
            {
                GamesDb.AddContestant(new Contestant(Rank, FullName.Parse(Name)));
            }, this.WhenAnyValue(x => x.Name, name => 
                FullName.TryParse(out var fullName, name) && 
                GamesDb.Contestants.All(x => !x.Name.Equals(fullName))));
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private MilitaryRank _rank;
        public MilitaryRank Rank
        {
            get => _rank;
            set => this.RaiseAndSetIfChanged(ref _rank, value);
        }
        
        public IEnumerable<MilitaryRank> MilitaryRanks { get; } = MilitaryRank.GetAll();

        public ReactiveCommand<Unit, Unit> Add { get; }
    }
}