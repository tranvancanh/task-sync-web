using System.ComponentModel.DataAnnotations;

namespace task_sync_web.Models
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "現在のユーザー")]
        public string CurrentUserCode { get; set; }

        [Display(Name = "現在のパスワード入力")]
        public string CurrentPass { get; set; }

        [Display(Name = "新しいパスワード入力")]
        public string NewPass { get; set; }

        [Display(Name = "新しいパスワード入力(確認用)")]
        public string ConfirmPass { get; set; }
    }
}
