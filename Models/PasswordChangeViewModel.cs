using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace task_sync_web.Models
{
    public class PasswordChangeViewModel :BaseViewModel
    {

        [Display(Name = "現在のパスワード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string CurrentPassword { get; set; }

        [Display(Name = "新しいパスワード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [RegularExpression("^[!-~]+$", ErrorMessageResourceName = "EW0011", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        [CustomValidation(typeof(PasswordChangeViewModel), nameof(ValidatePasswordComplexity))]
        public string NewPassword { get; set; }

        [Display(Name = "新しいパスワード(確認用)")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        [RegularExpression("^[!-~]+$", ErrorMessageResourceName = "EW0011", ErrorMessageResourceType = typeof(ErrorMessages))]
        [StringLength(12, MinimumLength = 8, ErrorMessageResourceName = "EW0003", ErrorMessageResourceType = typeof(ErrorMessages))]
        [CustomValidation(typeof(PasswordChangeViewModel), nameof(ValidatePasswordComplexity))]
        public string ConfirmNewPassword { get; set; }

        /// <summary>
        /// パスワードの複雑さを検証する
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ValidationResult ValidatePasswordComplexity(string password, ValidationContext context)
        {
            if (string.IsNullOrEmpty(password)) return ValidationResult.Success;

            //半角数字、半角英大文字、半角英小文字、半角記号のうち2種以上の組み合わせであるか調べる
            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigits = password.Any(char.IsDigit);
            bool hasSymbols = password.Any(ch => !char.IsLetterOrDigit(ch));
            int typeCount = new[] { hasUpperCase, hasLowerCase, hasDigits, hasSymbols }.Count(t => t);

            // 2種以上ならOK
            if (typeCount >= 2)
                return ValidationResult.Success;
            else
                // "パスワードは数字、大文字・小文字の英字、記号のうち2種以上の組合せである必要があります。"
                return new ValidationResult(ErrorMessages.EW1105);
        }

        public PasswordChangeViewModel()
        {
            DisplayName = "管理者パスワード変更";
        }
    }
}
