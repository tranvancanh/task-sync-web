using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;

namespace task_sync_web.Filters
{
    public class MyFilter : IActionFilter
    {
        // IActionFilter参考：https://qiita.com/c-yan/items/c5d499cea7c93e0f5d70

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var controller = context.Controller as Controller;

                // これからアクセスしようとしているコントローラー名を取得
                var accessPath = context.RouteData.Values["controller"].ToString();
                var requestCon = accessPath.ToLower(); // 念のため小文字に統一しておく

                // 共通アクセス可能なControllerはアクセス制御から除外
                if (requestCon != "login" && requestCon != "home")
                {
                    var administratorId = Convert.ToInt32(context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorId).First().Value);
                    var loginTimeStamp = context.HttpContext.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_TimeStamp).First().Value.ToString();

                    if (DateTime.TryParse(loginTimeStamp, out DateTime loginTime))
                    {
                        var db = controller.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value;

                        // ログイン時に記憶したタイムスタンプが書き換わっていないか確認
                        var isLoginTimeValid = IsLoginDateTimeValid(db, administratorId, loginTimeStamp);

                        // 書き換わっていたらログインページに戻す
                        if (!isLoginTimeValid)
                        {
                            var viewResult = controller.RedirectToAction("Logout", "Login", new { param = 1000 });
                            context.Result = viewResult;
                            return;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                var c = context.Controller as Controller;
                c.ViewData["test"] = null;

                // コントローラー名を取得
                var accessPath = context.RouteData.Values["controller"].ToString();
                var requestCon = accessPath.ToLower(); // 念のため小文字に統一しておく

                if (requestCon != "login")
                {
                    // テスト環境か判断
                    var db = c.User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value;
                    var connectionString = new GetConnectString(db).ConnectionString;
                    if (connectionString.Contains("0_test"))
                    {
                        c.ViewData["test"] = "test";
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public bool IsLoginDateTimeValid(string dbName, int administratorId, string lastLoginDate)
        {
            try
            {
                using (var db = new DbSqlKata(dbName))
                {
                    var administratorModels = db.Query("MAdministrator")
                        .Where("AdministratorId", administratorId).Where("LastLoginDateTime", lastLoginDate).Get<MAdministratorModel>().ToList();
                    if (administratorModels.Count == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}