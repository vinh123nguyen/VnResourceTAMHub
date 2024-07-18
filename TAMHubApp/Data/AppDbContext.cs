using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using TAMHubApp.Data.Configurations;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures.Enums;

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
            builder.ApplyConfiguration(new ActionLogConfiguration());
            builder.ApplyConfiguration(new TAMScanLogConfiguration());
            builder.ApplyConfiguration(new TAMScanLogHistoryConfiguration());
            builder.ApplyConfiguration(new ScheduleTaskConfiguration());

            var deviceid = Guid.NewGuid();
            builder.Entity<Device>().HasData([new Device()
            {
                Id = deviceid,
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
                Name = "Máy chấm công 3",
                MachineNo = 103,
                IPAddress = "10.20.5.85",
                Port = 5500,
                CommPassword = 0,
                DeviceSDKType = Infrastructures.Enums.TAMSDKTypes.E_REALAND,
                AllLogCount = 140000,
                LastTimeConnected = DateTime.Now.AddHours(-1),
            }
            ]);

            builder.Entity<ScheduleTask>().HasData([new ScheduleTask()
            {
                Id = Guid.NewGuid(),
                Name = "Schedule 1",
                CycleType = ScheduleCycleType.Daily,
                Hour = 10,
                Minute = 30
            },
                new ScheduleTask()
                {
                    Id = Guid.NewGuid(),
                    Name = "Schedule 1",
                    CycleType = ScheduleCycleType.Weekly,
                    Hour = 10,
                    Minute = 30,
                    DayOfWeek = DayOfWeek.Monday
                },
                new ScheduleTask()
                {
                    Id = Guid.NewGuid(),
                    Name = "Schedule 1",
                    CycleType = ScheduleCycleType.Monthly,
                    Hour = 10,
                    Minute = 30,
                    DayInMonth = 15
                },
            ]);

            builder.Entity<ActionLog>().HasData([
                new ActionLog(){
                    Id = Guid.NewGuid(),
                    DateDownDataFrom = new DateTime(2024,07,01),
                    DateDownDataTo = new DateTime(2024,07,31),
                    TimeLogFrom = DateTime.Now,
                    TimeLogTo = DateTime.Now.AddMinutes(5),
                    DownloadType = "Manual",
                    DeviceID = deviceid,
                    Status = "Success",
                    TotalRowDownload = 1000,
                    TotalNewlyData = 256,
                    TimeRun = new TimeSpan(0,5,3)
                },
                new ActionLog(){
                    Id = Guid.NewGuid(),
                    DateDownDataFrom = new DateTime(2024,07,01),
                    DateDownDataTo = new DateTime(2024,07,31),
                    TimeLogFrom = DateTime.Now,
                    TimeLogTo = DateTime.Now.AddMinutes(5),
                    DownloadType = "Manual",
                    DeviceID = deviceid,
                    Status = "Error",
                    Message = "Error test",
                }
                ]); ;

            builder.Entity<AppSetting>().HasData([
                new AppSetting(){
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.ENUM_INOUT_ZKTECO_IN_KEY.ToString(),
                    Value = "E_IN"
                },
                new AppSetting(){
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.ENUM_INOUT_ZKTECO_OUT_KEY.ToString(),
                    Value = "E_OUT"
                },
                new AppSetting(){
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.ENUM_INOUT_REALAND_IN_KEY.ToString(),
                    Value = "E_IN"
                },
                new AppSetting(){
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_KEY.ToString(),
                    Value = "E_OUT"
                }
                ]);
            
        }

        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<ActionLog> ActionLogs { get; set; }  
        public virtual DbSet<TAMScanLog> TAMScanLogs { get; set; }  
        public virtual DbSet<TAMScanLogHistory> TAMScanLogHistories { get; set; }  
        public virtual DbSet<ScheduleTask> ScheduleTasks { get; set; }

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
