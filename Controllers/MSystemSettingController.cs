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
        public IActionResult Index(MSystemSettingViewModel viewModel)
        {
            try
            {
                // 一覧表示用のページリストを取得
                viewModel.SystemSettingModels = GetPageList(viewModel);

                // 修正モーダルを表示する場合
                if (viewModel.EditSystemSettingId > 0)
                {
                    // システム設定IDから、システム設定情報を取得
                    viewModel.SystemSettingEditModel = viewModel.SystemSettingModels.Where(x => x.SystemSettingId == viewModel.EditSystemSettingId).ToList().FirstOrDefault();
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

        private IPagedList<MSystemSettingModel> GetPageList(MSystemSettingViewModel viewModel)
        {
            try
            {
                var listSetting = GetListMAdministrator(viewModel.SearchKeyWord);

                // 一覧表示用のページリストを作成
                var listPaged = listSetting.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);

                return listPaged;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult Edit(MSystemSettingViewModel viewModel)
        {
            try
            {
                // 一覧表示用のページリストを取得
                viewModel.SystemSettingModels = GetPageList(viewModel);

                // Update
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var efftedRows = db.Query("MSystemSetting")
                        .Where("SystemSettingId", viewModel.SystemSettingEditModel.SystemSettingId)
                        .Update(new
                        {
                            SystemSettingValue = viewModel.SystemSettingEditModel.SystemSettingValue,
                            SystemSettingStringValue = (viewModel.SystemSettingEditModel.SystemSettingStringValue ?? "").Trim(),
                            UpdateDateTime = DateTime.Now.Date,
                            UpdateAdministratorId = LoginUser.AdministratorId
                        });

                    if (efftedRows > 1)
                    {
                        ViewData["SuccessMessage"] = SuccessMessages.SW002;
                        viewModel.SystemSettingEditModel = new MSystemSettingModel();
                    }
                    else
                    {
                        ViewData["ErrorMessageEdit"] = ErrorMessages.EW0502;
                    }
                }

            }
            catch (Exception ex)
            {
                ViewData["ErrorMessageEdit"] = ErrorMessages.EW0502;
            }

            return View("Index", viewModel);
        }

        private List<MSystemSettingModel> GetListMAdministrator(string searchKey)
        {
            try
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
                        systemSettingModels = systemSettingList;
                    }
                }
                return systemSettingModels;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
