using System.Globalization;
using System.Windows.Input;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Services;

namespace TAMHubApp.ViewModels.UserControls
{
    public class GeneralSettingViewUCViewModel : BaseViewModel
    {
        private IUnitOfWork _unitOfWork;
        private MainViewModel _mainViewModel;

        public GeneralSettingViewUCViewModel(IUnitOfWork unitOfWork, MainViewModel mainViewModel)
        {
            _unitOfWork = unitOfWork;
            _mainViewModel = mainViewModel;

            SaveConfigCommand = new RelayCommand<object>(x => { return true; }, SaveConfigCommandExcute);
            CheckForUpdateCommand = new RelayCommand<object>(x => { return true; }, CheckForUpdateCommandExcute); 

            LoadDataConfig().Wait();
        }

        private void CheckForUpdateCommandExcute(object obj)
        {
            AutoUpdaterService.CheckForUpdate();
        }

        private bool _isSyncDb {  get; set; }
        public bool IsSyncDb { get => _isSyncDb; set {  _isSyncDb = value;  OnPropertyChanged(); } }

        private bool _isSyncApi { get; set; }
        public bool IsSyncApi { get => _isSyncApi; set { _isSyncApi = value; OnPropertyChanged(); } }

        private bool _isRunSyncForSchedule { get; set; }
        public bool IsRunSyncForSchedule { get => _isRunSyncForSchedule; set { _isRunSyncForSchedule = value; OnPropertyChanged(); } }

        private string _serverName { get; set; }
        public string ServerName { get => _serverName; set { _serverName = value; OnPropertyChanged();} }

        private string _dbName { get; set; }
        public string DbName { get => _dbName; set { _dbName = value; OnPropertyChanged(); } }

        private string _userId { get; set; }
        public string UserId { get => _userId; set { _userId = value; OnPropertyChanged(); } }

        private string _password { get; set; }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _apiAdress { get; set; }
        public string ApiAddress { get => _apiAdress; set { _apiAdress = value; OnPropertyChanged(); } }

        private DateTime? _minScheduleDateStart { get; set; }
        public DateTime? MinScheduleDateStart { get => _minScheduleDateStart; set { _minScheduleDateStart = value; OnPropertyChanged(); } }

        private string _maxRowNumPerRequest { get; set; }
        public string MaxRowNumPerRequest 
        { 
            get => _maxRowNumPerRequest; 
            set 
            { 
                if(int.TryParse(value, out int num))
                {
                    _maxRowNumPerRequest = num.ToString();
                }
                else
                {
                    _maxRowNumPerRequest = "";
                }
                OnPropertyChanged(); 
            } 
        }

        public ICommand SaveConfigCommand { get; set; }
        public ICommand CheckForUpdateCommand { get; set; }


        public async Task LoadDataConfig()
        {
            var config = await _unitOfWork.AppSettingRepository.GetAllAsync();
            var apiaddress = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_APIADDRESS.ToString());
            var maxdatarow = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_MAX_DATAROW_PER_REQUEST.ToString());
            var mindatestart = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_MIN_SCHEDULE_DATESTART.ToString());
            var isRunSyncForSchedule = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.IS_RUN_SYNC_FOR_SCHEDULE.ToString());

            if (mindatestart != null)
            {
                MinScheduleDateStart = string.IsNullOrEmpty(mindatestart.Value) ? null : DateTime.TryParseExact(mindatestart.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateout) ? dateout : null;
            }

            if (maxdatarow != null)
            {
                MaxRowNumPerRequest = maxdatarow.Value ?? "";
            }

            if (apiaddress != null)
            {
                ApiAddress = apiaddress.Value ?? "";
            }
            if (isRunSyncForSchedule != null)
            {
                IsRunSyncForSchedule = isRunSyncForSchedule.Value != null && isRunSyncForSchedule.Value == Boolean.TrueString ? true : false;
            }


        }

        private async void SaveConfigCommandExcute(object obj)
        {
            _mainViewModel.IsLoading = true;
            var config = await _unitOfWork.AppSettingRepository.GetAllAsync();
            var apiaddress = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_APIADDRESS.ToString());
            var maxdatarow = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_MAX_DATAROW_PER_REQUEST.ToString());
            var mindatestart = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_MIN_SCHEDULE_DATESTART.ToString());
            var isRunSyncForSchedule = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.IS_RUN_SYNC_FOR_SCHEDULE.ToString());

            if (isRunSyncForSchedule != null)
            {
                isRunSyncForSchedule.Value = IsRunSyncForSchedule.ToString();
            }
            else
            {
                var item = new AppSetting()
                {
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.IS_RUN_SYNC_FOR_SCHEDULE.ToString(),
                    Value = IsRunSyncForSchedule.ToString()
                };
                await _unitOfWork.AppSettingRepository.AddAsync(item);
            }

            if (maxdatarow != null)
            {
                maxdatarow.Value = MaxRowNumPerRequest;
            }
            else
            {
                var item = new AppSetting()
                {
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.SYNC_API_MAX_DATAROW_PER_REQUEST.ToString(),
                    Value = MaxRowNumPerRequest
                };
                await _unitOfWork.AppSettingRepository.AddAsync(item);
            }
            if (mindatestart != null)
            {
                mindatestart.Value = MinScheduleDateStart == null ? null : MinScheduleDateStart.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                var item = new AppSetting()
                {
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.SYNC_MIN_SCHEDULE_DATESTART.ToString(),
                    Value = MinScheduleDateStart == null ? null : MinScheduleDateStart.Value.ToString("dd/MM/yyyy")
                };
                await _unitOfWork.AppSettingRepository.AddAsync(item);
            }
            if (apiaddress != null)
            {
                apiaddress.Value = ApiAddress;
            }
            else
            {
                var item = new AppSetting()
                {
                    Id = Guid.NewGuid(),
                    Name = ConfigEnumConstant.SYNC_API_APIADDRESS.ToString(),
                    Value = ApiAddress
                };
                await _unitOfWork.AppSettingRepository.AddAsync(item);
            }
            await _unitOfWork.SaveChanageAsync();
            _mainViewModel.IsLoading = false;
            _mainViewModel.Action_ShowToastNotify(ConstantDisplay.LANG_GLO_ActionSuccess.TranslateString(), ToastNotifyType.Success);

        }

    }
}
