using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using task_sync_web.Commons;

namespace task_sync_web.Models
{
    public class MTaskUserModel
    {
        [Display(Name = "登録修正フラグ")]
        [JsonConverter(typeof(DefalultFlagConverter))]
        public string ModifiedFlag { get; set; }

        [Display(Name = "作業者ログインID")]
        public string TaskUserLoginId { get; set; }

        [Display(Name = "作業者名")]
        public string TaskUserName { get; set; }

        [Display(Name = "作業者名かな")]
        public string TaskUserNameKana { get; set; }

        [Display(Name = "所属名")]
        public string TaskUserDepartmentName { get; set; }

        [Display(Name = "グループ名")]
        public string TaskUserGroupName { get; set; }

        [Display(Name = "備考")]
        public string Remark { get; set; }

        [Display(Name = "利用停止フラグ")]
        [JsonConverter(typeof(BoolFormatConverter))]
        public bool IsNotUse { get; set; }

        [Display(Name = "登録日時")]
        [JsonConverter(typeof(DateFormatConverter), "yy/MM/dd")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime CreateDateTime { get; set; }

        [JsonIgnore]
        public string CreateAdministratorLoginId { get; set; }

        [JsonIgnore]
        public string CreateAdministratorName { get; set; }

        [Display(Name = "登録者")]
        public string CreateFor => $"{CreateAdministratorLoginId} {CreateAdministratorName}";

        [Display(Name = "更新日時")]
        [JsonConverter(typeof(DateFormatConverter), "yy/MM/dd")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime UpdateDateTime { get; set; }

        [JsonIgnore]
        public string UpdateAdministratorLoginId { get; set; }

        [JsonIgnore]
        public string UpdateAdministratorName { get; set; }

        [Display(Name = "更新者")]
        public string UpdateFor => $"{UpdateAdministratorLoginId} {UpdateAdministratorName}";
    }

}
