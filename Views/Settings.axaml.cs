using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class Settings : ReactiveUserControl<SettingsViewModel>
    {
        public Settings()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                this.Bind(
                        ViewModel,
                        x => x.IsRubetsEnabled,
                        x => x.RubetsCheckBox.IsChecked)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public CheckBox RubetsCheckBox => this.FindControl<CheckBox>(nameof(RubetsCheckBox));
    }
}