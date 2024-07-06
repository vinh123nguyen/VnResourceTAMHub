using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(IDeviceNavigationService navigationService,
            SingletonTAMInstance singletonTAMInstance)
        {
            _navigationService = navigationService;
            AddDeviceCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateDevice);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffDevice);

            DeviceItems = singletonTAMInstance.TAMs;
            _singletonTAMInstance = singletonTAMInstance;
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


        private ObservableCollection<BaseTAMService> _deviceItems;
        public ObservableCollection<BaseTAMService> DeviceItems
        {
            get => _deviceItems;
            set
            {
                _deviceItems = value;
                OnPropertyChanged();
            }
        }

        private SingletonTAMInstance _singletonTAMInstance;

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
                var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if (itemdeviceAction != null)
                {
                    itemdeviceAction.IsConnectLoading = true;
                    Task.Run(() =>
                    {
                        if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_ZKTECO)
                        {
                            Thread.Sleep(2000);
                            var exactlyDeviceInstance = (ZKTecoDeviceItemService)itemdeviceAction;
                            if (!itemdeviceAction.IsConnected)
                            {
                                itemdeviceAction.Action_Connect();
                            }
                            else
                            {
                                itemdeviceAction.Action_Disconnect();
                            }
                        }
                        else if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_REALAND)
                        {

                        }
                        itemdeviceAction.IsConnectLoading = false;
                    });

                    //ThreadPool.QueueUserWorkItem((state) =>
                    //{
                    //    if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_ZKTECO)
                    //    {
                    //        Thread.Sleep(2000);
                    //        var exactlyDeviceInstance = (ZKTecoDeviceItemService)itemdeviceAction;
                    //        if (!itemdeviceAction.IsConnected)
                    //        {
                    //            itemdeviceAction.Action_Connect();
                    //        }
                    //        else
                    //        {
                    //            itemdeviceAction.Action_Disconnect();
                    //        }
                    //    }
                    //    else if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_REALAND)
                    //    {

                    //    }
                    //    itemdeviceAction.IsConnectLoading = false;
                    //});

                }
            }
        }
    }
}
