using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows.Navigation;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private IUnitOfWork _unitOfWork;
        private IScheduleNavigationService _scheduleNavigationService;

        public ObservableCollection<ScheduleTaskModel> ScheduleTasks { get; set; }

        public ScheduleViewModel(IScheduleNavigationService scheduleNavigationService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _scheduleNavigationService = scheduleNavigationService;

            OpenInfoPopup = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateSchedule);
            InitScheduleTask().Wait();
        }


        public RelayCommand<object> OpenInfoPopup { get; set; }

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
                        IsRuning = false
                    };
                    lstScheduleDone.Add(scheduleadd);
                }
            }
            ScheduleTasks = new ObservableCollection<ScheduleTaskModel>(lstScheduleDone);
        }

        private void Action_OpenPopupAddOrUpdateSchedule(object parameter)
        {
            IsOverlayPopupOpen = true;
            OverlayNavigate.NavigateTo<ScheduleTaskItemInfoUCViewModel>();

            var daw = (ScheduleTaskItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = parameter == null ? null : parameter.ToString();
        }
    }
}
