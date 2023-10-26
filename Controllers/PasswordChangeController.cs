using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons.DbSqlKata;
using task_sync_web.Commons.DbSqlKata;
using task_sync_web.Models;
using tec_shipping_management_web.Commons;

namespace task_sync_web.Controllers
{
    //[Authorize]
    public class PasswordChangeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var userID = "100004";
            var userCode = User.Identity.Name;

            var changePassword = new ChangePasswordViewModel()
            {
                CurrentUserCode = userID,
                CurrentPass = string.Empty,
                NewPass = string.Empty,
                ConfirmPass = string.Empty
            };
            return View(changePassword);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ChangePasswordViewModel changePassword)
        {
            ViewData["Error"] = null;
            ViewData["Success"] = null;

            if(changePassword.CurrentPass == null 
                || changePassword.NewPass == null 
                || changePassword.ConfirmPass == null)
            {
                ViewData["Error"] = ErrorMessages.W3_1_2_1001;
                return View(changePassword);
            }

            if(changePassword.NewPass.Length > 10
                || changePassword.ConfirmPass.Length > 10)
            {
                ViewData["Error"] = ErrorMessages.W3_1_2_1002;
                return View(changePassword);
            }

            if (changePassword.NewPass != changePassword.ConfirmPass)
            {
                ViewData["Error"] = ErrorMessages.W3_1_2_1003;
                return View(changePassword);
            }

            try
            {
                using (var db = new DbSqlKata())
                {
                    var userModel = await db.Query("M_WorkUser").Where("WorkUserID", changePassword.CurrentUserCode).FirstOrDefaultAsync<M_WorkUserModel>();
                    if (userModel == null)
                    {
                        ViewData["Error"] = ErrorMessages.W3_1_2_1004;
                        return View(changePassword);
                    }

                    //新しいパスワード変更と現在のパスワードをチェック
                    var hashedPassword = userModel.WorkUserPassword;
                    var stringSalt = Hashing.ConvertStringToBytes(userModel.Salt);
                    var currentPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(changePassword.CurrentPass, stringSalt);
                    if (currentPassHash != hashedPassword)
                    {
                        ViewData["Error"] = ErrorMessages.W3_1_2_1005;
                        return View(changePassword);
                    }

                    //新しいパスワードハッシュを作成
                    var newSalt = Hashing.GetRandomSalt();
                    var newPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(changePassword.NewPass, newSalt);
                    var newStringSalt = Hashing.ConvertByteToString(newSalt);

                    // update
                    await db.Query("M_WorkUser")
                        .Where("WorkUserID", changePassword.CurrentUserCode)
                        .UpdateAsync(
                        new { WorkUserPassword = newPassHash, Salt = newStringSalt }
                        );

                    changePassword = new ChangePasswordViewModel()
                    {
                        CurrentUserCode = changePassword.CurrentUserCode,
                        CurrentPass = string.Empty,
                        NewPass = string.Empty,
                        ConfirmPass = string.Empty
                    };

                    ViewData["Success"] = ErrorMessages.W10001;
                }

                return View(changePassword);
            }
            catch (Exception)
            {
                ViewData["Error"] = ErrorMessages.W10003;
                return View(changePassword);
            }
        }


    }
}
