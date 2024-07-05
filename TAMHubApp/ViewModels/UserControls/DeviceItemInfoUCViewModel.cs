using TAMHubApp.Commands;

namespace TAMHubApp.ViewModels.UserControls
{
    class DeviceItemInfoUCViewModel : BaseViewModel
    {
        private DeviceViewModel _deviceViewModel;

        public DeviceItemInfoUCViewModel(DeviceViewModel deviceViewModel)
        {
            _deviceViewModel = deviceViewModel;
            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
        }

        public RelayCommand<object> CloseOverlayCommand { get; set; }

        private void CloseOverlayCommandExcute(object parameter)
        {
            _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
        }

    }
}
