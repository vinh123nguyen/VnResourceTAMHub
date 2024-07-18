using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ViewModels
{
    public class ActionLogViewModel : BaseViewModel
    {
        public ActionLogViewModel(SingletonTAMInstance singletonTAMInstance, 
            MainViewModel mainViewModel,
            IUnitOfWork unitOfWork)
        {
            CurrentPage = 1;
            TotalRow = 0;
            CurrentPageView = "0/0";
            PaginationBackCommand = new RelayCommand<object>(x => { return CurrentPage != 1; }, x => { GetDataChangePaging(x, false); });
            PaginationForwardCommand = new RelayCommand<object>(x => { return CurrentPage != TotalPages && TotalPages != 0; }, x => { GetDataChangePaging(x, true); });
            SearchCommand = new RelayCommand<object>(x => { return true; }, x => { GetDataPaging(); });
            DeviceItems = singletonTAMInstance.TAMs;
            _mainViewModel = mainViewModel;
            _unitOfWork = unitOfWork;
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

        private MainViewModel _mainViewModel;
        private IUnitOfWork _unitOfWork;

        public ObservableCollection<BaseTAMService> SelectedDeviceItems
        {
            get => _selectedDeviceItems ?? (_selectedDeviceItems = new ObservableCollection<BaseTAMService>());
            set { _selectedDeviceItems = value; OnPropertyChanged(); }
        }

        private DateTime? _dateFrom { get; set; }
        public DateTime? DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime? _dateTo { get; set; }
        public DateTime? DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

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
        public async void GetDataPaging(bool setDefaultPaging = true)
        {
            _mainViewModel.IsLoading = true;

            if (setDefaultPaging == true)
            {
                CurrentPage = 1;
            }
            var lstDeviceIdSelected = SelectedDeviceItems.Select(x => x.Id).ToList();
            var lstQuery = await _unitOfWork.ActionLogRepository.GetAllAsync();
            if (lstDeviceIdSelected.Any())
            {
                lstQuery = lstQuery.Where(x => x.DeviceID != null && lstDeviceIdSelected.Contains(x.DeviceID.Value));
            }
            if (DateFrom != null)
            {
                lstQuery = lstQuery.Where(x => x.TimeLogTo >= DateFrom);
            }
            if( DateTo != null)
            {
                lstQuery = lstQuery.Where(x => x.TimeLogFrom <= DateTo);
            }
            TotalRow = lstQuery.Count();
            lstQuery = lstQuery.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
            var lstData = lstQuery.Select(x => new ActionLogModel()
            {
                Id = x.Id,
                TimeLog = $"{x.TimeLogFrom.ToString("dd/MM/yyyy HH:mm:ss")} - {x.TimeLogTo.ToString("dd/MM/yyyy HH:mm:ss")}",
                DateDownData = $"{x.DateDownDataFrom.Value.ToString("dd/MM/yyyy")} - {x.DateDownDataFrom.Value.ToString("dd/MM/yyyy")}",
                TAMName = x.Device.Name,
                Status = x.Status,
                StatusView = x.Status.TranslateString(),
                TotalNewlyData = x.TotalNewlyData,
                TotalRowDownload = x.TotalRowDownload,
                Message = x.Message,
                DownloadType = x.DownloadType.TranslateString(),
                TimeRun = x.TimeRun == null ? null : x.TimeRun.ToString(),
            }).ToList();

            ListActionLogs = new ObservableCollection<ActionLogModel>(lstData);
            _mainViewModel.IsLoading = false;
        }

    }
}
