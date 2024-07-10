using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class DeviceActionLogRepository : GenericRepository<DeviceActionLog>
    {
        public DeviceActionLogRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
