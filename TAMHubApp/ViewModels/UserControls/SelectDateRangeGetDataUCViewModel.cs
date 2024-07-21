using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Data.Entities;

namespace TAMHubApp.ViewModels.UserControls
{
    class SelectDateRangeGetDataUCViewModel : BaseViewModel
    {
        private MainViewModel _mainWindow;
        private DataViewModel _dataViewModel;
        private SingletonTAMInstance _singletonTAMInstance;

        public SelectDateRangeGetDataUCViewModel(DataViewModel dataViewModel, 
            SingletonTAMInstance singletonTAMInstance,
            MainViewModel mainWindow)
        {
            _mainWindow = mainWindow;
            _dataViewModel = dataViewModel;
            _singletonTAMInstance = singletonTAMInstance;
            DateFrom = DateTime.Now.Date;
            DateTo = DateTime.Now.Date;
            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
            DownloadDataCommand = new RelayCommand<object>(x => { return DateFrom <= DateTo; }, x => { DownLoadLogData(false); });
            OnlyPushDataCommand = new RelayCommand<object>(x => { return true; }, x => { OnlyPushLogData(); });


            Items = _singletonTAMInstance.TAMs;
        }

        #region Command
        public RelayCommand<object> CloseOverlayCommand { get; set; }
        public RelayCommand<object> DownloadDataCommand { get; set; }
        public RelayCommand<object> OnlyPushDataCommand { get; set; }

        #endregion

        #region Prop

        private ObservableCollection<BaseTAMService> _items;
        private ObservableCollection<BaseTAMService> _selectedItems;
        public ObservableCollection<BaseTAMService> Items
        {
            get => _items ?? (_items = new ObservableCollection<BaseTAMService>());
            set => _items = value;
        }

        public ObservableCollection<BaseTAMService> SelectedItems
        {
            get => _selectedItems ?? (_selectedItems = new ObservableCollection<BaseTAMService>());
            set { _selectedItems = value; OnPropertyChanged(nameof(SelectedItems)); }
        }


        private DateTime _dateFrom { get; set; }
        public DateTime DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime _dateTo { get; set; }
        public DateTime DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

        private string _enrollNumber { get; set; }
        public string EnrollNumber { get => _enrollNumber; set {  _enrollNumber = value; OnPropertyChanged();} }

        #endregion


        private void CloseOverlayCommandExcute(object parameter)
        {
            _dataViewModel.IsOverlayPopupOpen = false;
        }

        public void DownLoadLogData(bool isGetAll = true)
        {
            var listIdSelected = SelectedItems.Select(x => x.Id).ToList();

            var lstDevice = _singletonTAMInstance.TAMs.Where(x => listIdSelected.Contains(x.Id)).ToList();

            if(lstDevice != null && lstDevice.Any())
            {
                var listTask = new List<Task>();
                _mainWindow.IsLoading = true;
                foreach(var device in lstDevice)
                {
                    var task = Task.Run(async () =>
                    {
                        var service = new TAMCommonService();
                        var result = false;
                        if (isGetAll)
                        {
                            //result = await service.Action_GetAllData(device);
                        }
                        else
                        {
                            result = await service.Action_GetDataByDate(device, DateFrom, DateTo, EnrollNumber);
                        }
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.IsLoading = false;
                            _dataViewModel.IsOverlayPopupOpen = false;
                            if (result)
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadSuccess.TranslateString(), device.Name), ToastNotifyType.Success);
                            else
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadError.TranslateString(), device.Name), ToastNotifyType.Error);
                        });
                    });
                    listTask.Add(task);
                }
            }
        }

        public void OnlyPushLogData()
        {
            var listIdSelected = SelectedItems.Select(x => x.Id).ToList();

            var lstDevice = _singletonTAMInstance.TAMs.Where(x => listIdSelected.Contains(x.Id)).ToList();

            if (lstDevice != null && lstDevice.Any())
            {
                _mainWindow.IsLoading = true;
                foreach (var device in lstDevice)
                {
                    var task = Task.Run(async () =>
                    {
                        var service = new TAMCommonService();
                        var result = false;
                        result = await service.Action_OnlyPushData(device, DateFrom, DateTo, EnrollNumber);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.IsLoading = false;
                            _dataViewModel.IsOverlayPopupOpen = false;
                            if (result)
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadSuccess.TranslateString(), device.Name), ToastNotifyType.Success);
                            else
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadError.TranslateString(), device.Name), ToastNotifyType.Error);
                        });
                    });
                }
            }
        }
    }
}
