using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MSystemSettingController : BaseController
    {
        [HttpGet]
        public IActionResult Index(MSystemSettingViewModel viewModel, int? pageNumber, string message = null)
        {
            try
            {
                var listSetting = GetListMAdministrator(viewModel.SearchKeyWord);
                if (!listSetting.Any())
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW0102;
                    return View(viewModel);
                }

                // page the list
                var listPaged = listSetting.ToPagedList(pageNumber ?? 1, viewModel.PageRowCount);
                viewModel.SystemSettingModels = listPaged;

                if (!string.IsNullOrEmpty(message))
                {
                    ViewData["SuccessMessage"] = message;
                }
                return View(viewModel);
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View(viewModel);
            }
        }

        [HttpPost]
        public IActionResult Edit(MSystemSettingModel settingModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settingModel.SystemSettingStringValue))
                    settingModel.SystemSettingStringValue = string.Empty;

                if (settingModel.SystemSettingStringValue.Length > 100)
                    throw new CustomExtention(ErrorMessages.EW002);

                // update
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var efftedRows = db.Query("MSystemSetting").Where("SystemSettingId", settingModel.SystemSettingId).Update(new
                    {
                        settingModel.SystemSettingStringValue,
                        UpdateDateTime = DateTime.Now.Date,
                        UpdateAdministratorId = LoginUser.AdministratorId
                    });
                    if (efftedRows > 0)
                        return Json(new { Result = "OK", Mess = SuccessMessages.SW002 });
                    else
                        return Json(new { Result = "NG", Mess = ErrorMessages.EW0502 });
                }

            }
            catch (Exception ex)
            {
                return Json(new { Result = "NG", Mess = ex.Message });
            }
        }

        private List<MSystemSettingModel> GetListMAdministrator(string searchKey)
        {
            var systemSettingModels = new List<MSystemSettingModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // DBからデータ一覧を取得
                var systemSettingList = db.Query("MSystemSetting")
                .Select(
                    "MSystemSetting.SystemSettingId",
                    "MSystemSetting.SystemSettingOutline",
                    "MSystemSetting.SystemSettingDetail",
                    "MSystemSetting.SystemSettingValue",
                    "MSystemSetting.SystemSettingStringValue",
                    "MSystemSetting.UpdateDateTime",
                    "MAdministrator.AdministratorLoginId as UpdateAdministratorId",
                    "MAdministrator.AdministratorName as UpdateAdministratorName"
                    )
                .LeftJoin("MAdministrator", "MSystemSetting.UpdateAdministratorId", "MAdministrator.AdministratorId")
                .OrderBy("MSystemSetting.SystemSettingId")
                .Get<MSystemSettingModel>().ToList();

                if (systemSettingList.Count == 0)
                {
                    throw new CustomExtention(ErrorMessages.EW0101);
                }

                searchKey = (searchKey ?? "").Trim();
                if (systemSettingList.Count > 0 && searchKey.Length > 0)
                {
                    // 検索キーワードが存在する場合
                    systemSettingModels = systemSettingList.Where(x => x.SystemSettingId.ToString().Contains(searchKey) || x.SystemSettingOutline.Contains(searchKey)).ToList();
                    if (systemSettingModels.Count == 0)
                    {
                        throw new CustomExtention(ErrorMessages.EW0102);
                    }
                }
                else
                {
                    list = db.Query("MSystemSetting")
                    .Select(
                        "MSystemSetting.SystemSettingId",
                        "MSystemSetting.SystemSettingOutline",
                        "MSystemSetting.SystemSettingDetail",
                        "MSystemSetting.SystemSettingStringValue",
                        "MSystemSetting.UpdateDateTime",
                        "MAdministrator.AdministratorLoginId as UpdateAdministratorId",
                        "MAdministrator.AdministratorName as UpdateAdministratorName"
                        )
                    .LeftJoin("MAdministrator", "MSystemSetting.UpdateAdministratorId", "MAdministrator.UpdateAdministratorId")
                    .WhereNotNull("MAdministrator.AdministratorName")
                    .WhereLike("SystemSettingId", $"%{keySearch}%")
                    .OrWhereLike("SystemSettingOutline", $"%{keySearch}%")
                    .OrderBy("MSystemSetting.SystemSettingId")
                    .Get<MSystemSettingModel>().ToList();
                }
                return systemSettingModels;
            }
        }
    }
}
