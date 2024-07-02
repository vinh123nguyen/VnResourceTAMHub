namespace TAMHubApp.Models
{
    public class TAMConnectInfoModel
    {
        public string DisplayDeviceName { get; set; }
        public int MachineNo { set; get; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }
    }
}
