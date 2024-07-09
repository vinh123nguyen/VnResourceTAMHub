using System.Collections.ObjectModel;
using TAMHubApp.Commands;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;
using TAMHubApp.ViewModels.UserControls;

namespace TAMHubApp.ViewModels
{
    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(IDeviceNavigationService navigationService,
            SingletonTAMInstance singletonTAMInstance)
        {
            _navigationService = navigationService;
            OpenInfoPopup = new RelayCommand<object>(x => { return true; }, Action_OpenPopupAddOrUpdateDevice);
            TongleOnOffCommand = new RelayCommand<object>(x => { return true; }, Action_TongleOnOffDevice);
            OpenSelectDatePopupCommand = new RelayCommand<object>(x => { return true; }, Action_OpenPopupSelectDateRange);

            DeviceItems = singletonTAMInstance.TAMs;
            _singletonTAMInstance = singletonTAMInstance;
        }


        private IDeviceNavigationService _navigationService;
        public IDeviceNavigationService OverlayNavigate
        {
            get => _navigationService;
            set { _navigationService = value; OnPropertyChanged(); }
        }
        private bool _isCreateOrUpdatePopupOpen;
        public bool IsCreateOrUpdatePopupOpen
        {
            get => _isCreateOrUpdatePopupOpen;
            set { _isCreateOrUpdatePopupOpen = value; OnPropertyChanged(); }
        }

        public RelayCommand<object> OpenSelectDatePopupCommand { get; set; }
        public RelayCommand<object> OpenInfoPopup { get; set; }
        public RelayCommand<object> GetDataCommand { get; set; }
        public RelayCommand<object> TongleOnOffCommand { get; set; }


        private ObservableCollection<BaseTAMService> _deviceItems;
        public ObservableCollection<BaseTAMService> DeviceItems
        {
            get => _deviceItems;
            set
            {
                _deviceItems = value;
                OnPropertyChanged();
            }
        }

        private SingletonTAMInstance _singletonTAMInstance;

        private void Action_OpenPopupSelectDateRange(object parameter)
        {
            if(parameter != null)
            {
                IsCreateOrUpdatePopupOpen = true;
                OverlayNavigate.NavigateTo<SelectDateRangeGetDataUCViewModel>();

                var daw = (SelectDateRangeGetDataUCViewModel)OverlayNavigate.CurrentView;
                daw.Id = parameter.ToString();
            }
        }


        private void Action_OpenPopupAddOrUpdateDevice(object parameter)
        {
            IsCreateOrUpdatePopupOpen = true;
            OverlayNavigate.NavigateTo<DeviceItemInfoUCViewModel>();

            var daw = (DeviceItemInfoUCViewModel)OverlayNavigate.CurrentView;
            daw.Id = parameter == null ? null : parameter.ToString();

            //if (parameter != null && parameter is Guid)
            //{
            //    var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)parameter);
            //    if (itemdeviceAction != null)
            //    {
            //        itemdeviceAction.IsConnectLoading = true;
            //        Task.Run(() =>
            //        {
            //            if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_ZKTECO)
            //            {
            //                itemdeviceAction.Action_GetAllGlog();

            //            }
            //            else if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_REALAND)
            //            {

            //            }
            //            itemdeviceAction.IsConnectLoading = false;
            //        });

            //    }
            //}
        }

        private void Action_TongleOnOffDevice(object obj)
        {
            if (obj != null && obj is Guid)
            {
                var itemdeviceAction = DeviceItems.FirstOrDefault(x => x.Id == (Guid)obj);
                if (itemdeviceAction != null)
                {
                    itemdeviceAction.IsConnectLoading = true;
                    //Task.Run(() =>
                    //{
                    //    if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_ZKTECO)
                    //    {
                    //        Thread.Sleep(2000);
                    //        var exactlyDeviceInstance = (ZKTecoDeviceItemService)itemdeviceAction;
                    //        if (!itemdeviceAction.IsConnected)
                    //        {
                    //            var d = itemdeviceAction.Action_Connect();
                    //            if (d)
                    //                itemdeviceAction.IsConnected = true;
                    //        }
                    //        else
                    //        {
                    //            var d = itemdeviceAction.Action_Disconnect();
                    //            if (d)
                    //                itemdeviceAction.IsConnected = false;
                    //        }

                    //        //var DbCStringBuilder = new SqlConnectionStringBuilder();
                    //        //DbCStringBuilder["Server"] = "172.21.55.3,1968";
                    //        //DbCStringBuilder["Database"] = "QC_2700";
                    //        //DbCStringBuilder["User Id"] = "se";
                    //        //DbCStringBuilder["Password"] = "Xq3EPN4w2aAiAkp";
                    //        //string connectionString = DbCStringBuilder.ToString();
                    //        //using (var sqlConnection = new SqlConnection(connectionString))
                    //        //{
                    //        //    sqlConnection.Open();

                    //        //    var commanGetData = "select * from Att_TAMScanLog aa left join Hre_Profile hp on aa.ProfileID = hp.ID";

                    //        //    SqlCommand cm1 = new SqlCommand(commanGetData, sqlConnection);

                    //        //    SqlDataReader sdr = cm1.ExecuteReader();

                    //        //    while (sdr.Read())
                    //        //    {
                    //        //        //listDataAlready.Add(new CHECKINOUTModel()
                    //        //        //{
                    //        //        //    USERID = sdr["USERID"].ToString(),
                    //        //        //    CHECKTIME = DateTime.Parse(sdr["CHECKTIME"].ToString())
                    //        //        //    //CHECKTIME = (DateTime)sdr["CHECKTIME"],
                    //        //        //    //CHECKTYPE = sdr["CHECKTYPE"].ToString(),
                    //        //        //    //VERIFYCODE = (int)sdr["VERIFYCODE"],
                    //        //        //    //SENSORID = sdr["SENSORID"].ToString(),
                    //        //        //    //Memoinfo = null,
                    //        //        //    //WorkCode = (int)sdr["WorkCode"],
                    //        //        //    //sn = sdr["Memoinfo"].ToString(),
                    //        //        //    //UserExtFmt = (int)sdr["UserExtFmt"],
                    //        //        //    //BADGENUMBER = sdr["BADGENUMBER"].ToString(),
                    //        //        //});
                    //        //    }
                    //        //}
                    //    }
                    //    else if (itemdeviceAction.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_REALAND)
                    //    {

                    //    }
                    //    itemdeviceAction.IsConnectLoading = false;
                    //    //itemdeviceAction.IsConnected = true;
                    //});


                    //var thread = new Thread(() =>
                    //{
                    //    var d = itemdeviceAction.Action_Connect();
                    //    itemdeviceAction.IsConnected = d;
                    //});
                    //thread.SetApartmentState(ApartmentState.MTA);
                    //thread.IsBackground = true;
                    //thread.Start();

                    //var observable = Observable.Interval(TimeSpan.FromSeconds(1))
                    //    .Select(async _ =>
                    //    {
                    //        await Task.Delay(1000); // Simulate async operation
                    //        return DateTime.Now;
                    //    })
                    //    .ObserveOnDispatcher()
                    //    .Subscribe(result =>
                    //    {
                    //        // Update UI with result
                    //    });
                    //ManualResetEvent evnt = new ManualResetEvent(false);

                    //Thread thread = new Thread(delegate ()
                    //{
                    //    itemdeviceAction.Action_Connect();
                    //    evnt.Set();
                    //});

                    //thread.SetApartmentState(ApartmentState.MTA);
                    //thread.Start();
                    //evnt.WaitOne();
                }
            }
        }


    }
}
