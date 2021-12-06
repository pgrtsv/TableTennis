using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using TableTennis.ViewModels;

namespace TableTennis.Views.Tournaments
{
    public class Tournament : ReactiveUserControl<TournamentViewModel>
    {
        private readonly AutoCompleteBox[] _contestantsBoxes;
        private readonly StackPanel[] _firstWaveScores;
        private readonly TextBlock[] _secondWaveContestants;
        private readonly StackPanel[] _secondWaveScores;
        private readonly TextBlock[] _semiFinalContestants;
        private readonly StackPanel[] _semiFinalScores;
        private readonly TextBlock[] _finalContestants;
        private readonly StackPanel[] _finalScores;
        private readonly Border[] _borders;
        
        public Tournament()
        {
            InitializeComponent();
            _borders = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var border = new Border
                    {
                        Classes = i switch
                        {
                            0 or 6 => Classes.Parse("first_wave"),
                            1 or 5 => Classes.Parse("second_wave"),
                            2 or 4 => Classes.Parse("semifinal"),
                            3 => Classes.Parse("final"),
                            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
                        }
                    };
                    Grid.SetRow(border, 0);
                    Grid.SetRowSpan(border, 15);
                    Grid.SetColumn(border, i);
                    return border;
                })
                .ToArray();
            _contestantsBoxes = Enumerable.Range(0, 16)
                .Select(i =>
                {
                    var box = new AutoCompleteBox();
                    var isLeftColumnFilled = i / 8 > 0;
                    Grid.SetRow(box, isLeftColumnFilled ? (i - 8) * 2 : i * 2);
                    Grid.SetColumn(box, isLeftColumnFilled ? 0 : 6);
                    return box;
                })
                .ToArray();
            _firstWaveScores = Enumerable.Range(0, 8)
                .Select(i =>
                {
                    var stackPanel = CreateScoresStackPanel();
                    var isLeftColumnFilled = i / 4 > 0;
                    Grid.SetRow(stackPanel, isLeftColumnFilled ? (i - 4) * 4 + 1 : i * 4 + 1);
                    Grid.SetColumn(stackPanel, isLeftColumnFilled ? 0 : 6);
                    return stackPanel;
                })
                .ToArray();
            _secondWaveContestants = Enumerable.Range(0, 8)
                .Select(i =>
                {
                    var textBlock = new TextBlock();
                    var isLeftColumnFilled = i / 4 > 0;
                    Grid.SetRow(textBlock, isLeftColumnFilled ? (i - 4) * 4 + 1 : i * 4 + 1);
                    Grid.SetColumn(textBlock, isLeftColumnFilled ? 1 : 5);
                    return textBlock;
                })
                .ToArray();
            _secondWaveScores = Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var stackPanel = CreateScoresStackPanel();
                    var isLeftColumnFilled = i / 2 > 0;
                    Grid.SetRow(stackPanel, isLeftColumnFilled ? (i - 2) * 4 + 1 : i * 4 + 1);
                    Grid.SetColumn(stackPanel, isLeftColumnFilled ? 0 : 6);
                    return stackPanel;
                })
                .ToArray();
            _semiFinalContestants = Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var textBlock = new TextBlock();
                    var isLeftColumnFilled = i / 2 > 0;
                    Grid.SetRow(textBlock, isLeftColumnFilled ? (i - 2) * 9 + 3 : i * 9 + 3);
                    Grid.SetColumn(textBlock, isLeftColumnFilled ? 2 : 4);
                    return textBlock;
                })
                .ToArray();
            _semiFinalScores = Enumerable.Range(0, 2)
                .Select(i =>
                {
                    var stackPanel = CreateScoresStackPanel();
                    var isLeftColumnFilled = i == 1;
                    Grid.SetColumn(stackPanel, isLeftColumnFilled ? 4 : 2);
                    Grid.SetRow(stackPanel, 7);
                    return stackPanel;
                })
                .ToArray();
            _finalContestants = Enumerable.Range(0, 4)
                .Select(i =>
                {
                    var textBlock = new TextBlock();
                    Grid.SetColumn(textBlock, 3);
                    Grid.SetRow(textBlock, i * 2 + 4);
                    return textBlock;
                })
                .ToArray();
            _finalScores = Enumerable.Range(0, 2)
                .Select(i =>
                {
                    var stackPanel = CreateScoresStackPanel();
                    Grid.SetRow(stackPanel, i == 0 ? 5 : 9);
                    Grid.SetColumn(stackPanel, 3);
                    return stackPanel;
                })
                .ToArray();
            
            Grid.Children.AddRange(_borders);
            Grid.Children.AddRange(_contestantsBoxes);
            Grid.Children.AddRange(_firstWaveScores);
            Grid.Children.AddRange(_secondWaveContestants);
            Grid.Children.AddRange(_secondWaveScores);
            Grid.Children.AddRange(_semiFinalContestants);
            Grid.Children.AddRange(_semiFinalScores);
            Grid.Children.AddRange(_finalContestants);
            Grid.Children.AddRange(_finalScores);
        }

        private StackPanel CreateScoresStackPanel()
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                IsVisible = false
            };
            stackPanel.Children.Add(new TextBlock());
            stackPanel.Children.Add(new TextBlock {Text = ":"});
            stackPanel.Children.Add(new TextBlock());
            return stackPanel;
        }

        private TextBlock GetWinnerTextBlock(StackPanel stackPanel) => (TextBlock) stackPanel.Children[0];
        private TextBlock GetLoserTextBlock(StackPanel stackPanel) => (TextBlock) stackPanel.Children[2];

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Grid Grid => this.FindControl<Grid>(nameof(Grid));
    }
}