using System.Collections.ObjectModel;
using System.Windows.Media;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(IDeviceNavigationService navigationService,
            SingletonTAMInstance singletonTAMInstance,
            MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _navigationService = navigationService;
            OpenInfoPopup = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateDevice);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffDevice);
            ConnectAllCommand = new RelayCommand<object>(x => { return true; }, x => { Action_ConnectDisconnectAllDevice(true); });
            DisconnectAllCommand = new RelayCommand<object>(x => { return true; }, x => { Action_ConnectDisconnectAllDevice(false); });

            DeviceItems = singletonTAMInstance.TAMs;
        }

        private MainViewModel _mainViewModel;
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

        public RelayCommand<object> ConnectAllCommand { get; set; }
        public RelayCommand<object> DisconnectAllCommand { get; set; }
        public RelayCommand<object> OpenInfoPopup { get; set; }
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

        private void Action_OpenPopupAddOrUpdateDevice(object obj)
        {
            if (obj != null && obj is Guid) 
            {
                var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if(itemdeviceAction != null && itemdeviceAction.IsConnected)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainViewModel.Action_ShowToastNotify(ConstantDisplay.LANG_DEV_DeviceIsConnectedCanNotDoAction.TranslateString(), ToastNotifyType.Warning);
                    });
                }
                else
                {
                    IsCreateOrUpdatePopupOpen = true;
                    OverlayNavigate.NavigateTo<DeviceItemInfoUCViewModel>();
                }
            }
            else
            {
                IsCreateOrUpdatePopupOpen = true;
                OverlayNavigate.NavigateTo<DeviceItemInfoUCViewModel>();
            }
            var daw = (DeviceItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = obj == null ? null : obj.ToString();


        }

        private void Action_TongleOnOffDevice(object obj)
        {
            if (obj != null && obj is Guid)
            {
                var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if (itemdeviceAction != null)
                {
                    itemdeviceAction.IsConnectLoading = true;
                    if (itemdeviceAction.IsConnected)
                    {
                        Task.Run(() =>
                        {
                            var service = new TAMCommonService();
                            service.Action_Dicconnect(itemdeviceAction);
                            itemdeviceAction.IsConnectLoading = false;
                            itemdeviceAction.IsConnected = false;
                        });
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            var service = new TAMCommonService();

                            var res = await service.Action_Connect(itemdeviceAction);

                            itemdeviceAction.IsConnectLoading = false;
                            itemdeviceAction.IsConnected = res;

                        });
                    }
                }
            }
        }
        public void Action_ConnectDisconnectAllDevice(bool isconnect = true)
        {
            var service = new TAMCommonService();
            if (isconnect)
            {
                service.Action_ConnectAll();
            }
            else
            {
                service.Action_DisconnectAll();
            }
        }
    }
}
