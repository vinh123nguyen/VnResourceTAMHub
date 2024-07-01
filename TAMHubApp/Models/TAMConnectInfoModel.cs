namespace TAMHubApp.Models
{
    public class TAMConnectInfoModel
    {
        public string DisplayDeviceName { get; set; }
        public int DeviceID { set; get; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }
    }
}
