using System.ComponentModel;
using System.Runtime.CompilerServices;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Models
{
    public class DeviceModel : INotifyPropertyChanged
    {
        public TAMSDKTypes DeviceSDKType { get; set; }
        public object DeviceObect { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MachineNo { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int? CommPassword { get; set; }

        private bool _isConnectLoading { get; set; }
        public bool IsConnectLoading
        {
            get => _isConnectLoading;
            set { _isConnectLoading = value; OnPropertyChanged(); } 
        }

        private bool _isConnected { get; set; }
        public bool IsConnected 
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
