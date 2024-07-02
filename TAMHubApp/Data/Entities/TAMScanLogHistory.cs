namespace TAMHubApp.Data.Entities
{
    public class TAMScanLogHistory
    {
        public Guid Id { get; set; }
        public Guid DeviceID { get; set; }
        public int EnrollNumber { get; set; }
        public string VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public string TimeLog { get; set; }

        public Device Device { get; set; }
    }
}
