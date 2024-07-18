using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TAMHubApp.Views;

namespace TAMHubApp.Services
{
    public class TrayBarService
    {
        public static void CreateTrayIcon(NotifyIcon notifyIcon)
        {
            notifyIcon.Icon = new Icon("Resources/Images/icon.ico");
            notifyIcon.Text = "SingletonSean";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            notifyIcon.ContextMenuStrip.Items.Add("Open", Image.FromFile("Resources/Images/icon.ico"), OnStatusClicked);
            notifyIcon.ContextMenuStrip.Items.Add("Exit", Image.FromFile("Resources/Images/icon.ico"), OnStatusClickedExit);
            
            
            
            //notifyIcon.ContextMenuStrip.Items.Add(new ToolStripLabel("Status: Running"));
            //notifyIcon.ContextMenuStrip.Items.Add(new ToolStripButton("Status: Running"));
            //notifyIcon.ContextMenuStrip.Items.Add(new ToolStripDropDownButton("Status: Running", null,
            //    new ToolStripLabel("Label 1"),
            //    new ToolStripLabel("Label 2")));

            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            notifyIcon.Click += NotifyIcon_Click;
        }

        private static void NotifyIcon_Click(object? sender, EventArgs e)
        {
            //mainWindows.WindowState = WindowState.Normal;
            //mainWindows.Activate();
        }

        private static void NotifyIcon_BalloonTipClicked(object? sender, EventArgs e)
        {
            System.Windows.MessageBox.Show("Application is running.", "Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowBalloonTipTest(NotifyIcon notifyIcon)
        {
            notifyIcon.ShowBalloonTip(3000, "SingletonSean", "Be sure to subscribe.", ToolTipIcon.Info);
        }

        private static void OnStatusClicked(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Windows[0].WindowState = WindowState.Normal;
        }
        private static void OnStatusClickedExit(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
