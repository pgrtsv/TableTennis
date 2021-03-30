using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.Models;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class AddGameResult : ReactiveUserControl<AddGameResultViewModel>
    {
        public AddGameResult()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                FirstContestantAutoCompleteBox.Items = ViewModel.Contestants;
                SecondContestantAutoCompleteBox.Items = ViewModel.Contestants;
                FirstContestantAutoCompleteBox.ValueMemberBinding = new Binding(nameof(Contestant.Name));
                SecondContestantAutoCompleteBox.ValueMemberBinding = new Binding(nameof(Contestant.Name));
                this.Bind(
                        ViewModel,
                        x => x.FirstContestant,
                        x => x.FirstContestantAutoCompleteBox.SelectedItem)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.SecondContestant,
                        x => x.SecondContestantAutoCompleteBox.SelectedItem)
                    .DisposeWith(cleanUp);
                FirstContestantScoreNumericUpDown.Minimum = SecondContestantScoreNumericUpDown.Minimum = 0;
                FirstContestantScoreNumericUpDown.Maximum = SecondContestantScoreNumericUpDown.Maximum = 11;
                this.Bind(
                    ViewModel,
                    x => x.FirstContestantScore,
                    x => x.FirstContestantScoreNumericUpDown.Value)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.SecondContestantScore,
                        x => x.SecondContestantScoreNumericUpDown.Value)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                        ViewModel,
                        x => x.AddGameResult,
                        x => x.AddButton)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public AutoCompleteBox FirstContestantAutoCompleteBox => this.FindControl<AutoCompleteBox>(nameof(FirstContestantAutoCompleteBox));
        public AutoCompleteBox SecondContestantAutoCompleteBox => this.FindControl<AutoCompleteBox>(nameof(SecondContestantAutoCompleteBox));

        public NumericUpDown FirstContestantScoreNumericUpDown =>
            this.FindControl<NumericUpDown>(nameof(FirstContestantScoreNumericUpDown));

        public NumericUpDown SecondContestantScoreNumericUpDown =>
            this.FindControl<NumericUpDown>(nameof(SecondContestantScoreNumericUpDown));

        public Button AddButton => this.FindControl<Button>(nameof(AddButton));
    }
}