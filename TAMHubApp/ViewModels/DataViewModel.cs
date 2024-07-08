using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Models;

namespace TAMHubApp.ViewModels
{
    public class DataViewModel : BaseViewModel
    {
        private IUnitOfWork _unitOfWork;
        private MainViewModel _mainViewModel;

        public DataViewModel(IUnitOfWork unitOfWork
            ,MainViewModel mainViewModel)
        {
            CurrentPage = 1;
            TotalRow = 0;
            _unitOfWork = unitOfWork;
            _mainViewModel = mainViewModel;
            PaginationBackCommand = new RelayCommand<object>(x=> { return CurrentPage != 1; }, x => { GetDataChangePaging(x, false); });
            PaginationForwardCommand = new RelayCommand<object>(x=> { return CurrentPage != TotalPages && TotalPages != 0; }, x => { GetDataChangePaging(x, true); });
            SearchCommand = new RelayCommand<object>(x=> { return true; }, x => { GetDataPaging(); });

            InitData();
        }

        #region Command

        public RelayCommand<object> PaginationBackCommand { get; set; }
        public RelayCommand<object> PaginationForwardCommand { get; set; }
        public RelayCommand<object> SearchCommand { get; set; }
        #endregion

        #region Properties
        private int _currentPage { get; set; }
        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); Event_UpdateCurrentPage(value); } }
        private int _totalRow { get; set; }
        public int TotalRow { get => _totalRow; set { _totalRow = value; OnPropertyChanged(); TotalPages = (int)Math.Ceiling((double)TotalRow / PageSize); } }
        private int PageSize = 50;
        private int TotalPages = 0;
        
        public ObservableCollection<Option> ListOptionsTAMs { get; set; }
        private Option _TAMSelected;
        public Option TAMSelected
        {
            get { return _TAMSelected; }
            set { _TAMSelected = value; OnPropertyChanged(); }
        }

        private string _enrollNumber { get; set; }
        public string EnrollNumber { get => _enrollNumber; set { _enrollNumber = value; OnPropertyChanged(); } }

        private bool _isAlreadySyncData { get; set; }
        public bool IsAlreadySyncData { get => _isAlreadySyncData; set {  _isAlreadySyncData = value; OnPropertyChanged(); } }

        private DateTime? _dateFrom { get; set; }
        public DateTime? DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime? _dateTo { get; set; }
        public DateTime? DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

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
        public void Event_UpdateCurrentPage(int currentpage)
        {

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
                        VerifyMode = x.VerifyMode,
                        InOutMode = x.InOutMode,
                        TimeLog = x.TimeLog,
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
                        VerifyMode = x.VerifyMode,
                        InOutMode = x.InOutMode,
                        TimeLog = x.TimeLog,
                    }).ToList();
                }

                if (!string.IsNullOrEmpty(EnrollNumber))
                {
                    lstlog = lstlog.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                }
                if (!string.IsNullOrEmpty(TAMSelected.Key))
                {
                    lstlog = lstlog.Where(x => x.DeviceID.ToString() == TAMSelected.Key).ToList();
                }
                if(DateFrom != null)
                {
                    lstlog = lstlog.Where(x => x.TimeLog >= DateFrom.Value).ToList();
                }
                if (DateTo != null)
                {
                    lstlog = lstlog.Where(x => x.TimeLog <= DateTo.Value).ToList();
                }
                lstlog = lstlog.OrderByDescending(x => x.TimeLog).ToList();

                lstlog = lstlog.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            });
        }


    }
}
