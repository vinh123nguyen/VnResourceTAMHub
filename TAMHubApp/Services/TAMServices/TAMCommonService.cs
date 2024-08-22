using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Riss.Devices;
using Serilog;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Text;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures.TAMUtilities;
using TAMHubApp.Models;
using TAMHubApp.ViewModels;
using zkemkeeper;
using static System.Net.Mime.MediaTypeNames;

namespace TAMHubApp.Services.TAMServices
{
    public class TAMCommonService
    {
        private SingletonTAMInstance _singletonTAMInstance;
        private MainViewModel _mainWindow;

        public TAMCommonService()
        {
            _singletonTAMInstance = ServiceLocator.Instance.GetRequiredService<SingletonTAMInstance>();
            _mainWindow = ServiceLocator.Instance.GetRequiredService<MainViewModel>();
        }

        public async Task<bool> Action_Connect(BaseTAMService device)
        {
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var result = false;
                try
                {
                    var dbDevice = await unitOfWork.DeviceRepository.GetByIdAsync(device.Id);
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

                            //RegisEventZk(zkdevice);

                            await unitOfWork.SaveChanageAsync();
                            _singletonTAMInstance.UpdateDeviceFromDb(device.Id);
                        }
                    }
                    else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
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
                        deviceins.ConnectionModel = 5;

                        var deviceConnectionIns = DeviceConnection.CreateConnection(ref deviceins);

                        if (deviceConnectionIns.Open() > 0)
                        {
                            rissdevice.DeviceIns = deviceins;
                            rissdevice.DeviceConnectionIns = deviceConnectionIns;
                            rissdevice.IsConnected = true;
                            result = true;
                            _ = Task.Run(async () =>
                            {
                                await Action_Realand_GetDeviceInfo(deviceins, deviceConnectionIns, rissdevice, device.Id);
                                _singletonTAMInstance.UpdateDeviceFromDb(device.Id);
                            });
                        }
                    }
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
        }

        public async Task Action_Realand_GetDeviceInfo(Riss.Devices.Device deviceins, DeviceConnection deviceConnectionIns, RealandDeviceItem rissdevice, Guid DeviceID)
        {
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var result = true;

                var dbDevice = await unitOfWork.DeviceRepository.GetByIdAsync(DeviceID);
                if (dbDevice == null)
                    return;

                object extraProperty2 = new object();
                object extraData2 = new object();
                var result1 = deviceConnectionIns.GetProperty(DeviceProperty.Status, extraProperty2, ref deviceins, ref extraData2);

                if (result1)
                {
                    UInt32[] count = (UInt32[])extraData2;

                    rissdevice.NewLogCount = (int)count[6];
                    dbDevice.NewLogCount = (int)count[6];

                    rissdevice.AllLogCount = (int)count[8];
                    dbDevice.AllLogCount = (int)count[8];
                }

                if (string.IsNullOrEmpty(rissdevice.SerialNumber))
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
                if (string.IsNullOrEmpty(rissdevice.ProductName))
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
                await unitOfWork.SaveChanageAsync();
            }
        }

        public bool Action_Dicconnect(BaseTAMService device)
        {
            try
            {
                if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    zkdevice.IsConnected = false;
                    zkdevice.DeviceIns.Disconnect();
                }
                else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
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

        public async Task<bool> Action_GetDataByDate(TAMCommonService service, BaseTAMService device, DateTime datefrom, DateTime dateend, string EnrollNumber, string DownloadType = "Manual", bool IsDownSchedule = false)
        {
            Log.Information($"[Start Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
            
            if(DownloadType != "Manual" && device.IsInactive)
            {
                Log.Information($"[{device.IPAddress}]:" + "Inactive skip device");
                Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                return true;
            }
            
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var actionLog = new ActionLog();
                actionLog.Id = Guid.NewGuid();
                datefrom = datefrom.Date;
                dateend = dateend.Date.AddDays(1).AddMicroseconds(-1);
                actionLog.DateDownDataFrom = datefrom;
                actionLog.DateDownDataTo = dateend;
                actionLog.TimeLogFrom = DateTime.Now;
                actionLog.DownloadType = DownloadType;
                actionLog.DeviceID = device.Id;

                Log.Information($"[{device.IPAddress}]:" + "actionLog Start: {@actionLog}", actionLog);

                try
                {
                    //var dbDesvice = await unitOfWork.TAMScanLogRepository.GetAllAsync();
                    //var d = dbDesvice.ToList();

                    if (!device.IsConnected)
                    {

                        var rescon = await service.Action_Connect(device);
                        if (!rescon)
                        {
                            actionLog.Message = "Connect fail";
                            actionLog.Status = "Error";

                            actionLog.TimeLogTo = DateTime.Now;

                            await unitOfWork.ActionLogRepository.AddAsync(actionLog);

                            await unitOfWork.SaveChanageAsync();

                            Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                            return false;
                        }
                    }

                    var dbDevice = await unitOfWork.DeviceRepository.GetByIdAsync(device.Id);

                    Log.Information($"[{device.IPAddress}]:" + "Start Get Data");

                    var listAttRecord = new List<TAMScanLog>();
                    if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                    {
                        var zkdevice = (ZKTecoDeviceItem)device;
                        //await GlobalEnumInOutConverter.InitUpdateEnumConverter("ZKTeco");
                        var settingenumconvert = GlobalEnumInOutConverter.ZKEnumConverter;
                        try
                        {
                            if (zkdevice.ZKTeco_IsCanGetLogByTime && zkdevice.ZKTeco_IsNewFirmwareMachine)
                            {
                                if (zkdevice.DeviceIns.ReadTimeGLogData(zkdevice.MachineNo, datefrom.ToString("yyyy-MM-dd HH:mm:ss"), dateend.ToString("yyyy-MM-dd HH:mm:ss")))
                                {
                                    Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 1");
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
                                                InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
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
                                                InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await Action_Connect(device);

                                    Log.Information($"[{device.IPAddress}]:" + " Start Read Data Lần 2, Res Connect: " + conres.ToString());

                                    if (conres)
                                    {
                                        if (zkdevice.DeviceIns.ReadTimeGLogData(zkdevice.MachineNo, datefrom.ToString("yyyy-MM-dd HH:mm:ss"), dateend.ToString("yyyy-MM-dd HH:mm:ss")))
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 2");
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
                                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
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
                                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                    });

                                                }
                                            }
                                        }
                                        else
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Fail Lần 2");
                                            device.IsConnected = false;
                                        }
                                    }
                                    else
                                    {
                                        Log.Information($"[{device.IPAddress}]:" + " Connect Fail Lần 2");
                                        device.IsConnected = false;
                                    }
                                }
                            }
                            else
                            {
                                var listAttRecordAll = new List<TAMScanLog>();
                                if (zkdevice.DeviceIns.ReadAllGLogData(zkdevice.MachineNo))
                                {
                                    Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 1");
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
                                                InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
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
                                                InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await Action_Connect(device);

                                    Log.Information($"[{device.IPAddress}]:" + " Start Read Data Lần 2, Res Connect: " + conres.ToString());

                                    if (conres)
                                    {
                                        if (zkdevice.DeviceIns.ReadAllGLogData(zkdevice.MachineNo))
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 1");
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
                                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
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
                                                        InOutModeView = settingenumconvert.TryGetValue(tInOutMode, out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                    });

                                                }
                                            }
                                        }
                                        else
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Fail Lần 2");
                                            device.IsConnected = false;
                                        }
                                    }
                                    else
                                    {
                                        Log.Information($"[{device.IPAddress}]:" + " Connect Fail Lần 2");
                                        device.IsConnected = false;
                                    }
                                }
                                listAttRecord = listAttRecordAll.Where(x => x.TimeLog >= datefrom && x.TimeLog <= dateend).ToList();
                            }
                        }
                        catch(Exception ex)
                        {
                            device.IsConnected = false;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                            });
                            return false;
                        }
                    }
                    else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                    {
                        var rissdevice = (RealandDeviceItem)device;
                        //await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
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
                            extraData = Global.DeviceBusy;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);

                            extraProperty = boolList;
                            extraData = dtList;

                            resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                            if (resultRiss)
                            {
                                List<Record> recordList = (List<Record>)extraData;
                                listAttRecord = recordList.Where(x => x.Clock >= datefrom && x.Clock <= dateend && x.DIN != 0).Select(x => new TAMScanLog()
                                {
                                    Id = Guid.NewGuid(),
                                    DeviceID = device.Id,
                                    EnrollNumber = (int)x.DIN,
                                    VerifyModeView = GlobalUtilities.VerifyType(x.Action),
                                    VerifyModeOrigin = x.Action,
                                    InOutModeOrigin = x.Verify,
                                    InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : null,
                                    TimeLog = x.Clock
                                }).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            device.IsConnected = false;
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
                        listAttRecord = listAttRecord.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                    }
                    Log.Information($"[{device.IPAddress}]:" + "End Get Data, Num row: " + listAttRecord.Count.ToString());
                    if (listAttRecord.Any())
                    {
                        actionLog.TotalRowDownload = listAttRecord.Count;

                        var lstDbTAMScanLog = (await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();
                        //var lstDbTAMScanLogHistory = (await unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();
                        var synctype = await unitOfWork.AppSettingRepository.FindAsync(x => x.Name == ConfigEnumConstant.SYNC_TYPE.ToString());

                        var lis1 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));
                        //var lis2 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));

                        //var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lis1.Concat(lis2));
                        var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lis1);

                        var listInsertscanlog = listAttRecord.Where(item => !hsetDataDbTAMScanLog.Contains((item.EnrollNumber, item.TimeLog))).ToList();

                        Log.Information($"[{device.IPAddress}]:" + "Insert Data, Num row: " + listInsertscanlog.Count.ToString());

                        actionLog.TotalNewlyData = 0;

                        if (listInsertscanlog != null && listInsertscanlog.Any())
                        {
                            actionLog.TotalNewlyData = listInsertscanlog.Count;

                            await unitOfWork.TAMScanLogRepository.AddRangeAsync(listInsertscanlog);
                        }

                        actionLog.Status = "Success";

                        await unitOfWork.SaveChanageAsync();

                        Log.Information($"[{device.IPAddress}]:" + " Start Sync Data");

                        if (synctype != null && synctype.Value == "DB")
                        {
                            await service.SyncDataToDb(dbDevice, actionLog, datefrom, dateend, EnrollNumber);
                        }
                        if (synctype != null && synctype.Value == "API")
                        {
                            await service.SyncDataByApi(dbDevice, actionLog, datefrom, dateend, EnrollNumber);
                        }

                        Log.Information($"[{device.IPAddress}]:" + " End Sync Data");



                    }
                    else
                    {
                        actionLog.Status = "Error";
                        actionLog.Message = "Data Downloaded is empty";
                    }
                    actionLog.TimeLogTo = DateTime.Now;
                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                    await unitOfWork.SaveChanageAsync();
                    Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"[{device.IPAddress}]:" + " Have Exception: " + ex.Message);

                    actionLog.Status = "Error";

                    actionLog.TimeLogTo = DateTime.Now;

                    actionLog.Message = ex.Message;

                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);

                    await unitOfWork.SaveChanageAsync();

                    Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                    return false;
                }
            }
                
        }

        public void Action_ConnectAll()
        {
            var listdevice = _singletonTAMInstance.TAMs;
            foreach (var devive in listdevice)
            {
                if (devive.IsConnected)
                    continue;
                devive.IsConnectLoading = true;
                Task.Run(async () =>
                {
                    var service = new TAMCommonService();
                    var res = await service.Action_Connect(devive);

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
                if (!devive.IsConnected)
                    continue;
                Task.Run(() =>
                {
                    var service = new TAMCommonService();
                    service.Action_Dicconnect(devive);
                    devive.IsConnected = false;
                });
                //Task.Run(() =>
                //{
                //    var service = new TAMCommonService();
                //    service.Action_Dicconnect(itemdeviceAction);
                //    itemdeviceAction.IsConnectLoading = false;
                //    itemdeviceAction.IsConnected = false;
                //});
            }
        }

        // Đặc thù taisun
        public async Task<bool> SyncDataToDb(Data.Entities.Device devicedb, ActionLog log, DateTime datefrom, DateTime dateend, string EnrollNumber)
        {
            Log.Information($"[{devicedb.IPAddress}]:" + " Start Sync Data to DB");
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var config = await unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains(ConfigEnumConstant.SYNC_DB_.ToString()));

                var servercf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_SERVER.ToString());
                var dbnamecf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_DBNAME.ToString());
                var useridcf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_USERID.ToString());
                var pwdcf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_DB_PWD.ToString());

                if (servercf != null && !string.IsNullOrEmpty(servercf.Value)
                    && dbnamecf != null && !string.IsNullOrEmpty(dbnamecf.Value)
                    && useridcf != null && !string.IsNullOrEmpty(useridcf.Value)
                    && pwdcf != null && !string.IsNullOrEmpty(pwdcf.Value))
                {
                    try
                    {
                        var listData = await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == devicedb.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend);
                        if (!string.IsNullOrEmpty(EnrollNumber))
                        {
                            listData = listData.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                        }

                        var DbCStringBuilder = new SqlConnectionStringBuilder();
                        DbCStringBuilder["Server"] = servercf.Value;
                        DbCStringBuilder["Database"] = dbnamecf.Value;
                        DbCStringBuilder["User Id"] = useridcf.Value;
                        DbCStringBuilder["Password"] = pwdcf.Value;

                        string connectionString = DbCStringBuilder.ToString();

                        Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "Connection string: " + connectionString);

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

                        Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "listDataAlready count: " + listDataAlready.Count);

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

                        Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "ListUserIF count: " + ListUserIF.Count);

                        if (!ListUserIF.Any())
                        {
                            log.Status = "Error";
                            log.Message = "Can not get userinfomation!";
                            return false;
                        }

                        var lstAdd = new List<CHECKINOUTModel>();

                        Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "listData join count: " + listData.Count());

                        var ds = from d in listData
                                 join u in ListUserIF on d.EnrollNumber.ToString() equals u.BADGENUMBER
                                 select new CHECKINOUTModel
                                 {
                                     USERID = u.USERID,
                                     CHECKTIME = d.TimeLog,
                                     CHECKTYPE = d.InOutModeView,
                                     VERIFYCODE = d.VerifyModeOrigin,
                                     SENSORID = devicedb.MachineNo.ToString(),
                                     Memoinfo = null,
                                     WorkCode = 0,
                                     sn = devicedb.SerialNumber == null ? "" : devicedb.SerialNumber.ToString(),
                                     UserExtFmt = 1,
                                     BADGENUMBER = u.BADGENUMBER,
                                 };

                        Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "data after join count: " + ds.Count());

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
                            Log.Information($"[{devicedb.IPAddress}]: Sync Data To DB" + "new data count: " + count);
                            log.Status = "Success";
                            log.TotalDataAdded = count;
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[{devicedb.IPAddress}]:" + " Sync data to DB have Error: " + ex.Message);

                        log.Status = "Error";
                        log.Message = ex.Message;
                        return false;
                    }
                    return true;
                }
                else
                {
                    Log.Error($"[{devicedb.IPAddress}]:" + " Sync data to DB have Error: Missing Db connection info");

                    log.Status = "Error";
                    log.Message = "Missing Db connection info";
                    return false;
                }
            }
        }


        public async Task<bool> SyncDataByApi(Data.Entities.Device devicedb, ActionLog log, DateTime datefrom, DateTime dateend, string EnrollNumber)
        {
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                log.TotalDataAdded = 0;
                int maxrow = 100;
                string url = string.Empty;
                var config = await unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains(ConfigEnumConstant.SYNC_API_.ToString()));

                var baseapiaddressconf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_APIADDRESS.ToString());
                var maxrowconf = config.FirstOrDefault(x => x.Name == ConfigEnumConstant.SYNC_API_MAX_DATAROW_PER_REQUEST.ToString());

                if (baseapiaddressconf == null || string.IsNullOrEmpty(baseapiaddressconf.Value))
                {
                    log.Status = "Error";
                    log.Message = "Missing base sync address";
                    return false;
                }
                else
                {
                    url = Path.Combine(baseapiaddressconf.Value, "Att_GetData", "SyncDataFromTAMHub").Replace(@"\", "/");
                }
                if (maxrowconf != null && !string.IsNullOrEmpty(maxrowconf.Value) && int.TryParse(maxrowconf.Value, out int maxrowcon))
                {
                    maxrow = maxrowcon;
                }

                try
                {
                    var listData = await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == devicedb.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend);

                    if (!string.IsNullOrEmpty(EnrollNumber))
                    {
                        listData = listData.Where(x => x.EnrollNumber.ToString() == EnrollNumber).ToList();
                    }

                    foreach (var listDataChunk in listData.Chunk(maxrow))
                    {
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(300);
                                var lst = listDataChunk.Select(x => new SyncTAMScanLogDTO()
                                {
                                    ID = x.Id,
                                    EnrollNumber = x.EnrollNumber,
                                    InOutMode = x.InOutModeView,
                                    TimeLog = x.TimeLog,
                                    MachineNo = devicedb.MachineNo,
                                    CompanyCode = devicedb.CompanyCode
                                });
                                var settings = new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                };
                                var json = JsonConvert.SerializeObject(lst, settings);

                                var payload = new
                                {
                                    jsondata = json
                                };

                                var content = new StringContent(JsonConvert.SerializeObject(payload, settings), Encoding.UTF8, "application/json");

                                HttpResponseMessage response = await client.PostAsync(url, content);

                                response.EnsureSuccessStatusCode();

                                if (response.IsSuccessStatusCode)
                                {
                                    string responseBody = await response.Content.ReadAsStringAsync();

                                    var ids = JsonConvert.DeserializeObject<List<Guid>>(responseBody);

                                    log.TotalDataAdded += ids == null ? 0 : ids.Count;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Status = "Error";
                            log.Message = ex.Message;
                            return false;
                        }
                    }
                    log.Status = "Success";
                    return true;

                }
                catch (Exception ex)
                {
                    log.Status = "Error";
                    log.Message = ex.Message;
                    return false;
                }
            }
        }


        public async Task<bool> Action_OnlyPushData(BaseTAMService device, DateTime datefrom, DateTime dateend, string EnrollNumber)
        {
            using (var dbContext = new AppDbContext())
            {
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var actionLog = new ActionLog();
                actionLog.Id = Guid.NewGuid();
                datefrom = datefrom.Date;
                dateend = dateend.Date.AddDays(1).AddMicroseconds(-1);
                actionLog.DateDownDataFrom = datefrom;
                actionLog.DateDownDataTo = dateend;
                actionLog.TimeLogFrom = DateTime.Now;
                actionLog.DownloadType = "Manual";
                actionLog.DeviceID = device.Id;
                try
                {
                    var dbDevice = await unitOfWork.DeviceRepository.GetByIdAsync(device.Id);
                    var synctype = await unitOfWork.AppSettingRepository.FindAsync(x => x.Name == ConfigEnumConstant.SYNC_TYPE.ToString());
                    actionLog.Status = "Success";

                    actionLog.TimeLogTo = DateTime.Now;
                    var res = true;
                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                    if (synctype != null && synctype.Value == "DB")
                    {
                        res = await SyncDataToDb(dbDevice, actionLog, datefrom, dateend, EnrollNumber);
                    }
                    if (synctype != null && synctype.Value == "API")
                    {
                        res = await SyncDataByApi(dbDevice, actionLog, datefrom, dateend, EnrollNumber);
                    }

                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);

                    await unitOfWork.SaveChanageAsync();
                    return res;

                }
                catch (Exception ex)
                {
                    actionLog.Status = "Error";

                    actionLog.TimeLogTo = DateTime.Now;

                    actionLog.Message = ex.Message;

                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);

                    await unitOfWork.SaveChanageAsync();

                    return false;
                }
            }
        }
    }
}
