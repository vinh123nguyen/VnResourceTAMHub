using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using TAMHubApp.Data.Configurations;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new AppSettingConfiguration());
            builder.ApplyConfiguration(new DeviceConfiguration());
            builder.ApplyConfiguration(new DeviceActionLogConfiguration());
            builder.ApplyConfiguration(new TAMScanLogConfiguration());
            builder.ApplyConfiguration(new TAMScanLogHistoryConfiguration());


            builder.Entity<Device>().HasData([new Device()
            {
                Id = Guid.NewGuid(),
                Name = "Máy chấm công 1",
                MachineNo = 103,
                IPAddress = "10.0.0.231",
                Port = 4370,
                CommPassword = 0,
                DeviceSDKType = Infrastructures.Enums.TAMSDKTypes.E_ZKTECO,
                AllLogCount = 140000,
                LastTimeConnected = DateTime.Now.AddHours(-1),
                ZKTeco_IsCanGetLogByTime = true
            },
            new Device()
            {
                Id = Guid.NewGuid(),
                Name = "Máy chấm công 2",
                MachineNo = 103,
                IPAddress = "10.0.0.13",
                Port = 4370,
                CommPassword = 0,
                DeviceSDKType = Infrastructures.Enums.TAMSDKTypes.E_ZKTECO,
                AllLogCount = 140000,
                LastTimeConnected = DateTime.Now.AddHours(-1),
                ZKTeco_IsCanGetLogByTime = false

            },
            new Device()
            {
                Id = Guid.NewGuid(),
                Name = "Máy chấm công 2",
                MachineNo = 103,
                IPAddress = "10.20.5.85",
                Port = 5500,
                CommPassword = 0,
                DeviceSDKType = Infrastructures.Enums.TAMSDKTypes.E_REALAND,
                AllLogCount = 140000,
                LastTimeConnected = DateTime.Now.AddHours(-1),
            }
            ]);
        }

        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<DeviceActionLog> DeviceActionLogs { get; set; }  
        public virtual DbSet<TAMScanLog> TAMScanLogs { get; set; }  
        public virtual DbSet<TAMScanLogHistory> TAMScanLogHistories { get; set; }  

    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "AppDB.db");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
