using System.ComponentModel;
using System.Runtime.CompilerServices;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Services.TAMServices
{
    public abstract class BaseTAMService : INotifyPropertyChanged
    {
        public TAMSDKTypes DeviceSDKType { get; set; }
        public Guid Id { get; set; }
        private string _name { get; set; }
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }
        private int _machineNo { get; set; }
        public int MachineNo
        {
            get => _machineNo;
            set { _machineNo = value; OnPropertyChanged(); }
        }
        private string _ipAddress { get; set; }
        public string IPAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }
        private int _port { get; set; }

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }
        public int? CommPassword { get; set; }

        private int _allLogCount { get; set; }
        public int AllLogCount
        {
            get => _allLogCount;
            set { _allLogCount = value; OnPropertyChanged(); }
        }
        public int _newLogCount { get; set; }
        public int NewLogCount
        {
            get => _newLogCount;
            set { _newLogCount = value; OnPropertyChanged(); }
        }

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


        public abstract Task<bool> Action_Connect();
        public abstract Task<bool> Action_Disconnect();



        #region Prop change
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
