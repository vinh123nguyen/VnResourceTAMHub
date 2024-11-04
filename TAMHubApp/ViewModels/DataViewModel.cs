using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;
using TAMHubApp.Views.UserControls;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Views;

namespace TAMHubApp.ViewModels
{
    public class DataViewModel : BaseViewModel
    {
        private IUnitOfWork _unitOfWork;
        private MainViewModel _mainViewModel;
        private IDataNavigationService _dataNavigationService;

        public DataViewModel(IUnitOfWork unitOfWork
            ,MainViewModel mainViewModel
            , IDataNavigationService dataNavigationService
            , SingletonTAMInstance singletonTAMInstance)
        {
            IsAlreadySyncData = true;
            _dataNavigationService = dataNavigationService;
            CurrentPage = 1;
            TotalRow = 0;
            CurrentPageView = "0/0";
            _unitOfWork = unitOfWork;
            _mainViewModel = mainViewModel;
            PaginationBackCommand = new RelayCommand<object>(x=> { return CurrentPage != 1; }, x => { GetDataChangePaging(x, false); });
            PaginationForwardCommand = new RelayCommand<object>(x=> { return CurrentPage != TotalPages && TotalPages != 0; }, x => { GetDataChangePaging(x, true); });
            SearchCommand = new RelayCommand<object>(x=> { return true; }, x => { GetDataPaging(); });
            DownloadDataCommand = new RelayCommand<object>(x => { return true; }, OpenDownloadDataPopupExcute);
            CloseOverlayPopupCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayPopupExcute);
            RemoveDataCommand = new RelayCommand<object>(x => { return true; }, RemoveDataCommandExcute);
            Devices = singletonTAMInstance.TAMs;
            Event_UpdateCurrentPage();

            //InitData();
        }

        #region Command

        public RelayCommand<object> PaginationBackCommand { get; set; }
        public RelayCommand<object> PaginationForwardCommand { get; set; }
        public RelayCommand<object> SearchCommand { get; set; }
        public RelayCommand<object> DownloadDataCommand { get; set; }
        public RelayCommand<object> CloseOverlayPopupCommand { get; set; }
        public RelayCommand<object> RemoveDataCommand { get; set; }
        #endregion

        #region Properties
        public IDataNavigationService OverlayNavigate { get => _dataNavigationService; set { _dataNavigationService = value; OnPropertyChanged(); } }
        private bool _isOverlayPopupOpen;
        public bool IsOverlayPopupOpen { get => _isOverlayPopupOpen; set { _isOverlayPopupOpen = value; OnPropertyChanged(); } }
        private int _currentPage { get; set; }
        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
        private int _totalRow { get; set; }
        public int TotalRow { get => _totalRow; set { _totalRow = value; OnPropertyChanged(); TotalPages = (int)Math.Ceiling((double)TotalRow / PageSize); } }
        private int PageSize = 50;
        private int TotalPages = 0;
        
        private ObservableCollection<TAMScanLogModel> _listTAMScanLogs { get; set; }
        public ObservableCollection<TAMScanLogModel> ListTAMScanLogs { get => _listTAMScanLogs; set { _listTAMScanLogs = value; OnPropertyChanged(); } }
        public ObservableCollection<Option> ListOptionsTAMs { get; set; }
        private Option _TAMSelected;
        public Option TAMSelected
        {
            get { return _TAMSelected; }
            set { _TAMSelected = value; OnPropertyChanged(); }
        }

        private string _currentPageView { get; set; }
        public string CurrentPageView { get => _currentPageView; set { _currentPageView = value; OnPropertyChanged(); } }

        private string _dataRowInfo { get; set; }
        public string DataRowInfo { get => _dataRowInfo; set { _dataRowInfo = value; OnPropertyChanged(); } }

        private string _enrollNumber { get; set; }
        public string EnrollNumber { get => _enrollNumber; set { _enrollNumber = value; OnPropertyChanged(); } }

        private bool _isAlreadySyncData { get; set; }
        public bool IsAlreadySyncData { get => _isAlreadySyncData; set {  _isAlreadySyncData = value; OnPropertyChanged(); } }

        private DateTime? _dateFrom { get; set; }
        public DateTime? DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime? _dateTo { get; set; }
        public DateTime? DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }


        private ObservableCollection<BaseTAMService> _devices;
        private ObservableCollection<BaseTAMService> _selectedDevices;
        public ObservableCollection<BaseTAMService> Devices
        {
            get => _devices ?? (_devices = new ObservableCollection<BaseTAMService>());
            set { _devices = value; OnPropertyChanged(); }
            }

        public ObservableCollection<BaseTAMService> SelectedDevices
        {
            get => _selectedDevices ?? (_selectedDevices = new ObservableCollection<BaseTAMService>());
            set { _selectedDevices = value; OnPropertyChanged(); }
        }

        #endregion

        public async void InitData()
        {
            var lstDevice = await _unitOfWork.DeviceRepository.GetAllAsync();
            var lstDeviceOptions = new ObservableCollection<Option>();
            lstDevice.ToList().ForEach(x =>
            {
                lstDeviceOptions.Add(new Option()
                {
                    Key = x.Id.ToString(),
                    Name = x.Name
                });
            });
            var defaulselect = new Option() { Key = "", Name = "Vui lòng chọn..." };
            lstDeviceOptions.Insert(0, defaulselect);
            TAMSelected = defaulselect;
            ListOptionsTAMs = lstDeviceOptions;
        }
        public void Event_UpdateCurrentPage()
        {
            CurrentPageView = $"{CurrentPage}/{TotalPages}";
            var startitemcount = TotalRow != 0 ? (((CurrentPage - 1) * PageSize) + 1): 0;
            var enditemcount = TotalRow != 0 ? ((CurrentPage-1) * PageSize + (ListTAMScanLogs != null ? ListTAMScanLogs.Count : 0)) : 0;
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
            _mainViewModel.IsLoading = true;

            if(setDefaultPaging == true)
            {
                CurrentPage = 1;
            }

            Task.Run(async () =>
            {
                var lstlog = new List<TAMScanLogModel>();
                if (!IsAlreadySyncData)
                {
                    var lstQuery = await _unitOfWork.TAMScanLogRepository.GetAllAsync();
                    lstlog = lstQuery.Select(x => new TAMScanLogModel()
                    {
                        Id = x.Id,
                        DeviceID = x.DeviceID,
                        EnrollNumber = x.EnrollNumber,
                        VerifyMode = x.VerifyModeView,
                        InOutModeView = x.InOutModeView,
                        TimeLog = x.TimeLog,
                        TimeLogView = x.TimeLog.ToString("dd/MM/yyyy HH:mm:ss"),
                        //DeviceName = x.Device.Name
                    }).ToList();
                }
                else
                {
                    var lstQuery = await _unitOfWork.TAMScanLogHistoryRepository.GetAllAsync();
                    lstlog = lstQuery.Select(x => new TAMScanLogModel()
                    {
                        Id = x.Id,
                        DeviceID = x.DeviceID,
                        EnrollNumber = x.EnrollNumber,
                        VerifyMode = x.VerifyModeView,
                        InOutModeView = x.InOutModeView,
                        TimeLog = x.TimeLog,
                        TimeLogView = x.TimeLog.ToString("dd/MM/yyyy HH:mm:ss"),
                        //DeviceName = x.Device.Name
                    }).ToList();
                }

                if (!string.IsNullOrEmpty(EnrollNumber))
                {
                    lstlog = lstlog.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                }
                if(SelectedDevices != null && SelectedDevices.Any())
                {
                    var selectedId = SelectedDevices.Select(x => x.Id).ToList();
                    lstlog = lstlog.Where(x => selectedId.Contains(x.DeviceID)).ToList();
                }
                if(DateFrom != null)
                {
                    lstlog = lstlog.Where(x => x.TimeLog >= DateFrom.Value).ToList();
                }
                if (DateTo != null)
                {
                    lstlog = lstlog.Where(x => x.TimeLog <= DateTo.Value.Date.AddDays(1).AddMicroseconds(-1)).ToList();
                }
                TotalRow = lstlog.Count;
                lstlog = lstlog.OrderByDescending(x => x.TimeLog).ToList();

                lstlog = lstlog.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                var lstDevice = (await _unitOfWork.DeviceRepository.GetAllAsync()).ToList();

                var newlist = from l in lstlog
                              join dvl in lstDevice on l.DeviceID equals dvl.Id into dv
                              from devicelj in dv.DefaultIfEmpty()
                              select new TAMScanLogModel()
                              {
                                  Id = l.Id,
                                  DeviceID = l.DeviceID,
                                  EnrollNumber = l.EnrollNumber,
                                  VerifyMode = l.VerifyMode,
                                  InOutModeView = l.InOutModeView,
                                  TimeLog = l.TimeLog,
                                  TimeLogView = l.TimeLogView,
                                  DeviceName = devicelj?.Name ?? "",
                              };
                ListTAMScanLogs = new ObservableCollection<TAMScanLogModel>(newlist);

                _mainViewModel.IsLoading = false;
                Event_UpdateCurrentPage();
            });
        }

        private void OpenDownloadDataPopupExcute(object obj)
        {
            IsOverlayPopupOpen = true;
            OverlayNavigate.NavigateTo<SelectDateRangeGetDataUCViewModel>();
        }
        private void CloseOverlayPopupExcute(object obj)
        {
            IsOverlayPopupOpen = false;
        }
        private void RemoveDataCommandExcute(object obj)
        {
            _mainViewModel.IsLoading = true;

            Task.Run(async () =>
            {
                if (IsAlreadySyncData)
                {
                    var lstQuery = await _unitOfWork.TAMScanLogHistoryRepository.GetAllAsync();

                    if (!string.IsNullOrEmpty(EnrollNumber))
                    {
                        lstQuery = lstQuery.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                    }
                    if (SelectedDevices != null && SelectedDevices.Any())
                    {
                        var selectedId = SelectedDevices.Select(x => x.Id).ToList();
                        lstQuery = lstQuery.Where(x => selectedId.Contains(x.DeviceID)).ToList();
                    }
                    if (DateFrom != null)
                    {
                        lstQuery = lstQuery.Where(x => x.TimeLog >= DateFrom.Value).ToList();
                    }
                    if (DateTo != null)
                    {
                        lstQuery = lstQuery.Where(x => x.TimeLog <= DateTo.Value.Date.AddDays(1).AddMicroseconds(-1)).ToList();
                    }

                    _unitOfWork.TAMScanLogHistoryRepository.RemoveRange(lstQuery);

                    await _unitOfWork.SaveChanageAsync();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainViewModel.IsLoading = false;
                        _mainViewModel.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_DAT_DeleteData.TranslateString(), lstQuery.Count()), ToastNotifyType.Success);
                    });
                }
                else
                {
                    var lstQuery = await _unitOfWork.TAMScanLogRepository.GetAllAsync();

                    if (!string.IsNullOrEmpty(EnrollNumber))
                    {
                        lstQuery = lstQuery.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                    }
                    if (SelectedDevices != null && SelectedDevices.Any())
                    {
                        var selectedId = SelectedDevices.Select(x => x.Id).ToList();
                        lstQuery = lstQuery.Where(x => selectedId.Contains(x.DeviceID)).ToList();
                    }
                    if (DateFrom != null)
                    {
                        lstQuery = lstQuery.Where(x => x.TimeLog >= DateFrom.Value).ToList();
                    }
                    if (DateTo != null)
                    {
                        lstQuery = lstQuery.Where(x => x.TimeLog <= DateTo.Value.Date.AddDays(1).AddMicroseconds(-1)).ToList();
                    }

                    _unitOfWork.TAMScanLogRepository.RemoveRange(lstQuery);

                    await _unitOfWork.SaveChanageAsync();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainViewModel.IsLoading = false;
                        _mainViewModel.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_DAT_DeleteData.TranslateString(), lstQuery.Count()), ToastNotifyType.Success);
                    });
                }
                
                GetDataPaging();
            });
        }

    }
}
