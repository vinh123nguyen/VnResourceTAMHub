using TAMHubApp.Commands;
using TAMHubApp.ViewModels;

namespace TAMHubApp.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            NavigateDeviceCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<DeviceViewModel>(); });
            NavigateDataCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<DataViewModel>(); });
            NavigateScheduleCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<ScheduleViewModel>(); });
            NavigateSettingCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<SettingViewModel>(); });
            NavigateActionLogCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<ActionLogViewModel>(); });
                
        }

        private MainViewModel _mainViewModel;

        public RelayCommand<BaseViewModel> NavigateDeviceCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDataCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateScheduleCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateSettingCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateActionLogCommand { get; set; }



    }
}
