using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Windows;
using System.Windows.Forms;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.ScheduleTasks;
using TAMHubApp.Services;
using TAMHubApp.ViewModels;
using TAMHubApp.ViewModels.UserControls;
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
            SQLitePCL.Batteries.Init();
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            notifyIcon = new NotifyIcon();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // DI ViewModel
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<DeviceViewModel>();
            services.AddSingleton<DataViewModel>();
            services.AddSingleton<ScheduleViewModel>();
            services.AddSingleton<SettingViewModel>();
            services.AddSingleton<Device_CreateOrUpdateUCViewModel>();

            // Schedule task DI Register
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<QuartzHostedService>();
            services.AddHostedService(provider => provider.GetService<QuartzHostedService>());

            // Navigate Service
            services.AddSingleton<ITabNavigationService, TabNavigationService>();
            services.AddSingleton<IDeviceNavigationService, DeviceNavigationService>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));

            // DB DI Register
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlite($"Data Source=Data/AppDB.db");
            });

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
            //services.AddSingleton<MainWindow>();

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

            //TrayBarService.CreateTrayIcon(notifyIcon);
            AutoUpdaterService.CheckForUpdate();
            InitializeScheduler().GetAwaiter().GetResult();
            DataBaseService.EnsureUpdateDatabase(serviceProvider);
            //LocalizationHelper.SetLanguage("vi-VN");

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
