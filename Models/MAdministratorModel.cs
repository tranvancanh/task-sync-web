using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorModel
    {
        [Display(Name = "管理者ログインID")]
        public string AdministratorLoginId { get; set; }

        [Display(Name = "管理者名")]
        public string AdministratorName { get; set; }

        [Display(Name = "管理者名かな")]
        public string AdministratorNameKana { get; set; }

        [Display(Name = "利用停止フラグ")]
        public bool IsNotUse { get; set; }

    }
}