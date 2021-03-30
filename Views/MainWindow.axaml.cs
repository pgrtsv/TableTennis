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

            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ListBox MenuListBox => this.FindControl<ListBox>(nameof(MenuListBox));

        public ViewModelViewHost ViewModelViewHost => this.FindControl<ViewModelViewHost>(nameof(ViewModelViewHost));
    }
}