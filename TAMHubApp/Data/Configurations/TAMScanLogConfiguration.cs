using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Configurations
{
    public class TAMScanLogConfiguration : IEntityTypeConfiguration<TAMScanLog>
    {
        public void Configure(EntityTypeBuilder<TAMScanLog> builder)
        {
            builder.ToTable("TAMScanLogs");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Device).WithMany(x => x.TAMScanLogs).HasForeignKey(x => x.DeviceID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
