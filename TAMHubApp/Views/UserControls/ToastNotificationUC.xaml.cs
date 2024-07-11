using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TAMHubApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ToastNotificationUC.xaml
    /// </summary>
    public partial class ToastNotificationUC : UserControl
    {
        public ToastNotificationUC()
        {
            InitializeComponent();
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ToastNotificationUC));

        public bool IsShowingToast
        {
            get { return (bool)GetValue(IsShowingToastProperty); }
            set { SetValue(IsShowingToastProperty, value); }
        }

        public static readonly DependencyProperty IsShowingToastProperty =
            DependencyProperty.Register("IsShowingToast", typeof(string), typeof(ToastNotificationUC));

        public Brush ToastColor
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("ToastColor", typeof(Brush), typeof(ToastNotificationUC));
    }
}
