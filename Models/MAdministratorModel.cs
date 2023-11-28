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

        /// <summary>
        /// 利用開始日
        /// </summary>
        public DateTime LoginAdministratorEnableStartDate { get; set; }

        /// <summary>
        /// 利用終了日
        /// </summary>
        public DateTime LoginAdministratorEnableEndDate { get; set; }

        //[Display(Name = "利用停止フラグ")]
        //public bool IsNotUse { get; set; }

        [Key]
        public int AdministratorId { get; set; }

        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsLogin { get; set; }

    }
}