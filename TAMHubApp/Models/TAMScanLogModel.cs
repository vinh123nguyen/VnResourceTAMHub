namespace TAMHubApp.Models
{
    public class TAMScanLogModel
    {
        public Guid Id { get; set; }
        public Guid DeviceID { get; set; }
        public int EnrollNumber { get; set; }
        public string VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public DateTime TimeLog { get; set; }
    }
}
