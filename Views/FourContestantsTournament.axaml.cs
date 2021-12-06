using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TableTennis.Views
{
    public class FourContestantsTournament : UserControl
    {
        public FourContestantsTournament()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}