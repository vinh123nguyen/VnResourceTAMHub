using TAMHubApp.Commands;

namespace TAMHubApp.ViewModels.UserControls
{
    class SelectDateRangeGetDataUCViewModel : BaseViewModel
    {
        private DeviceViewModel _deviceViewModel;
        public SelectDateRangeGetDataUCViewModel(DeviceViewModel deviceViewModel)
        {
            _deviceViewModel = deviceViewModel;

            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
        }

        #region Command
        public RelayCommand<object> CloseOverlayCommand { get; set; }

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
                //Action_SetDataInfoPopup(value);
            }
        }


        private DateTime _dateFrom { get; set; }
        public DateTime DateFrom { get => _dateFrom; set { _dateFrom = value; OnPropertyChanged(); } }

        private DateTime _dateTo { get; set; }
        public DateTime DateTo { get => _dateTo; set { _dateTo = value; OnPropertyChanged(); } }

        #endregion


        private void CloseOverlayCommandExcute(object parameter)
        {
            _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
        }

    }
}
