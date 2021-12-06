using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class Contestants : ReactiveUserControl<ContestantsViewModel>
    {
        public Contestants()
        {
            InitializeComponent();
            this.WhenActivated(cleanUp =>
            {
                this.OneWayBind(
                        ViewModel,
                        x => x.Contestants,
                        x => x.ContestantsListBox.Items)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.IsAddingContestant,
                        x => x.AddToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsAddingContestant,
                        x => x.AddContestant.IsVisible)
                    .DisposeWith(cleanUp);
                AddContestant.ViewModel = ViewModel.AddContestantViewModel;

                this.OneWayBind(
                        ViewModel,
                        x => x.IsAddingContestant,
                        x => x.AddContestant.AddButton.IsDefault)
                    .DisposeWith(cleanUp);
                ContestantsListBox.Events().DoubleTapped
                    .Select(args =>
                    {
                        if (!(args.Source is Control control))
                            throw new Exception();
                        var contestant = (ContestantReadViewModel) control.DataContext!;
                        return contestant.Contestant.Guid;
                    })
                    .InvokeCommand(ViewModel.AppCommands.ShowContestantGamesResults)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.IsShowingDateTimePicker,
                        x => x.PickDateToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsShowingDateTimePicker,
                        x => x.BecamePrivateDateBorder.IsVisible)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.CanShowDateTimePicker,
                        x => x.PickDateToggleButton.IsEnabled)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.SelectedContestant,
                        x => x.ContestantsListBox.SelectedItem)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.SelectedContestant.Contestant.BecamePrivateDateTime,
                        x => x.BecamePrivateDatePicker.SelectedDate,
                        date => date == default
                            ? new DateTimeOffset(new DateTime(2020, 1, 1))
                            : new DateTimeOffset(date))
                    .DisposeWith(cleanUp);
                BecamePrivateDatePicker.YearVisible = false;
                BecamePrivateDatePicker.MinYear = new DateTimeOffset(new DateTime(2020, 1, 1));
                BecamePrivateDatePicker.MaxYear = new DateTimeOffset(new DateTime(2020, 12, 31));
                BecamePrivateDatePicker.Events().SelectedDateChanged
                    .Subscribe(args =>
                    {
                        if (ViewModel.SelectedContestant == null) return;
                        if (args.NewDate!.Value.Day == 1 && args.NewDate.Value.Month == 1) return;
                        ViewModel.SelectedContestant.Contestant.SetBecamePrivateDateTime(args.NewDate.Value.Date);
                    })
                    .DisposeWith(cleanUp);

                this.BindSorting(
                    ViewModel,
                    x => x.RatingPositionToggleButton,
                    x => x.SortViewModels.MonthlyScore)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.RankToggleButton,
                        x => x.SortViewModels.Rank)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.NameToggleButton,
                        x => x.SortViewModels.Name)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.RatingToggleButton,
                        x => x.SortViewModels.MonthlyScore)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.GamesTotalToggleButton,
                        x => x.SortViewModels.GamesTotal)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.WinsToggleButton,
                        x => x.SortViewModels.Wins)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.LossesToggleButton,
                        x => x.SortViewModels.Losses)
                    .DisposeWith(cleanUp);
                this.BindSorting(
                        ViewModel,
                        x => x.WTToggleButton,
                        x => x.SortViewModels.WinTotalRatio)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ListBox ContestantsListBox => this.FindControl<ListBox>(nameof(ContestantsListBox));
        public AddContestant AddContestant => this.FindControl<AddContestant>(nameof(AddContestant));
        public ToggleButton AddToggleButton => this.FindControl<ToggleButton>(nameof(AddToggleButton));
        public ToggleButton PickDateToggleButton => this.FindControl<ToggleButton>(nameof(PickDateToggleButton));
        public Border BecamePrivateDateBorder => this.FindControl<Border>(nameof(BecamePrivateDateBorder));
        public DatePicker BecamePrivateDatePicker => this.FindControl<DatePicker>(nameof(BecamePrivateDatePicker));

        public ToggleButton RatingPositionToggleButton =>
            this.FindControl<ToggleButton>(nameof(RatingPositionToggleButton));

        public ToggleButton RankToggleButton =>
            this.FindControl<ToggleButton>(nameof(RankToggleButton));

        public ToggleButton NameToggleButton =>
            this.FindControl<ToggleButton>(nameof(NameToggleButton));

        public ToggleButton RatingToggleButton =>
            this.FindControl<ToggleButton>(nameof(RatingToggleButton));

        public ToggleButton GamesTotalToggleButton =>
            this.FindControl<ToggleButton>(nameof(GamesTotalToggleButton));

        public ToggleButton WinsToggleButton =>
            this.FindControl<ToggleButton>(nameof(WinsToggleButton));

        public ToggleButton LossesToggleButton =>
            this.FindControl<ToggleButton>(nameof(LossesToggleButton));
        
        public ToggleButton WTToggleButton =>
            this.FindControl<ToggleButton>(nameof(WTToggleButton));
    }
}