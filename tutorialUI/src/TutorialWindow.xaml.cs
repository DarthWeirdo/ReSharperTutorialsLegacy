using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;


namespace tutorialUI
{
    /// <summary>
    /// Interaction logic for TutorialWindow.xaml
    /// </summary>
    public partial class TutorialWindow
    {

        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private string _tutorialText;

        public TutorialWindow()
        {
            InitializeComponent();
            
            Owner = Application.Current.MainWindow;
            Width = Owner.Width / 3;
            Height = Owner.Height / 4;
            Left = Owner.Left + (Owner.Width - ActualWidth) - 30;
            Top = Owner.Top + (Owner.Height - ActualHeight) - 30;

            Focusable = false;

            SourceInitialized += (s, e) =>
            {
                var interopHelper = new WindowInteropHelper(this);
                int exStyle = GetWindowLong(interopHelper.Handle, GWL_EXSTYLE);
                SetWindowLong(interopHelper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
            };
        }

        /// <summary>
        /// Subscribe event handlers for WPF controls
        /// </summary>        
        /// <param name="actionType"></param>
        /// <param name="controlEvent"></param>
        /// <param name="eventHandler"></param>
        public void Subscribe(UiActionType actionType , RoutedEventHandler controlEvent, Action eventHandler)
        {
            throw new NotImplementedException();
        }


        #region Unimplemented DepProp
        //        // TODO: this is too complicated. We can and probably should get rid of DependencyProperty and use simple Property
        //        /// <summary>
        //        /// It's my attempt to implement dependency property to make autoupdate of text in TutorialWindow
        //        /// </summary>
        //        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        //            "Text", typeof (string), typeof (TutorialWindow), new PropertyMetadata(default(string)));
        //
        //        public string Text
        //        {
        //            get { return (string) GetValue(TextProperty); }
        //            set
        //            { SetValue(TextProperty, value); }
        //        }
        //            // Listen to Text property changes and update TextBlock
        //            var textDescriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(TitleWindow));
        //            textDescriptor?.AddValueChanged(this, delegate
        //            {
        //                InlineXamlText.SetFormattedText(TextContent, Text);
        //            });
        #endregion


        public string TutorialText
        {
            get { return _tutorialText; }
            set
            {
                _tutorialText = value;
                InlineXamlText.SetFormattedText(TextContent, value);                
            }
        }
        

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
