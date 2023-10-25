using Microsoft.AspNetCore.Mvc;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    public class PasswordChangeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var userName = User.Identity.Name;
            var userID = "10001";
            var changePassword = new ChangePasswordViewModel()
            {
                CurrentUserCode = userName,
                CurrentPass = string.Empty,
                NewPass = string.Empty,
                ConfirmPass = string.Empty
            };
            return View(changePassword);
        }

        [HttpPost]
        public IActionResult Index(ChangePasswordViewModel changePassword)
        {

            return View();
        }

    }
}
