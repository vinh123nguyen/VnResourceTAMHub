using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Configurations
{
    public class TAMScanLogHistoryConfiguration : IEntityTypeConfiguration<TAMScanLogHistory>
    {
        public void Configure(EntityTypeBuilder<TAMScanLogHistory> builder)
        {
            builder.ToTable("TAMScanLogHistories");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Device).WithMany(x => x.TAMScanLogHistories).HasForeignKey(x => x.DeviceID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
