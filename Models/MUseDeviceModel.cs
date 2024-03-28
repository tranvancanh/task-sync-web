using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MUseDeviceModel
    {
        [Display(Name = "利用デバイスID")]
        public string UseDeviceId { get; set; }

        [Display(Name = "利用デバイス有効開始日付")]
        public DateTime UseDeviceEnableStartDate { get; set; }

        [Display(Name = "利用デバイス有効終了日付")]
        public DateTime UseDeviceEnableEndDate { get; set; }

        public MUseDeviceModel()
        {

        }
    }
}
