using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
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
                        var zkdevice = new ZKTecoDeviceItem()
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
                            DeviceSDKTypeView = "ZKTeco",
                            CompanyCode = devicedb.CompanyCode,
                            InOutType = devicedb.InOutType,
                            SerialNumber = devicedb.SerialNumber,
                            ProductName = devicedb.ProductName,
                            AllLogCount = devicedb.AllLogCount,
                            ZKTeco_IsCanGetLogByTime = devicedb.ZKTeco_IsCanGetLogByTime,
                            ZKTeco_IsIFaceDevice = devicedb.ZKTeco_IsIFaceDevice,
                            IsInactive = devicedb.IsInactive,
                            ZKTeco_IsNewFirmwareMachine = devicedb.ZKTeco_IsNewFirmwareMachine == null ? false : devicedb.ZKTeco_IsNewFirmwareMachine.Value,
                            ZKTeco_IsTFTMachine = devicedb.ZKTeco_IsTFTMachine == null ? false : devicedb.ZKTeco_IsTFTMachine.Value
                        };
                        lstDevicReturn.Add(zkdevice);
                    }
                    else if (devicedb.DeviceSDKType == TAMSDKTypes.E_REALAND)
                    {
                        var zkdevice = new RealandDeviceItem()
                        {
                            DeviceSDKType = devicedb.DeviceSDKType,
                            Id = devicedb.Id,
                            Name = devicedb.Name,
                            MachineNo = devicedb.MachineNo,
                            IPAddress = devicedb.IPAddress,
                            Port = devicedb.Port,
                            CommPassword = devicedb.CommPassword,
                            CompanyCode = devicedb.CompanyCode,
                            InOutType = devicedb.InOutType,
                            SerialNumber = devicedb.SerialNumber,
                            ProductName = devicedb.ProductName,
                            IsConnectLoading = false,
                            IsConnected = false,
                            AllLogCount = devicedb.AllLogCount,
                            NewLogCount = devicedb.NewLogCount,
                            DeviceSDKTypeView = "Realand",
                            IsInactive = devicedb.IsInactive,
                        };
                        lstDevicReturn.Add(zkdevice);
                    }
                }

                return lstDevicReturn;
            }

            return new ObservableCollection<BaseTAMService>();
        }

        public async void UpdateDeviceFromDb(Guid deviceId)
        {
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);

                var devicedb = await unitOfWork.DeviceRepository.GetByIdAsync(deviceId);
                if (devicedb != null)
                {
                    var existDevice = TAMs.FirstOrDefault(x => x.Id == deviceId);
                    if (existDevice != null)
                    {
                        existDevice.DeviceSDKType = devicedb.DeviceSDKType;
                        existDevice.Id = devicedb.Id;
                        existDevice.Name = devicedb.Name;
                        existDevice.MachineNo = devicedb.MachineNo;
                        existDevice.IPAddress = devicedb.IPAddress;
                        existDevice.Port = devicedb.Port;
                        existDevice.CommPassword = devicedb.CommPassword;
                        existDevice.AllLogCount = devicedb.AllLogCount;
                        existDevice.NewLogCount = devicedb.NewLogCount;
                        existDevice.DeviceSDKTypeView = "Realand";
                        existDevice.CompanyCode = devicedb.CompanyCode;
                        existDevice.InOutType = devicedb.InOutType;
                        existDevice.SerialNumber = devicedb.SerialNumber;
                        existDevice.ProductName = devicedb.ProductName;
                        existDevice.IsInactive = devicedb.IsInactive;
                        if (existDevice.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                        {
                            existDevice.DeviceSDKTypeView = "ZKTeco";
                            ((ZKTecoDeviceItem)existDevice).ZKTeco_IsNewFirmwareMachine = devicedb.ZKTeco_IsNewFirmwareMachine == null ? false : devicedb.ZKTeco_IsNewFirmwareMachine.Value;
                            ((ZKTecoDeviceItem)existDevice).ZKTeco_IsTFTMachine = devicedb.ZKTeco_IsTFTMachine == null ? false : devicedb.ZKTeco_IsTFTMachine.Value;
                            ((ZKTecoDeviceItem)existDevice).ZKTeco_IsCanGetLogByTime = devicedb.ZKTeco_IsCanGetLogByTime;
                            ((ZKTecoDeviceItem)existDevice).ZKTeco_IsIFaceDevice = devicedb.ZKTeco_IsIFaceDevice;
                        }
                    }
                    else
                    {
                        if (devicedb.DeviceSDKType == TAMSDKTypes.E_REALAND)
                        {
                            var zkdevice = new RealandDeviceItem()
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
                                NewLogCount = devicedb.NewLogCount,
                                SerialNumber = devicedb.SerialNumber,
                                ProductName = devicedb.ProductName,
                                CompanyCode = devicedb.CompanyCode,
                                InOutType = devicedb.InOutType,
                                DeviceSDKTypeView = "Realand",
                                IsInactive = devicedb.IsInactive,
                            };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TAMs.Add(zkdevice);
                            });
                        }
                        else if (devicedb.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                        {
                            var zkdevice = new ZKTecoDeviceItem()
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
                                DeviceSDKTypeView = "ZKTeco",
                                NewLogCount = devicedb.NewLogCount,
                                SerialNumber = devicedb.SerialNumber,
                                ProductName = devicedb.ProductName,
                                CompanyCode = devicedb.CompanyCode,
                                InOutType = devicedb.InOutType,
                                IsInactive = devicedb.IsInactive,
                                ZKTeco_IsCanGetLogByTime = devicedb.ZKTeco_IsCanGetLogByTime,
                                ZKTeco_IsIFaceDevice = devicedb.ZKTeco_IsIFaceDevice,
                                ZKTeco_IsNewFirmwareMachine = devicedb.ZKTeco_IsNewFirmwareMachine == null ? false : devicedb.ZKTeco_IsNewFirmwareMachine.Value,
                                ZKTeco_IsTFTMachine = devicedb.ZKTeco_IsTFTMachine == null ? false : devicedb.ZKTeco_IsTFTMachine.Value
                            };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TAMs.Add(zkdevice);
                            });
                        }
                    }
                }
            }
                //TAMs = 
            
        }

        public void RemoveDeviceToSingletonInstance(Guid deviceId)
        {
            var existDevice = TAMs.FirstOrDefault(x => x.Id == deviceId);
            if(existDevice != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TAMs.Remove(existDevice);
                });
            }
        }
    }
}
