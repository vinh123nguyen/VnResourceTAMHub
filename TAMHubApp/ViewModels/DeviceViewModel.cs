using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Models.TAMDataModel;
using TAMHubApp.ScheduleTasks.Jobs;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;
using zkemkeeper;
using Log = Serilog.Log;

namespace TAMHubApp.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(IDeviceNavigationService navigationService,
            SingletonTAMInstance singletonTAMInstance,
            ScheduleTaskService scheduleTaskService)
        {
            _navigationService = navigationService;
            OpenInfoPopup = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateDevice);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffDevice);
            OpenSelectDatePopupCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupSelectDateRange);

            DeviceItems = singletonTAMInstance.TAMs;
            _singletonTAMInstance = singletonTAMInstance;
            _scheduleTaskService = scheduleTaskService;
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

        public RelayCommand<object> OpenSelectDatePopupCommand { get; set; }
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

        private SingletonTAMInstance _singletonTAMInstance;
        private ScheduleTaskService _scheduleTaskService;

        private void Action_OpenPopupSelectDateRange(object parameter)
        {
            if(parameter != null)
            {
                IsCreateOrUpdatePopupOpen = true;
                OverlayNavigate.NavigateTo<SelectDateRangeGetDataUCViewModel>();

                var daw = (SelectDateRangeGetDataUCViewModel)OverlayNavigate.CurrentView;
                daw.Id = parameter.ToString();
            }
        }


        private void Action_OpenPopupAddOrUpdateDevice(object parameter)
        {
            _scheduleTaskService.GetRuningSchedule();
            IsCreateOrUpdatePopupOpen = true;
            OverlayNavigate.NavigateTo<DeviceItemInfoUCViewModel>();

            var daw = (DeviceItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = parameter == null ? null : parameter.ToString();
        }

        private void Action_TongleOnOffDevice(object obj)
        {
            if (obj != null && obj is Guid)
            {
                var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if (itemdeviceAction != null)
                {
                    itemdeviceAction.IsConnectLoading = true;
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
}
