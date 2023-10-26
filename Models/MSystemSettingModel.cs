namespace task_sync_web.Models
{
    public class MSystemSettingModel
    {
        public int SystemSettingId { get; set; }
        public string SystemSettingTitle { get; set; }
        public string SystemSettingDetail { get; set; }
        public int SystemSettingValue { get; set; }
        public string SystemSettingStringValue { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public int UpdateLoginId { get; set; }
    }
}
