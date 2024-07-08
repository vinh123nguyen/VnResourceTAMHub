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

            InitData();
        }

        #region Command

        public RelayCommand<object> PaginationBackCommand { get; set; }
        public RelayCommand<object> PaginationForwardCommand { get; set; }
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

            Task.Run(() =>
            {

            });
        }


    }
}
