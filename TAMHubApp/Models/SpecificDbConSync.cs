namespace TAMHubApp.Models
{
    public class CHECKINOUTModel
    {
        public string USERID { set; get; }
        public DateTime CHECKTIME { set; get; }
        public string CHECKTYPE { set; get; }
        public int VERIFYCODE { set; get; }
        public string SENSORID { set; get; }
        public string Memoinfo { set; get; }
        public int WorkCode { set; get; }
        public string sn { set; get; }
        public int UserExtFmt { set; get; }
        public string BADGENUMBER { set; get; }
    }
    public class ZkUserInfo
    {
        public string USERID { set; get; }
        public string BADGENUMBER { set; get; }
    }
}
