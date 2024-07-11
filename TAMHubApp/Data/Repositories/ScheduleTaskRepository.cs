using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class ScheduleTaskRepository : GenericRepository<ScheduleTask>
    {
        public ScheduleTaskRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
