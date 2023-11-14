using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using task_sync_web.Commons;

namespace task_sync_web.Models
{
    public class DTaskRecordModel
    {
        [JsonIgnore]
        public int TaskRecordId { get; set; }

        [Display(Name = "ログイン日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm:ss}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm:ss")]
        public DateTime LoginDateTime { get; set; } // DLoginTaskUserRecordを参照

        [Display(Name = "作業日付")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd")]
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
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm:ss}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm:ss")]
        public DateTime TaskStartDateTrackTime { get; set; }

        [Display(Name = "作業終了時刻(記録)")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm:ss}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm:ss")]
        public DateTime TaskEndDateTrackTime { get; set; }

        [Display(Name = "中断時間(記録)")]
        public int TaskInterruptTrackTotalTime { get; set; }

        [JsonIgnore]
        public bool IsDisplayTaskInterruptTrack { get; set; }

        [Display(Name = "作業開始時刻(実績)")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm:ss}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm:ss")]
        public DateTime TaskStartDateTime { get; set; }

        [Display(Name = "作業終了時刻(実績)")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm:ss}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm:ss")]
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


        [Display(Name = "備考")]
        public string Remark { get; set; }

        [Display(Name = "削除フラグ")]
        [JsonConverter(typeof(BoolFormatConverter))]
        public bool IsDelete {  get; set; }


        [Display(Name = "作成日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm")]
        public DateTime CreateDateTime { get; set; }

        [Display(Name = "作成管理者")]
        public string CreateAdministratorId { get; set; }

        [JsonIgnore]
        public string CreateAdministratorName {  get; set; }

        [Display(Name = "作成作業者")]
        public int CreateTaskUserId { get; set; }

        [JsonIgnore]
        public string CreateTaskUserName { get; set; }

        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm")]
        public DateTime UpdateDateTime { get; set; }

        [Display(Name = "更新管理者")]
        public string UpdateAdministratorId { get; set; }

        [JsonIgnore]
        public string UpdateAdministratorName { get; set; }


        [Display(Name = "更新作業者")]
        public int UpdateTaskUserId { get; set; }

        [JsonIgnore]
        public string UpdateTaskUserName { get; set; }
    }
}
