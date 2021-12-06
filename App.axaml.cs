using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TableTennis.Models;
using TableTennis.ViewModels;
using TableTennis.Views;
using Splat;
using ReactiveUI;
using TableTennis.Views.Tournaments;

namespace TableTennis
{
    public class App : Application
    {
        public override void Initialize()
        {
            Locator.CurrentMutable.Register(() => new Contestants(), typeof(IViewFor<ContestantsViewModel>));
            Locator.CurrentMutable.Register(() => new Games(), typeof(IViewFor<GamesViewModel>));
            Locator.CurrentMutable.Register(() => new Settings(), typeof(IViewFor<SettingsViewModel>));
            Locator.CurrentMutable.Register(() => new Tournament(), typeof(IViewFor<TournamentViewModel>));
            Locator.CurrentMutable.Register(() => new Statistics(), typeof(IViewFor<StatisticsViewModel>));
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    ViewModel = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}