namespace TAMHubApp.Models
{
    public class SyncTAMScanLogDTO
    {
        public Guid ID { get; set; }
        public int EnrollNumber { get; set; }
        public string? InOutMode { get; set; }
        public DateTime TimeLog { get; set; }
        public int MachineNo { get; set; }
        public string? CompanyCode { get; set; }
    }
    public class SyncTAMScanLogResDTO
    {
        public List<Guid>? InsertedIds { get; set; }
        public List<Guid>? DuplicateIds { get; set; }
    }
}
