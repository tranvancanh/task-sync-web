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
                else if (!string.IsNullOrEmpty(Convert.ToString(TempData["MessageError"])))
                {
                    ViewData["MessageError"] = TempData["MessageError"];
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
        public RedirectToActionResult Edit(MSystemSettingModel settingModel, string searchKeyWord, int pageNumber)
        {
            var viewModel = new MSystemSettingViewModel();
            var updateLoginId = "8";
            try
            {
                TempData["MessageSuccess"] = null;
                TempData["MessageError"] = null;
                // update
                using (var db = new DbSqlKata())
                {
                    var efftedRows = db.Query("MSystemSetting").Where("SystemSettingId", settingModel.SystemSettingId).Update(new
                    {
                        SystemSettingStringValue = settingModel.SystemSettingStringValue,
                        UpdateDateTime = DateTime.Now.Date,
                        UpdateLoginId = updateLoginId
                    });
                    if (efftedRows > 0)
                        TempData["MessageSuccess"] = ErrorMessages.EW503;
                    else
                        TempData["MessageError"] = ErrorMessages.EW502;
                }

                viewModel = new MSystemSettingViewModel() { SearchKeyWord = searchKeyWord };

            }
            catch (Exception ex)
            {
                TempData["MessageError"] = ex.Message;
            }
           
            return RedirectToAction("Index", new
            {
                searchKeyWord = viewModel.SearchKeyWord,
                pageNumber = pageNumber
            });
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
                        "MSystemSetting.UpdateLoginId",
                        "MAdministrator.AdministratorName as UpdateLoginName"
                        )
                    .LeftJoin("MAdministrator", "MSystemSetting.UpdateLoginId", "MAdministrator.AdministratorId")
                    .WhereNotNull("MAdministrator.AdministratorName")
                    .OrderBy("MSystemSetting.UpdateLoginId")
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
                        "MSystemSetting.UpdateLoginId",
                        "MAdministrator.AdministratorName as UpdateLoginName"
                        )
                    .LeftJoin("MAdministrator", "MSystemSetting.UpdateLoginId", "MAdministrator.AdministratorId")
                    .WhereNotNull("MAdministrator.AdministratorName")
                    .WhereLike("SystemSettingId", $"%{keySearch}%")
                    .OrWhereLike("SystemSettingOutline", $"%{keySearch}%")
                    .OrderBy("MSystemSetting.UpdateLoginId")
                    .Get<MSystemSettingModel>().ToList();
                }
            }
            return list;
        }
    }
}
