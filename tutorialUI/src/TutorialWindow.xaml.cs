using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tutorialUI
{
    /// <summary>
    /// Interaction logic for TutorialWindow.xaml
    /// </summary>
    public partial class TutorialWindow: Window
    {
        public TutorialWindow()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;


        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Owner = Application.Current.MainWindow;
            Left = Owner.Left + (Owner.Width - ActualWidth) - 30;
            Top = Owner.Top + (Owner.Height - ActualHeight) - 30;
        }        


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
