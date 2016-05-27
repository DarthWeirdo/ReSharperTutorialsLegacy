using System.Windows;
using System.Windows.Input;


namespace tutorialUI
{
    /// <summary>
    /// Interaction logic for TitleWindow.xaml
    /// </summary>
    public partial class TitleWindow
    {
        private string _introText;

        public bool Restart { get; private set; }


        public TitleWindow(string introText, bool firstTime)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (!firstTime)
            {
                BtnStart.Content = "Continue Tutorial";
                BtnRestart.Visibility = Visibility.Visible;
            }     

            IntroText = introText;                        

        }

        public string IntroText
        {
            get { return _introText; }
            set
            {
                _introText = value;
                InlineXamlText.SetFormattedText(TextContent, value);
            }
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;            
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Restart = true;
        }
    }
}
