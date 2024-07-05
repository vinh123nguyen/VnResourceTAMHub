using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Models
{
    public class TAMConnectInfoModel
    {
        public TAMSDKTypes DeviceSDKType { get; set; }
        public string Name { get; set; }
        public int MachineNo { set; get; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }
    }
}
