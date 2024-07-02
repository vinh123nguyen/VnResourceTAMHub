using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Configurations
{
    public class DeviceActionLogConfiguration : IEntityTypeConfiguration<DeviceActionLog>
    {
        public void Configure(EntityTypeBuilder<DeviceActionLog> builder)
        {
            builder.ToTable("DeviceActionLogs");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Device).WithMany(x => x.DeviceActionLogs).HasForeignKey(x => x.DeviceID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
