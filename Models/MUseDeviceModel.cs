using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class MUseDeviceModel
    {
        [Display(Name = "利用デバイスID")]
        public string UseDeviceId { get; set; }

        [Display(Name = "利用デバイス有効日付")]
        public DateTime UseDeviceEnableDate { get; set; }

        [Display(Name = "利用デバイス無効日付")]
        public DateTime UseDeviceDisableDate { get; set; }

        public MUseDeviceModel()
        {

        }
    }
}
