using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class AddGameResultViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<GamesDb> _gamesDb;
        private readonly ObservableAsPropertyHelper<ContestantsDb> _contestantsDb;

        public AddGameResultViewModel(IObservable<GamesDb> gamesDb, IObservable<ContestantsDb> contestantsDb)
        {
            Observable.Switch(contestantsDb.Select(x => x.ContestantsConnect()))
                .Sort(SortExpressionComparer<Contestant>.Ascending(x => x.Name))
                .Bind(out var contestants)
                .Subscribe();
            _gamesDb = gamesDb.ToProperty(this, nameof(GamesDb));
            _contestantsDb = contestantsDb.ToProperty(this, nameof(ContestantsDb));

            Contestants = contestants;
            AddGameResult = ReactiveCommand.Create(() =>
            {
                GamesDb.AddGameResult(new GameResult(
                    new ContestantScore(FirstContestant!, FirstContestantScore),
                    new ContestantScore(SecondContestant!, SecondContestantScore)),
                    ContestantsDb);
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
        private ContestantsDb ContestantsDb => _contestantsDb.Value;
        [Reactive] public Contestant? FirstContestant { get; set; }
        [Reactive] public Contestant? SecondContestant { get; set; }
        public ReadOnlyObservableCollection<Contestant> Contestants { get; }
        [Reactive] public int FirstContestantScore { get; set; }
        [Reactive] public int SecondContestantScore { get; set; }
        public ReactiveCommand<Unit, Unit> AddGameResult { get; }
    }
}