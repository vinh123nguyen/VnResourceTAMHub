namespace TAMHubApp.Models
{
    public class DeviceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MachineNo { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }
        public bool IsConnected { get; set; }
    }
}
