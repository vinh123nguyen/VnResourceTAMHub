using System.IO;
using System.Reflection;
using System.Windows.Navigation;
using TAMHubApp.Commands;
using TAMHubApp.Services;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        private ISettingNavigationService _settingNavigationService;
        public SettingViewModel(ISettingNavigationService settingNavigationService)
        {
            _settingNavigationService = settingNavigationService;

            NavigateGeneralCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "General"; }, x => { SettingNavigate.NavigateTo<GeneralSettingViewUCViewModel>(); TabActive = "General"; });
            NavigateZKTecoCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "ZKTeco"; }, x => { SettingNavigate.NavigateTo<ZKTecoSettingViewUCViewModel>(); TabActive = "ZKTeco"; });
            NavigateRealandCommand = new RelayCommand<BaseViewModel>(x => { return TabActive != "Realand"; }, x => { SettingNavigate.NavigateTo<RealandSettingViewUCViewModel>(); TabActive = "Realand"; });

            //AppVersion = "Version: " + Properties.Settings.Default.AppVersion;
            AppVersion = "Version: " + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            var dllPath = @"TAMHubApp.dll";
            if (File.Exists(dllPath))
            {
                ReleaseDate = File.GetLastWriteTime(dllPath).ToString("dd/MM/yyyy");
            }
            Task.Run(() =>
            {
                TabActive = "General";
                SettingNavigate.NavigateTo<GeneralSettingViewUCViewModel>(); 
            });
        }

        private string _tabActive;
        public string TabActive
        {
            get => _tabActive;
            set { _tabActive = value; OnPropertyChanged(); }
        }

        public ISettingNavigationService SettingNavigate
        {
            get => _settingNavigationService;
            set { _settingNavigationService = value; OnPropertyChanged(); }
        }

        public string AppVersion { get; set; }
        public string ReleaseDate { get; set; }

        public RelayCommand<BaseViewModel> NavigateGeneralCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateZKTecoCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateRealandCommand { get; set; }
    }
}
