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
