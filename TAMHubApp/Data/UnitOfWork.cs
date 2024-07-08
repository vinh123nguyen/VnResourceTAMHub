using TAMHubApp.Data.Repositories;

namespace TAMHubApp.Data
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        DeviceRepository DeviceRepository { get; }
        TAMScanLogRepository TAMScanLogRepository { get; }
        TAMScanLogHistoryRepository TAMScanLogHistoryRepository { get; }


        Task<int> SaveChanageAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        #region Private Dispose Fields

        private bool _disposed;

        #endregion

        private DeviceRepository _deviceRepository;
        private TAMScanLogRepository _TAMScanLogRepository;
        private TAMScanLogHistoryRepository _TAMScanLogHistoryRepository;
        public DeviceRepository DeviceRepository => _deviceRepository ?? (_deviceRepository = new DeviceRepository(_dbContext));
        public TAMScanLogRepository TAMScanLogRepository => _TAMScanLogRepository ?? (_TAMScanLogRepository = new TAMScanLogRepository(_dbContext));
        public TAMScanLogHistoryRepository TAMScanLogHistoryRepository => _TAMScanLogHistoryRepository ?? (_TAMScanLogHistoryRepository = new TAMScanLogHistoryRepository(_dbContext));


        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await _dbContext.DisposeAsync();
                }

                _disposed = true;
            }
        }

        public Task<int> SaveChanageAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
    }
}
