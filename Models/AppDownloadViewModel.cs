using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class AppDownloadViewModel
    {
        public string CompanyId { get; set; }

        [Display(Name = "�_�E�����[�h�R�[�h")]
        [Required(ErrorMessage = "�_�E�����[�h�R�[�h�͕K�{���͂ł��B")]
        public string DownloadCord { get; set; }

        public AppDownloadViewModel()
        {
            DownloadCord = "";
        }
    }
}