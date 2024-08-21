using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using System.Windows;
using System.Windows.Forms;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.ScheduleTasks;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
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
            ServiceLocator.Init(serviceProvider);
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
            services.AddSingleton<ActionLogViewModel>();
            services.AddSingleton<GeneralSettingViewUCViewModel>();
            services.AddSingleton<ZKTecoSettingViewUCViewModel>();
            services.AddSingleton<RealandSettingViewUCViewModel>();
            services.AddTransient<DeviceItemInfoUCViewModel>();
            services.AddTransient<SelectDateRangeGetDataUCViewModel>();
            services.AddTransient<ScheduleTaskItemInfoUCViewModel>();

            // Schedule task DI Register
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<QuartzHostedService>();
            services.AddHostedService(provider => provider.GetService<QuartzHostedService>());
            services.AddSingleton<ScheduleTaskService>();



            // Navigate Service
            services.AddSingleton<ITabNavigationService, TabNavigationService>();
            services.AddSingleton<IDeviceNavigationService, DeviceNavigationService>();
            services.AddSingleton<IScheduleNavigationService, ScheduleNavigationService>();
            services.AddSingleton<IDataNavigationService, DataNavigationService>();
            services.AddSingleton<ISettingNavigationService, SettingNavigationService>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider => viewModelType => (BaseViewModel)serviceProvider.GetRequiredService(viewModelType));

            // DB DI Register
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlite($"Data Source=Data/AppDB.db");
            }, ServiceLifetime.Transient);

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            //services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<SingletonTAMInstance>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

        }
        private async Task InitializeScheduler()
        {
            var aa = serviceProvider.GetService<ISchedulerFactory>();
            var scheduler = await aa.GetScheduler();
            await scheduler.Start();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationHelper.SetLanguage();

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            InitializeScheduler().GetAwaiter().GetResult();

            mainWindows = serviceProvider.GetRequiredService<MainWindow>();
            mainWindows.Show();

            TrayBarService.CreateTrayIcon(notifyIcon);
            //AutoUpdaterService.CheckForUpdate();
            DataBaseService.EnsureUpdateDatabase(serviceProvider);
            

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            //mainWindows.Hide();
            notifyIcon.Dispose();

            //base.OnExit(e);
        }
    }

}
