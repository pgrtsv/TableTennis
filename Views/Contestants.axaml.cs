using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
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
                
                this.Bind(
                    ViewModel,
                    x => x.IsShowingSortPanel,
                    x => x.SortToggleButton.IsChecked)
                    .DisposeWith(cleanUp);
                SortingPanel.ViewModel = ViewModel;
                this.OneWayBind(
                        ViewModel,
                        x => x.IsShowingSortPanel,
                        x => x.SortingPanel.IsVisible)
                    .DisposeWith(cleanUp);
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
                        var contestant = (ContestantReadViewModel) control.DataContext;
                        return contestant.Contestant.Guid;
                    })
                    .InvokeCommand(ViewModel.AppCommands.ShowContestantGamesResults)
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
        public ToggleButton SortToggleButton => this.FindControl<ToggleButton>(nameof(SortToggleButton));
        public SortingPanel SortingPanel => this.FindControl<SortingPanel>(nameof(SortingPanel));
    }
}