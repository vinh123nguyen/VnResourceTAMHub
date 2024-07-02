using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Windows;
using System.Windows.Forms;
using TAMHubApp.Data;
using TAMHubApp.ScheduleTasks;
using TAMHubApp.Services;
using TAMHubApp.ViewModels;
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
            // Schedule task DI Register
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<Quartz.QuartzHostedService>();
            services.AddHostedService(provider => provider.GetService<Quartz.QuartzHostedService>());

            // DB DI Register
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlite($"Data Source=Data/AppDB.db");
            });

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
            services.AddSingleton<MainWindow>();
        }
        private async Task InitializeScheduler()
        {
            var aa = serviceProvider.GetService<ISchedulerFactory>();
            var scheduler = await aa.GetScheduler();
            await scheduler.Start();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            mainWindows = serviceProvider.GetRequiredService<MainWindow>();
            mainWindows.Show();

            TrayBarService.CreateTrayIcon(notifyIcon);
            AutoUpdaterService.CheckForUpdate();
            InitializeScheduler().GetAwaiter().GetResult();
            DataBaseService.EnsureUpdateDatabase(serviceProvider);

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            TrayBarService.ShowBalloonTipTest(notifyIcon);

            notifyIcon.Dispose();
            serviceProvider.Dispose();

            base.OnExit(e);
        }
    }

}
