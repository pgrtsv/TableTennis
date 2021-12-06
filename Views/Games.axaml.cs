using System;
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
                FilterGameResults.ViewModel = ViewModel.FilterViewModel;
                this.OneWayBind(
                        ViewModel,
                        x => x.AddGameResultViewModel,
                        x => x.AddGameResult.ViewModel)
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
                        x => x.IsAddingGameResult,
                        x => x.AddGameResult.IsVisible)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsAddingGameResult,
                        x => x.AddGameResult.AddButton.IsDefault)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                        ViewModel,
                        x => x.NextPage,
                        x => x.NextPageButton)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                        ViewModel,
                        x => x.PreviousPage,
                        x => x.PreviousPageButton)
                    .DisposeWith(cleanUp);
                this.WhenAnyValue(
                        x => x.ViewModel.CurrentPage,
                        x => x.ViewModel.PagesCount,
                        (page, count) => $"{page}/{count}")
                    .Subscribe(text => PageTextBlock.Text = text)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.DateTimeToggleButton,
                        x => x.SortViewModels.DateTime)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.FirstContestantToggleButton,
                        x => x.SortViewModels.FirstContestant)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.SecondContestantToggleButton,
                        x => x.SortViewModels.SecondContestant)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ListBox GamesListBox => this.FindControl<ListBox>(nameof(GamesListBox));
        public AddGameResult AddGameResult => this.FindControl<AddGameResult>(nameof(AddGameResult));

        public ToggleButton AddGameResultToggleButton =>
            this.FindControl<ToggleButton>(nameof(AddGameResultToggleButton));

        public ToggleButton FilterToggleButton => this.FindControl<ToggleButton>(nameof(FilterToggleButton));

        public FilterGameResults FilterGameResults => this.FindControl<FilterGameResults>(nameof(FilterGameResults));

        public RepeatButton PreviousPageButton => this.FindControl<RepeatButton>(nameof(PreviousPageButton));
        public RepeatButton NextPageButton => this.FindControl<RepeatButton>(nameof(NextPageButton));
        public TextBlock PageTextBlock => this.FindControl<TextBlock>(nameof(PageTextBlock));

        public ToggleButton DateTimeToggleButton => this.FindControl<ToggleButton>(nameof(DateTimeToggleButton));

        public ToggleButton FirstContestantToggleButton =>
            this.FindControl<ToggleButton>(nameof(FirstContestantToggleButton));

        public ToggleButton SecondContestantToggleButton =>
            this.FindControl<ToggleButton>(nameof(SecondContestantToggleButton));
    }
}