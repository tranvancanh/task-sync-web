using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class DTaskInterruptModalEditViewModel
    {
        public int TaskRecordId { get; set; }
        public int TaskUserId { get; set; }
        public string TaskUserLoginId { get; set; }
        public string TaskUserName { get; set; }
        public bool IsAdmin { get; set; } = false;

        public DateTime TaskStartDateTrackTime { get; set; }
        public DateTime TaskEndDateTrackTime { get; set; }
        public int TaskTrackTotalTime { get; set; }
        public int TaskInterruptTrackTotalTime { get; set; } // 単位：分

        [Display(Name = "作業開始時刻")]
        public string TaskStartDate { get; set; }
        public string TaskStartTime { get; set; }

        [Display(Name = "作業終了時刻")]
        public string TaskEndDate { get; set; }
        public string TaskEndTime { get; set; }

        [Display(Name = "中断時間(分)")]
        public int TaskInterruptTotalTime { get; set; } // 単位：分

        [Display(Name = "作業選択項目")]
        public string TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem {  get; set; }

        [Display(Name = "備考")]
        [StringLength(200, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string Remark {  get; set; }

        [Display(Name = "削除")]
        public bool IsDelete { get; set; }
    }
}
