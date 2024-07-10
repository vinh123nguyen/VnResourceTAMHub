using zkemkeeper;

namespace TAMHubApp.Services.TAMServices
{
    public class ZKTecoDeviceItem : BaseTAMService
    {
        public CZKEM DeviceIns { get; set; }

        public bool ZKTeco_IsCanGetLogByTime { get; set; }
        public bool ZKTeco_IsNewFirmwareMachine { get; set; }
        public bool ZKTeco_IsTFTMachine { get; set; }
        


        public ZKTecoDeviceItem()
        {
            IsConnected = false;
            DeviceIns = new CZKEM();
            ZKTeco_IsNewFirmwareMachine = ZKTeco_IsTFTMachine = false;
        }
    }
}
