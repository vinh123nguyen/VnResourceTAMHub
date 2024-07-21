using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Riss.Devices;
using Serilog;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Infrastructures.TAMUtilities;
using TAMHubApp.Models;
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

                        await _unitOfWork.SaveChanageAsync();
                    }
                }
                else if(device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;

                    var deviceins = new Riss.Devices.Device();
                    deviceins.DN = device.MachineNo;
                    if (device.CommPassword != null)
                        deviceins.Password = device.CommPassword.ToString();

                    deviceins.Model = "ZDC2911";
                    deviceins.IpAddress = device.IPAddress;
                    deviceins.IpPort = device.Port;
                    deviceins.CommunicationType = CommunicationType.Tcp;

                    var deviceConnectionIns = DeviceConnection.CreateConnection(ref deviceins);

                    if (deviceConnectionIns.Open() > 0)
                    {
                        object extraProperty = new object();
                        object extraData = new object();
                        //extraData = Global.DeviceBusy;
                        //bool resultRiss = deviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, deviceins, extraData);

                        List<DateTime> dtList = new List<DateTime>();
                        dtList.Add(DateTime.MinValue);
                        dtList.Add(DateTime.MaxValue);
                        extraProperty = true;
                        extraData = dtList;
                        bool resultRiss = deviceConnectionIns.GetProperty(DeviceProperty.AttRecordsCount, extraProperty, ref deviceins, ref extraData);
                        if (result)
                        {
                            rissdevice.NewLogCount = (int)extraData;
                            dbDevice.NewLogCount = (int)extraData;
                        }
                        extraProperty = false;
                        resultRiss = deviceConnectionIns.GetProperty(DeviceProperty.AttRecordsCount, extraProperty, ref deviceins, ref extraData);
                        if (result)
                        {
                            rissdevice.AllLogCount = (int)extraData;
                            dbDevice.AllLogCount = (int)extraData;
                        }

                        if (string.IsNullOrEmpty(device.SerialNumber))
                        {
                            object extraProperty1 = new object();
                            object extraData1 = Zd2911Utils.DeviceSerialNo;
                            result = deviceConnectionIns.GetProperty(DeviceProperty.SerialNo, extraProperty1, ref deviceins, ref extraData1);
                            if (result)
                            {
                                rissdevice.SerialNumber = (string)extraData1;
                                dbDevice.SerialNumber = (string)extraData1;
                            }
                        }
                        if (string.IsNullOrEmpty(device.ProductName))
                        {
                            object extraProperty1 = new object();
                            object extraData1 = Zd2911Utils.DeviceModel;
                            result = deviceConnectionIns.GetProperty(DeviceProperty.Model, extraProperty1, ref deviceins, ref extraData1);
                            if (result)
                            {
                                rissdevice.ProductName = (string)extraData1;
                                dbDevice.ProductName = (string)extraData1;
                            }
                        }
                        //extraData = Global.DeviceIdle;
                        //resultRiss = deviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, deviceins, extraData);

                        rissdevice.DeviceIns = deviceins;
                        rissdevice.DeviceConnectionIns = deviceConnectionIns;
                        rissdevice.IsConnected = true;
                    }
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
            device.IsConnected = result;
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
                else if(device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;
                    rissdevice.DeviceConnectionIns.Close();
                    rissdevice.IsConnected = false;
                    rissdevice.DeviceConnectionIns = null;
                    rissdevice.DeviceIns = null;
                }
                
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
                                    VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                    VerifyModeOrigin = tVerifyMode,
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
                                    VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                    VerifyModeOrigin = tVerifyMode,
                                    InOutModeOrigin = tInOutMode,
                                    InOutModeView = enumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                    TimeLog = new DateTime(year, month, day, hour, minute, second),
                                });

                            }
                        }
                    }
                }
                else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;
                    await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
                    var settingenumconvert = GlobalEnumInOutConverter.RealandEnumConverter;
                    List<DateTime> dtList = new List<DateTime>(); 
                    dtList.Add(DateTime.MinValue); 
                    dtList.Add(DateTime.MaxValue);

                    object extraProperty = new object(); 
                    object extraData = new object();

                    List<bool> boolList = new List<bool>();
                    boolList.Add(false);
                    boolList.Add(false);

                    try
                    {
                        extraData = Global.DeviceBusy;
                        var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);

                        extraProperty = boolList;
                        extraData = dtList;
                        resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                        if (resultRiss)
                        {
                            List<Record> recordList = (List<Record>)extraData;
                            listAttRecord = recordList.Select(x => new TAMScanLog()
                            {
                                Id = Guid.NewGuid(),
                                DeviceID = device.Id,
                                EnrollNumber = (int)x.DIN,
                                VerifyModeView = GlobalUtilities.VerifyType(x.Action),
                                VerifyModeOrigin = x.Action,
                                InOutModeOrigin = x.Verify,
                                InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : "",
                                TimeLog = x.Clock
                            }).ToList();
                        }
                    }catch(Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                        });
                        return false;
                    }
                    finally
                    {
                        extraData = Global.DeviceIdle;
                        var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
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

        public async Task<bool> Action_GetDataByDate(BaseTAMService device, DateTime datefrom, DateTime dateend, string EnrollNumber, string DownloadType = "Manual", bool IsDownSchedule = false)
        {
            var actionLog = new ActionLog();
            actionLog.Id = Guid.NewGuid();
            datefrom = datefrom.Date;
            dateend = dateend.Date.AddDays(1).AddMicroseconds(-1);
            actionLog.DateDownDataFrom = datefrom;
            actionLog.DateDownDataTo = dateend;
            actionLog.TimeLogFrom = DateTime.Now;
            actionLog.DownloadType  = DownloadType;
            actionLog.DeviceID = device.Id;
            try
            {
                if (!device.IsConnected)
                {
                    var rescon = await Action_Connect(device);
                    if (!rescon)
                    {
                        actionLog.Message = "Connect fail";
                        actionLog.Status = "Error";

                        actionLog.TimeLogTo = DateTime.Now;

                        await _unitOfWork.ActionLogRepository.AddAsync(actionLog);

                        await _unitOfWork.SaveChanageAsync();

                        return false;
                    }
                }

                var dbDevice = await _unitOfWork.DeviceRepository.GetByIdAsync(device.Id);

                var listAttRecord = new List<TAMScanLog>();
                if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    await GlobalEnumInOutConverter.InitUpdateEnumConverter("ZKTeco");
                    var settingenumconvert = GlobalEnumInOutConverter.ZKEnumConverter;
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
                                        VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        VerifyModeOrigin = tVerifyMode,
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
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
                                        VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        VerifyModeOrigin = tVerifyMode,
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
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
                                        VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        VerifyModeOrigin = tVerifyMode,
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
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
                                        VerifyModeView = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                                        VerifyModeOrigin = tVerifyMode,
                                        InOutModeOrigin = tInOutMode,
                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : "",
                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                    });

                                }
                            }
                        }
                        listAttRecord = listAttRecordAll.Where(x => x.TimeLog >= datefrom && x.TimeLog <= dateend).ToList();
                    }
                }
                else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;
                    await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
                    var settingenumconvert = GlobalEnumInOutConverter.RealandEnumConverter;

                    List<DateTime> dtList = new List<DateTime>();
                    dtList.Add(datefrom);
                    dtList.Add(dateend);

                    object extraProperty = new object();
                    object extraData = new object();

                    List<bool> boolList = new List<bool>();
                    boolList.Add(false);
                    boolList.Add(false);

                    try
                    {
                        if (IsDownSchedule)
                        {
                            extraData = Global.DeviceBusy;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);

                            extraProperty = true;
                            extraData = dtList;

                            resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                            if (resultRiss)
                            {
                                List<Record> recordList = (List<Record>)extraData;
                                listAttRecord = recordList.Select(x => new TAMScanLog()
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceID = device.Id,
                                    EnrollNumber = (int)x.DIN,
                                    VerifyModeView = GlobalUtilities.VerifyType(x.Action),
                                    VerifyModeOrigin = x.Action,
                                    InOutModeOrigin = x.Verify,
                                    InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : "",
                                    TimeLog = x.Clock
                                }).ToList();
                            }
                        }
                        else
                        {
                            extraData = Global.DeviceBusy;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);

                            extraProperty = boolList;
                            extraData = dtList;

                            resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                            if (resultRiss)
                            {
                                List<Record> recordList = (List<Record>)extraData;
                                listAttRecord = recordList.Select(x => new TAMScanLog()
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceID = device.Id,
                                    EnrollNumber = (int)x.DIN,
                                    VerifyModeView = GlobalUtilities.VerifyType(x.Action),
                                    VerifyModeOrigin = x.Action,
                                    InOutModeOrigin = x.Verify,
                                    InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : "",
                                    TimeLog = x.Clock
                                }).ToList();
                            }
                        }
                    }catch(Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                        });
                        return false;
                    }
                    finally
                    {
                        extraData = Global.DeviceIdle;
                        var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                    }
                }
                if (!string.IsNullOrEmpty(EnrollNumber))
                {
                    listAttRecord.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                }
                if (listAttRecord.Any())
                {
                    actionLog.TotalRowDownload = listAttRecord.Count;

                    var lstDbTAMScanLog = (await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();
                    var lstDbTAMScanLogHistory = (await _unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();
                    var synctype = await _unitOfWork.AppSettingRepository.FindAsync(x => x.Name == ConfigEnumConstant.SYNC_TYPE.ToString());

                    var lis1 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));
                    var lis2 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));

                    var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lis1.Concat(lis2));

                    var listInsertscanlog = listAttRecord.Where(item => !hsetDataDbTAMScanLog.Contains((item.EnrollNumber, item.TimeLog))).ToList();

                    actionLog.TotalNewlyData = listInsertscanlog?.Count;

                    await _unitOfWork.TAMScanLogRepository.AddRangeAsync(listInsertscanlog);

                    actionLog.Status = "Success";

                    actionLog.TimeLogTo = DateTime.Now;

                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                    await _unitOfWork.SaveChanageAsync();
                    if (synctype != null && synctype.Value == "DB")
                    {
                        await SyncDataToDb(dbDevice, actionLog, datefrom, dateend);
                    }

                    await _unitOfWork.ActionLogRepository.AddAsync(actionLog);

                    await _unitOfWork.SaveChanageAsync();
                }
                return true;
            }
            catch(Exception ex)
            {
                actionLog.Status = "Error";

                actionLog.TimeLogTo = DateTime.Now;

                actionLog.Message = ex.Message;

                await _unitOfWork.ActionLogRepository.AddAsync(actionLog);

                await _unitOfWork.SaveChanageAsync();

                return false;
            }
        }

        public void Action_ConnectAll()
        {
            var listdevice = _singletonTAMInstance.TAMs;
            foreach(var devive in listdevice)
            {
                if (devive.IsConnected)
                    continue;
                devive.IsConnectLoading = true;
                Task.Run(async () =>
                {
                    var res = await Action_Connect(devive);

                    devive.IsConnectLoading = false;
                    devive.IsConnected = res;
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
                    devive.IsConnected = false;
                });
            }
        }

        public async Task<bool> Action_Realand_GetNewlyData(BaseTAMService device)
        {
            try
            {
                if (!device.IsConnected)
                {
                    var rescon = await Action_Connect(device);
                    if (!rescon)
                        return false;
                }
                await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
                var settingenumconvert = GlobalEnumInOutConverter.RealandEnumConverter;
                var dbDevice = await _unitOfWork.DeviceRepository.GetByIdAsync(device.Id);

                var listAttRecord = new List<TAMScanLog>();

                if(device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;

                    List<DateTime> dtList = new List<DateTime>();
                    dtList.Add(DateTime.MinValue);
                    dtList.Add(DateTime.MaxValue);

                    object extraProperty = new object();
                    object extraData = new object();

                    try
                    {
                        extraData = Global.DeviceBusy;
                        var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);

                        extraProperty = true;
                        extraData = dtList;

                        resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                        if (resultRiss)
                        {
                            List<Record> recordList = (List<Record>)extraData;
                            listAttRecord = recordList.Select(x => new TAMScanLog()
                            {
                                Id = Guid.NewGuid(),
                                DeviceID = device.Id,
                                EnrollNumber = (int)x.DIN,
                                VerifyModeView = GlobalUtilities.VerifyType(x.Action),
                                VerifyModeOrigin = x.Action,
                                InOutModeOrigin = x.Verify,
                                InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : "",
                                TimeLog = x.Clock
                            }).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                        });
                        return false;
                    }
                    finally
                    {
                        extraData = Global.DeviceIdle;
                        var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                    }
                }
                if (listAttRecord.Any())
                {
                    var datefrom = listAttRecord.Min(x => x.TimeLog);
                    var dateend = listAttRecord.Max(x => x.TimeLog);

                    var lstDbTAMScanLog = (await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();
                    var lstDbTAMScanLogHistory = (await _unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();

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


        // Đặc thù taisun
        public async Task<bool> SyncDataToDb(Data.Entities.Device devicedb ,ActionLog log, DateTime datefrom, DateTime dateend)
        {
            var config = await _unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains(ConfigEnumConstant.SYNC_DB_.ToString()));

            var servercf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_SERVER.ToString());
            var dbnamecf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_DBNAME.ToString());
            var useridcf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_USERID.ToString());
            var pwdcf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_PWD.ToString());

            if(servercf != null && !string.IsNullOrEmpty(servercf.Value) 
                && dbnamecf != null && !string.IsNullOrEmpty(dbnamecf.Value)
                && useridcf != null && !string.IsNullOrEmpty(useridcf.Value)
                && pwdcf != null && !string.IsNullOrEmpty(pwdcf.Value))
            {
                try
                {
                    var listData = await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == devicedb.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend);

                    var DbCStringBuilder = new SqlConnectionStringBuilder();
                    DbCStringBuilder["Server"] = servercf.Value;
                    DbCStringBuilder["Database"] = dbnamecf.Value;
                    DbCStringBuilder["User Id"] = useridcf.Value;
                    DbCStringBuilder["Password"] = pwdcf.Value;

                    string connectionString = DbCStringBuilder.ToString();

                    Log.Information("Connection string: " + connectionString);

                    var strDateStart = datefrom.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var strDateEnd = dateend.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    var listDataAlready = new List<CHECKINOUTModel>();
                    var ListUserIF = new List<ZkUserInfo>();

                    using (var sqlConnection = new SqlConnection(connectionString))
                    {
                        sqlConnection.Open();

                        var commanGetData = $"Select co.*,BADGENUMBER from CHECKINOUT co " +
                            "left join USERINFO us on co.USERID = us.USERID " +
                            $"where CHECKTIME >= '{strDateStart}' " +
                            $"and CHECKTIME <= '{strDateEnd}'";

                        SqlCommand cm1 = new SqlCommand(commanGetData, sqlConnection);

                        SqlDataReader sdr = cm1.ExecuteReader();

                        while (sdr.Read())
                        {
                            listDataAlready.Add(new CHECKINOUTModel()
                            {
                                USERID = sdr["USERID"].ToString(),
                                CHECKTIME = DateTime.Parse(sdr["CHECKTIME"].ToString())
                            });
                        }
                    }

                    Log.Information("listDataAlready count: " + listDataAlready.Count);

                    using (var sqlConnection = new SqlConnection(connectionString))
                    {
                        sqlConnection.Open();
                        var commandgetuser = "select USERID, BADGENUMBER  from USERINFO";

                        SqlCommand cm2 = new SqlCommand(commandgetuser, sqlConnection);

                        SqlDataReader sdr = cm2.ExecuteReader();

                        while (sdr.Read())
                        {
                            ListUserIF.Add(new ZkUserInfo()
                            {
                                USERID = sdr["USERID"].ToString(),
                                BADGENUMBER = sdr["BADGENUMBER"].ToString(),
                            });
                        }
                    }

                    Log.Information("ListUserIF count: " + ListUserIF.Count);

                    if (!ListUserIF.Any())
                    {
                        log.Status = "Error";
                        log.Message = "Can not get userinfomation!";
                        return false;
                    }

                    var lstAdd = new List<CHECKINOUTModel>();

                    Log.Information("listData join count: " + listData.Count());

                    var ds = from d in listData
                             join u in ListUserIF on d.EnrollNumber.ToString() equals u.BADGENUMBER
                             select new CHECKINOUTModel
                             {
                                 USERID = u.USERID,
                                 CHECKTIME = d.TimeLog,
                                 CHECKTYPE = d.InOutModeView,
                                 VERIFYCODE = d.VerifyModeOrigin ,
                                 SENSORID = d.Device.MachineNo.ToString(),
                                 Memoinfo = null,
                                 WorkCode = 0,
                                 sn = d.Device.SerialNumber.ToString(),
                                 UserExtFmt = 1,
                                 BADGENUMBER = u.BADGENUMBER,
                             };

                    Log.Information("data after join count: " + ds.Count());

                    using (var sqlConnection = new SqlConnection(connectionString))
                    {
                        sqlConnection.Open();
                        int count = 0;
                        foreach (var row in ds)
                        {
                            if (listDataAlready.Any(x => x.USERID == row.USERID && x.CHECKTIME == row.CHECKTIME))
                            {
                                continue;
                            }
                            var commandins = "INSERT INTO CHECKINOUT (USERID, CHECKTIME, CHECKTYPE, VERIFYCODE, SENSORID, WorkCode, sn, UserExtFmt) " +
                                $"VALUES ({row.USERID}, '{row.CHECKTIME.ToString("yyyy-MM-dd HH:mm:ss.fff")}', '{row.CHECKTYPE}', {row.VERIFYCODE}, '{row.SENSORID}', {row.WorkCode}, '{row.sn}', {row.UserExtFmt});";

                            SqlCommand cm3 = new SqlCommand(commandins, sqlConnection);
                            cm3.ExecuteNonQuery();
                            cm3.Dispose();
                            count++;
                        }
                        Log.Information("new data count: " + count);
                        log.Status = "Success";
                        log.TotalDataAdded = count;
                    }

                }
                catch (Exception ex)
                {
                    log.Status = "Error";
                    log.Message = ex.Message;
                    return false;
                }
                return true;
            }
            else
            {
                log.Status = "Error";
                log.Message = "Missing Db connection info";
                return false;
            }

        }


        public async Task<bool> SyncDataByApi(Data.Entities.Device devicedb, ActionLog log, DateTime datefrom, DateTime dateend)
        {
            int maxrow = 100;
            string url = string.Empty;
            var config = await _unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains(ConfigEnumConstant.SYNC_DB_.ToString()));

            var baseapiaddressconf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_APIADDRESS.ToString());
            var maxrowconf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_MAX_DATAROW_PER_REQUEST.ToString());

            if(baseapiaddressconf == null || string.IsNullOrEmpty(baseapiaddressconf.Value))
            {
                log.Status = "Error";
                log.Message = "Missing base sync address";
                return false;
            }
            else
            {
                url = Path.Combine(baseapiaddressconf.Value, "Att_GetData", "SyncDataFromTAMHub");
            }
            if(maxrowconf != null && !string.IsNullOrEmpty(maxrowconf.Value) && int.TryParse(maxrowconf.Value, out int maxrowcon))
            {
                maxrow = maxrowcon;
            }

            try
            {
                var listData = await _unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == devicedb.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend);

                foreach(var listDataChunk in listData.Chunk(maxrow))
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var json = JsonConvert.SerializeObject(listDataChunk);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            HttpResponseMessage response = await client.PostAsync(url, content);

                            response.EnsureSuccessStatusCode();

                            string responseBody = await response.Content.ReadAsStringAsync();
                            
                            var dw = JsonConvert.DeserializeObject<List<Guid>>(responseBody);
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }
                return true;

            }
            catch(Exception ex)
            {
                log.Status = "Error";
                log.Message = ex.Message;
                return false;
            }
        }

    }
}
