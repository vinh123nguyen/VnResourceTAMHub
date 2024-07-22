using System.Windows;
using System.Windows.Input;


namespace TAMHubApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NavPopup.MouseEnter += (s, e) => NavPopup.IsOpen = true;
            NavPopup.MouseLeave += (s, e) => NavPopup.IsOpen = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            System.Windows.Application.Current.Windows[0].WindowState = WindowState.Minimized;
        }
        private void NavButton_MouseEnter(object sender, MouseEventArgs e)
        {
            NavPopup.IsOpen = true;
        }

        private void NavButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!NavPopup.IsMouseOver)
            {
                NavPopup.IsOpen = false;
            }
        }
    }
}