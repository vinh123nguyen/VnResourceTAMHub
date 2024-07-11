using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using TAMHubApp.Commands;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Infrastructures.TAMUtilities;
using TAMHubApp.Models;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ViewModels.UserControls
{
    class DeviceItemInfoUCViewModel : BaseViewModel
    {
        private SingletonTAMInstance _singletonTAMInstance;
        private DeviceViewModel _deviceViewModel;
        private IUnitOfWork _unitOfWork;
        private MainViewModel _mainViewModel;

        public DeviceItemInfoUCViewModel(DeviceViewModel deviceViewModel,
            SingletonTAMInstance singletonTAMInstance, 
            IUnitOfWork unitOfWork,
            MainViewModel mainViewModel)
        {
            var temp = new ObservableCollection<Option>();
            var lstEnum = Enum.GetValues(typeof(TAMSDKTypes)).Cast<TAMSDKTypes>().ToList();
            lstEnum.ForEach(x =>
            {
                temp.Add(new Option
                {
                    Key = x.ToString(),
                    Name = x.ToString().TranslateString()
                });
            });
            SDKsOptions = temp;
            _mainViewModel = mainViewModel;
            _unitOfWork = unitOfWork;
            _singletonTAMInstance = singletonTAMInstance;
            _deviceViewModel = deviceViewModel;
            CloseOverlayCommand = new RelayCommand<object>(x => { return true; }, CloseOverlayCommandExcute);
            SavePopupCommand = new RelayCommand<object>(x => { return true; }, SavePopupCommandExcute);
            RemmoveDeviceCommand = new RelayCommand<object>(x => { return !string.IsNullOrEmpty(Id); }, RemmoveDeviceCommandExcute);
        }

        
        #region Command

        public RelayCommand<object> CloseOverlayCommand { get; set; }
        public RelayCommand<object> SavePopupCommand { get; set; }
        public RelayCommand<object> RemmoveDeviceCommand { get; set; }

        #endregion


        #region Observable Properties
        private ObservableCollection<Option> _options;
        public ObservableCollection<Option>? SDKsOptions
        {
            get { return _options; }
            set { _options = value; OnPropertyChanged(); }
        }

        private Option _SDKSelected;
        public Option SDKSelected
        {
            get { return _SDKSelected; }
            set { _SDKSelected = value; OnPropertyChanged(); Event_ChangeSDKType(value); }
        }

        private string _id;
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

        private string _deviceName { get; set; }
        public string DeviceName
        {
            get => _deviceName;
            set { _deviceName = value; OnPropertyChanged(); }
        }

        private string _deviceNumber { get; set; }
        public string DeviceNumber { get => _deviceNumber; set { _deviceNumber = value; OnPropertyChanged(); } }

        private string _ipAddress { get; set; }
        public string IPAddress { get => _ipAddress; set { _ipAddress = value; OnPropertyChanged(); } }

        private string _port { get; set; }
        public string Port { get => _port; set { _port = value; OnPropertyChanged(); } }

        private string _password { get; set; }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _validateMessage { get; set; }
        public string ValidateMessage { get => _validateMessage; set { _validateMessage = value; OnPropertyChanged(); } }

        private bool _supportGetDataByDate { get; set; }
        public bool SupportGetDataByDate { get => _supportGetDataByDate; set { _supportGetDataByDate = value; OnPropertyChanged(); } }

        private string _sdkType { get; set; }
        public string SDKType { get => _sdkType; set { _sdkType = value; OnPropertyChanged(); } }

        private string _companyCode { get; set; }
        public string CompanyCode { get => _companyCode; set { _companyCode = value; OnPropertyChanged(); } }

        #endregion

        private void Action_SetDataInfoPopup(string Id)
        {
            if (string.IsNullOrEmpty(Id))
                return;

            var currentDevice = _singletonTAMInstance.TAMs.FirstOrDefault(x => x.Id == Guid.Parse(Id));

            SDKSelected = SDKsOptions.FirstOrDefault(x => x.Key == currentDevice.DeviceSDKType.ToString());
            DeviceName = currentDevice.Name;
            DeviceNumber = currentDevice.MachineNo.ToString();
            IPAddress = currentDevice.IPAddress;
            Port = currentDevice.Port.ToString();
            Password = currentDevice.CommPassword.ToString();

            if(currentDevice.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
            {
                SupportGetDataByDate = ((ZKTecoDeviceItem)currentDevice).ZKTeco_IsCanGetLogByTime;
            }
        }
        private void Event_ChangeSDKType(Option option)
        {
            SDKType = option.Key;
        }
        private async void SavePopupCommandExcute(object parameter)
        {
            var validate = ValidateDeviceInfoPopup();
            if (!validate)
                return;

            _mainViewModel.IsLoading = true;
            await Task.Run(async () =>
            {
                var ObjectId = Guid.Empty;
                if (parameter != null && Guid.TryParse(parameter.ToString(), out ObjectId))
                {
                    var currentObject = await _unitOfWork.DeviceRepository.GetByIdAsync(ObjectId);

                    if (currentObject != null)
                    {
                        currentObject.Name = DeviceName;
                        currentObject.MachineNo = int.Parse(DeviceNumber);
                        currentObject.IPAddress = IPAddress;
                        currentObject.Port = int.Parse(Port);
                        currentObject.CommPassword = string.IsNullOrEmpty(Password) ? null : int.Parse(Password);
                        currentObject.DeviceSDKType = (TAMSDKTypes)Enum.Parse(typeof(TAMSDKTypes), SDKSelected.Key);
                    }
                }
                else
                {
                    var currentObject = new Device();
                    currentObject.Id = Guid.NewGuid();
                    currentObject.Name = DeviceName;
                    currentObject.MachineNo = int.Parse(DeviceNumber);
                    currentObject.IPAddress = IPAddress;
                    currentObject.Port = int.Parse(Port);
                    currentObject.CommPassword = string.IsNullOrEmpty(Password) ? null : int.Parse(Password);
                    currentObject.DeviceSDKType = (TAMSDKTypes)Enum.Parse(typeof(TAMSDKTypes), SDKSelected.Key);
                    await _unitOfWork.DeviceRepository.AddAsync(currentObject);
                    ObjectId = currentObject.Id;
                }

                await _unitOfWork.SaveChanageAsync();
                _singletonTAMInstance.UpdateDeviceFromDb(ObjectId);
                _mainViewModel.IsLoading = false;
                _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
            });
        }

        private void CloseOverlayCommandExcute(object parameter)
        {
            _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
        }
        private bool ValidateDeviceInfoPopup()
        {
            var result = true;
            var message = string.Empty;
            var devicenumber = 0;
            if (SDKSelected == null || string.IsNullOrEmpty(SDKSelected.Key))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_SKDType.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (string.IsNullOrEmpty(DeviceName))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_DeviceName.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (string.IsNullOrEmpty(DeviceNumber))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_DeviceNumber.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (!string.IsNullOrEmpty(DeviceNumber) && !int.TryParse(DeviceNumber, out devicenumber))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_DeviceNumber.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if (string.IsNullOrEmpty(IPAddress))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_IPAddress.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (!string.IsNullOrEmpty(IPAddress) && !GlobalUtilities.IsCorrenctIP(IPAddress))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_IPAddress.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if (string.IsNullOrEmpty(Port))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_Port.TranslateString() + " " + ConstantDisplay.LANG_GLO_CanNotBeBlank.TranslateString();
            }
            else if (!string.IsNullOrEmpty(Port) && !int.TryParse(Port, out devicenumber))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_Port.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            else if (!string.IsNullOrEmpty(Password) && !int.TryParse(Password, out devicenumber))
            {
                result = false;
                message = ConstantDisplay.LANG_DEV_Password.TranslateString() + " " + ConstantDisplay.LANG_GLO_NotMatchDataType.TranslateString();
            }
            ValidateMessage = message;
            return result;
        }

        private async void RemmoveDeviceCommandExcute(object parameter)
        {
            var ObjectId = Guid.Empty;
            if (parameter != null && Guid.TryParse(parameter.ToString(), out ObjectId))
            {
                _mainViewModel.IsLoading = true;
                var objectdel = await _unitOfWork.DeviceRepository.GetByIdAsync(ObjectId);
                if(objectdel != null)
                {
                    _unitOfWork.DeviceRepository.Remove(objectdel);
                    await _unitOfWork.SaveChanageAsync();
                    _deviceViewModel.IsCreateOrUpdatePopupOpen = false;
                    _singletonTAMInstance.RemoveDeviceToSingletonInstance(ObjectId);
                }
                _mainViewModel.IsLoading = false;
            }
        }
    }
}
