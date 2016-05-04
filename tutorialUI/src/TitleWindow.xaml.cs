using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace tutorialUI
{
    /// <summary>
    /// Interaction logic for TitleWindow.xaml
    /// </summary>
    public partial class TitleWindow
    {
        private string _introText;

        public TitleWindow(string introText)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
//            Left = Owner.Left + (Owner.Width / 2 - ActualWidth / 2);
//            Top = Owner.Top + (Owner.Height / 2 - ActualHeight / 2);

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
            Close();
            // TODO: Implement ShowDialog logic for Cancel as well
        }


    }
}
