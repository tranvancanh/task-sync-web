using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class AppDownloadViewModel
    {
        public string CompanyId { get; set; }

        [Display(Name = "ダウンロードコード")]
        [Required(ErrorMessage = "ダウンロードコードは必須入力です。")]
        public string DownloadCord { get; set; }

        public AppDownloadViewModel()
        {
            DownloadCord = "";
        }
    }
}