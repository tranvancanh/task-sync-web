using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class DTaskInterruptRecordViewModel
    {
        public int TaskRecordId { get; set; }
        public int TaskUserId { get; set; }
        public string TaskUserLoginId { get; set; }
        public string TaskUserName { get; set; }

        public DateTime TaskStartDateTrackTime { get; set; }
        public DateTime TaskEndDateTrackTime { get; set; }
        public int TaskTrackTotalTime { get; set; }
        public int TaskInterruptTrackTotalTime { get; set; } // 単位：分

        public List<DTaskInterruptRecordModel> InterruptRecords { get; set; } = new List<DTaskInterruptRecordModel>();
    }

    public class DTaskInterruptRecordModel
    {
        public int TaskInterruptRecordId { get; set; }
        public int TaskRecordId { get; set; }

        [Display(Name = "中断開始")]
        public DateTime TaskInterruptStartDateTrackTime { get; set; }

        [Display(Name = "作業再開")]
        public DateTime TaskInterruptEndDateTrackTime { get; set; }

        [Display(Name = "作業時間(分)")]
        public int TaskInterruptTime { get; set; } // 単位：分

        [Display(Name = "中断理由")]
        public int TaskInterruptReasonCode { get; set; }

        public string TaskInterruptReasonName { get; set; }

        public DateTime CreateDateTime { get; set; }
        public int CreateTaskUserId { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public int UpdateTaskUserId { get; set; }
    }
}
