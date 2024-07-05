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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TAMHubApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Nav_NavigationButtonUC.xaml
    /// </summary>
    public partial class NavigationButtonUC : UserControl
    {
        public NavigationButtonUC()
        {
            InitializeComponent();
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(NavigationButtonUC));

        public string TabActive
        {
            get { return (string)GetValue(TabActiveProperty); }
            set { SetValue(TabActiveProperty, value); }
        }
        public static readonly DependencyProperty TabActiveProperty = DependencyProperty.Register("TabActive", typeof(string), typeof(NavigationButtonUC));

        public string NameTab
        {
            get { return (string)GetValue(NameTabProperty); }
            set { SetValue(NameTabProperty, value); }
        }
        public static readonly DependencyProperty NameTabProperty = DependencyProperty.Register("NameTab", typeof(string), typeof(NavigationButtonUC));


        public PathGeometry Icon
        {
            get { return (PathGeometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(PathGeometry), typeof(NavigationButtonUC));


        public ICommand NavigateCommand
        {
            get { return (ICommand)GetValue(NavigateCommandProperty); }
            set { SetValue(NavigateCommandProperty, value); }
        }
        public static readonly DependencyProperty NavigateCommandProperty = DependencyProperty.Register("NavigateCommand", typeof(ICommand), typeof(NavigationButtonUC));
    }
}
