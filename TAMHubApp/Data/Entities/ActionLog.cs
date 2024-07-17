namespace TAMHubApp.Data.Entities
{
    public class ActionLog
    {
        public Guid Id { get; set; }
        public DateTime TimeLogFrom { get; set; }
        public DateTime TimeLogTo { get; set; }
        public DateTime? DateDownDataFrom { get; set; }
        public DateTime? DateDownDataTo { get; set; }
        public string DownloadType { get; set; }
        public Guid? DeviceID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int? TotalRowDownload { get; set; }
        public int? TotalNewlyData { get; set; }
        public TimeSpan? TimeRun { get; set; }

        public Device Device { get; set; }
    }
}
