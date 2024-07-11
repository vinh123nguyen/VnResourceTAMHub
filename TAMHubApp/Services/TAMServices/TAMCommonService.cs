using Microsoft.Extensions.DependencyInjection;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.ViewModels;
using zkemkeeper;

namespace TAMHubApp.Services.TAMServices
{
    public class TAMCommonService
    {
        private SingletonTAMInstance _singletonTAMInstance;

        private IUnitOfWork _unitOfWork;
        private MainViewModel _mainWindow;

        public TAMCommonService()
        {
            _singletonTAMInstance = ServiceLocator.Instance.GetRequiredService<SingletonTAMInstance>();
            _unitOfWork = ServiceLocator.Instance.GetRequiredService<IUnitOfWork>();
            _mainWindow = ServiceLocator.Instance.GetRequiredService<MainViewModel>();
        }

        public async Task<bool> Action_Connect(BaseTAMService device)
        {
            var result = false;
            try
            {
                var dbDevice = await _unitOfWork.DeviceRepository.GetByIdAsync(device.Id);
                if (dbDevice == null)
                    return false;

                if (device.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    var _deviceIns = new CZKEM();
                    if (device.CommPassword != null)
                        _deviceIns.SetCommPassword(device.CommPassword.Value);
                    result = _deviceIns.Connect_Net(device.IPAddress, device.Port);
                    if (result)
                    {
                        void Event_OnDisconnected()
                        {
                            zkdevice.IsConnected = false;
                        }

                        void Event_OnConnected()
                        {
                            zkdevice.IsConnected = true;
                        }
                        if (_deviceIns.RegEvent(1, 32767))
                        {
                            _deviceIns.OnConnected += Event_OnConnected;
                            _deviceIns.OnDisConnected += Event_OnDisconnected;
                        }

                        int statusValue = 0;
                        if (_deviceIns.GetDeviceStatus(zkdevice.MachineNo, 6, ref statusValue))
                        {
                            zkdevice.AllLogCount = statusValue;
                            dbDevice.AllLogCount = statusValue;
                        }
                        if (string.IsNullOrEmpty(device.SerialNumber))
                        {
                            var serial = "";
                            if (_deviceIns.GetSerialNumber(zkdevice.MachineNo, out serial))
                            {
                                zkdevice.SerialNumber = serial;
                                dbDevice.SerialNumber = serial;
                            }
                        }
                        if (string.IsNullOrEmpty(device.ProductName))
                        {
                            var prod = "";
                            if (_deviceIns.GetProductCode(zkdevice.MachineNo, out prod))
                            {
                                zkdevice.ProductName = prod;
                                dbDevice.ProductName = prod;
                            }
                        }

                        dbDevice.ZKTeco_IsNewFirmwareMachine = _deviceIns.IsNewFirmwareMachine(zkdevice.MachineNo);
                        dbDevice.ZKTeco_IsTFTMachine = _deviceIns.IsTFTMachine(zkdevice.MachineNo);
                        dbDevice.LastTimeConnected = DateTime.Now;

                        zkdevice.DeviceIns = _deviceIns;
                        zkdevice.IsConnected = result;
                    }
                }
                else if(device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {

                }

                await _unitOfWork.SaveChanageAsync();
                _singletonTAMInstance.UpdateDeviceFromDb(device.Id);
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                });
                result = false;
            }

            return result;
        }

        public bool Action_Dicconnect(BaseTAMService device)
        {
            try
            {
                if(device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    zkdevice.IsConnected = false;
                    zkdevice.DeviceIns.Disconnect();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                });
                return true;
            }
        }

        public async Task<bool> Action_GetAllData(BaseTAMService device)
        {
            try
            {
                if (!device.IsConnected)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.Action_ShowToastNotify(ConstantDisplay.LANG_DEV_DeviceNotConnected.TranslateString(), ToastNotifyType.Warning);
                    });
                    return false;
                }
                var dbDevice = await _unitOfWork.DeviceRepository.GetByIdAsync(device.Id);

                var listAttRecord = new List<TAMScanLog>();
                if(device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    var enumconvert = TAMEnumConverter.ZKTecoEnumConvert;
                    if (zkdevice.DeviceIns.ReadAllGLogData(zkdevice.MachineNo))
                    {
                        if (zkdevice.ZKTeco_IsTFTMachine)
                        {
                            int tVerifyMode, tInOutMode, year, month, day, hour, minute, second, workCode = 0;
                            string enrollNumber;
                            while (zkdevice.DeviceIns.SSR_GetGeneralLogData(zkdevice.MachineNo,
                                out enrollNumber, out tVerifyMode,
                                out tInOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                            {
                                listAttRecord.Add(new TAMScanLog()
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceID = device.Id,
                                    EnrollNumber = int.Parse(enrollNumber),
                                    VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                    InOutModeOrigin = tInOutMode,
                                    InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                    TimeLog = new DateTime(year, month, day, hour, minute, second),
                                });

                            }
                        }
                        else
                        {
                            int tVerifyMode = 0, tInOutMode = 0, year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;
                            int enrollNumber = 0;
                            while (zkdevice.DeviceIns.GetAllGLogData(zkdevice.MachineNo,
                                ref enrollNumber, ref tVerifyMode,
                                ref tInOutMode, ref year, ref month, ref day, ref hour, ref minute, ref second, ref workCode))
                            {
                                listAttRecord.Add(new TAMScanLog()
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceID = device.Id,
                                    EnrollNumber = enrollNumber,
                                    VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                    InOutModeOrigin = tInOutMode,
                                    InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                    TimeLog = new DateTime(year, month, day, hour, minute, second),
                                });

                            }
                        }
                    }
                }

                if (listAttRecord.Any())
                {
                    var lstDbTAMScanLog = (await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();
                    var lstDbTAMScanLogHistory = (await _unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();

                    var lis1 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));
                    var lis2 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));

                    var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lis1.Concat(lis2));

                    var listInsertscanlog = listAttRecord.Where(item => !hsetDataDbTAMScanLog.Contains((item.EnrollNumber, item.TimeLog))).ToList();

                    await _unitOfWork.TAMScanLogRepository.AddRangeAsync(listInsertscanlog);

                    await _unitOfWork.SaveChanageAsync();
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_DEV_GetDataSuccess.TranslateString(), listAttRecord.Count), ToastNotifyType.Success);
                });
                return true;

            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                });
                return false;
            }
        }


        public async Task<bool> Action_GetDataByDate(BaseTAMService device, DateTime datefrom, DateTime dateend)
        {
            datefrom = datefrom.Date;
            dateend = dateend.Date.AddDays(1).AddMicroseconds(-1);
            try
            {
                if (!device.IsConnected)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.Action_ShowToastNotify(ConstantDisplay.LANG_DEV_DeviceNotConnected.TranslateString(), ToastNotifyType.Warning);
                    });
                    return false;
                }

                var dbDevice = await _unitOfWork.DeviceRepository.GetByIdAsync(device.Id);

                var listAttRecord = new List<TAMScanLog>();
                if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    var enumconvert = TAMEnumConverter.ZKTecoEnumConvert;
                    if (zkdevice.ZKTeco_IsCanGetLogByTime && zkdevice.ZKTeco_IsNewFirmwareMachine)
                    {
                        if (zkdevice.DeviceIns.ReadTimeGLogData(zkdevice.MachineNo, datefrom.ToString("yyyy-MM-dd HH:mm:ss"), dateend.ToString("yyyy-MM-dd HH:mm:ss")))
                        {
                            if (zkdevice.ZKTeco_IsTFTMachine)
                            {
                                int tVerifyMode, tInOutMode, year, month, day, hour, minute, second, workCode = 0;
                                string enrollNumber;
                                while (zkdevice.DeviceIns.SSR_GetGeneralLogData(zkdevice.MachineNo,
                                    out enrollNumber, out tVerifyMode,
                                    out tInOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                                {
                                    listAttRecord.Add(new TAMScanLog()
                                    {
                                        Id = Guid.NewGuid(),
                                        DeviceID = device.Id,
                                        EnrollNumber = int.Parse(enrollNumber),
                                        VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                    });

                                }
                            }
                            else
                            {
                                int tVerifyMode = 0, tInOutMode = 0, year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;
                                int enrollNumber = 0;
                                while (zkdevice.DeviceIns.GetAllGLogData(zkdevice.MachineNo,
                                    ref enrollNumber, ref tVerifyMode,
                                    ref tInOutMode, ref year, ref month, ref day, ref hour, ref minute, ref second, ref workCode))
                                {
                                    listAttRecord.Add(new TAMScanLog()
                                    {
                                        Id = Guid.NewGuid(),
                                        DeviceID = device.Id,
                                        EnrollNumber = enrollNumber,
                                        VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                    });

                                }
                            }
                        }
                    }
                    else
                    {
                        var listAttRecordAll = new List<TAMScanLog>();
                        if (zkdevice.DeviceIns.ReadAllGLogData(zkdevice.MachineNo))
                        {
                            if (zkdevice.ZKTeco_IsTFTMachine)
                            {
                                int tVerifyMode, tInOutMode, year, month, day, hour, minute, second, workCode = 0;
                                string enrollNumber;
                                while (zkdevice.DeviceIns.SSR_GetGeneralLogData(zkdevice.MachineNo,
                                    out enrollNumber, out tVerifyMode,
                                    out tInOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                                {
                                    listAttRecordAll.Add(new TAMScanLog()
                                    {
                                        Id = Guid.NewGuid(),
                                        DeviceID = device.Id,
                                        EnrollNumber = int.Parse(enrollNumber),
                                        VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                    });
                                }
                            }
                            else
                            {
                                int tVerifyMode = 0, tInOutMode = 0, year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;
                                int enrollNumber = 0;
                                while (zkdevice.DeviceIns.GetAllGLogData(zkdevice.MachineNo,
                                    ref enrollNumber, ref tVerifyMode,
                                    ref tInOutMode, ref year, ref month, ref day, ref hour, ref minute, ref second, ref workCode))
                                {
                                    listAttRecordAll.Add(new TAMScanLog()
                                    {
                                        Id = Guid.NewGuid(),
                                        DeviceID = device.Id,
                                        EnrollNumber = enrollNumber,
                                        VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                    });

                                }
                            }
                        }
                        listAttRecord = listAttRecordAll.Where(x => x.TimeLog >= datefrom && x.TimeLog <= dateend).ToList();
                    }
                }
                if (listAttRecord.Any())
                {
                    var lstDbTAMScanLog = (await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();
                    var lstDbTAMScanLogHistory = (await _unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();

                    var lis1 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));
                    var lis2 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));

                    var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lis1.Concat(lis2));

                    var listInsertscanlog = listAttRecord.Where(item => !hsetDataDbTAMScanLog.Contains((item.EnrollNumber, item.TimeLog))).ToList();

                    await _unitOfWork.TAMScanLogRepository.AddRangeAsync(listInsertscanlog);

                    await _unitOfWork.SaveChanageAsync();
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(string.Format(ConstantDisplay.LANG_DEV_GetDataSuccess.TranslateString(), listAttRecord.Count), ToastNotifyType.Success);
                });
                return true;
            }
            catch(Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                });
                return false;
            }
        }

        public void Action_ConnectAll()
        {
            var listdevice = _singletonTAMInstance.TAMs;
            foreach(var devive in listdevice)
            {
                if (!devive.IsConnected)
                    continue;
                Task.Run(async () =>
                {
                   await Action_Connect(devive);
                });
            }
        }
        public void Action_DisconnectAll()
        {
            var listdevice = _singletonTAMInstance.TAMs;
            foreach (var devive in listdevice)
            {
                if (devive.IsConnected)
                    continue;
                Task.Run(() =>
                {
                    Action_Dicconnect(devive);
                });
            }
        }
    }
}
