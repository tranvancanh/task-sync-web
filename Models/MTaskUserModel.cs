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

        [Display(Name = "利用停止フラグ")]
        public bool IsNotUse { get; set; }

        [Display(Name = "作成日")]
        public DateTime CreateDateTime { get; set; }

        [Display(Name = "作成者ID")]
        public int CreateAdministratorId { get; set; }

        [Display(Name = "更新日")]
        public DateTime UpdateDateTime { get; set; }

        [Display(Name = "更新者ID")]
        public int UpdateAdministratorId { get; set; }
    }
}
