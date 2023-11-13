using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class DTaskRecordModel
    {
        public int TaskRecordId { get; set; }

        [Display(Name = "ログイン日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime LoginDateTime { get; set; } // DLoginTaskUserRecordを参照

        [Display(Name = "作業日付")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd}")]
        public DateTime TaskDate {  get; set; }

        [Display(Name = "所得名")]
        public string TaskUserDepartmentName { get; set; } // MTaskUserを参照

        [Display(Name = "グループ名")]
        public string TaskUserGroupName { get; set; } // MTaskUserを参照

        [Display(Name = "作業者ログインID")]
        public int TaskUserLoginId { get; set; } // MTaskUserを参照

        [Display(Name = "作業者名")]
        public string TaskUserName { get; set; } // MTaskUserを参照

        [Display(Name = "作業項目ID")]
        public int TaskItemId { get; set; }

        [Display(Name = "作業項目コード")]
        public int TaskItemCode { get; set; }

        [Display(Name = "作業項目大項目")]
        public string TaskPrimaryItem { get; set; }

        [Display(Name = "作業項目中項目")]
        public string TaskSecondaryItem { get; set; }

        [Display(Name = "作業項目小項目")]
        public string TaskTertiaryItem { get; set; }

        [Display(Name = "作業項目分類")]
        public string TaskItemCategory { get; set; } // MTaskItemを参照

        [Display(Name = "作業開始時刻(記録)")]
        public DateTime TaskStartDateTrackTime { get; set; }

        [Display(Name = "作業終了時刻(記録)")]
        public DateTime TaskEndDateTrackTime { get; set; }

        [Display(Name = "中断時間(記録)")]
        public int TaskInterruptTrackTotalTime { get; set; }


        [Display(Name = "作業開始時刻(実績)")]
        public DateTime TaskStartDateTime { get; set; }

        [Display(Name = "作業終了時刻(実績)")]
        public DateTime TaskEndDateTime { get; set; }

        [Display(Name = "中断時間(実績)")]
        public int TaskInterruptTotalTime { get; set; }


        [Display(Name = "作業時間")]
        public TimeSpan TaskTime {  get; set; } //

        [Display(Name = "純作業時間")]
        public TimeSpan PureTaskTime {  get; set; }


        [Display(Name = "作業時間(分)")]
        public int TaskTimeMinute { get; set; }

        [Display(Name = "純作業時間(分)")]
        public int PureTaskTimeMinute { get; set; }


        [Display(Name = "メモ")]
        public string Remark { get; set; }

        [Display(Name = "修正フラグ")]
        public bool IsDelete {  get; set; }



        [Display(Name = "作成日時")]
        public DateTime CreateDateTime { get; set; }

        [Display(Name = "作成管理者")]
        public int CreateAdministratorId { get; set; }

        [Display(Name = "作成作業者")]
        public int CreateTaskUserId { get; set; }

        [Display(Name = "更新日時")]
        public DateTime UpdateDateTime { get; set; }

        [Display(Name = "更新管理者")]
        public int UpdateAdministratorId { get; set; }

        [Display(Name = "更新作業者")]
        public int UpdateTaskUserId { get; set; }
    }
}
