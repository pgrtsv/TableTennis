using System;
using System.Reactive;
using ReactiveUI;

namespace TableTennis.ViewModels
{
    public sealed class AppCommands
    {
        public AppCommands(ReactiveCommand<Guid, Unit> showContestantGamesResults)
        {
            ShowContestantGamesResults = showContestantGamesResults;
        }

        public ReactiveCommand<Guid, Unit> ShowContestantGamesResults { get; }
    }
}