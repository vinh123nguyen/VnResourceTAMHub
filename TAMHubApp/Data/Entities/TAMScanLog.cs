namespace TAMHubApp.Data.Entities
{
    public class TAMScanLog
    {
        public Guid Id { get; set; }
        public Guid DeviceID { get; set; }
        public int EnrollNumber { get; set; }
        public string VerifyMode { get; set; }
        public string InOutModeView { get; set; }
        public string InOutModeOrigin { get; set; }
        public DateTime TimeLog { get; set; }

        public Device Device { get; set; }
    }
}
