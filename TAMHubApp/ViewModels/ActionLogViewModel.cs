using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ViewModels
{
    public class ActionLogViewModel : BaseViewModel
    {
        public ActionLogViewModel(SingletonTAMInstance singletonTAMInstance)
        {
            CurrentPage = 1;
            TotalRow = 0;
            CurrentPageView = "0/0";
            PaginationBackCommand = new RelayCommand<object>(x => { return CurrentPage != 1; }, x => { GetDataChangePaging(x, false); });
            PaginationForwardCommand = new RelayCommand<object>(x => { return CurrentPage != TotalPages && TotalPages != 0; }, x => { GetDataChangePaging(x, true); });
            SearchCommand = new RelayCommand<object>(x => { return true; }, x => { GetDataPaging(); });
            DeviceItems = singletonTAMInstance.TAMs;
        }


        #region Command
        public RelayCommand<object> PaginationBackCommand { get; set; }
        public RelayCommand<object> PaginationForwardCommand { get; set; }
        public RelayCommand<object> SearchCommand { get; set; }


        #endregion

        #region Paging prop
        private int _currentPage { get; set; }
        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); Event_UpdateCurrentPage(value); } }
        private int _totalRow { get; set; }
        public int TotalRow { get => _totalRow; set { _totalRow = value; OnPropertyChanged(); TotalPages = (int)Math.Ceiling((double)TotalRow / PageSize); Event_UpdateCurrentPage(CurrentPage); } }
        private int PageSize = 50;
        private int TotalPages = 0;
        private string _currentPageView { get; set; }
        public string CurrentPageView { get => _currentPageView; set { _currentPageView = value; OnPropertyChanged(); } }
        private string _dataRowInfo { get; set; }
        public string DataRowInfo { get => _dataRowInfo; set { _dataRowInfo = value; OnPropertyChanged(); } }

        private ObservableCollection<ActionLogModel> _listActionLogs { get; set; }
        public ObservableCollection<ActionLogModel> ListActionLogs { get => _listActionLogs; set { _listActionLogs = value; OnPropertyChanged(); } }


        private ObservableCollection<BaseTAMService> _deviceItems;
        private ObservableCollection<BaseTAMService> _selectedDeviceItems;
        public ObservableCollection<BaseTAMService> DeviceItems
        {
            get => _deviceItems ?? (_deviceItems = new ObservableCollection<BaseTAMService>());
            set => _deviceItems = value;
        }

        public ObservableCollection<BaseTAMService> SelectedDeviceItems
        {
            get => _selectedDeviceItems ?? (_selectedDeviceItems = new ObservableCollection<BaseTAMService>());
            set { _selectedDeviceItems = value; OnPropertyChanged(); }
        }

        #endregion

        public void Event_UpdateCurrentPage(int currentpage)
        {
            CurrentPageView = $"{currentpage}/{TotalPages}";
            var startitemcount = TotalRow != 0 ? (((CurrentPage - 1) * PageSize) + 1) : 0;
            var enditemcount = TotalRow != 0 ? ((CurrentPage - 1) * PageSize + (ListActionLogs != null ? ListActionLogs.Count : 0)) : 0;
            DataRowInfo = $"{startitemcount} - {enditemcount} of {TotalRow} items";
        }
        public void GetDataChangePaging(object parameter, bool isForward)
        {
            if (parameter != null && parameter.ToString() == "True")
            {
                CurrentPage = isForward ? TotalPages : 1;
            }
            else
            {
                CurrentPage += isForward ? 1 : -1;
            }
            GetDataPaging(false);
        }
        public void GetDataPaging(bool setDefaultPaging = true)
        {

        }

    }
}
