using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons.MssSqlKata;
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

            var listError = new List<string>();

            if(changePassword.CurrentPass == null 
                || changePassword.NewPass == null 
                || changePassword.ConfirmPass == null)
            {
                listError.Add(ErrorMessages.W3_1_2_1001);
                ViewData["Error"] = listError;
                return View(changePassword);
            }

            if(changePassword.NewPass.Length > 10
                || changePassword.ConfirmPass.Length > 10)
            {
                listError.Add(ErrorMessages.W3_1_2_1002);
                ViewData["Error"] = listError;
                return View(changePassword);
            }

            if (changePassword.NewPass != changePassword.ConfirmPass)
            {
                listError.Add(ErrorMessages.W3_1_2_1003);
                ViewData["Error"] = listError;
                return View(changePassword);
            }

            //var salt1 = Hashing.GetRandomSalt();
            //var currentPassHash2 = Hashing.ConvertPlaintextPasswordToHashedPassword("100004", salt1);
            //var stringSalt1 = Hashing.ConvertByteToString(salt1);

            using (var db = new MssSqlKata())
            {
                var userModel = await db.Query("M_WorkUser").Where("WorkUserID", changePassword.CurrentUserCode).FirstOrDefaultAsync<M_WorkUserModel>();
                if (userModel == null)
                {
                    listError.Add(ErrorMessages.W3_1_2_1004);
                    ViewData["Error"] = listError;
                    return View(changePassword);
                }

                var hashedPassword = userModel.WorkUserPassword;
                var stringSalt = Hashing.ConvertStringToBytes(userModel.Salt);
                var currentPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(changePassword.CurrentPass, stringSalt);
                if (currentPassHash != hashedPassword)
                {
                    listError.Add(ErrorMessages.W3_1_2_1005);
                    ViewData["Error"] = listError;
                    return View(changePassword);
                }

                var newSalt = Hashing.GetRandomSalt();
                var newPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(changePassword.NewPass, newSalt);
                var newStringSalt = Hashing.ConvertByteToString(newSalt);

                // update
                await db.Query("M_WorkUser").Where("WorkUserID", changePassword.CurrentUserCode).UpdateAsync(new { WorkUserPassword = newPassHash, Salt = newStringSalt });

                changePassword = new ChangePasswordViewModel()
                {
                    CurrentUserCode = changePassword.CurrentUserCode,
                    CurrentPass = string.Empty,
                    NewPass = string.Empty,
                    ConfirmPass = string.Empty
                };

                ViewData["Success"] = new List<string>() { ErrorMessages.W10001 };
            }

            return View(changePassword);
        }



    }
}
