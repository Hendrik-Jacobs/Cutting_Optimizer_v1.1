using System.Windows;
using System.Windows.Controls;

namespace Cutting_Optimizer
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            BoardsBox.Text = HelpTexts.Boards;
            BoardManagerBox.Text = HelpTexts.BoardManager;
            PartsBox.Text = HelpTexts.Parts;
            SpaceBox.Text = HelpTexts.Space;
            RunBox.Text = HelpTexts.Run;
            OptionsBox.Text = HelpTexts.Options;
            ShortcutsBox.Text = HelpTexts.Shortcuts;
            AboutBox.Text = HelpTexts.About;

            BoardsBox.Visibility = Visibility.Collapsed;
            BoardManagerBox.Visibility = Visibility.Collapsed;
            PartsBox.Visibility = Visibility.Collapsed;
            SpaceBox.Visibility = Visibility.Collapsed;
            RunBox.Visibility = Visibility.Collapsed;
            OptionsBox.Visibility = Visibility.Collapsed;
            AboutBox.Visibility = Visibility.Collapsed;
            ShortcutsBox.Visibility = Visibility.Collapsed;
        }

        private void ClickClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            Button srcButton = e.Source as Button;
            string srcName = srcButton.Name;
            TextBlock textBlock = new TextBlock();

            switch (srcName)
            {
                case "ShowBoards":
                    textBlock = BoardsBox;
                    break;
                case "ShowBoardManager":
                    textBlock = BoardManagerBox;
                    break;
                case "ShowParts":
                    textBlock = PartsBox;
                    break;
                case "ShowSpace":
                    textBlock = SpaceBox;
                    break;
                case "ShowRun":
                    textBlock = RunBox;
                    break;
                case "ShowOptions":
                    textBlock = OptionsBox;
                    break;
                case "ShowShortcuts":
                    textBlock = ShortcutsBox;
                    break;
                case "ShowAbout":
                    textBlock = AboutBox;
                    break;
            };

            if (textBlock.Visibility == Visibility.Collapsed)
            {
                textBlock.Visibility = Visibility.Visible;
                srcButton.Content = "Show less";
            }
            else
            {
                textBlock.Visibility = Visibility.Collapsed;
                srcButton.Content = "Show more";
            }
        }
    }
}
