using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MInterruptReasonModel
    {
        [HiddenInput]
        public int InterruptReasonId { get; set; }

        [Display(Name = "中断理由コード")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [FromForm(Name = "InterruptReasonModels[0].InterruptReasonCode")]
        public string InterruptReasonCode { get; set; }

        [Display(Name = "中断理由名")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [FromForm(Name = "InterruptReasonModels[0].InterruptReasonName")]
        public string InterruptReasonName { get; set; }

        [Display(Name = "備考")]
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
