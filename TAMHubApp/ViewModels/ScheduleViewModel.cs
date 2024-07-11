using Quartz;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows.Navigation;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private IUnitOfWork _unitOfWork;
        private IScheduleNavigationService _scheduleNavigationService;

        private ObservableCollection<ScheduleTaskModel> _scheduleItems;
        public ObservableCollection<ScheduleTaskModel> ScheduleTasks
        {
            get => _scheduleItems;
            set
            {
                _scheduleItems = value;
                OnPropertyChanged();
            }
        }

        public ScheduleViewModel(IScheduleNavigationService scheduleNavigationService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _scheduleNavigationService = scheduleNavigationService;

            OpenInfoPopupCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateSchedule);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffScheduleCommandExcute);
            InitScheduleTask().Wait();
        }

        private void Action_TongleOnOffScheduleCommandExcute(object obj)
        {
            throw new NotImplementedException();
        }

        public RelayCommand<object> OpenInfoPopupCommand { get; set; }
        public RelayCommand<object> TongleOnOffCommand { get; set; }

        public IScheduleNavigationService OverlayNavigate
        {
            get => _scheduleNavigationService;
            set { _scheduleNavigationService = value; OnPropertyChanged(); }
        }

        private bool _isOverlayPopupOpen;
        public bool IsOverlayPopupOpen
        {
            get => _isOverlayPopupOpen;
            set { _isOverlayPopupOpen = value; OnPropertyChanged(); }
        }

        public async Task InitScheduleTask()
        {
            var lstScheduleDb = await _unitOfWork.ScheduleTaskRepository.GetAllAsync();
            var lstScheduleDone = new List<ScheduleTaskModel>();

            if(lstScheduleDb != null && lstScheduleDb.Any())
            {
                foreach(var schedule in lstScheduleDb)
                {
                    var scheduleadd = new ScheduleTaskModel()
                    {
                        Id = schedule.Id,
                        Name = schedule.Name,
                        LastTimeRun = schedule.LastTimeRun,
                        Hour = schedule.Hour,
                        Minute = schedule.Minute,
                        DayInMonth = schedule.DayInMonth,
                        DayOfWeek = schedule.DayOfWeek,
                        CycleType = schedule.CycleType,
                        IsRuning = false,
                        IsLoading = false
                    };
                    lstScheduleDone.Add(scheduleadd);
                }
            }
            ScheduleTasks = new ObservableCollection<ScheduleTaskModel>(lstScheduleDone);
        }

        public async void UpdateScheduleTaskSingleton(Guid id)
        {
            var schedule = await _unitOfWork.ScheduleTaskRepository.GetByIdAsync(id);
            if (schedule != null)
            {
                var scheduleadd = new ScheduleTaskModel()
                {
                    Id = schedule.Id,
                    Name = schedule.Name,
                    LastTimeRun = schedule.LastTimeRun,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute,
                    DayInMonth = schedule.DayInMonth,
                    DayOfWeek = schedule.DayOfWeek,
                    CycleType = schedule.CycleType,
                    IsRuning = false,
                    IsLoading = false
                };
                ScheduleTasks.Add(scheduleadd);
            }
        }

        private void Action_OpenPopupAddOrUpdateSchedule(object parameter)
        {
            if(parameter != null)
            {
                var check = ScheduleTasks.FirstOrDefault(x => x.Id.ToString() == parameter.ToString());
                if(check != null && !check.IsRuning)
                {
                    check.IsRuning = true;
                }
            }

            IsOverlayPopupOpen = true;
            OverlayNavigate.NavigateTo<ScheduleTaskItemInfoUCViewModel>();

            var daw = (ScheduleTaskItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = parameter == null ? null : parameter.ToString();
        }
    }
}
