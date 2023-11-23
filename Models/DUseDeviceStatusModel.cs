using System.ComponentModel.DataAnnotations;
using static task_sync_web.Commons.Enums;

namespace task_sync_web.Models
{
    public class DUseDeviceStatusViewModel
    {
        [Display(Name = "利用デバイス状況ID")]
        public int UseDeviceStatusId { get; set; }

        [Display(Name = "利用デバイスID")]
        public string UseDeviceId { get; set; }

        [Display(Name = "デバイス名")]
        public string DeviceName { get; set; }

        [Display(Name = "モデル")]
        public string Model { get; set; }

        [Display(Name = "メーカー")]
        public string Manufacturer { get; set; }

        public DateTime RegistDateTime { get; set; }
        [Display(Name = "登録日時")]
        public string RegistDateTimeString { get; set; }

        public DateTime UseDeviceEnableDate { get; set; }
        [Display(Name = "利用開始日")]
        public string UseDeviceEnableDateString { get; set; }

        [Display(Name = "利用状況")]
        public UseDeviceStatus UseDeviceStatus { get; set; }

        //[Display(Name = "解除日時")]
        //public DateTime CancelDateTime { get; set; }

        public DUseDeviceStatusViewModel()
        {

        }
    }
}
