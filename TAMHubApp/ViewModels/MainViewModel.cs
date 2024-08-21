using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Models;
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

        private bool _isShowingToast { set; get; }
        public bool IsShowingToast
        {
            get => _isShowingToast;
            set { _isShowingToast = value; OnPropertyChanged(); }
        }

        private string _toastMessage { set; get; }
        public string ToastMessage
        {
            get => _toastMessage;
            set { _toastMessage = value; OnPropertyChanged(); }
        }

        private Brush _toastColor { set; get; }
        public Brush ToastColor
        {
            get => _toastColor;
            set { _toastColor = value; OnPropertyChanged(); }
        }

        public RelayCommand<BaseViewModel> NavigateHomeCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDeviceCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDataCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateScheduleCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateSettingCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateActionLogCommand { get; set; }

        public void Action_ShowToastNotify(string message, ToastNotifyType type)
        {
            ToastMessage = message;
            var color = Color.FromRgb(255, 255, 255);

            if (type == ToastNotifyType.Success)
                color = Color.FromRgb(179, 255, 179);
            else if (type == ToastNotifyType.Error)
                color = Color.FromRgb(255, 133, 102);
            else if (type == ToastNotifyType.Warning)
                color = Color.FromRgb(255, 255, 102);

            ToastColor = new SolidColorBrush(color);

            IsShowingToast = true;
            IsShowingToast = false;
        }

        private ObservableCollection<LanguageOption> _listLanguages { get; set; }
        public ObservableCollection<LanguageOption> ListLanguages 
        {
            get => _listLanguages;
            set
            {
                _listLanguages = value;
                OnPropertyChanged();
            }
        }
        private LanguageOption _selectedLanguages { get; set; }
        public LanguageOption SelectedLanguages
        {
            get => _selectedLanguages;
            set
            {
                _selectedLanguages = value;
                if (Properties.Settings.Default.Language != value.Key)
                {
                    OnPropertyChanged();
                    LocalizationHelper.SetLanguage(value.Key);
                    Properties.Settings.Default.Language = value.Key;
                    var lsttemp = new ObservableCollection<LanguageOption>();
                    foreach (var item in ListLanguages)
                    {
                        item.Language = item.LanguageKey.TranslateString();
                        lsttemp.Add(item);
                    }
                    ListLanguages = lsttemp;
                }
            }
        }
        public MainViewModel(ITabNavigationService navigationService)
        {
            ToastColor = Brushes.LightGreen;
            _navigationService = navigationService;

            ListLanguages = new ObservableCollection<LanguageOption>()
            {
                new LanguageOption()
                {
                    Key = "en",
                    Language = ConstantDisplay.LANG_GLO_English.TranslateString(),
                    LanguageKey = ConstantDisplay.LANG_GLO_English,
                    ImagePath = new BitmapImage(new Uri("/Resources/Images/image-language-en.png", UriKind.RelativeOrAbsolute))
                },
                new LanguageOption()
                {
                    Key = "vi",
                    Language = ConstantDisplay.LANG_GLO_Vietnamese.TranslateString(),
                    LanguageKey = ConstantDisplay.LANG_GLO_Vietnamese,
                    ImagePath = new BitmapImage(new Uri("/Resources/Images/image-language-vi.png", UriKind.RelativeOrAbsolute))
                },
            };
            var defaultlang = Properties.Settings.Default.Language != null ? Properties.Settings.Default.Language : "vi";
            SelectedLanguages = ListLanguages.FirstOrDefault(x => x.Key == defaultlang);
            OnPropertyChanged("SelectedLanguages");

            NavigateHomeCommand = new RelayCommand<BaseViewModel>(x => { return true; }, x => { MainViewNavigate.NavigateTo<HomeViewModel>(); TabActive = "HomeView"; });
            NavigateDeviceCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "DeviceView"; }, x => { MainViewNavigate.NavigateTo<DeviceViewModel>(); TabActive = "DeviceView"; });
            NavigateDataCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "DataView"; }, x => { MainViewNavigate.NavigateTo<DataViewModel>(); TabActive = "DataView"; });
            NavigateScheduleCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "ScheduleView"; }, x => { MainViewNavigate.NavigateTo<ScheduleViewModel>(); TabActive = "ScheduleView"; });
            NavigateSettingCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "SettingView"; }, x => { MainViewNavigate.NavigateTo<SettingViewModel>(); TabActive = "SettingView"; });
            NavigateActionLogCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "ActionLogView"; }, x => { MainViewNavigate.NavigateTo<ActionLogViewModel>(); TabActive = "ActionLogView"; });

            Task.Run(MainViewNavigate.NavigateTo<HomeViewModel>);
        }


        #endregion
    }
}
