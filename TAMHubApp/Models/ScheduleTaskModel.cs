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
        public ScheduleCycleType CycleType { get => _cycleType; set { _cycleType = value; OnPropertyChanged(); CycleTypeView = $"LANG_CYCLE_{CycleType}".TranslateString(); } }

        private string _cycleTypeView { get; set; }
        public string CycleTypeView { get => _cycleTypeView; set { _cycleTypeView = value; OnPropertyChanged(); } }

        private int _hour { get; set; }
        public int Hour { get => _hour; set { _hour = value; OnPropertyChanged(); } }

        private int _minute { get; set; }
        public int Minute { get => _minute; set { _minute = value; OnPropertyChanged(); } }

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

        private DateTime? _nexttimerun { get; set; }
        public DateTime? NextTimeRun { get => _nexttimerun; set { _nexttimerun = value; OnPropertyChanged(); NextTimeRunView = value != null ? ((DateTime)value).ToString("dd/MM/yyyy HH:mm") : null; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
