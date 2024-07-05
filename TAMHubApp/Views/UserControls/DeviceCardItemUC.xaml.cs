using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TAMHubApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceCardItemUC.xaml
    /// </summary>
    public partial class DeviceCardItemUC : UserControl
    {
        public DeviceCardItemUC()
        {
            InitializeComponent();
        }

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }
        public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register("IsConnected", typeof(bool), typeof(DeviceCardItemUC));

        public bool IsConnectLoading
        {
            get { return (bool)GetValue(IsConnectLoadingProperty); }
            set { SetValue(IsConnectLoadingProperty, value); }
        }
        public static readonly DependencyProperty IsConnectLoadingProperty = DependencyProperty.Register("IsConnectLoading", typeof(bool), typeof(DeviceCardItemUC));

        public ICommand TongleOnOffCommand
        {
            get { return (ICommand)GetValue(TongleOnOffCommandProperty); }
            set { SetValue(TongleOnOffCommandProperty, value); }
        }
        public static readonly DependencyProperty TongleOnOffCommandProperty = DependencyProperty.Register("TongleOnOffCommand", typeof(ICommand), typeof(DeviceCardItemUC));

    }
}
