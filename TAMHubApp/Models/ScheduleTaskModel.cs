using System.ComponentModel;
using System.Runtime.CompilerServices;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Models
{
    public class ScheduleTaskModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastTimeRun { get; set; }
        public ScheduleCycleType CycleType { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int? DayInMonth { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public bool IsRuning { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
