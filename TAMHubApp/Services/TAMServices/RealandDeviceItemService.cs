using Riss.Devices;
using TAMHubApp.Infrastructures.TAMUtilities;

namespace TAMHubApp.Services.TAMServices
{
    public class RealandDeviceItemService : BaseTAMService
    {
        //public bool IsConnected { get; set; }
        private Device _device;
        private DeviceConnection _deviceConnection;

        public RealandDeviceItemService()
        {
            //IsConnected = false;
            //_device = new Device();
            //_device.DN = deviceInfo.MachineNo;

            //if (deviceInfo.CommPassword != null)
            //    _device.Password = deviceInfo.CommPassword.ToString();
            //_device.Model = "ZDC2911";
            //_device.ConnectionModel = 5;
            //_device.IpAddress = deviceInfo.IPAddress;
            //_device.IpPort = deviceInfo.MachineNo;
            //_device.CommunicationType = CommunicationType.Tcp;
        }

        public bool Action_Connect(ref string message)
        {
            message = "Success";
            if (IsConnected)
                return IsConnected;
            _deviceConnection = DeviceConnection.CreateConnection(ref _device);
            if(_deviceConnection.Open() > 0)
            {
                IsConnected = true;
            }
            else
            {
                IsConnected = false;
            }
            return IsConnected;
        }

        //public void Action_Disconnect()
        //{
        //    if(_deviceConnection != null)
        //    {
        //        _deviceConnection.Close();
        //        _deviceConnection = null;
        //        IsConnected = false;
        //    }
        //}

        private bool Action_SetDeviceStatus(object status)
        {
            if (!IsConnected)
                return false;
            try
            {
                object extraData = status;
                object extraProperty = new object();
                var res = _deviceConnection.SetProperty(DeviceProperty.Enable, extraProperty, _device, extraData);
                return res;
            }catch(Exception ex)
            {
                return false;
            }
        }

        public List<Record> Action_GetNewlyGlog(ref string message)
        {
            message = "Success";
            var result = true;
            var lstResult = new List<Record>();
            try
            {
                object extraProperty = new object();
                object extraData = new object();

                result = Action_SetDeviceStatus(RealandStatusGlobal.DeviceBusy);
                if(!result)
                {
                    message = "Set DeviceBusy Fail";
                    return lstResult;
                }
                result = _deviceConnection.GetProperty(DeviceProperty.AttRecordsCount, extraProperty, ref _device,
                    ref extraData);
                if (!result)
                {
                    message = "Get AttRecordsCount Fail";
                    return lstResult;
                }
                int recordCount = (int)extraData;
                if (0 == recordCount)
                    return lstResult;

                List<bool> boolList = new List<bool>();
                boolList.Add(true); /// true => get data new
                boolList.Add(true);   /// remove newly log
                extraProperty = boolList;
                extraData = new List<DateTime> (){ DateTime.Now, DateTime.Now};

                result = _deviceConnection.GetProperty(DeviceProperty.AttRecords, extraProperty, ref _device, ref extraData);

                if(result)
                {
                    lstResult = (List<Record>)extraData;
                }
                else
                {
                    message = "Get AttRecords Fail";
                }
                return lstResult;
            }
            catch(Exception ex)
            {
                message = "Get AttRecords Error:" + ex.Message;
                return lstResult;
            }
            finally
            {
                Action_SetDeviceStatus(RealandStatusGlobal.DeviceIdle);
            }
        }


        public List<Record> Action_GetAllGlog(List<DateTime> dtList, ref string message)
        {
            message = "Success";
            var result = true;
            var lstResult = new List<Record>();
            try
            {
                object extraProperty = new object();
                object extraData = new object();

                result = Action_SetDeviceStatus(RealandStatusGlobal.DeviceBusy);
                if (!result)
                {
                    message = "Set DeviceBusy Fail";
                    return lstResult;
                }
                result = _deviceConnection.GetProperty(DeviceProperty.AttRecordsCount, extraProperty, ref _device,
                    ref extraData);
                if (!result)
                {
                    message = "Get AttRecordsCount Fail";
                    return lstResult;
                }
                int recordCount = (int)extraData;
                if (0 == recordCount)
                    return lstResult;

                List<bool> boolList = new List<bool>();
                boolList.Add(false); /// true => get data new
                boolList.Add(true);   /// remove newly log
                extraProperty = boolList;
                extraData = dtList;

                result = _deviceConnection.GetProperty(DeviceProperty.AttRecords, extraProperty, ref _device, ref extraData);

                if (result)
                {
                    lstResult = (List<Record>)extraData;
                }
                else
                {
                    message = "Get AttRecords Fail";
                }
                return lstResult;
            }
            catch (Exception ex)
            {
                message = "Get AttRecords Error:" + ex.Message;
                return lstResult;
            }
            finally
            {
                Action_SetDeviceStatus(RealandStatusGlobal.DeviceIdle);
            }
        }

        public override bool Action_Connect()
        {
            throw new NotImplementedException();
        }

        public override bool Action_Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void Action_GetAllGlog()
        {
            throw new NotImplementedException();
        }
    }
}
