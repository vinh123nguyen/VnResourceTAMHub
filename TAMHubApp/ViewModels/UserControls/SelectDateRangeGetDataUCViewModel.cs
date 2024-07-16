using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;

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
            //DownloadAllDateCommand = new RelayCommand<object>(x => { return true; }, x => { DownLoadLogData(true); });


            var fesf = new List<DeviceMultiSelectItem>();
            for (int i = 1; i <= 100; i++)
            {
                fesf.Add(new DeviceMultiSelectItem()
                {
                    Id = i.ToString(),
                    Name = "Name " + i.ToString(),
                });
            }
            Items = new ObservableCollection<DeviceMultiSelectItem>(fesf);
        }

        #region Command
        public RelayCommand<object> CloseOverlayCommand { get; set; }
        public RelayCommand<object> DownloadDataCommand { get; set; }
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

        private ObservableCollection<DeviceMultiSelectItem> _items;
        private ObservableCollection<DeviceMultiSelectItem> _selectedItems;
        public ObservableCollection<DeviceMultiSelectItem> Items
        {
            get => _items ?? (_items = new ObservableCollection<DeviceMultiSelectItem>());
            set => _items = value;
        }

        public ObservableCollection<DeviceMultiSelectItem> SelectedItems
        {
            get => _selectedItems ?? (_selectedItems = new ObservableCollection<DeviceMultiSelectItem>());
            set { _selectedItems = value; OnPropertyChanged(nameof(SelectedItems)); }
        }


        private DateTime _dateFrom { get; set; }
        public DateTime DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime _dateTo { get; set; }
        public DateTime DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

        #endregion


        private void CloseOverlayCommandExcute(object parameter)
        {
            _dataViewModel.IsOverlayPopupOpen = false;
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
                    _dataViewModel.IsOverlayPopupOpen = false;
                    _mainWindow.IsLoading = false;
                });
            }

        }



    }
}
