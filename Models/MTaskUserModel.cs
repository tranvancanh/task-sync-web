using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using task_sync_web.Commons;

namespace task_sync_web.Models
{
    public class MTaskUserModel
    {
        [Display(Name = "登録修正フラグ")]
        [JsonConverter(typeof(DefalultFlagConverter))]
        [DefaultValue("")]
        public string ModifiedFlag { get; set; }

        [Display(Name = "作業者ログインID")]
        [DefaultValue("")]
        public string TaskUserLoginId { get; set; }

        [Display(Name = "作業者名")]
        [DefaultValue("")]
        public string TaskUserName { get; set; }

        [Display(Name = "作業者名かな")]
        [DefaultValue("")]
        public string TaskUserNameKana { get; set; }

        [Display(Name = "所属名")]
        [DefaultValue("")]
        public string TaskUserDepartmentName { get; set; }

        [Display(Name = "グループ名")]
        [DefaultValue("")]
        public string TaskUserGroupName { get; set; }

        [Display(Name = "備考")]
        [DefaultValue("")]
        public string Remark { get; set; }

        [Display(Name = "利用停止フラグ")]
        [JsonConverter(typeof(BoolFormatConverter))]
        public bool IsNotUse { get; set; }

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
            set { ; }
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
            set { ; }
        }
    }

}
