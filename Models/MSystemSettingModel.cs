using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MSystemSettingModel
    {
        [Display(Name = "システム設定ID")]
        public int SystemSettingId { get; set; }

        [Display(Name = "システム設定概要")]
        public string SystemSettingOutline { get; set; }

        [Display(Name = "システム設定詳細")]
        public string SystemSettingDetail { get; set; }

        [Display(Name = "システム設定値")]
        public int SystemSettingValue { get; set; }

        [Display(Name = "システム設定値(文字列)")]
        public string SystemSettingStringValue { get; set; }

        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        [Display(Name = "更新日時")]
        public DateTime UpdateDateTime { get; set; }

        public int UpdateLoginId { get; set; }

        public string UpdateLoginName { get; set; }

        [Display(Name = "更新ログインID")]
        public string UpdateLoginInfor => $"{UpdateLoginId} {UpdateLoginName}";

    }
}
