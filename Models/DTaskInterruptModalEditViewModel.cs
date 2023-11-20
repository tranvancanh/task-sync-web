using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class DTaskInterruptModalEditViewModel
    {
        public int TaskRecordId { get; set; }
        public int TaskUserId { get; set; }
        public string TaskUserLoginId { get; set; }
        public string TaskUserName { get; set; }

        public DateTime TaskStartDateTrackTime { get; set; }
        public DateTime TaskEndDateTrackTime { get; set; }
        public int TaskTrackTotalTime { get; set; }
        public int TaskInterruptTrackTotalTime { get; set; } // 単位：分

        [Display(Name = "作業開始時刻(実績)")]
        [DefaultValue("")]
        public string TaskStartDate { get; set; }
        [DefaultValue("")]
        public string TaskStartTime { get; set; }

        [Display(Name = "作業終了時刻(実績)")]
        [DefaultValue("")]
        public string TaskEndDate { get; set; }
        [DefaultValue("")]
        public string TaskEndTime { get; set; }

        [Display(Name = "中断時間(分)(実績)")]
        [DefaultValue(0)]
        public int TaskInterruptTotalTime { get; set; } // 単位：分

        public int TaskItemCode { get; set; }
        public string TaskPrimaryItem { get; set; }
        public string TaskSecondaryItem { get; set; }
        public string TaskTertiaryItem { get; set; }

        [Display(Name = "作業項目")]
        [DefaultValue("")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem {  get; set; }

        [Display(Name = "備考")]
        [StringLength(200, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        [DefaultValue("")]
        public string Remark {  get; set; }

        [Display(Name = "削除")]
        public bool IsDelete { get; set; }
    }
}
