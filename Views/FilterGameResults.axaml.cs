using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.Models;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class FilterGameResults : ReactiveUserControl<FilterGameResultsViewModel>
    {
        public FilterGameResults()
        {
            InitializeComponent();
            this.WhenActivated(cleanUp =>
            {
                ContestantAutoCompleteBox.Items = ViewModel.Contestants;
                ContestantAutoCompleteBox.ValueMemberBinding = new Binding(nameof(Contestant.Name));
                this.Bind(ViewModel,
                        x => x.IsFilteringByContestant,
                        x => x.FilterByContestantCheckBox.IsChecked)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.SelectedContestant,
                        x => x.ContestantAutoCompleteBox.SelectedItem)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.IsFilteringByContestant,
                        x => x.ContestantAutoCompleteBox.IsEnabled)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public CheckBox FilterByContestantCheckBox => this.FindControl<CheckBox>(nameof(FilterByContestantCheckBox));
        public AutoCompleteBox ContestantAutoCompleteBox => this.FindControl<AutoCompleteBox>(nameof(ContestantAutoCompleteBox));
    }
}