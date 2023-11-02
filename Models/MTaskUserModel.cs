using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MTaskUserModel
    {
        [Key]
        public int TaskUserId {  get; set; }

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
        public bool IsNotUse { get; set; }

        [Display(Name = "登録日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime CreateDateTime { get; set; }

        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime UpdateDateTime { get; set; }

        public string CreateAdministratorLoginId { get; set; }

        public string CreateAdministratorName { get; set; }

        public string UpdateAdministratorLoginId { get; set; }

        public string UpdateAdministratorName { get; set; }

        [Display(Name = "登録者")]
        public string CreateFor => $"{CreateAdministratorLoginId} {CreateAdministratorName}";

        [Display(Name = "更新者")]
        public string UpdateFor => $"{UpdateAdministratorLoginId} {UpdateAdministratorName}";
    }
}
