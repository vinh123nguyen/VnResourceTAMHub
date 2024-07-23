namespace TAMHubApp.Models
{
    public class ActionLogModel
    {
        public Guid Id { get; set; }
        public string TimeLog { get; set; }
        public string DateDownData { get; set; }
        public string? TAMName { get; set; }
        public string StatusView { get; set; }
        public string Status { get; set; }
        public string DownloadType { get; set; }
        public int? TotalRowDownload { get; set; }
        public int? TotalDataAdded { get; set; }
        public int? TotalNewlyData { get; set; }
        public string? TimeRun { get; set; }
        public string? Message { get; set; }
    }
}
