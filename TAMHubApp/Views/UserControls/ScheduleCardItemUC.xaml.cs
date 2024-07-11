using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TAMHubApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ScheduleCardItemUC.xaml
    /// </summary>
    public partial class ScheduleCardItemUC : UserControl
    {
        public ScheduleCardItemUC()
        {
            InitializeComponent();
        }
        public ICommand TongleOnOffCommand
        {
            get { return (ICommand)GetValue(TongleOnOffCommandProperty); }
            set { SetValue(TongleOnOffCommandProperty, value); }
        }
        public static readonly DependencyProperty TongleOnOffCommandProperty = DependencyProperty.Register("TongleOnOffCommand", typeof(ICommand), typeof(ScheduleCardItemUC));
        public ICommand OpenEditPopupCommand
        {
            get { return (ICommand)GetValue(OpenEditPopupCommandProperty); }
            set { SetValue(OpenEditPopupCommandProperty, value); }
        }
        public static readonly DependencyProperty OpenEditPopupCommandProperty = DependencyProperty.Register("OpenEditPopupCommand", typeof(ICommand), typeof(ScheduleCardItemUC));

    }
}
