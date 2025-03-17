using TAMHubApp.Infrastructures.Enums;

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
        public TAMSDKTypes DeviceSDKType { get; set; }
        public int? AllLogCount { get; set; }
        public int? NewLogCount { get; set; }
        public DateTime? LastTimeConnected { get; set; }
        public string? SerialNumber { get; set; }
        public string? ProductName { get; set; }
        public string? CompanyCode { get; set; }
        public bool IsInactive { get; set; }
        //ZKTeco prop
        public bool ZKTeco_IsCanGetLogByTime { get; set; }
        public bool ZKTeco_IsIFaceDevice { get; set; }
        public bool? ZKTeco_IsTFTMachine { get; set; }
        public bool? ZKTeco_IsNewFirmwareMachine { get; set; }


        public List<TAMScanLog> TAMScanLogs { get; set; }
        public List<TAMScanLogHistory> TAMScanLogHistories { get; set; }
        public List<ActionLog> ActionLogs { get; set; }
    }
}
