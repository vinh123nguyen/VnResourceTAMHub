namespace TAMHubApp.Data.Entities
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MachineNo { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }


        public List<TAMScanLog> TAMScanLogs { get; set; }
        public List<TAMScanLogHistory> TAMScanLogHistories { get; set; }
        public List<DeviceActionLog> DeviceActionLogs { get; set; }
    }
}
