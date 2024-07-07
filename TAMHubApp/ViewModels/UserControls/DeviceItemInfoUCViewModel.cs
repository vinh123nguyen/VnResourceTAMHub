using System.Collections.ObjectModel;
using TAMHubApp.Commands;

namespace TAMHubApp.ViewModels.UserControls
{
    class DeviceItemInfoUCViewModel : BaseViewModel
    {
        private DeviceViewModel _deviceViewModel;

        public DeviceItemInfoUCViewModel(DeviceViewModel deviceViewModel)
        {
            Options = new ObservableCollection<Option>
            {
                new Option { Name = "Option 1", Key = "1" },
                new Option { Name = "Option 2", Key = "2" },
                new Option { Name = "Option 3", Key = "3" }
            };

            SelectedOption = Options.First();
            _deviceViewModel = deviceViewModel;
            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
        }

        public RelayCommand<object> CloseOverlayCommand { get; set; }

        private void CloseOverlayCommandExcute(object parameter)
        {
            _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
        }
        private ObservableCollection<Option> _options;
        public ObservableCollection<Option> Options
        {
            get { return _options; }
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }
        private Option _selectedOption;
        public Option SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                _selectedOption = value;
                OnPropertyChanged();
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();

            }
        }
    }
    public class Option
    {

        public string Name { get; set; }
        public string Key { get; set; }
    }
}
