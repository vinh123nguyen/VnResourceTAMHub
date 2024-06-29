using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Forms;
using TAMHubApp.Services;
using TAMHubApp.Views;

namespace TAMHubApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly ServiceProvider serviceProvider;
        private MainWindow mainWindows;
        private readonly NotifyIcon notifyIcon;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            notifyIcon = new NotifyIcon();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            mainWindows = serviceProvider.GetService<MainWindow>();
            mainWindows.Show();

            TrayBarService.CreateTrayIcon(notifyIcon);
            AutoUpdaterService.CheckForUpdate();

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            TrayBarService.ShowBalloonTipTest(notifyIcon);

            notifyIcon.Dispose();

            base.OnExit(e);
        }
    }

}
