using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Configurations
{
    public class ActionLogConfiguration : IEntityTypeConfiguration<ActionLog>
    {
        public void Configure(EntityTypeBuilder<ActionLog> builder)
        {
            builder.ToTable("ActionLogs");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Device).WithMany(x => x.ActionLogs).HasForeignKey(x => x.DeviceID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
