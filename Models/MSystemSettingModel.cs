using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MSystemSettingModel
    {
        [Display(Name = "システム設定ID")]
        public int SystemSettingId {  get; set; }

        [Display(Name = "システム設定タイトル")]
        public string SystemSettingTitle { get; set; }

        [Display(Name = "システム設定詳細")]
        public string SystemSettingDetail { get; set; }

        [Display(Name = "システム設定値")]
        public int SystemSettingValue { get; set; }

        [Display(Name = "システム設定値(文字列)")]
        public string SystemSettingStringValue { get; set; }

        [Display(Name = "更新日時")]
        public DateTime UpdateDateTime { get; set; }

        [Display(Name = "更新ログインID")]
        public int UpdateLoginId { get; set; }
    }
}
