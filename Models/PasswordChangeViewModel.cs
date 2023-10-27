using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class PasswordChangeViewModel :BaseViewModel
    {
        [Key]
        public int AddministratorId { get; set; }

        [Display(Name = "現在のパスワード")]
        [Required(ErrorMessageResourceName = "EW001", ErrorMessageResourceType = typeof(ErrorMessages))]
        //[StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string CurrentPassword { get; set; }

        [Display(Name = "新しいパスワード")]
        [Required(ErrorMessageResourceName = "EW001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string NewPassword { get; set; }

        [Display(Name = "新しいパスワード(確認用)")]
        [Required(ErrorMessageResourceName = "EW001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW003", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string ConfirmNewPassword { get; set; }

        public PasswordChangeViewModel()
        {
            DisplayName = "パスワード変更";
        }
    }
}
