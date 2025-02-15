﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task_sync_web.Models;
using task_sync_web.Commons;
using SqlKata.Execution;
using System.Data.SqlClient;
using Dapper;
using Humanizer;

namespace task_sync_web.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index(int paramID)
        {
            if (paramID == 1000)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW1010;
            }
            var accountModel = new LoginViewModel();
            return View(accountModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel loginViewModel)
        {
            try
            {
                this.Init();
                // ログインチェック・会社・管理者情報の取得
                var loginData = LoginCheck(loginViewModel);
                var company = loginData.company;
                var administrator = loginData.administrator;

                // ログイン時刻を取得
                var now = DateTime.Now.ToString();

                // ログインに必要なプリンシパルを作る
                var claims = new[] {
                    new Claim(CustomClaimTypes.ClaimType_CampanyName, company.CompanyName.ToString()),
                    new Claim(CustomClaimTypes.ClaimType_CompanyWebPath, company.CompanyWebPath.ToString()),
                    new Claim(CustomClaimTypes.ClaimType_AdministratorId, administrator.AdministratorId.ToString()),
                    new Claim(CustomClaimTypes.ClaimType_AdministratorLoginId, administrator.AdministratorLoginId.ToString()),
                    new Claim(CustomClaimTypes.ClaimType_AdministratorName, administrator.AdministratorName.ToString()),
                    new Claim(CustomClaimTypes.ClaimType_TimeStamp, now)
                        };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = false,
                    //ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                    IsPersistent = loginViewModel.RememberMe // セキュリティの懸念から現状常にFalse
                };

                // ログイン
                await HttpContext.SignInAsync(
                  principal,
                  authProperties
                  );

                // 最終ログイン情報を更新
                var updateLoginDataResult = UpdateIsLogin(company.CompanyDatabaseName.ToString(), administrator.AdministratorId, now);

                // ホーム画面に遷移
                return RedirectToAction("Index", "Home");

            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(loginViewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW1004;
                return View(loginViewModel);
            }
        }

        /// <summary>
        /// ログインを確認して会社情報と管理者情報を取得
        /// </summary>
        /// <returns></returns>
        private (MCompanyModel company, MAdministratorModel administrator) LoginCheck(LoginViewModel loginViewModel)
        {
            try
            {
                var company = GetCompany();
                var administratorModel = CheckLogin(company.CompanyDatabaseName, loginViewModel.AdministratorLoginId, loginViewModel.Password);
                return (company, administratorModel);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 会社情報の取得
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private MCompanyModel GetCompany()
        {
            try
            {
                // URLからパスを取得(https://www.tozan.co.jp/の直後１つ目のパス)
                var urlWebPath = HttpContext.Request.PathBase.ToString().Substring(1);
                if (urlWebPath != "")
                {
                    using (var db = new DbSqlKata())
                    {
                       List<MCompanyModel> companyModels = db.Query("MCompany")
                            .Where("CompanyWebPath", urlWebPath)
                            .Where("IsNotUse", false).Get<MCompanyModel>()
                            .ToList();

                        // 会社情報の取得に失敗した場合はエラー(会社情報が存在しない、該当の会社が２件以上存在する、データベース名が未設定)
                        if (companyModels.Count == 0 || companyModels.Count > 1 || (companyModels.FirstOrDefault().CompanyDatabaseName ?? "") == "")
                        {
                            throw new CustomExtention(ErrorMessages.EW1002);
                        }

                        return companyModels.FirstOrDefault();
                    }
                }
                else
                {
                    throw new CustomExtention(ErrorMessages.EW1002);
                }
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// 管理者IDとパスワードからログイン情報を取得
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="administratorLoginId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private MAdministratorModel CheckLogin(string dbName, string administratorLoginId, string password)
        {
            try
            {
                using (var db = new DbSqlKata(dbName))
                {
                    // 管理者ログインIDから管理者情報を取得
                    //var administrator = db.Query("MAdministrator")
                    //    .Where("AdministratorLoginId", administratorLoginId).Where("IsNotUse", false).Get<MAdministratorModel>().ToList().FirstOrDefault();

                    // 2024/03/27 #207 パスワード設定条件の修正
                    // 管理者ログインIDを大文字に変更する依頼に伴い、
                    // ログインチェック時に大文字・小文字の区別がされるようSQL修正
                    MAdministratorModel administrator = new();
                    var sql = $@"
                        SELECT
                            *
                        FROM 
                            MAdministrator
                        WHERE
                            AdministratorLoginId = '{administratorLoginId}' COLLATE Japanese_CS_AS_KS_WS
                            AND IsNotUse = 0
                    ";

                    var connectionString = new GetConnectString(dbName).ConnectionString;
                    using (var connection = new SqlConnection())
                    {
                        connection.ConnectionString = connectionString;
                        connection.Open();

                        administrator = connection.Query<MAdministratorModel>(sql).ToList().FirstOrDefault();
                    }

                    if (administrator == null || administrator.AdministratorId == 0)
                    {
                        // 有効な管理者ログインIDが存在しない場合はエラー
                        throw new CustomExtention(ErrorMessages.EW1001);
                    }

                    // 利用開始日～利用終了日の範囲内であるか
                    var today = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd 00:00:00"));
                    if (!(administrator.LoginAdministratorEnableStartDate <= today))
                    {
                        // 利用開始日前である場合はエラー
                        throw new CustomExtention(ErrorMessages.EW1005);
                    }
                    else if (!(administrator.LoginAdministratorEnableEndDate >= today))
                    {
                        // 利用終了日を過ぎている場合はエラー
                        throw new CustomExtention(ErrorMessages.EW1006);
                    }

                    // DBから取得したソルトをbyte配列に変換し、入力したパスワードをハッシュ化
                    var byteSalt = Hashing.ConvertStringToBytes(administrator.Salt);
                    var inputPasswordHash = Hashing.ConvertPlaintextPasswordToHashedPassword(password, byteSalt);

                    // 入力したパスワード(ハッシュ)とDBのパスワード(ハッシュ)が一致していたらログインOK
                    if (inputPasswordHash == administrator.Password)
                    {
                        // 成功
                        return administrator;
                    }
                    else if (administrator.Salt.Length == 0)
                    {
                        // 初期ログイン時のみ
                        // ソルトが空白の場合、ハッシュ化していないパスワードで一致しているかチェック
                        // 現在のパスワードと、入力した現在のパスワードが異なっている場合はエラー
                        if (administrator.Password == password)
                        {
                            // 成功
                            return administrator;
                        }
                        else
                        {
                            // 一致しない場合はエラー
                            throw new CustomExtention(ErrorMessages.EW1001);
                        }
                    }
                    else
                    {
                        // 一致しない場合はエラー
                        throw new CustomExtention(ErrorMessages.EW1001);
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// ログイン状態をログイン中に更新
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="administratorId"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private bool UpdateIsLogin(string dbName, int administratorId, string now)
        {
            try
            {
                using (var db = new DbSqlKata(dbName))
                {
                    // DB更新
                    var updateResult = db.Query("MAdministrator")
                        .Where("AdministratorId", administratorId)
                        .Update(new { IsLogin = 1, LastLoginDateTime = now });

                    // ログイン更新に失敗
                    if (updateResult == 0)
                    {
                        throw new CustomExtention(ErrorMessages.EW1003);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// ログイン状態をログアウトに更新
        /// </summary>
        /// <param name="administratorId"></param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private bool UpdateIsLogout(string administratorId)
        {
            try
            {
                // データベース名を取得
                var companyWebPath = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyWebPath).First().Value.ToString();
                var databeseName = new ConvertDatabaseName(companyWebPath).ComapnyDatabeseName;

                using (var db = new DbSqlKata(databeseName))
                {
                    // DB更新
                    var updateResult = db.Query("MAdministrator")
                        .Where("AdministratorId", administratorId)
                        .Update(new { IsLogin = 0 });

                    // ログアウト更新に失敗
                    if (updateResult == 0)
                    {
                        throw new CustomExtention();
                    }

                    return true;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<IActionResult> Logout(int? param)
        {
            try
            {
                string administratorId = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorId).First().Value;
                var logoutUpdate = UpdateIsLogout(administratorId);

                // サインアウト
                // 認証クッキーをレスポンスから削除
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // 別ログインがあった場合、強制ログアウトさせメッセージを表示
                if (param == 1000)
                {
                    return RedirectToAction("Index", new { paramID = 1000 });
                }

                // ログイン画面にリダイレクト
                return RedirectToAction("Index", new { paramID = 0 });
            }
            catch (Exception ex)
            {
                // 正常にログアウト出来なかった場合でもスルーしてログイン画面に戻す
                return RedirectToAction("Index", new { paramID = 0 });
            }
        }

        private void Init()
        {
            var cookieSidebar = Request.Cookies[Cookies.CLASSSIDEBARC];
            if (!string.IsNullOrWhiteSpace(cookieSidebar))
            {
                Response.Cookies.Delete(Cookies.CLASSSIDEBARC);
            }
        }
    }
}
