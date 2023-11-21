using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class HomeViewModel :BaseViewModel
    {
        [Display(Name = "システム管理者からのお知らせ")]
        public string CompanyMessageBySystem { get; set; }

        [Display(Name = "スマホアプリのダウンロードコード")]
        public string SmartphoneAppDownloadCode { get; set; }

        [Display(Name = "現在の最新Ver.")]
        public string SmartphoneAppMinVersion { get; set; }

        // 利用デバイス状況一覧
        [Display(Name = "現在有効な利用デバイスID")]
        public int UseDeviceEnableCount { get; set; }
        [Display(Name = "利用中")]
        public int UseDeviceCount { get; set; }
        [Display(Name = "待機中")]
        public int NotUseDeviceCount { get; set; }

        public List<DUseDeviceStatusViewModel> UseDeviceStatusModels { get; set; }

        public HomeViewModel()
        {
            DisplayName = "ホーム";
        }
    }
}
