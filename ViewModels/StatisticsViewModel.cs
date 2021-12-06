using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TableTennis.Models;

namespace TableTennis.ViewModels
{
    public record GamesCountForDay
    {
        public GamesCountForDay(DateTime date, int count)
        {
            Day = date;
            Count = count;
        }

        public DateTime Day { get; }
            
        public int Count { get; }
    }
    
    public sealed class StatisticsViewModel : ChildViewModelBase
    {
        public StatisticsViewModel(IObservable<GamesDb> gamesDb)
        {
            Observable.Switch(gamesDb.Select(db => db.GamesResultsConnect()))
                .Sort(SortExpressionComparer<GameResult>.Ascending(x => x.DateTime))
                .Group(gameResult => gameResult.DateTime.Date)
                .Transform(group => new GamesCountForDay(group.Key, group.Cache.Count))
                .Bind(out var gamesCounts)
                .Subscribe();
            GamesCounts = gamesCounts;
        }
        
        public ReadOnlyObservableCollection<GamesCountForDay> GamesCounts { get; }

        public override string Name => "Статистика";
    }
}