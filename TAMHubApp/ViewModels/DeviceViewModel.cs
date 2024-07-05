using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(IDeviceNavigationService navigationService,
            ObservableCollection<DeviceModel> listDevice)
        {
            _navigationService = navigationService;
            AddDeviceCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateDevice);
            //GetDataCommand = new RelayCommand<object>(x => )
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffDevice);

            DeviceItems = new ObservableCollection<DeviceModel>
            {
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    IsConnectLoading=true,
                    MachineNo = 103
                },
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103,
                    IsConnected = true
                },
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103
                },
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103
                },
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103
                },
                new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103
                },new DeviceModel()
                {
                    Id = Guid.NewGuid(),
                    Name = "Máy chấm công 1",
                    IPAddress = "10.0.0.231",
                    Port = 4370,
                    MachineNo = 103
                }
            };
        }


        private IDeviceNavigationService _navigationService;
        public IDeviceNavigationService OverlayNavigate
        {
            get => _navigationService;
            set { _navigationService = value; OnPropertyChanged(); }
        }
        private bool _isCreateOrUpdatePopupOpen;
        public bool IsCreateOrUpdatePopupOpen
        {
            get => _isCreateOrUpdatePopupOpen;
            set { _isCreateOrUpdatePopupOpen = value; OnPropertyChanged(); }
        }


        public RelayCommand<object> AddDeviceCommand { get; set; }

        public RelayCommand<object> GetDataCommand { get; set; }
        public RelayCommand<object> TongleOnOffCommand { get; set; }


        private ObservableCollection<DeviceModel> _deviceItems;
        public ObservableCollection<DeviceModel> DeviceItems
        {
            get => _deviceItems;
            set
            {
                _deviceItems = value;
                OnPropertyChanged();
            }
        }


        private void Action_OpenPopupAddOrUpdateDevice(object parameter)
        {
            if(parameter == null)
            {
                IsCreateOrUpdatePopupOpen = true;
                OverlayNavigate.NavigateTo<DeviceItemInfoUCViewModel>();

                var daw = OverlayNavigate.CurrentView;

                //Task.Delay(3000).Wait();
                //IsCreateOrUpdatePopupOpen = false;
            }
            else
            {

            }
        }

        private void Action_TongleOnOffDevice(object obj)
        {
            if (obj != null && obj is Guid)
            {
                var dwa = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if (dwa != null)
                {
                    dwa.IsConnectLoading = true;
                    //DeviceItems.Remove(dwa);

                    //ThreadPool.QueueUserWorkItem((state) =>
                    //{
                    //    // Simulate a long-running task
                    //    Thread.Sleep(3000);

                    //    dwa.IsConnectLoading = false;
                    //});
                    Task.Run(() =>
                    {
                        // Simulate a long-running task
                        Thread.Sleep(3000); // Replace with your actual time-consuming task
                        dwa.IsConnectLoading = false;
                        // Optionally return a result if needed
                    });

                }
            }
        }
    }
}
