namespace TAMHubApp.Data.Entities
{
    public class ActionLog
    {
        public Guid Id { get; set; }
        public string ActionName { get; set; }
        public DateTime ActionDate { get; set; }
        public Guid? DeviceID { get; set; }
        public string ResultStatus { get; set; }
        public string Data { get; set; }
        public string ActionType { get; set; }

        public Device Device { get; set; }
    }
}
