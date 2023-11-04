using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class PasswordChangeViewModel :BaseViewModel
    {

        [Display(Name = "現在のパスワード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string CurrentPassword { get; set; }

        [Display(Name = "新しいパスワード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "EW0010", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string NewPassword { get; set; }

        [Display(Name = "新しいパスワード(確認用)")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "EW0010", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string ConfirmNewPassword { get; set; }

        public PasswordChangeViewModel()
        {
            DisplayName = "管理者パスワード変更";
        }
    }
}
