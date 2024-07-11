using TAMHubApp.Commands;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.Views;
using TAMHubApp.Infrastructures.Helpers;

namespace TAMHubApp.ViewModels.UserControls
{
    class SelectDateRangeGetDataUCViewModel : BaseViewModel
    {
        private MainViewModel _mainWindow;
        private DeviceViewModel _deviceViewModel;
        private SingletonTAMInstance _singletonTAMInstance;

        public SelectDateRangeGetDataUCViewModel(DeviceViewModel deviceViewModel, 
            SingletonTAMInstance singletonTAMInstance,
            MainViewModel mainWindow)
        {
            _mainWindow = mainWindow;
            _deviceViewModel = deviceViewModel;
            _singletonTAMInstance = singletonTAMInstance;
            DateFrom = DateTime.Now.Date;
            DateTo = DateTime.Now.Date;
            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
            DownloadDataByDateCommand = new RelayCommand<object>(x => { return DateFrom <= DateTo; }, x => { DownLoadLogData(false); });
            DownloadAllDateCommand = new RelayCommand<object>(x => { return true; }, x => { DownLoadLogData(true); });
        }

        #region Command
        public RelayCommand<object> CloseOverlayCommand { get; set; }
        public RelayCommand<object> DownloadDataByDateCommand { get; set; }
        public RelayCommand<object> DownloadAllDateCommand { get; set; }

        #endregion

        #region Prop
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value))
                    CloseOverlayCommandExcute(true);
            }
        }


        private DateTime _dateFrom { get; set; }
        public DateTime DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime _dateTo { get; set; }
        public DateTime DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

        #endregion


        private void CloseOverlayCommandExcute(object parameter)
        {
            _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
        }

        public void DownLoadLogData(bool isGetAll = true)
        {
            var device = _singletonTAMInstance.TAMs.FirstOrDefault(x => x.Id.ToString() == Id);
            
            if (device != null)
            {
                _mainWindow.IsLoading = true;
                Task.Run(async() =>
                {
                    var service = new TAMCommonService();
                    var result = false;
                    if (isGetAll)
                    {
                        result = await service.Action_GetAllData(device);
                    }
                    else
                    {
                        result = await service.Action_GetDataByDate(device, DateFrom, DateTo);
                    }
                    _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
                    _mainWindow.IsLoading = false;
                });
            }

        }

    }
}
