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

namespace TAMHubApp.Views.UserControls
{
    /// <summary>
    /// Interaction logic for PaginationBarUC.xaml
    /// </summary>
    public partial class PaginationBarUC : UserControl
    {
        public PaginationBarUC()
        {
            InitializeComponent();
        }
        public ICommand PaginationBackCommand
        {
            get { return (ICommand)GetValue(PaginationBackCommandProperty); }
            set { SetValue(PaginationBackCommandProperty, value); }
        }
        public static readonly DependencyProperty PaginationBackCommandProperty = DependencyProperty.Register("PaginationBackCommand", typeof(ICommand), typeof(PaginationBarUC));
        public ICommand PaginationForwardCommand
        {
            get { return (ICommand)GetValue(PaginationForwardCommandProperty); }
            set { SetValue(PaginationForwardCommandProperty, value); }
        }
        public static readonly DependencyProperty PaginationForwardCommandProperty = DependencyProperty.Register("PaginationForwardCommand", typeof(ICommand), typeof(PaginationBarUC));
        public string CurrentPage
        {
            get { return (string)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }
        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register("CurrentPage", typeof(string), typeof(PaginationBarUC));
        public string TotalPagePages
        {
            get { return (string)GetValue(TotalPagePageProperty); }
            set { SetValue(TotalPagePageProperty, value); }
        }
        public static readonly DependencyProperty TotalPagePageProperty = DependencyProperty.Register("TotalPagePage", typeof(string), typeof(PaginationBarUC));
    }
}
