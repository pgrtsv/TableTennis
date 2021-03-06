using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(cleanUp =>
            {
                MenuListBox.Items = ViewModel.ViewModels;
                this.Bind(
                    ViewModel,
                    x => x.SelectedViewModel,
                    x => x.MenuListBox.SelectedItem
                ).DisposeWith(cleanUp);
                this.OneWayBind(
                    ViewModel,
                    x => x.SelectedViewModel,
                    x => x.ViewModelViewHost.ViewModel
                ).DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.DateTime,
                        x => x.ClockTextBlock.Text,
                        time => time.ToString("HH:mm"))
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.DateTime,
                        x => x.DateTextBlock.Text,
                        time => time.ToString("dd.MM.yyyy"))
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.EatingTimeInfo,
                        x => x.EatingTimeTextBlock.Text)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.DembelFact,
                        x => x.RandomFactTextBlock.Text)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                    ViewModel,
                    x => x.IsShowingDembelFact,
                    x => x.RandomFactBorder.IsVisible)
                    .DisposeWith(cleanUp);
                this.BindCommand(
                    ViewModel,
                    x => x.CloseDembelFact,
                    x => x.RandomFactCloseButton)
                    .DisposeWith(cleanUp);
                this.OneWayBind(
                        ViewModel,
                        x => x.SettingsViewModel.IsRubetsEnabled,
                        x => x.RubetsTextBlock.IsVisible)
                    .DisposeWith(cleanUp);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ListBox MenuListBox => this.FindControl<ListBox>(nameof(MenuListBox));

        public ViewModelViewHost ViewModelViewHost => this.FindControl<ViewModelViewHost>(nameof(ViewModelViewHost));
        public TextBlock ClockTextBlock => this.FindControl<TextBlock>(nameof(ClockTextBlock));
        public TextBlock DateTextBlock => this.FindControl<TextBlock>(nameof(DateTextBlock));
        public TextBlock EatingTimeTextBlock => this.FindControl<TextBlock>(nameof(EatingTimeTextBlock));
        public Border RandomFactBorder => this.FindControl<Border>(nameof(RandomFactBorder));
        public TextBlock RandomFactTextBlock => this.FindControl<TextBlock>(nameof(RandomFactTextBlock));
        public Button RandomFactCloseButton => this.FindControl<Button>(nameof(RandomFactCloseButton));
        public TextBlock RubetsTextBlock => this.FindControl<TextBlock>(nameof(RubetsTextBlock));
    }
}