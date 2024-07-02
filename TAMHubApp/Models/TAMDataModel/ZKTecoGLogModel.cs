namespace TAMHubApp.Models.TAMDataModel
{
    public class ZKTecoGLogModel
    {
        public Guid ID { set; get; }
        public int MachineNo { get; set; }
        public string EnrollNumber { get; set; }
        public string VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public DateTime TimeLog { get; set; }
        public int WorkCode { get; set; }
    }
}
