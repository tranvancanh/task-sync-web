using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MInterruptReasonModel
    {

        public int InterruptReasonId { get; set; }

        [Display(Name = "中断理由コード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string InterruptReasonCode { get; set; }

        [Display(Name = "中断理由名")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string InterruptReasonName { get; set; }

        [Display(Name = "備考")]
        [StringLength(200, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string Remark { get; set; }

        [Display(Name = "利用停止フラグ")]
        public bool IsNotUse { get; set; }

        [Display(Name = "登録日")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime CreateDateTime { get; set; }

        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0: yyyy/MM/dd HH:mm}")]
        public DateTime UpdateDateTime { get; set; }

        public string AdministratorIdCreate { get; set; }

        public string AdministratorNameCreate { get; set; }

        public string AdministratorIdUpdate { get; set; }

        public string AdministratorNameUpdate { get; set; }

        [Display(Name = "登録者")]
        public string CreateAdministratorId => $"{AdministratorIdCreate} {AdministratorNameCreate}";

        [Display(Name = "更新者")]
        public string UpdateAdministratorId => $"{AdministratorIdUpdate} {AdministratorNameUpdate}";

    }
}
