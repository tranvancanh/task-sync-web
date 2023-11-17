using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.ComponentModel.Design;
using task_sync_web.Commons;
using task_sync_web.Controllers;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    [AllowAnonymous]
    public class AppDownloadController : Controller
    {
        private readonly ILogger<AppDownloadController> _logger;

        public AppDownloadController(ILogger<AppDownloadController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string companyId)
        {
            var appDownloadViewModel = new AppDownloadViewModel();
            appDownloadViewModel.CompanyId = companyId;
            return View(appDownloadViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexAsync(AppDownloadViewModel appDownloadViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // 一番最初のプロパティのエラーの最初のメッセージ（エラーが複数ある場合）
                    ViewData["Error"] = ModelState.Values.First().Errors.First().ErrorMessage;
                    return View();
                }

                var downloadCord = "";
                var targetFolderPath = "";

                var companies = new List<MCompanyModel>();
                using (var db = new DbSqlKata())
                {
                    companies = db.Query("MCompany")
                         .Where("CompanyId", appDownloadViewModel.CompanyId)
                         .Where("IsNotUse", false).Get<MCompanyModel>()
                         .ToList();
                }

                if (companies.Count != 1)
                {
                    throw new Exception();
                }
                else
                {
                    var company = companies[0];
                    downloadCord = company.SmartphoneAppDownloadCode;
                    targetFolderPath = @$"\{company.SmartphoneAppMinVersion}"; ;
                }

                if ((appDownloadViewModel.DownloadCord ?? "").Trim() != downloadCord)
                {
                    ViewData["Error"] = "不正なダウンロードコードです。";
                    return View();
                }

                var apkFileName = @"\tozan.tasksync_app.apk";

                // 画像のフォルダパス
                var rootPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(rootPath, @"wwwroot\appfiles" + targetFolderPath + apkFileName);

                // パスが存在するか
                if (!System.IO.File.Exists(filePath))
                {
                    ViewData["Error"] = "ダウンロードファイルがサーバー上に存在しません。";
                    return View();
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "application/vnd.android.package-archive", Path.GetFileName(filePath));

            }
            catch (Exception ex)
            {
                ViewData["Error"] = "データの取得に失敗しました。"+ ex.Message;
                return View();
            }

        }

    }
}