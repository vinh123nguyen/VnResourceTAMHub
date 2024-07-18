using System.ComponentModel;
using System.Runtime.CompilerServices;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Services.TAMServices
{
    public abstract class BaseTAMService : INotifyPropertyChanged
    {
        public bool IsEnabled = true;
        public TAMSDKTypes DeviceSDKType { get; set; }
        public Guid Id { get; set; }

        private string _deviceSDKTypeView { get; set; }
        public string DeviceSDKTypeView { get => _deviceSDKTypeView; set { _deviceSDKTypeView = value; OnPropertyChanged(); } }

        private string _name { get; set; }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private int _machineNo { get; set; }
        public int MachineNo { get => _machineNo; set { _machineNo = value; OnPropertyChanged(); } }

        private string _ipAddress { get; set; }
        public string IPAddress { get => _ipAddress; set { _ipAddress = value; OnPropertyChanged(); } }

        private int _port { get; set; }
        public int Port { get => _port; set { _port = value; OnPropertyChanged(); } }

        public int? CommPassword { get; set; }

        private int? _allLogCount { get; set; }
        public int? AllLogCount { get => _allLogCount; set { _allLogCount = value; OnPropertyChanged(); } }

        public int? _newLogCount { get; set; }
        public int? NewLogCount{ get => _newLogCount; set { _newLogCount = value; OnPropertyChanged(); } }

        private bool _isConnectLoading { get; set; }
        public bool IsConnectLoading { get => _isConnectLoading;  set { _isConnectLoading = value; OnPropertyChanged(); } }

        private bool _isConnected { get; set; }
        public bool IsConnected { get => _isConnected; set { _isConnected = value; OnPropertyChanged(); } }

        public string SerialNumber { get; set; }

        private string _productName { get; set; }
        public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }


        #region Prop change
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
