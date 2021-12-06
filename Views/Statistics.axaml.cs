using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using ReactiveUI;
using TableTennis.ViewModels;
using DateTimeAxis = OxyPlot.Avalonia.DateTimeAxis;
using LinearAxis = OxyPlot.Avalonia.LinearAxis;

namespace TableTennis.Views
{
    public class Statistics : ReactiveUserControl<StatisticsViewModel>
    {
        public Statistics()
        {
            InitializeComponent();

            this.WhenActivated(cleanUp =>
            {
                Plot.Series.Clear();
                Plot.Series.Add(new LineSeries
                {
                    DataFieldX = nameof(GamesCountForDay.Day),
                    DataFieldY = nameof(GamesCountForDay.Count),
                    Items = ViewModel.GamesCounts,
                });
                Plot.Axes.Clear();
                Plot.Axes.Add(
                    new DateTimeAxis
                    {
                        Position = AxisPosition.Bottom,
                        IntervalType = DateTimeIntervalType.Days,
                    });
                Plot.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                });
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Plot Plot => this.FindControl<Plot>(nameof(Plot));
    }
}