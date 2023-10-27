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

                return View(viewModel);
            }
            catch
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View(viewModel);
            }
        }

        [HttpPost]
        public RedirectToActionResult Edit(MSystemSettingModel settingModel, string searchKeyWord, int pageNumber)
        {
            var viewModel = new MSystemSettingViewModel();
            try
            {
                // update
                using (var db = new DbSqlKata())
                {
                    var efftedRows = db.Query("MSystemSetting").Where("SystemSettingId", settingModel.SystemSettingId).Update(new
                    {
                        SystemSettingStringValue = settingModel.SystemSettingStringValue
                    });
                    //if(efftedRows > 0)
                    //    TempData[""] = ErrorMessages
                    //else

                }

                viewModel = new MSystemSettingViewModel() { SearchKeyWord = searchKeyWord };

            }
            catch (Exception ex)
            {

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
