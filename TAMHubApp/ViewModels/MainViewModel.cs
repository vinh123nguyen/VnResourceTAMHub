using System.Windows;
using System.Windows.Input;
using TAMHubApp.Commands;
using TAMHubApp.Services;

namespace TAMHubApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Navigate
        private ITabNavigationService _navigationService;
        public ITabNavigationService MainViewNavigate
        {
            get => _navigationService;
            set { _navigationService = value; OnPropertyChanged(); }
        }

        private string _tabActive;
        public string TabActive
        {
            get => _tabActive;
            set { _tabActive = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public RelayCommand<BaseViewModel> NavigateHomeCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDeviceCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDataCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateScheduleCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateSettingCommand { get; set; }



        public MainViewModel(ITabNavigationService navigationService)
        {
            TabActive = "HomeView";

            _navigationService = navigationService;

            NavigateHomeCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "HomeView"; }, x => { MainViewNavigate.NavigateTo<HomeViewModel>(); TabActive = "HomeView"; });
            NavigateDeviceCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "DeviceView"; }, x => { MainViewNavigate.NavigateTo<DeviceViewModel>(); TabActive = "DeviceView"; });
            NavigateDataCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "DataView"; }, x => { MainViewNavigate.NavigateTo<DataViewModel>(); TabActive = "DataView"; });
            NavigateScheduleCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "ScheduleView"; }, x => { MainViewNavigate.NavigateTo<ScheduleViewModel>(); TabActive = "ScheduleView"; });
            NavigateSettingCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "SettingView"; }, x => { MainViewNavigate.NavigateTo<SettingViewModel>(); TabActive = "SettingView"; });
        }


        #endregion
    }
}
