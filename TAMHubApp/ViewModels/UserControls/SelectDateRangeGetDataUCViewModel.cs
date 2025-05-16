using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Data.Entities;
using TAMHubApp.Services;
using System.Collections.Concurrent;
using System.Windows.Threading;
using Riss.Devices;

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
            DownloadDataCommand = new RelayCommand<object>(x => { return DateFrom <= DateTo; }, x => { DownLoadLogData(); });
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

        public async void DownLoadLogData()
        {
            var listIdSelected = SelectedItems.Select(x => x.Id).ToList();

            var lstDevice = _singletonTAMInstance.TAMs.Where(x => listIdSelected.Contains(x.Id)).ToList();

            if(lstDevice != null && lstDevice.Any())
            {
                var listTask = new List<Task>();
                _mainWindow.IsLoading = true;

                var lstSDK = lstDevice.Select(x => x.DeviceSDKType).ToList();
                //if (lstSDK.Contains(TAMSDKTypes.E_ZKTECO))
                //{
                //    await GlobalEnumInOutConverter.InitUpdateEnumConverter("ZKTeco");
                //}
                //if (lstSDK.Contains(TAMSDKTypes.E_REALAND))
                //{
                //    await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
                //}
                var numdevice = lstDevice.Count();
                var lstDeviceRunDone = new ConcurrentBag<Guid>();
                foreach (var device in lstDevice)
                {
                    var task = Task.Run(async () =>
                    {
                        var service = new TAMCommonService();
                        var result = false;
                        result = await service.Action_GetDataByDateV2(service, device, DateFrom, DateTo.Date.AddDays(1).AddMicroseconds(-1), EnrollNumber);
                        lstDeviceRunDone.Add(device.Id);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if(lstDeviceRunDone.Count == numdevice)
                            {
                                _mainWindow.IsLoading = false;
                                _dataViewModel.IsOverlayPopupOpen = false;
                            }
                            if (result)
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadSuccess.TranslateString(), device.Name), ToastNotifyType.Success);
                            else
                                _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_GLO_DeviceDownloadError.TranslateString(), device.Name), ToastNotifyType.Error);
                        });
                    });
                    listTask.Add(task);
                }
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ConstantDisplay.LANG_DAT_PleaseSelectDevice.TranslateString(), ToastNotifyType.Warning);
                });
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
                        result = await service.Action_OnlyPushData(device, DateFrom, DateTo.Date.AddDays(1).AddMicroseconds(-1), EnrollNumber);
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
