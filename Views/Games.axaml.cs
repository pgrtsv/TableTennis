using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class Games : ReactiveUserControl<GamesViewModel>
    {
        public Games()
        {
            InitializeComponent();
            this.WhenActivated(cleanUp =>
            {
                GamesListBox.Items = ViewModel.GameResults;
                SortingPanel.ViewModel = ViewModel;
                FilterGameResults.ViewModel = ViewModel.FilterViewModel;
                this.OneWayBind(
                        ViewModel,
                        x => x.AddGameResultViewModel,
                        x => x.AddGameResult.ViewModel)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.IsSorting,
                        x => x.SortToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.IsAddingGameResult,
                        x => x.AddGameResultToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.IsFiltering,
                        x => x.FilterToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsFiltering,
                        x => x.FilterGameResults.IsVisible)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsSorting,
                        x => x.SortingPanel.IsVisible)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsAddingGameResult,
                        x => x.AddGameResult.IsVisible)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsAddingGameResult,
                        x => x.AddGameResult.AddButton.IsDefault)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ListBox GamesListBox => this.FindControl<ListBox>(nameof(GamesListBox));
        public AddGameResult AddGameResult => this.FindControl<AddGameResult>(nameof(AddGameResult));
        public SortingPanel SortingPanel => this.FindControl<SortingPanel>(nameof(SortingPanel));

        public ToggleButton AddGameResultToggleButton =>
            this.FindControl<ToggleButton>(nameof(AddGameResultToggleButton));

        public ToggleButton SortToggleButton => this.FindControl<ToggleButton>(nameof(SortToggleButton));

        public ToggleButton FilterToggleButton => this.FindControl<ToggleButton>(nameof(FilterToggleButton));

        public FilterGameResults FilterGameResults => this.FindControl<FilterGameResults>(nameof(FilterGameResults));
    }
}