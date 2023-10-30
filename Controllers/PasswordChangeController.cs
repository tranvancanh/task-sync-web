using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    public class PasswordChangeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var changePassword = new PasswordChangeViewModel();
            return View(changePassword);
        }

        [HttpPost]
        public IActionResult Index(PasswordChangeViewModel viewModel)
        {
            int administratorId = Convert.ToInt32(User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorId).First().Value);

            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage)
                         .Distinct().FirstOrDefault();
                ViewData["ErrorMessage"] = errorMessage;
                return View(viewModel);
            }

            try
            {
                // 現在のパスワードと新しいパスワードが同じ場合はエラー
                if (viewModel.CurrentPassword == viewModel.NewPassword)
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW1103;
                    return View(viewModel);
                }

                // 新しいパスワードと新しいパスワード(確認用)が同じでない場合はエラー
                if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW1102;
                    return View(viewModel);
                }

                var dbName = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value;
                using (var db = new DbSqlKata(dbName))
                {
                    var administratorList = db
                        .Query("MAdministrator")
                        .Where("AdministratorId", administratorId)
                        .FirstOrDefault<MAdministratorModel>();
                    if (administratorList == null || administratorList.AdministratorLoginId == null)
                    {
                        ViewData["ErrorMessage"] = ErrorMessages.EW0900;
                        return View(viewModel);
                    }

                    // 現在のパスワードが正しいかチェック
                    var currentPassword = administratorList.Password;
                    if (administratorList.Salt.Length == 0)
                    {
                        // 初期ログイン時のみ：ソルトが空白の場合、ハッシュ化していないパスワードで一致しているかチェック
                        // 現在のパスワードと、入力した現在のパスワードが異なっている場合はエラー
                        if (currentPassword != viewModel.CurrentPassword)
                        {
                            ViewData["ErrorMessage"] = ErrorMessages.EW1101;
                            return View(viewModel);
                        }
                    }
                    else
                    {
                        var currentByteSalt = Hashing.ConvertStringToBytes(administratorList.Salt);
                        var inputCurrentPasswordHash = Hashing.ConvertPlaintextPasswordToHashedPassword(viewModel.CurrentPassword, currentByteSalt);
                        if (currentPassword != inputCurrentPasswordHash)
                        {
                            ViewData["ErrorMessage"] = ErrorMessages.EW1101;
                            return View(viewModel);
                        }
                    }

                    // 新しいソルトでパスワードをハッシュ化
                    var newSalt = Hashing.GetRandomSalt();
                    var newPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(viewModel.NewPassword, newSalt);
                    var newStringSalt = Hashing.ConvertByteToString(newSalt);

                    // DB更新
                    db.Query("MAdministrator")
                        .Where("AdministratorId", viewModel.AddministratorId)
                        .Update(new { Password = newPassHash, Salt = newStringSalt });

                    ViewData["SuccessMessage"] = SuccessMessages.SW001;
                }

                return View(viewModel);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW0900;
                return View(viewModel);
            }
        }


    }
}
