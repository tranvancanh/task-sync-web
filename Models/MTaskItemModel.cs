using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using task_sync_web.Commons;
using Newtonsoft.Json;

namespace task_sync_web.Models
{
    public class MTaskItemModel
    {
        [Display(Name = "登録修正フラグ")]
        [JsonConverter(typeof(DefalultFlagConverter))]
        [DefaultValue("")]
        public string ModifiedFlag { get; set; }

        [Display(Name = "作業項目コード")]
        public int TaskItemCode { get; set; }

        [Display(Name = "作業大項目")]
        [DefaultValue("")]
        public string TaskPrimaryItem { get; set; }

        [Display(Name = "作業中項目")]
        [DefaultValue("")]
        public string TaskSecondaryItem { get; set; }

        [Display(Name = "作業小項目")]
        [DefaultValue("")]
        public string TaskTertiaryItem { get; set; }

        [Display(Name = "作業項目分類")]
        [DefaultValue("")]
        public string TaskItemCategory { get; set; }

        [Display(Name = "備考")]
        [DefaultValue("")]
        public string Remark { get; set; }

        [Display(Name = "利用停止フラグ")]
        [JsonConverter(typeof(BoolFormatConverter))]
        public bool IsNotUse { get; set; }

        [Display(Name = "作業項目ID(自動連番)")]
        public int? TaskItemId { get; set; }

        [Display(Name = "登録日時")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime CreateDateTime { get; set; }

        [JsonIgnore]
        public string CreateAdministratorLoginId { get; set; }

        [JsonIgnore]
        public string CreateAdministratorName { get; set; }

        [Display(Name = "登録者")]
        public string CreateFor
        {
            get
            {
                return $"{CreateAdministratorLoginId} {CreateAdministratorName}";
            }
            set {; }
        }

        [Display(Name = "更新日時")]
        [JsonConverter(typeof(DateFormatConverter), "yyyy/MM/dd HH:mm")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime UpdateDateTime { get; set; }

        [JsonIgnore]
        public string UpdateAdministratorLoginId { get; set; }

        [JsonIgnore]
        public string UpdateAdministratorName { get; set; }

        [Display(Name = "更新者")]
        public string UpdateFor
        {
            get
            {
                return $"{UpdateAdministratorLoginId} {UpdateAdministratorName}";
            }
            set {; }
        }
    }
}
