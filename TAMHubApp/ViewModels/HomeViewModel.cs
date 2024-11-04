using System.Drawing;
using System.Timers;
using TAMHubApp.Commands;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Helpers;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Views;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace TAMHubApp.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private System.Timers.Timer _timer;
        public HomeViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            NavigateDeviceCommand = _mainViewModel.NavigateDeviceCommand;
            NavigateDataCommand = _mainViewModel.NavigateDataCommand;
            NavigateScheduleCommand = _mainViewModel.NavigateScheduleCommand;
            NavigateSettingCommand = _mainViewModel.NavigateSettingCommand;
            NavigateActionLogCommand = _mainViewModel.NavigateActionLogCommand;

            //NavigateDeviceCommand = new RelayCommand<BaseViewModel>(x => { return _mainViewModel.TabActive != "DeviceView"; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<DeviceViewModel>(); _mainViewModel.TabActive = "DeviceView"; });
            //NavigateDataCommand = new RelayCommand<BaseViewModel>(x => { return _mainViewModel.TabActive != "DataView"; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<DataViewModel>(); _mainViewModel.TabActive = "DataView"; });
            //NavigateScheduleCommand = new RelayCommand<BaseViewModel>(x => { return _mainViewModel.TabActive != "ScheduleView"; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<ScheduleViewModel>(); _mainViewModel.TabActive = "ScheduleView"; });
            //NavigateSettingCommand = new RelayCommand<BaseViewModel>(x => { return _mainViewModel.TabActive != "SettingView"; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<SettingViewModel>(); _mainViewModel.TabActive = "SettingView"; });
            //NavigateActionLogCommand = new RelayCommand<BaseViewModel>(x => { return _mainViewModel.TabActive != "ActionLogView"; }, x => { _mainViewModel.MainViewNavigate.NavigateTo<ActionLogViewModel>(); _mainViewModel.TabActive = "ActionLogView"; });


            _timer = new System.Timers.Timer(1000); // Update every minute
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.Date;
            ChangeImageWelcome();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.Date;
            CurrentHour = DateTime.Now.Hour;
        }
        public void ChangeImageWelcome()
        {
            var imagepath = "";
            var welcome = "";
            var currentHour = DateTime.Now.Hour;
            if(currentHour >= 6 && currentHour < 11)
            {
                imagepath = "/Resources/Images/image-morning.png";
                welcome = ConstantDisplay.LANG_GLO_GoodMorning;
            }
            else if(currentHour >= 11 && currentHour < 16)
            {
                imagepath = "/Resources/Images/image-noon.png";
                welcome = ConstantDisplay.LANG_GLO_GoodNoon;
            }
            else if(currentHour >= 16 && currentHour < 18)
            {
                imagepath = "/Resources/Images/image-afternoon.png";
                welcome = ConstantDisplay.LANG_GLO_GoodAfternoon;
            }
            else if(currentHour >= 18 && currentHour < 21)
            {
                imagepath = "/Resources/Images/image-night.png";
                welcome = ConstantDisplay.LANG_GLO_GoodNight;
            }
            else if(currentHour >= 21 || currentHour < 6)
            {
                imagepath = "/Resources/Images/image-midnight.png";
                welcome = ConstantDisplay.LANG_GLO_GoodMidNight;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                GreetingMessage = welcome.TranslateString();
                ImagePath = new BitmapImage(new Uri(imagepath, UriKind.RelativeOrAbsolute));
            });
        }

        public void onChangeLang()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var datetofweek = $"LANG_GLO_{CurrentDate.DayOfWeek.ToString()}".TranslateString();
                CurrentDateView = $"{datetofweek}, {CurrentDate.Day} {ConstantDisplay.LANG_GLO_Month.TranslateString()} {CurrentDate.Month}";
            });
        }

        private string _currentTime { get; set; }
        public string CurrentTime { get => _currentTime; set { _currentTime = value; OnPropertyChanged(); } }

        private string _greetingMessage { get; set; }
        public string GreetingMessage { get => _greetingMessage; set { _greetingMessage = value; OnPropertyChanged(); } }

        private BitmapImage _imagePath { get; set; }
        public BitmapImage ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(); } }

        private string _currentDateView { get; set; }
        public string CurrentDateView { get => _currentDateView; set { _currentDateView = value; OnPropertyChanged(); } }
        private DateTime _currentDate { get; set; }
        public DateTime CurrentDate 
        { 
            get => _currentDate; 
            set 
            {
                if (!_currentDate.Equals(value))
                {
                    _currentDate = value; 
                    OnPropertyChanged();
                    var datetofweek = $"LANG_GLO_{value.DayOfWeek.ToString()}".TranslateString();
                    CurrentDateView = $"{datetofweek}, {value.Day} {ConstantDisplay.LANG_GLO_Month.TranslateString()} {value.Month}";
                }
            } 
        }

        private int _currentHour{ get; set; }
        public int CurrentHour
        {
            get => _currentHour;
            set
            {
                if (!_currentHour.Equals(value))
                {
                    _currentHour = value;
                    ChangeImageWelcome();
                }
            }
        }

        private MainViewModel _mainViewModel;

        public RelayCommand<BaseViewModel> NavigateDeviceCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateDataCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateScheduleCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateSettingCommand { get; set; }
        public RelayCommand<BaseViewModel> NavigateActionLogCommand { get; set; }



    }
}
