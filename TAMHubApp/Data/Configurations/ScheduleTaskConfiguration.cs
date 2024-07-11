using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.Data.Configurations
{
    public class ScheduleTaskConfiguration : IEntityTypeConfiguration<ScheduleTask>
    {
        public void Configure(EntityTypeBuilder<ScheduleTask> builder)
        {
            builder.ToTable("ScheduleTasks");
            builder.HasKey(x => x.Id);
        }
    }
}
