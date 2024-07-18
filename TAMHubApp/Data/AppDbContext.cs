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
