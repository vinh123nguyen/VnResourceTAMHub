using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class TAMScanLogRepository : GenericRepository<TAMScanLog>
    {
        public TAMScanLogRepository(AppDbContext dbContext) : base(dbContext)
        {
            
        }
    }
}
