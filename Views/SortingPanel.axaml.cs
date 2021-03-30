using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class SortingPanel : ReactiveUserControl<IHasSorting>
    {
        public SortingPanel()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                SortComboBox.Items = ViewModel.SortViewModels;
                this.Bind(
                        ViewModel,
                        x => x.SelectedSortViewModel,
                        x => x.SortComboBox.SelectedItem)
                    .DisposeWith(cleanUp);
                this.Bind(
                        ViewModel,
                        x => x.SelectedSortViewModel.IsDescending,
                        x => x.SortIsDescendingCheckBox.IsChecked)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public ComboBox SortComboBox => this.FindControl<ComboBox>(nameof(SortComboBox));
        public CheckBox SortIsDescendingCheckBox => this.FindControl<CheckBox>(nameof(SortIsDescendingCheckBox));
        public StackPanel SortStackPanel => this.FindControl<StackPanel>(nameof(SortStackPanel));
    }
}