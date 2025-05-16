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
                        else
                        {
                            var deviceConnectionIns2 = DeviceConnection.CreateConnection(ref deviceins);
                            if (deviceConnectionIns2.Open() > 0)
                            {
                                rissdevice.DeviceIns = deviceins;
                                rissdevice.DeviceConnectionIns = deviceConnectionIns2;
                                rissdevice.IsConnected = true;
                                result = true;
                                _ = Task.Run(async () =>
                                {
                                    await Action_Realand_GetDeviceInfo(deviceins, deviceConnectionIns2, rissdevice, device.Id);
                                    _singletonTAMInstance.UpdateDeviceFromDb(device.Id);
                                });
                            }
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
            
            if(IsDownSchedule && device.IsInactive)
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
                if(!IsDownSchedule)
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
                        //var settingenumconvert = GlobalEnumInOutConverter.ZKEnumConverter;
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    service.Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await service.Action_Connect(device);

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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    service.Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await service.Action_Connect(device);

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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                        //var settingenumconvert = GlobalEnumInOutConverter.RealandEnumConverter;

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
                            var rissdevice = (RealandDeviceItem)device;
                            extraData = Global.DeviceBusy;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                            if (!resultRiss)
                            {
                                Log.Information($"[{device.IPAddress}]: " + "Set DeviceBusy Fail");

                                service.Action_Dicconnect(device);
                                device.IsConnected = false;
                                var conres = await service.Action_Connect(device);
                                Log.Information($"[{device.IPAddress}]:" + " Set DeviceBusy lần 2, Res Connect: " + conres.ToString());
                                if (conres)
                                {
                                    rissdevice = (RealandDeviceItem)device;
                                    extraData = Global.DeviceBusy;
                                    resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                                }
                                else
                                {
                                    actionLog.Status = "Error";
                                    actionLog.Message = "Reconnect fail";
                                    actionLog.TimeLogTo = DateTime.Now;
                                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                    await unitOfWork.SaveChanageAsync();
                                    Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                                    return false;
                                }
                            }
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
                                    //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                    TimeLog = x.Clock,
                                    DateCreate = DateTime.Now
                                }).ToList();
                            }
                            else
                            {
                                Log.Information($"[{device.IPAddress}]: " + "Get AttRecords Fail");

                                service.Action_Dicconnect(device);
                                device.IsConnected = false;
                                var conres = await service.Action_Connect(device);
                                rissdevice = (RealandDeviceItem)device;
                                Log.Information($"[{device.IPAddress}]:" + "Connect Get AttRecords lần 2, Res Connect: " + conres.ToString());

                                if (conres)
                                {
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
                                            //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                            TimeLog = x.Clock,
                                            DateCreate = DateTime.Now
                                        }).ToList();
                                    }
                                    else
                                    {
                                        Log.Information($"[{device.IPAddress}]: " + "Get AttRecords Lần 2 Fail");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Information($"[{device.IPAddress}]: " + "Get AttRecords Error: +" + ex.Message);

                            try
                            {
                                Log.Information($"[{device.IPAddress}]: " + "Start get in catch");

                                service.Action_Dicconnect(device);
                                device.IsConnected = false;
                                var conres = await service.Action_Connect(device);
                                var rissdevice = (RealandDeviceItem)device;
                                Log.Information($"[{device.IPAddress}]:" + "Connect Get AttRecords lần 2 in catch, Res Connect: " + conres.ToString());

                                if (conres)
                                {
                                    extraProperty = boolList;
                                    extraData = dtList;

                                    var resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
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
                                            //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                            TimeLog = x.Clock,
                                            DateCreate = DateTime.Now
                                        }).ToList();
                                    }
                                }
                            }
                            catch(Exception ex1)
                            {
                                device.IsConnected = false;
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _mainWindow.Action_ShowToastNotify(ex1.Message, ToastNotifyType.Error);
                                });

                                actionLog.Status = "Error";
                                actionLog.Message = ex1.Message;
                                actionLog.TimeLogTo = DateTime.Now;
                                actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                await unitOfWork.SaveChanageAsync();
                                Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                                return false;
                            }
                        }
                        finally
                        {
                            var rissdevice = (RealandDeviceItem)device;
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

                        var lstDbTAMScanLog = (await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();
                        var lstDbTAMScanLogHistory = (await unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();

                        var lst1 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));
                        var lst2 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));

                        var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lst1.Concat(lst2));

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

                        await service.SyncDataByApi(dbDevice, actionLog, datefrom, dateend, EnrollNumber, IsDownSchedule);

                        Log.Information($"[{device.IPAddress}]:" + " End Sync Data");
                    }
                    else
                    {
                        actionLog.Status = "Success";
                        actionLog.TotalRowDownload = 0;
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

                    actionLog.Id = Guid.NewGuid();
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
                if (devive.IsConnected || devive.IsInactive)
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
            }
        }

        public async Task<bool> SyncDataByApi(Data.Entities.Device devicedb, ActionLog log, DateTime datefrom, DateTime dateend, string EnrollNumber, bool IsDownSchedule = false)
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
                    url = Path.Combine(baseapiaddressconf.Value, "Att_GetData", "SyncDataFromTAMHubV2").Replace(@"\", "/");

                if (maxrowconf != null && !string.IsNullOrEmpty(maxrowconf.Value) && int.TryParse(maxrowconf.Value, out int maxrowcon))
                    maxrow = maxrowcon;

                try
                {
                    var listData = new List<SyncTAMScanLogDTO>();

                    var LstTAMScanLog = await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == devicedb.Id);

                    var listDataConfig = await unitOfWork.AppSettingRepository.GetAllAsync();

                    var zkInKey = listDataConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_IN_KEY.ToString());
                    var zkOutKey = listDataConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_OUT_KEY.ToString());
                    var dicConfig = new Dictionary<string, string>();
                    if (zkInKey != null && !string.IsNullOrEmpty(zkInKey.Value))
                        dicConfig.Add(EnumInOutType.E_IN.ToString(), zkInKey.Value);

                    if (zkOutKey != null && !string.IsNullOrEmpty(zkOutKey.Value))
                        dicConfig.Add(EnumInOutType.E_OUT.ToString(), zkOutKey.Value);

                    var listDatanew = LstTAMScanLog.Select(x => new SyncTAMScanLogDTO()
                    {
                        ID = x.Id,
                        EnrollNumber = x.EnrollNumber,
                        //InOutMode = x.InOutModeView,
                        TimeLog = x.TimeLog,
                        MachineNo = devicedb.MachineNo,
                        CompanyCode = devicedb.CompanyCode,
                        InOutMode = dicConfig.TryGetValue(devicedb?.InOutType ?? "", out var _inOutMode) ? _inOutMode : null,
                    });
                    if (listDatanew.Any())
                        listData.AddRange(listDatanew);
                    if (!IsDownSchedule)
                    {
                        var listDatold = (await unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == devicedb.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).Select(x => new SyncTAMScanLogDTO()
                        {
                            ID = x.Id,
                            EnrollNumber = x.EnrollNumber,
                            //InOutMode = x.InOutModeView,
                            TimeLog = x.TimeLog,
                            MachineNo = devicedb.MachineNo,
                            CompanyCode = devicedb.CompanyCode,
                            InOutMode = dicConfig.TryGetValue(devicedb?.InOutType ?? "", out var _inOutMode) ? _inOutMode : null
                        });
                        if (listDatold.Any())
                            listData.AddRange(listDatold);
                    }
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

                                var settings = new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                };
                                var json = JsonConvert.SerializeObject(listDataChunk, settings);

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

                                    var resparse = JsonConvert.DeserializeObject<SyncTAMScanLogResDTO>(responseBody);

                                    log.TotalDataAdded += resparse != null && resparse.InsertedIds != null ? resparse.InsertedIds.Count : 0;
                                    List<Guid> lstCheck = new List<Guid>();
                                    if (resparse != null && resparse.InsertedIds != null && resparse.InsertedIds.Any())
                                        lstCheck.AddRange(resparse.InsertedIds);
                                    if(resparse != null && resparse.DuplicateIds != null && resparse.DuplicateIds.Any())
                                        lstCheck.AddRange(resparse.DuplicateIds);

                                    if (lstCheck.Any())
                                    {
                                        var lstRemove = LstTAMScanLog.Where(x => lstCheck.Contains(x.Id)).ToList();
                                        var lstAdd = lstRemove.Select(x => new TAMScanLogHistory()
                                        {
                                            Id = Guid.NewGuid(),
                                            DeviceID = x.DeviceID,
                                            EnrollNumber = x.EnrollNumber,
                                            VerifyModeOrigin = x.VerifyModeOrigin,
                                            VerifyModeView = x.VerifyModeView,
                                            InOutModeView = x.InOutModeView,
                                            InOutModeOrigin = x.InOutModeOrigin,
                                            TimeLog = x.TimeLog,
                                            DateCreate = x.DateCreate,
                                        });
                                        unitOfWork.TAMScanLogRepository.RemoveRange(lstRemove);
                                        await unitOfWork.TAMScanLogHistoryRepository.AddRangeAsync(lstAdd);
                                        await unitOfWork.SaveChanageAsync();
                                    }
                                }
                                else
                                {
                                    log.Status = "Error";
                                    log.Message = "Status Code: " + response.StatusCode.ToString();
                                    return false;
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
                    actionLog.Status = "Success";

                    actionLog.TimeLogTo = DateTime.Now;
                    var res = true;
                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);

                    res = await SyncDataByApi(dbDevice, actionLog, datefrom, dateend, EnrollNumber);

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

        public bool Action_DicconnectV2(BaseTAMService device)
        {
            try
            {
                if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                {
                    var zkdevice = (ZKTecoDeviceItem)device;
                    zkdevice.IsConnected = false;
                    zkdevice.DeviceIns.Disconnect();
                    zkdevice.DeviceIns = null;
                }
                else if (device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;
                    if(rissdevice.DeviceConnectionIns != null)
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
        public async Task Action_GetDeviceInfo_RealandV2(Riss.Devices.Device deviceins, DeviceConnection deviceConnectionIns, RealandDeviceItem rissdevice, Data.Entities.Device dbDevice, IUnitOfWork unitOfWork)
        {
            var result = false;
            var isHaveChange = false;

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
                isHaveChange = true;
            }
            if (string.IsNullOrEmpty(dbDevice.SerialNumber))
            {
                object extraProperty1 = new object();
                object extraData1 = Zd2911Utils.DeviceSerialNo;
                result = deviceConnectionIns.GetProperty(DeviceProperty.SerialNo, extraProperty1, ref deviceins, ref extraData1);
                if (result)
                {
                    rissdevice.SerialNumber = (string)extraData1;
                    dbDevice.SerialNumber = (string)extraData1;
                }
                isHaveChange = true;
            }
            if (string.IsNullOrEmpty(dbDevice.ProductName))
            {
                object extraProperty1 = new object();
                object extraData1 = Zd2911Utils.DeviceModel;
                result = deviceConnectionIns.GetProperty(DeviceProperty.Model, extraProperty1, ref deviceins, ref extraData1);
                if (result)
                {
                    rissdevice.ProductName = (string)extraData1;
                    dbDevice.ProductName = (string)extraData1;
                }
                isHaveChange = true;
            }
            if (isHaveChange)
                await unitOfWork.SaveChanageAsync();
        }
        public async Task<bool> Action_ConnectV2(BaseTAMService device, IUnitOfWork unitOfWork, bool IsDownSchedule = false, bool IsRetry = false)
        {
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
                else if(device.DeviceSDKType == Infrastructures.Enums.TAMSDKTypes.E_REALAND)
                {
                    var rissdevice = (RealandDeviceItem)device;

                    if(rissdevice.DeviceConnectionIns != null && rissdevice.DeviceIns != null)
                    {
                        try
                        {
                            rissdevice.DeviceConnectionIns.Close();
                            rissdevice.DeviceConnectionIns = null;
                            rissdevice.DeviceIns = null;
                        }
                        catch (Exception)
                        {
                        }
                    }

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

                    if(deviceConnectionIns.Open() > 0)
                    {
                        rissdevice.DeviceIns = deviceins;
                        rissdevice.DeviceConnectionIns = deviceConnectionIns;
                        rissdevice.IsConnected = true;
                        result = true;
                        if(!IsDownSchedule && !IsRetry)
                            await Action_GetDeviceInfo_RealandV2(deviceins, deviceConnectionIns, rissdevice, dbDevice, unitOfWork);
                    }
                    else
                    {
                        // retry connect
                        var deviceConnectionIns2 = DeviceConnection.CreateConnection(ref deviceins);
                        if (deviceConnectionIns2.Open() > 0)
                        {
                            rissdevice.DeviceIns = deviceins;
                            rissdevice.DeviceConnectionIns = deviceConnectionIns2;
                            rissdevice.IsConnected = true;
                            result = true;
                            if (!IsDownSchedule && !IsRetry)
                                await Action_GetDeviceInfo_RealandV2(deviceins, deviceConnectionIns, rissdevice, dbDevice, unitOfWork);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                });
                result = false;
            }
            return result;
        }

        public async Task<bool> Action_GetDataByDateV2(TAMCommonService service, BaseTAMService device, DateTime datefrom, DateTime dateend, string EnrollNumber, string DownloadType = "Manual", bool IsDownSchedule = false)
        {
            Log.Information($"[Start Get Single Device FNV2] + Name: {device.Name} + DeviceIP: {device.IPAddress}");
            if(IsDownSchedule && device.IsInactive)
            {
                Log.Information($"[End Get Single Device FNV2] + Name: {device.Name} + DeviceIP: {device.IPAddress} + Message: Skip Inactive device");
            }

            using (var dbContext = new AppDbContext())
            {
                bool MasterResult = false;
                UnitOfWork unitOfWork = new UnitOfWork(dbContext);
                var actionLog = new ActionLog();
                actionLog.Id = Guid.NewGuid();
                if (!IsDownSchedule)
                    datefrom = datefrom.Date;
                dateend = dateend.Date.AddDays(1).AddMicroseconds(-1);

                actionLog.DateDownDataFrom = datefrom; // set log
                actionLog.DateDownDataTo = dateend; // set log
                actionLog.TimeLogFrom = DateTime.Now; // set log
                actionLog.DownloadType = DownloadType; // set log
                actionLog.DeviceID = device.Id; // set log

                Log.Information($"V2:[{device.IPAddress}]: " + "actionLog Start: {@actionLog}", actionLog);

                try
                {
                    var listAttRecord = new List<TAMScanLog>();
                    // check connect and connect on first times
                    if (!device.IsConnected)
                    {
                        var resconnect = await service.Action_ConnectV2(device, unitOfWork, IsDownSchedule);
                        if (!resconnect)
                        {
                            actionLog.Message = "Connect Fail";
                            actionLog.Status = "Error";

                            actionLog.TimeLogTo = DateTime.Now;
                            actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                            await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                            await unitOfWork.SaveChanageAsync();

                            Log.Information($"[End Get Single Device FNV2] + Name: {device.Name} + DeviceIP: {device.IPAddress} + Message: Can not connect in first time");
                            return false;
                        }
                    }

                    if (device.DeviceSDKType == TAMSDKTypes.E_ZKTECO)
                    {
                        var zkdevice = (ZKTecoDeviceItem)device;
                        //var settingenumconvert = GlobalEnumInOutConverter.ZKEnumConverter;
                        var inKey = await unitOfWork.AppSettingRepository.FindAsync(x => x.Name.Contains(ConfigEnumConstant.ENUM_INOUT_IN_KEY.ToString()));
                        try
                        {
                            if (zkdevice.ZKTeco_IsCanGetLogByTime && zkdevice.ZKTeco_IsNewFirmwareMachine)
                            {
                                if (zkdevice.DeviceIns.ReadTimeGLogData(zkdevice.MachineNo, datefrom.ToString("yyyy-MM-dd HH:mm:ss"), dateend.ToString("yyyy-MM-dd HH:mm:ss")))
                                {
                                    Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 1");
                                    MasterResult = true;
                                    if (zkdevice.ZKTeco_IsTFTMachine || zkdevice.ZKTeco_IsIFaceDevice)
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    service.Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await service.Action_Connect(device);

                                    Log.Information($"[{device.IPAddress}]:" + " Start Read Data Lần 2, Res Connect: " + conres.ToString());

                                    if (conres)
                                    {
                                        if (zkdevice.DeviceIns.ReadTimeGLogData(zkdevice.MachineNo, datefrom.ToString("yyyy-MM-dd HH:mm:ss"), dateend.ToString("yyyy-MM-dd HH:mm:ss")))
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 2");
                                            MasterResult = true;
                                            if (zkdevice.ZKTeco_IsTFTMachine || zkdevice.ZKTeco_IsIFaceDevice)
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                    MasterResult = true;
                                    if (zkdevice.ZKTeco_IsTFTMachine || zkdevice.ZKTeco_IsIFaceDevice)
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
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
                                                //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                DateCreate = DateTime.Now
                                            });

                                        }
                                    }
                                }
                                else
                                {
                                    service.Action_Dicconnect(device);
                                    device.IsConnected = false;
                                    var conres = await service.Action_Connect(device);

                                    Log.Information($"[{device.IPAddress}]:" + " Start Read Data Lần 2, Res Connect: " + conres.ToString());

                                    if (conres)
                                    {
                                        if (zkdevice.DeviceIns.ReadAllGLogData(zkdevice.MachineNo))
                                        {
                                            Log.Information($"[{device.IPAddress}]:" + " Read Data Done Lần 1");
                                            MasterResult = true;
                                            if (zkdevice.ZKTeco_IsTFTMachine || zkdevice.ZKTeco_IsIFaceDevice)
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                                                        //InOutModeView = settingenumconvert.TryGetValue(device.InOutType ?? "", out string cvenum) ? cvenum : null,
                                                        TimeLog = new DateTime(year, month, day, hour, minute, second),
                                                        DateCreate = DateTime.Now
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
                        catch (Exception ex)
                        {
                            device.IsConnected = false;
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                _mainWindow.Action_ShowToastNotify(ex.Message, ToastNotifyType.Error);
                            });
                            return false;
                        }
                    }
                    else if(device.DeviceSDKType == TAMSDKTypes.E_REALAND)
                    {
                        object extraProperty = new object();
                        object extraData = new object();

                        List<DateTime> dtList = new List<DateTime>();
                        dtList.Add(datefrom);
                        dtList.Add(dateend);
                        List<bool> boolList = new List<bool>();
                        boolList.Add(false);
                        boolList.Add(false);

                        try
                        {
                            #region Check Connect and Retry Connect/Set Busy Device
                            var rissdevice = (RealandDeviceItem)device;
                            extraData = Global.DeviceBusy;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                            if (!resultRiss)
                            {
                                Log.Information($"V2:[{device.IPAddress}]: " + "Set DeviceBusy Fail");
                                Action_DicconnectV2(device);

                                var resReconnect = await service.Action_ConnectV2(device, unitOfWork, IsDownSchedule, true);
                                Log.Information($"V2:[{device.IPAddress}]:" + " Reconenct Time 2, Res Connect: " + resReconnect.ToString());
                                if (resReconnect)
                                {
                                    extraProperty = new object();
                                    extraData = Global.DeviceBusy;
                                    resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                                    Log.Information($"V2:[{device.IPAddress}]:" + " Set DeviceBusy lần 2, Res Connect: " + resReconnect.ToString());
                                    if (!resultRiss)
                                    {
                                        actionLog.Status = "Error";
                                        actionLog.Message = "Reconnect fail";
                                        actionLog.TimeLogTo = DateTime.Now;
                                        actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                        await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                        await unitOfWork.SaveChanageAsync();
                                        Log.Information($"V2:[End Get Single Device] + Name: {device.Name} + DeviceIP: {device.IPAddress} Message: Set DeviceBusy Time 2 Fail");
                                        return false;
                                    }
                                }
                                else
                                {
                                    actionLog.Status = "Error";
                                    actionLog.Message = "Reconnect fail";
                                    actionLog.TimeLogTo = DateTime.Now;
                                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                    await unitOfWork.SaveChanageAsync();
                                    Log.Information($"V2:[End Get Single Device] + Name: {device.Name} + DeviceIP: {device.IPAddress} Message: Reconnect Fail");
                                    return false;
                                }
                            }
                            #endregion


                            #region Get AttRecords
                            extraProperty = boolList;
                            extraData = dtList;

                            resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                            Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 1 Result: " + resultRiss.ToString());
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
                                    //InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : null,
                                    TimeLog = x.Clock,
                                    DateCreate = DateTime.Now
                                }).ToList();
                                Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 1 Success, Count: " + listAttRecord.Count.ToString());
                                MasterResult = true;
                            }
                            else
                            {
                                Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 1 Result Fail ==> retry connect + get");

                                Action_DicconnectV2(device);

                                var resReconnect = await service.Action_ConnectV2(device, unitOfWork, IsDownSchedule, true);
                                Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 2 Result: " + resReconnect.ToString());
                                if (resReconnect)
                                {
                                    extraProperty = boolList;
                                    extraData = dtList;
                                    resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                                    Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 2 Result: " + resultRiss.ToString());
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
                                            //InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : null,
                                            TimeLog = x.Clock,
                                            DateCreate = DateTime.Now
                                        }).ToList();
                                        Log.Information($"V2:[{device.IPAddress}]: " + "Get AttRecords Time 2 Success, Count: " + listAttRecord.Count.ToString());
                                        MasterResult = true;
                                    }
                                }
                                else
                                {
                                    actionLog.Status = "Error";
                                    actionLog.Message = "Reconnect fail";
                                    actionLog.TimeLogTo = DateTime.Now;
                                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                    await unitOfWork.SaveChanageAsync();
                                    Log.Information($"V2:[End Get Single Device] + Name: {device.Name} + DeviceIP: {device.IPAddress} Message: Get AttRecords Time 2 Fail");
                                    return false;
                                }

                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            var rissdevice = (RealandDeviceItem)device;
                            Log.Information($"V2:[{device.IPAddress}]: " + "Have Exception ==> retry");

                            Action_DicconnectV2(device);

                            var resReconnect = await service.Action_ConnectV2(device, unitOfWork, IsDownSchedule, true);
                            Log.Information($"V2:[{device.IPAddress}]: " + "Have Exception --- Get AttRecords Result: " + resReconnect.ToString());
                            if (resReconnect)
                            {
                                extraProperty = boolList;
                                extraData = dtList;
                                var resultRiss = rissdevice.DeviceConnectionIns.GetProperty(DeviceProperty.AttRecords, extraProperty, ref rissdevice.DeviceIns, ref extraData);
                                Log.Information($"V2:[{device.IPAddress}]: " + "Have Exception --- Get AttRecords Result: " + resultRiss.ToString());
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
                                        //InOutModeView = settingenumconvert.TryGetValue(x.Verify, out string cvenum) ? cvenum : null,
                                        TimeLog = x.Clock,
                                        DateCreate = DateTime.Now
                                    }).ToList();
                                    Log.Information($"V2:[{device.IPAddress}]: " + "Have Exception --- Get AttRecords Success, Count: " + listAttRecord.Count.ToString());
                                    MasterResult = true;
                                }
                            }
                            else
                            {
                                actionLog.Status = "Error";
                                actionLog.Message = ex.Message;
                                actionLog.TimeLogTo = DateTime.Now;
                                actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                                await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                                await unitOfWork.SaveChanageAsync();
                                Log.Information($"V2:[End Get Single Device] + Name: {device.Name} + DeviceIP: {device.IPAddress} Message: Have Exception");
                                return false;
                            }
                        }
                        finally
                        {
                            var rissdevice = (RealandDeviceItem)device;
                            extraData = Global.DeviceIdle;
                            var resultRiss = rissdevice.DeviceConnectionIns.SetProperty(DeviceProperty.Enable, extraProperty, rissdevice.DeviceIns, extraData);
                        }
                    }
                    if(MasterResult == true)
                    {
                        if (listAttRecord.Any())
                        {
                            actionLog.TotalRowDownload = listAttRecord.Count; // set to Action log

                            var lstDbTAMScanLog = (await unitOfWork.TAMScanLogRepository.GetManyAsync(x => x.DeviceID == device.Id)).ToList();
                            var lstDbTAMScanLogHistory = (await unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id && x.TimeLog >= datefrom && x.TimeLog <= dateend)).ToList();

                            var lst1 = lstDbTAMScanLogHistory.Select(item => (item.EnrollNumber, item.TimeLog));
                            var lst2 = lstDbTAMScanLog.Select(item => (item.EnrollNumber, item.TimeLog));

                            var hsetDataDbTAMScanLog = new HashSet<(int, DateTime)>(lst1.Concat(lst2));

                            var listInsertscanlog = listAttRecord.Where(item => !hsetDataDbTAMScanLog.Contains((item.EnrollNumber, item.TimeLog))).ToList();

                            Log.Information($"V2:[{device.IPAddress}]:" + "Insert Data, Num row: " + listInsertscanlog.Count.ToString());

                            actionLog.TotalNewlyData = 0;

                            if (listInsertscanlog != null && listInsertscanlog.Any())
                            {
                                actionLog.TotalNewlyData = listInsertscanlog.Count;

                                await unitOfWork.TAMScanLogRepository.AddRangeAsync(listInsertscanlog);
                            }
                            await unitOfWork.SaveChanageAsync();

                            actionLog.Status = "Success";

                            Log.Information($"[{device.IPAddress}]:" + " Start Sync Data");

                            var dbDevice = await unitOfWork.DeviceRepository.GetByIdAsync(device.Id);
                            var ressync = await service.SyncDataByApi(dbDevice, actionLog, datefrom, dateend, EnrollNumber, IsDownSchedule);
                            if (!ressync)
                            {
                                actionLog.Status = "Error";
                                //actionLog.Message = "Sync Data to HRM Error";
                                MasterResult = false;
                            }
                        }
                        else
                        {
                            actionLog.TotalRowDownload = 0;
                            actionLog.TotalNewlyData = 0;
                            actionLog.TotalDataAdded = 0;
                            actionLog.Status = "Success";
                        }
                    }
                    else
                    {
                        actionLog.Status = "Error";
                        actionLog.TotalRowDownload = 0;
                        actionLog.Message = "Data Downloaded is empty or error";
                        MasterResult = false;
                    }
                    actionLog.TimeLogTo = DateTime.Now;
                    actionLog.TimeRun = actionLog.TimeLogTo.Subtract(actionLog.TimeLogFrom);
                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);
                    await unitOfWork.SaveChanageAsync();
                    Log.Information($"[End Get Single Device] + {DateTime.Now} + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                    return MasterResult;
                }
                catch(Exception ex)
                {
                    Log.Error($"[{device.IPAddress}]:" + " Have Exception: " + ex.Message);

                    actionLog.Id = Guid.NewGuid();
                    actionLog.Status = "Error";

                    actionLog.TimeLogTo = DateTime.Now;

                    actionLog.Message = ex.Message;

                    await unitOfWork.ActionLogRepository.AddAsync(actionLog);

                    await unitOfWork.SaveChanageAsync();

                    Log.Information($"V2:[End Get Single Device] + Name: {device.Name} + DeviceIP: {device.IPAddress}");
                    return false;
                }
                
            }

        }

    }
}
