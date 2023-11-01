using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MSystemSettingModel
    {
        [Key]
        [Display(Name = "システム設定ID")]
        public int SystemSettingId { get; set; }

        [Display(Name = "システム設定概要")]
        public string SystemSettingOutline { get; set; }

        [Display(Name = "システム設定詳細")]
        public string SystemSettingDetail { get; set; }

        [Display(Name = "システム設定値")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [RegularExpression(@"[0-9]+", ErrorMessageResourceName = "EW0009", ErrorMessageResourceType = typeof(ErrorMessages))]
        public int SystemSettingValue { get; set; }

        [Display(Name = "システム設定値(文字列)")]
        [StringLength(100, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SystemSettingStringValue { get; set; }

        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        [Display(Name = "更新日時")]
        public DateTime UpdateDateTime { get; set; }

        public string UpdateAdministratorId { get; set; }

        public string UpdateAdministratorName { get; set; }

        [Display(Name = "更新者")]
        public string UpdateLoginInfor => $"{UpdateAdministratorId} {UpdateAdministratorName}";

    }
}
