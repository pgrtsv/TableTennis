using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class AddContestant : ReactiveUserControl<AddContestantViewModel>
    {
        public AddContestant()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                RankComboBox.Items = ViewModel.MilitaryRanks;
                this.Bind(
                    ViewModel,
                    x => x.Rank,
                    x => x.RankComboBox.SelectedItem)
                    .DisposeWith(cleanUp);
                this.Bind(
                    ViewModel,
                    x => x.Name,
                    x => x.NameTextBox.Text)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                        ViewModel,
                        x => x.Add,
                        x => x.AddButton)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ComboBox RankComboBox => this.FindControl<ComboBox>(nameof(RankComboBox));
        public TextBox NameTextBox => this.FindControl<TextBox>(nameof(NameTextBox));
        public Button AddButton => this.FindControl<Button>(nameof(AddButton));
    }
}