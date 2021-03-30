using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData.Binding;
using ReactiveUI;

namespace TableTennis.ViewModels
{
    public sealed class SortViewModel<T> : ReactiveObject, ISortViewModel
    {
        public SortViewModel(string name, Func<T, IComparable> comparer)
        {
            Name = name;
            Comparer = comparer;
        }

        public string Name { get; }

        private bool _isDescending;

        public bool IsDescending
        {
            get => _isDescending;
            set => this.RaiseAndSetIfChanged(ref _isDescending, value);
        }

        public Func<T, IComparable> Comparer { get; }

        public override string ToString() => Name;

        public IObservable<IComparer<T>> GetObservable() => this.WhenAnyValue(x => x.IsDescending)
            .StartWith(IsDescending)
            .Select(isDescending => isDescending
                ? SortExpressionComparer<T>.Descending(Comparer)
                : SortExpressionComparer<T>.Ascending(Comparer));
    }
}