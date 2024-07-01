using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Models;
using TAMHubApp.Models.TAMDataModel;
using zkemkeeper;

namespace TAMHubApp.Services.TAMServices
{
    public class ZKTecoDeviceItemService
    {
        public bool IsConnected { get; set; }
        private CZKEM _zkemClient { get; set; }
        private TAMConnectInfoModel _deviceInfo { get; set; }
        private bool _isNewFirmwareMachine { get; set; }
        private bool _isTFTMachine { get; set; }

        

        public ZKTecoDeviceItemService(TAMConnectInfoModel deviceInfo)
        {
            IsConnected = false;
            _deviceInfo = deviceInfo;
            _zkemClient = new CZKEM();
            _isNewFirmwareMachine = _isTFTMachine = false;
        }

        public bool Action_Connect(ref string message)
        {
            message = "Success";
            try
            {
                if (IsConnected)
                    return IsConnected;

                if (_deviceInfo.CommPassword != null)
                    IsConnected = _zkemClient.SetCommPassword(_deviceInfo.CommPassword.Value);

                IsConnected = _zkemClient.Connect_Net(_deviceInfo.IPAddress, _deviceInfo.Port);

                if (IsConnected)
                {
                    _isNewFirmwareMachine = _zkemClient.IsNewFirmwareMachine(_deviceInfo.DeviceID);
                    _isTFTMachine = _zkemClient.IsTFTMachine(_deviceInfo.DeviceID);
                    if(_zkemClient.RegEvent(1, 32767))
                    {
                        _zkemClient.OnConnected += Event_OnConnected;
                        _zkemClient.OnDisConnected += Event_OnDisconnected;
                    }
                }
                else
                {
                    var errcode = 0;
                    _zkemClient.GetLastError(ref errcode);
                }
                return IsConnected;
            }
            catch(Exception ex)
            {
                message = "Error|" + ex.Message;
                return false;
            }
        }

        private void Event_OnDisconnected()
        {
            IsConnected = false;
        }

        private void Event_OnConnected()
        {
            IsConnected = true;
        }

        public void Action_Disconnect()
        {
            _zkemClient.OnConnected -= Event_OnConnected;
            _zkemClient.OnDisConnected -= Event_OnDisconnected;
            IsConnected = false;
            _zkemClient.Disconnect();
        }


        public void Action_GetAllGlog()
        {
            var listAttRecord = new List<ZKTecoGLogModel>();
            if (_zkemClient.ReadAllGLogData(_deviceInfo.DeviceID))
            {
                if (_isTFTMachine)
                {
                    int tVerifyMode, tInOutMode, year, month, day, hour, minute, second, workCode = 0;
                    string enrollNumber;
                    while (_zkemClient.SSR_GetGeneralLogData(_deviceInfo.DeviceID,
                        out enrollNumber, out tVerifyMode,
                        out tInOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
                    {
                        listAttRecord.Add(new ZKTecoGLogModel()
                        {
                            ID = Guid.NewGuid(),
                            DeviceID = _deviceInfo.DeviceID,
                            EnrollNumber = enrollNumber,
                            VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                            InOutMode = ((ZKTecoINOUTMode)tInOutMode).ToString(),
                            TimeLog = new DateTime(year, month, day, hour, minute, second),
                            WorkCode = workCode
                        });

                    }
                }
                else
                {
                    int tVerifyMode = 0, tInOutMode = 0, year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;
                    int enrollNumber = 0;
                    while (_zkemClient.GetAllGLogData(_deviceInfo.DeviceID,
                        ref enrollNumber, ref tVerifyMode,
                        ref tInOutMode, ref year, ref month, ref day, ref hour, ref minute, ref second, ref workCode))
                    {
                        listAttRecord.Add(new ZKTecoGLogModel()
                        {
                            ID = Guid.NewGuid(),
                            DeviceID = _deviceInfo.DeviceID,
                            EnrollNumber = enrollNumber.ToString(),
                            VerifyMode = ((ZKTecoVerifyMode)tVerifyMode).ToString(),
                            InOutMode = ((ZKTecoINOUTMode)tInOutMode).ToString(),
                            TimeLog = new DateTime(year, month, day, hour, minute, second),
                            WorkCode = workCode
                        });

                    }
                }
            }
        }
    }
}
