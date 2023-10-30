using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons.DbSqlKata;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MSystemSettingController : Controller
    {
        [HttpGet]
        public IActionResult Index(MSystemSettingViewModel viewModel, int? pageNumber)
        {
            try
            {
                var listSetting = GetListMAdministrator(viewModel.SearchKeyWord);
                // page the list
                var listPaged = listSetting.ToPagedList(pageNumber ?? 1, viewModel.PageRowCount);
                viewModel.SystemSettingModels = listPaged;

                if (!string.IsNullOrEmpty(Convert.ToString(TempData["MessageSuccess"])))
                {
                    ViewData["MessageSuccess"] = TempData["MessageSuccess"];
                }

                return View(viewModel);
            }
            catch
            {
                ViewData["MessageError"] = ErrorMessages.EW500;
                return View(viewModel);
            }
        }

        [HttpPost]
        public IActionResult Edit(MSystemSettingModel settingModel)
        {
            var updateLoginId = "8";
            try
            {
                // update
                using (var db = new DbSqlKata())
                {
                    var efftedRows = db.Query("MSystemSetting").Where("SystemSettingId", settingModel.SystemSettingId).Update(new
                    {
                        SystemSettingStringValue = settingModel.SystemSettingStringValue,
                        UpdateDateTime = DateTime.Now.Date,
                        UpdateAdministratorId = updateLoginId
                    });
                    if (efftedRows > 0)
                        return Json(new {Result = "OK", Mess = ErrorMessages.EW503}); 
                    else
                        return Json(new { Result = "NG", Mess = ErrorMessages.EW502 });
                }

            }
            catch (Exception ex)
            {
                return Json(new { Result = "NG", Mess = ex.Message });
            }
        }

        private List<MSystemSettingModel> GetListMAdministrator(string keySearch)
        {
            var list = new List<MSystemSettingModel>();
            using (var db = new DbSqlKata())
            {
                if (string.IsNullOrWhiteSpace(keySearch))
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
                    .OrderBy("MSystemSetting.SystemSettingId")
                    .Get<MSystemSettingModel>().ToList();
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
            }
            return list;
        }
    }
}
