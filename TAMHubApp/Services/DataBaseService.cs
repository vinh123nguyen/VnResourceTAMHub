using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using TAMHubApp.Data;

namespace TAMHubApp.Services
{
    public class DataBaseService
    {
        private readonly AppDbContext _dbContext;

        public DataBaseService(AppDbContext context)
        {
            _dbContext = context;
        }
        public static void EnsureUpdateDatabase(ServiceProvider serviceProvider)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "AppDB.db");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (!File.Exists(dbFilePath) || Properties.Settings.Default.IsMigrateDb)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    context.Database.Migrate();
                }
            }
            //if (Properties.Settings.Default.IsMigrateDb)
            //{
            //    Properties.Settings.Default.IsMigrateDb = false;
            //    Properties.Settings.Default.Save();
            //}
        }
    }
}
