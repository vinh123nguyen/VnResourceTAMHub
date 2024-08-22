using System.ComponentModel;
using System.Runtime.CompilerServices;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;

namespace TAMHubApp.Models
{
    public class ScheduleTaskModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }

        private string _name { get; set; }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private DateTime? _lastTimeRun { get; set; }
        public DateTime? LastTimeRun { get => _lastTimeRun; set { _lastTimeRun = value; OnPropertyChanged(); LastTimeRunView = value != null ? ((DateTime)value).ToString("dd/MM/yyyy HH:mm") : null; } }

        private ScheduleCycleType _cycleType { get; set; }
        public ScheduleCycleType CycleType { get => _cycleType; set { _cycleType = value; OnPropertyChanged(); CycleTypeView = SetCycleTypeView(); } }

        private string _cycleTypeView { get; set; }
        public string CycleTypeView { get => _cycleTypeView; set { _cycleTypeView = value; OnPropertyChanged(); } }

        private int _hour { get; set; }
        public int Hour { get => _hour; set { _hour = value; OnPropertyChanged(); SetTimeRunView(value, Minute); } }

        private int _minute { get; set; }
        public int Minute { get => _minute; set { _minute = value; OnPropertyChanged(); SetTimeRunView(Hour, value); } }

        private int? _dayInMonth { get; set; }
        public int? DayInMonth { get => _dayInMonth; set { _dayInMonth = value; OnPropertyChanged(); } }

        private DayOfWeek? _dayOfWeek { get; set; }
        public DayOfWeek? DayOfWeek { get => _dayOfWeek; set { _dayOfWeek = value; OnPropertyChanged(); DayOfWeekView = value == null ? null : $"LANG_GLO_{value.ToString()}".TranslateString(); } }

        private string _dayOfWeekView { get; set; }
        public string DayOfWeekView { get => _dayOfWeekView; set { _dayOfWeekView = value; OnPropertyChanged(); } }

        private bool _isRuning { get; set; }
        public bool IsRuning { get => _isRuning; set { _isRuning = value; OnPropertyChanged(); } }

        private bool _isLoading { get; set; }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        private string _lasttimerunView { get; set; }
        public string LastTimeRunView { get => _lasttimerunView; set { _lasttimerunView = value; OnPropertyChanged(); } }

        private string _nexttimerunView { get; set; }
        public string NextTimeRunView { get => _nexttimerunView; set { _nexttimerunView = value; OnPropertyChanged(); } }

        private string _timerunview { get; set; }
        public string TimeRunView { get => _timerunview; set { _timerunview = value; OnPropertyChanged(); } }

        private void SetTimeRunView(int h, int m)
        {
            TimeSpan timeSpan = new TimeSpan(h, m, 0);
            TimeRunView = timeSpan.ToString(@"hh\:mm");
        }

        private DateTime? _nexttimerun { get; set; }
        public DateTime? NextTimeRun { get => _nexttimerun; set { _nexttimerun = value; OnPropertyChanged(); NextTimeRunView = value != null ? ((DateTime)value).ToString("dd/MM/yyyy HH:mm") : null; } }

        private int? _repeatByValue { get; set; }
        public int? RepeatByValue { get => _repeatByValue; set { _repeatByValue = value; OnPropertyChanged(); } }

        private RepeatByType? _repeatByType { get; set; }
        public RepeatByType? RepeatByType { get => _repeatByType; set { _repeatByType = value; OnPropertyChanged(); } }

        private string SetCycleTypeView()
        {
            if (CycleType == ScheduleCycleType.RepeatBy && RepeatByValue != null && RepeatByType != null)
            {
                return $"LANG_CYCLE_{CycleType}".TranslateString() + $" {RepeatByValue} " + $"LANG_REPEATTYPE_{RepeatByType}".TranslateString();
            }
            else
            {
                return $"LANG_CYCLE_{CycleType}".TranslateString();
            }
        }

        public TimeSpan? StartTime { get; set; }
        public string? ExcludeDayOfWeek { get; set; }
        public TimeSpan? ExcludeTimeFrom { get; set; }
        public TimeSpan? ExcludeTimeTo { get; set; }
        public TimeSpan? ExcludeTime2From { get; set; }
        public TimeSpan? ExcludeTime2To { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
