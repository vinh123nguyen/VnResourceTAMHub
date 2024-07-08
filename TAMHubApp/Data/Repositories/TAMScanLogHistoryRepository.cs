using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class TAMScanLogHistoryRepository : GenericRepository<TAMScanLogHistory>
    {
        public TAMScanLogHistoryRepository(AppDbContext dbContext) : base(dbContext)
        {

        }
    }
}
