namespace TAMHubApp.Data.Entities
{
    public class TAMScanLogHistory
    {
        public Guid Id { get; set; }
        public Guid DeviceID { get; set; }
        public int EnrollNumber { get; set; }
        public int VerifyModeOrigin { get; set; }
        public string? VerifyModeView { get; set; }
        public string? InOutModeView { get; set; }
        public int InOutModeOrigin { get; set; }
        public DateTime TimeLog { get; set; }
        public DateTime DateCreate { get; set; }

        public Device Device { get; set; }
    }
}
