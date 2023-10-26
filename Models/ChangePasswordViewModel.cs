using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "現在のユーザー")]
        public string CurrentUserCode { get; set; }

        [Display(Name = "現在のパスワード入力")]
        [MaxLength(10, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [MinLength(4, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "W3_1_2_1002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string CurrentPass { get; set; }

        [Display(Name = "新しいパスワード入力")]
        [MaxLength(10, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [MinLength(4, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "W3_1_2_1002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string NewPass { get; set; }

        [Display(Name = "新しいパスワード入力(確認用)")]
        [MaxLength(10, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [MinLength(4, ErrorMessageResourceName = "W3_1_2_1001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [Required(ErrorMessageResourceName = "W3_1_2_1002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string ConfirmPass { get; set; }
    }
}
