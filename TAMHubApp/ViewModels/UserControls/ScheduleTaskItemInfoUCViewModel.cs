using System.Collections.ObjectModel;
using System.Data;
using System.Reflection.Metadata;
using TAMHubApp.Commands;
using TAMHubApp.Data;
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

            _mainViewModel = mainViewModel;
            _unitOfWork = unitOfWork;

            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
            RemmoveScheduleCommand = new RelayCommand<object>(x => { return !string.IsNullOrEmpty(Id); }, RemmoveScheduleCommandExcute);
        }

        public RelayCommand<object> RemmoveScheduleCommand { get; set; }
        public RelayCommand<object> CloseOverlayCommand { get; set; }

        #region Observable Properties
        private ObservableCollection<Option>? _options;
        public ObservableCollection<Option>? CycleTypeOptions
        {
            get { return _options; }
            set { _options = value; OnPropertyChanged(); }
        }

        private Option? _cycleTypeSelected;
        public Option? CycleTypeSelected
        {
            get { return _cycleTypeSelected; }
            set { _cycleTypeSelected = value; OnPropertyChanged(); Event_ChangeCycleType(value); }
        }

        private ObservableCollection<Option>? _optionDayInWeeks;
        public ObservableCollection<Option>? DayInWeekOptions
        {
            get { return _optionDayInWeeks; }
            set { _optionDayInWeeks = value; OnPropertyChanged(); }
        }

        private Option? _dayInWeekSelected;
        public Option? DayInWeekSelected
        {
            get { return _dayInWeekSelected; }
            set { _dayInWeekSelected = value; OnPropertyChanged(); }
        }



        private string _id { get; set; }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
                //Action_SetDataInfoPopup(value);
            }
        }

        private string _cycleType { get; set; }
        public string CycleType
        {
            get => _cycleType;
            set { _cycleType = value; OnPropertyChanged(); }
        }

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
                }
                _mainViewModel.IsLoading = false;
            }
        }
    }
}
