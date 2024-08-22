using Quartz;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection.Metadata;
using System.Xml.Linq;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ViewModels.UserControls
{
    public class ScheduleTaskItemInfoUCViewModel : BaseViewModel
    {
        private ScheduleViewModel _scheduleViewModel;
        private MainViewModel _mainViewModel;
        private IUnitOfWork _unitOfWork;
        public ScheduleTaskItemInfoUCViewModel(ScheduleViewModel scheduleViewModel, MainViewModel mainViewModel, IUnitOfWork unitOfWork)
        {
            _scheduleViewModel = scheduleViewModel;

            var dayofweek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(x => new Option()
            {
                Key = x.ToString(),
                Name = $"LANG_GLO_{x.ToString()}".TranslateString()
            });
            DayInWeekOptions = new ObservableCollection<Option>(dayofweek);

            var cycletype = Enum.GetValues(typeof(ScheduleCycleType)).Cast<ScheduleCycleType>().Select(x => new Option()
            {
                Key = x.ToString(),
                Name = $"LANG_CYCLE_{x.ToString()}".TranslateString()
            });
            CycleTypeOptions = new ObservableCollection<Option>(cycletype);

            var repeattype = Enum.GetValues(typeof(RepeatByType)).Cast<RepeatByType>().Select(x => new Option()
            {
                Key = x.ToString(),
                Name = $"LANG_REPEATTYPE_{x.ToString()}".TranslateString()
            });
            RepeatByTypeOptions = new ObservableCollection<Option>(repeattype);
            RepeatByTypeSelected = RepeatByTypeOptions.First();


            var times = new List<Option>();
            var startTime = TimeSpan.Zero; // Start at 00:00
            var endTime = new TimeSpan(23, 59, 59); // End at 23:59
            var interval = TimeSpan.FromMinutes(30);
            var dwd = interval.ToString(@"hh\:mm\:ss");
            while (startTime <= endTime)
            {
                times.Add(new Option()
                {
                    Key = startTime.ToString(@"hh\:mm\:ss"),
                    Name = startTime.ToString(@"hh\:mm"),
                    Time = startTime
                });
                startTime = startTime.Add(interval);
            }
            times.Add(new Option()
            {
                Key = endTime.ToString(@"hh\:mm\:ss"),
                Name = endTime.ToString(@"hh\:mm"),
                Time = endTime
            });
            TimeInDayOptions = new ObservableCollection<Option>(times);


            _mainViewModel = mainViewModel;
            _unitOfWork = unitOfWork;

            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
            RemmoveScheduleCommand = new RelayCommand<object>(x => { return true; }, RemmoveScheduleCommandExcute);
            SaveSchedulePopupInfoCommand = new RelayCommand<object>(x => { return true; }, SaveSchedulePopupInfoCommandExcute);
        }

        public RelayCommand<object> RemmoveScheduleCommand { get; set; }
        public RelayCommand<object> CloseOverlayCommand { get; set; }
        public RelayCommand<object> SaveSchedulePopupInfoCommand { get; set; }

        private string _id { get; set; }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
                Action_SetDataInfoPopup(value);
            }
        }

        private async void Action_SetDataInfoPopup(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            var currentschedule = await _unitOfWork.ScheduleTaskRepository.GetByIdAsync(Guid.Parse(value));
            if (currentschedule != null)
            {
                ScheduleName = currentschedule.Name;
                CycleTypeSelected = CycleTypeOptions?.FirstOrDefault(x => x.Key == currentschedule.CycleType.ToString());
                CycleHour = currentschedule.Hour.ToString();
                CycleMinute = currentschedule.Minute.ToString();

                if (CycleTypeSelected?.Key == ScheduleCycleType.Weekly.ToString())
                {
                    DayInWeekSelected = DayInWeekOptions?.FirstOrDefault(x => x.Key == currentschedule?.DayOfWeek.ToString());
                }
                if (CycleTypeSelected?.Key == ScheduleCycleType.Monthly.ToString())
                {
                    DayInMonth = currentschedule?.DayInMonth?.ToString();
                }


            }
        }

        #region Observable Properties
        private ObservableCollection<Option>? _options;
        public ObservableCollection<Option>? CycleTypeOptions { get { return _options; } set { _options = value; OnPropertyChanged(); } }

        private Option? _cycleTypeSelected;
        public Option? CycleTypeSelected { get { return _cycleTypeSelected; } set { _cycleTypeSelected = value; OnPropertyChanged(); Event_ChangeCycleType(value); } }

        private ObservableCollection<Option>? _optionDayInWeeks;
        public ObservableCollection<Option>? DayInWeekOptions { get { return _optionDayInWeeks; } set { _optionDayInWeeks = value; OnPropertyChanged(); } }

        private Option? _dayInWeekSelected;
        public Option? DayInWeekSelected { get { return _dayInWeekSelected; } set { _dayInWeekSelected = value; OnPropertyChanged(); } }
        private ObservableCollection<Option>? _repeatByTypeOptions;
        public ObservableCollection<Option>? RepeatByTypeOptions { get => _repeatByTypeOptions; set { _repeatByTypeOptions = value; OnPropertyChanged(); } }

        private Option? _repeatByTypeSelected;
        public Option? RepeatByTypeSelected { get => _repeatByTypeSelected; set { _repeatByTypeSelected = value; OnPropertyChanged(); } }

        private ObservableCollection<Option>? _excludeDaySelected;
        public ObservableCollection<Option>? ExcludeDaySelected { get { return _excludeDaySelected; } set { _excludeDaySelected = value; OnPropertyChanged(); } }
        
        private string _excludeTime2Hour { get; set; }
        public string ExcludeTime2Hour { get => _excludeTime2Hour; set { _excludeTime2Hour = value; OnPropertyChanged(); } }

        private string _cycleType { get; set; }
        public string CycleType { get => _cycleType; set { _cycleType = value; OnPropertyChanged(); } }

        private string _cycleHour { get; set; }
        public string CycleHour { get => _cycleHour; set { _cycleHour = value; OnPropertyChanged(); } }

        private string _cycleMinute { get; set; }
        public string CycleMinute { get => _cycleMinute; set { _cycleMinute = value; OnPropertyChanged(); } }

        private string _repeatByValue { get; set; }
        public string RepeatByValue { get => _repeatByValue; set { _repeatByValue = value; OnPropertyChanged(); } }

        private string _dayInMonth { get; set; }
        public string DayInMonth { get => _dayInMonth; set { _dayInMonth = value; OnPropertyChanged(); } }

        private string _validateMessage { get; set; }
        public string ValidateMessage { get => _validateMessage; set { _validateMessage = value; OnPropertyChanged(); } }

        private string _scheduleName { get; set; }
        public string ScheduleName { get => _scheduleName; set { _scheduleName = value; OnPropertyChanged(); } }

        private ObservableCollection<Option> _timeInDayOptions;
        public ObservableCollection<Option> TimeInDayOptions { get => _timeInDayOptions; set { _timeInDayOptions = value; OnPropertyChanged(); } }

        private Option? _excludeTimeFrom;
        public Option? ExcludeTimeFrom { get => _excludeTimeFrom; set { _excludeTimeFrom = value; OnPropertyChanged(); } }

        private Option? _excludeTimeTo;
        public Option? ExcludeTimeTo { get => _excludeTimeTo; set { _excludeTimeTo = value; OnPropertyChanged(); } }

        private Option? _excludeTime2From;
        public Option? ExcludeTime2From { get => _excludeTime2From; set { _excludeTime2From = value; OnPropertyChanged(); } }

        private Option? _excludeTime2To;
        public Option? ExcludeTime2To { get => _excludeTime2To; set { _excludeTime2To = value; OnPropertyChanged(); } }

        #endregion



        private void Event_ChangeCycleType(Option option)
        {
            CycleType = option.Key;
        }

        private void CloseOverlayCommandExcute(object parameter)
        {
            _scheduleViewModel.IsOverlayPopupOpen = false;
        }

        private async void RemmoveScheduleCommandExcute(object parameter)
        {
            var ObjectId = Guid.Empty;
            if (parameter != null && Guid.TryParse(parameter.ToString(), out ObjectId))
            {
                _mainViewModel.IsLoading = true;

                var objectdel = await _unitOfWork.ScheduleTaskRepository.GetByIdAsync(ObjectId);

                if(objectdel != null)
                {
                    _unitOfWork.ScheduleTaskRepository.Remove(objectdel);
                    await _unitOfWork.SaveChanageAsync();
                    _scheduleViewModel.IsOverlayPopupOpen = false;
                    var deleteObj = _scheduleViewModel.ScheduleTasks.FirstOrDefault(x => x.Id == ObjectId);
                    if(deleteObj != null)
                        _scheduleViewModel.ScheduleTasks.Remove(deleteObj);
                }
                _mainViewModel.IsLoading = false;
            }
        }

        private async void SaveSchedulePopupInfoCommandExcute(object parameter)
        {
            _mainViewModel.IsLoading = true;
            var validate = ValidateScheduleInfoPopup();
            if (!validate)
            {
                _mainViewModel.IsLoading = false;
                return;
            }
            var ObjectId = Guid.Empty;
            if (parameter != null && Guid.TryParse(parameter.ToString(), out ObjectId))
            {
                var currentObjectDb = await _unitOfWork.ScheduleTaskRepository.GetByIdAsync(ObjectId);
                var currentObject = _scheduleViewModel.ScheduleTasks.FirstOrDefault(x => x.Id == ObjectId);

                if (currentObject != null && currentObjectDb != null)
                {
                    currentObject.Name = ScheduleName;
                    currentObjectDb.Name = ScheduleName;

                    currentObject.LastTimeRun = currentObject.LastTimeRun;
                    currentObjectDb.LastTimeRun = currentObjectDb.LastTimeRun;

                    var Hour = int.TryParse(CycleHour, out int value) ? value : 0;
                    currentObject.Hour = Hour;
                    currentObjectDb.Hour = Hour;

                    var Minute = int.TryParse(CycleMinute, out int value2) ? value2 : 0;
                    currentObject.Minute = Minute;
                    currentObjectDb.Minute = Minute;

                    int? _DayInMonth = int.TryParse(DayInMonth, out int value3) ? value3 : null;
                    currentObject.DayInMonth = _DayInMonth;
                    currentObjectDb.DayInMonth = _DayInMonth;

                    currentObject.DayOfWeek = DayInWeekSelected != null ? (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayInWeekSelected?.Key) : null;
                    currentObjectDb.DayOfWeek = DayInWeekSelected != null ? (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayInWeekSelected?.Key) : null;

                    currentObject.CycleType = (ScheduleCycleType)Enum.Parse(typeof(ScheduleCycleType), CycleTypeSelected.Key);
                    currentObjectDb.CycleType = (ScheduleCycleType)Enum.Parse(typeof(ScheduleCycleType), CycleTypeSelected.Key);
                }

            }
            else
            {
                var currentObject = new ScheduleTask();

                currentObject.Id = Guid.NewGuid();
                currentObject.Name = ScheduleName;
                currentObject.LastTimeRun = currentObject.LastTimeRun;
                var Hour = int.TryParse(CycleHour, out int value) ? value : 0;
                currentObject.Hour = Hour;
                var Minute = int.TryParse(CycleMinute, out int value2) ? value2 : 0;
                currentObject.Minute = Minute;
                int? _DayInMonth = int.TryParse(DayInMonth, out int value3) ? value3 : null;
                currentObject.DayInMonth = _DayInMonth;
                currentObject.DayOfWeek = DayInWeekSelected != null ? (DayOfWeek)Enum.Parse(typeof(DayOfWeek), DayInWeekSelected?.Key) : null;

                currentObject.CycleType = (ScheduleCycleType)Enum.Parse(typeof(ScheduleCycleType), CycleTypeSelected.Key);


                var scheduleadd = new ScheduleTaskModel()
                {
                    Id = currentObject.Id,
                    Name = currentObject.Name,
                    LastTimeRun = currentObject.LastTimeRun,
                    Hour = currentObject.Hour,
                    Minute = currentObject.Minute,
                    DayInMonth = currentObject.DayInMonth,
                    DayOfWeek = currentObject.DayOfWeek,
                    CycleType = currentObject.CycleType,
                    IsRuning = false,
                    IsLoading = false
                };
                _scheduleViewModel.ScheduleTasks.Add(scheduleadd);

                await _unitOfWork.ScheduleTaskRepository.AddAsync(currentObject);
            }
            await _unitOfWork.SaveChanageAsync();
            _scheduleViewModel.IsOverlayPopupOpen = false;
            _mainViewModel.IsLoading = false;
        }

        private bool ValidateScheduleInfoPopup()
        {
            var result = true;
            var message = string.Empty;
            int dayinmonth = 0;
            int hour = 0;
            int minute = 0;
            if (string.IsNullOrEmpty(ScheduleName))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_Name.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (CycleTypeSelected == null || string.IsNullOrEmpty(CycleTypeSelected.Name))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_CycleType.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (CycleTypeSelected.Key == ScheduleCycleType.Weekly.ToString() && (DayInWeekSelected == null || string.IsNullOrEmpty(DayInWeekSelected.Key)))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_DayOfWeek.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (CycleTypeSelected.Key == ScheduleCycleType.Monthly.ToString() && string.IsNullOrEmpty(DayInMonth))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_DayInMonth.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (CycleTypeSelected.Key == ScheduleCycleType.Monthly.ToString() && !int.TryParse(DayInMonth, out dayinmonth))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_DayInMonth.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if (CycleTypeSelected.Key == ScheduleCycleType.Monthly.ToString() && (dayinmonth < 1 || dayinmonth > 31))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_DayInMonth.TranslateString() + " " + ConstantDisplay.LANG_GLO_OutOfRange.TranslateString();
            }
            else if ((string.IsNullOrEmpty(CycleHour) || string.IsNullOrEmpty(CycleMinute)) && CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString())
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_TimeRun.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if ((!int.TryParse(CycleHour, out hour) || !int.TryParse(CycleMinute, out minute)) && CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString())
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_TimeRun.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if ((hour < 0 || hour > 23 || minute < 0 || minute > 59))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_TimeRun.TranslateString() + " " + ConstantDisplay.LANG_GLO_OutOfRange.TranslateString();
            }
            else if(CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString() && (string.IsNullOrEmpty(RepeatByValue) || RepeatByTypeSelected == null))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_Cycle.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString() && (!int.TryParse(RepeatByValue, out hour) || !int.TryParse(RepeatByValue, out minute)))
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_Cycle.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if (CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString() && (ExcludeTimeFrom != null && ExcludeTimeTo != null) && ExcludeTimeFrom.Time >= ExcludeTimeTo.Time)
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_ExcludeTime.TranslateString() + " " + ConstantDisplay.LANG_GLO_TimeFromMustBeEarlyThanTimeTo.TranslateString();
            }
            else if (CycleTypeSelected.Key != ScheduleCycleType.RepeatBy.ToString() && (ExcludeTime2From != null && ExcludeTime2To != null) && ExcludeTime2From.Time >= ExcludeTime2To.Time)
            {
                result = false;
                message = ConstantDisplay.LANG_SCH_ExcludeTime2.TranslateString() + " " + ConstantDisplay.LANG_GLO_TimeFromMustBeEarlyThanTimeTo.TranslateString();
            }

            ValidateMessage = message;
            return result;

        }
    }
}
