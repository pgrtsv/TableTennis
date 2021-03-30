using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public sealed class AddGameResultViewModel : ReactiveObject
    {
        private Contestant _firstContestant;
        private Contestant _secondContestant;
        private int _firstContestantScore;
        private int _secondContestantScore;
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;

        public AddGameResultViewModel(IObservable<GamesDb> gamesDb)
        {
            Observable.Switch(gamesDb.Select(x => x.ContestantsConnect()))
                .Sort(SortExpressionComparer<Contestant>.Ascending(x => x.Name))
                .Bind(out var contestants)
                .Subscribe();
            _gamesDb = gamesDb.ToProperty(this, nameof(GamesDb));

            Contestants = contestants;
            AddGameResult = ReactiveCommand.Create(() =>
            {
                GamesDb.AddGameResult(new GameResult(
                    new ContestantResult(FirstContestant, FirstContestantScore),
                    new ContestantResult(SecondContestant, SecondContestantScore)));
            }, this.WhenAnyValue(
                x => x.FirstContestant,
                x => x.SecondContestant,
                x => x.FirstContestantScore,
                x => x.SecondContestantScore,
                (firstContestant, secondContestant, firstScore, secondScore) =>
                    firstContestant != null
                    && secondContestant != null
                    && firstContestant.Guid != secondContestant.Guid
                    && !(firstScore != 11 && secondScore != 11)
                    && !(firstScore == 11 && secondScore == 11)));
        }

        private GamesDb GamesDb => _gamesDb.Value;

        public Contestant FirstContestant
        {
            get => _firstContestant;
            set => this.RaiseAndSetIfChanged(ref _firstContestant, value);
        }

        public Contestant SecondContestant
        {
            get => _secondContestant;
            set => this.RaiseAndSetIfChanged(ref _secondContestant, value);
        }


        public ReadOnlyObservableCollection<Contestant> Contestants { get; }

        public int FirstContestantScore
        {
            get => _firstContestantScore;
            set => this.RaiseAndSetIfChanged(ref _firstContestantScore, value);
        }

        public int SecondContestantScore
        {
            get => _secondContestantScore;
            set => this.RaiseAndSetIfChanged(ref _secondContestantScore, value);
        }

        public ReactiveCommand<Unit, Unit> AddGameResult { get; }
    }
}