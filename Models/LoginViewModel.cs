using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace task_sync_web.Models
{
    public class LoginViewModel
    {
        [Display(Name = "管理者ログインID")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string AdministratorLoginId { get; set; }

        [Display(Name = "パスワード")]
        [Required(ErrorMessageResourceName = "EW0001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public LoginViewModel()
        {
            RememberMe = false;
        }
    }
}