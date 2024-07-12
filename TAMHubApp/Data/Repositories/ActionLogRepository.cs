using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class ActionLogRepository : GenericRepository<ActionLog>
    {
        public ActionLogRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
