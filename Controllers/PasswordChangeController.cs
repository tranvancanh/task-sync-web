using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    public class PasswordChangeController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            var changePassword = new PasswordChangeViewModel();
            return View(changePassword);
        }

        [HttpPost]
        public IActionResult Change(PasswordChangeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage)
                         .Distinct().FirstOrDefault();
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("Index");
            }

            try
            {
                // 現在のパスワードと新しいパスワードが同じ場合はエラー
                if (viewModel.CurrentPassword == viewModel.NewPassword)
                {
                    TempData["ErrorMessage"] = ErrorMessages.EW1103;
                    return RedirectToAction("Index");
                }

                // 新しいパスワードと新しいパスワード(確認用)が同じでない場合はエラー
                if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = ErrorMessages.EW1102;
                    return RedirectToAction("Index");
                }

                // ログイン中の管理者情報を取得
                var loginAdministrator = new MAdministratorModel();
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var administratorList = db
                        .Query("MAdministrator")
                        .Where("AdministratorId", LoginUser.AdministratorId)
                        .Get<MAdministratorModel>().ToList();

                    if (administratorList.Count == 1)
                    {
                        // 管理者情報の取得成功
                        loginAdministrator = administratorList.FirstOrDefault();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = ErrorMessages.EW1104;
                        return RedirectToAction("Index");
                    }
                }

                // 現在のパスワードが正しいかチェック
                var currentPassword = loginAdministrator.Password;
                if (loginAdministrator.Salt.Length == 0)
                {
                    // 初期ログイン時のみ：ソルトが空白の場合、ハッシュ化していないパスワードで一致しているかチェック
                    // 現在のパスワードと、入力した現在のパスワードが異なっている場合はエラー
                    if (currentPassword != viewModel.CurrentPassword)
                    {
                        TempData["ErrorMessage"] = ErrorMessages.EW1101;
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var currentByteSalt = Hashing.ConvertStringToBytes(loginAdministrator.Salt);
                    var inputCurrentPasswordHash = Hashing.ConvertPlaintextPasswordToHashedPassword(viewModel.CurrentPassword, currentByteSalt);
                    if (currentPassword != inputCurrentPasswordHash)
                    {
                        TempData["ErrorMessage"] = ErrorMessages.EW1101;
                        return RedirectToAction("Index");
                    }
                }

                // 新しいソルトでパスワードをハッシュ化
                var newSalt = Hashing.GetRandomSalt();
                var newPassHash = Hashing.ConvertPlaintextPasswordToHashedPassword(viewModel.NewPassword, newSalt);
                var newStringSalt = Hashing.ConvertByteToString(newSalt);

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    // DB更新
                    var efftedRows = db.Query("MAdministrator")
                            .Where("AdministratorId", LoginUser.AdministratorId)
                            .Update(new { Password = newPassHash, Salt = newStringSalt });

                    if (efftedRows > 0)
                    {
                        TempData["SuccessMessage"] = SuccessMessages.SW002;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = ErrorMessages.EW0502;
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = ErrorMessages.EW9000;
                return RedirectToAction("Index");
            }
        }


    }
}
