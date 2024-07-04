using System.Windows;
using System.Windows.Input;
using TAMHubApp.Commands;
using TAMHubApp.Services;

namespace TAMHubApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Navigate
        private INavigationService _navigationService;
        public INavigationService MainViewNavigate
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

        public RelayCommand<BaseViewModel> NavigateHomeCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDeviceCommand { get; set; }



        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            NavigateHomeCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { MainViewNavigate.NavigateTo<HomeViewModel>(); TabActive = "HomeView"; });
            NavigateDeviceCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { MainViewNavigate.NavigateTo<DeviceViewModel>(); TabActive = "DeviceView"; });

        }


        #endregion
    }
}
