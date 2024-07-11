using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Data.Entities
{
    public class ScheduleTask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastTimeRun { get; set; }
        public ScheduleCycleType CycleType { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int? DayInMonth { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
    }
}
