using System.Collections.ObjectModel;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Enums;

namespace TAMHubApp.Services.TAMServices
{
    public class SingletonTAMInstance
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObservableCollection<BaseTAMService> TAMs { get; set; }

        public SingletonTAMInstance(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
            InitializeAsync().Wait();
        }
        private async Task InitializeAsync()
        {
            //init Instant from data db
            TAMs = await InitDeviceFromDb();
        }

        public void AddTAM(BaseTAMService tam)
        {
            TAMs.Add(tam);
        }

        public void RemoveTAM(BaseTAMService tam)
        {

        }
        public async Task<ObservableCollection<BaseTAMService>> InitDeviceFromDb()
        {
            var lstDeviceDb = await _unitOfWork.DeviceRepository.GetAllAsync();
            var lstDevicReturn = new ObservableCollection<BaseTAMService>();

            if (lstDeviceDb != null && lstDeviceDb.Any())
            {

                foreach(var devicedb in lstDeviceDb)
                {
                    if(devicedb.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                    {
                        var zkdevice = new ZKTecoDeviceItemService()
                        {
                            DeviceSDKType = devicedb.DeviceSDKType,
                            Id = devicedb.Id,
                            Name = devicedb.Name,
                            MachineNo = devicedb.MachineNo,
                            IPAddress = devicedb.IPAddress,
                            Port = devicedb.Port,
                            CommPassword = devicedb.CommPassword,
                            IsConnectLoading = false,
                            IsConnected = false,
                            AllLogCount = devicedb.AllLogCount,
                            ZKTeco_IsCanGetLogByTime = devicedb.ZKTeco_IsCanGetLogByTime,
                        };
                        lstDevicReturn.Add(zkdevice);
                    }
                    else if (devicedb.DeviceSDKType == TAMSDKTypes.E_REALAND)
                    {
                        var zkdevice = new RealandDeviceItemService()
                        {
                            DeviceSDKType = devicedb.DeviceSDKType,
                            Id = devicedb.Id,
                            Name = devicedb.Name,
                            MachineNo = devicedb.MachineNo,
                            IPAddress = devicedb.IPAddress,
                            Port = devicedb.Port,
                            CommPassword = devicedb.CommPassword,
                            IsConnectLoading = false,
                            IsConnected = false,
                            AllLogCount = devicedb.AllLogCount,
                            NewLogCount = devicedb.NewLogCount
                        };
                        lstDevicReturn.Add(zkdevice);
                    }
                }

                return lstDevicReturn;
            }

            return new ObservableCollection<BaseTAMService>();
        }
    }
}
