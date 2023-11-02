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
        [RegularExpression(@"[0-9]+", ErrorMessageResourceName = "EW0009", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Range(1, 999, ErrorMessageResourceName = "EW0004", ErrorMessageResourceType = typeof(ErrorMessages))]
        public int? InterruptReasonCode { get; set; }

        [Display(Name = "中断理由名")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [MaxLength(10, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string InterruptReasonName { get; set; }

        [Display(Name = "備考")]
        [StringLength(200, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
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
