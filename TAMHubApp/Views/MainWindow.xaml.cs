using System.Windows;

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
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            System.Windows.Application.Current.Windows[0].WindowState = WindowState.Minimized;
        }
    }
}