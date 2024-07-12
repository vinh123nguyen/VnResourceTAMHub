using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Models;
using TAMHubApp.Services;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class ScheduleViewModel : BaseViewModel
    {
        private ScheduleTaskService _scheduleTaskService;
        private MainViewModel _mainViewModel;
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

        public ScheduleViewModel(IScheduleNavigationService scheduleNavigationService
            , MainViewModel mainViewModel
            , ScheduleTaskService scheduleTaskService)
        {
            _scheduleTaskService = scheduleTaskService;
            _mainViewModel = mainViewModel;
            _scheduleNavigationService = scheduleNavigationService;

            OpenInfoPopupCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateSchedule);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffScheduleCommandExcute);
            StartAllCommand = new RelayCommand<object>(x => { return true; }, Action_StartAllCommandExcute);
            StopAllCommand = new RelayCommand<object>(x => { return true; }, Action_StopAllCommandExcute);
            ScheduleTasks = scheduleTaskService.ScheduleTasks;
        }

        private async void Action_StopAllCommandExcute(object obj)
        {
            foreach(var schedule in ScheduleTasks)
            {
                if (!schedule.IsRuning)
                    continue;
                await Task.Run(async () =>
                {
                    await _scheduleTaskService.StopSchedule(schedule);
                });
            }
            await _scheduleTaskService.UpdateScheduleRuningStatus();
        }

        private async void Action_StartAllCommandExcute(object obj)
        {
            foreach (var schedule in ScheduleTasks)
            {
                if (schedule.IsRuning)
                    continue;
                await Task.Run(async () =>
                {
                    await _scheduleTaskService.New_AddJob(schedule);
                });
            }
            await _scheduleTaskService.UpdateScheduleRuningStatus();
        }

        private async void Action_TongleOnOffScheduleCommandExcute(object obj)
        {
            if (obj != null && obj is Guid)
            {
                var itemAction = ScheduleTasks.FirstOrDefault(x => x.Id == (Guid)obj);
                if(itemAction != null)
                {
                    itemAction.IsLoading = true;
                    if (!itemAction.IsRuning)
                    {
                        await _scheduleTaskService.New_AddJob(itemAction);
                    }
                    else
                    {
                        await _scheduleTaskService.StopSchedule(itemAction);
                    }
                    await _scheduleTaskService.UpdateScheduleRuningStatus();
                    itemAction.IsLoading = false;
                }
            }
        }

        public RelayCommand<object> OpenInfoPopupCommand { get; set; }
        public RelayCommand<object> TongleOnOffCommand { get; set; }
        public RelayCommand<object> StartAllCommand { get; set; }
        public RelayCommand<object> StopAllCommand { get; set; }

        public IScheduleNavigationService OverlayNavigate { get => _scheduleNavigationService; set { _scheduleNavigationService = value; OnPropertyChanged(); } }

        private bool _isOverlayPopupOpen;
        public bool IsOverlayPopupOpen { get => _isOverlayPopupOpen; set { _isOverlayPopupOpen = value; OnPropertyChanged(); } }

        
        private void Action_OpenPopupAddOrUpdateSchedule(object parameter)
        {
            if(parameter != null)
            {
                var check = ScheduleTasks.FirstOrDefault(x => x.Id.ToString() == parameter.ToString());
                if(check != null && check.IsRuning)
                {
                    _mainViewModel.Action_ShowToastNotify(ConstantDisplay.LANG_SCH_ScheduleIsRuningCanNotDoAction.TranslateString(), Infrastructures.Enums.ToastNotifyType.Warning);
                    return;
                }
            }

            IsOverlayPopupOpen = true;
            OverlayNavigate.NavigateTo<ScheduleTaskItemInfoUCViewModel>();

            var daw = (ScheduleTaskItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = parameter == null ? null : parameter.ToString();
        }
    }
}
