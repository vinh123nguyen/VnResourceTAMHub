using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Repositories
{
    public class DeviceRepository : GenericRepository<Device>
    {
        public DeviceRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
