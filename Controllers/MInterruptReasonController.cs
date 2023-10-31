using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{

    public class MInterruptReasonController : Controller
    {
        [HttpGet]
        public IActionResult Index(MInterruptReasonViewModel viewModel, int? pageNumber, string mess = null)
        {
            try
            {
                var dbName = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value;
                var listInterruptReason = GetListInterruptReason(viewModel.SearchKeyWord, dbName);
                if (!listInterruptReason.Any())
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW0102;
                    return View(viewModel);
                }

                // page the list
                var listPaged = listInterruptReason.ToPagedList(pageNumber ?? 1, viewModel.PageRowCount);
                viewModel.InterruptReasonModels = listPaged;

                if (!string.IsNullOrEmpty(mess))
                {
                    ViewData["SuccessMessage"] = mess;
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                if(ex is CustomExtention)
                {
                    ViewData["ErrorMessage"] = ex.Message;
                    return View(viewModel);
                }
                else
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW500;
                    return View(viewModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectToActionResult Edit(MInterruptReasonModel model, string searchKeyWord, int pageNumber)
        {
            try
            {
                var dbName = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value;
                var administratorId = User.Claims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorId).First().Value;
                var efftedRows = -1;

                if(string.IsNullOrWhiteSpace(model.InterruptReasonCode))
                    model.InterruptReasonCode = string.Empty;

                if (string.IsNullOrWhiteSpace(model.InterruptReasonName))
                    model.InterruptReasonName = string.Empty;

                // update
                using (var db = new DbSqlKata(dbName))
                {
                    efftedRows = db.Query("MInterruptReason").Where("InterruptReasonId", model.InterruptReasonId).Update(new
                    {
                        InterruptReasonCode = model.InterruptReasonCode,
                        InterruptReasonName = model.InterruptReasonName,
                        IsNotUse = model.IsNotUse,
                        UpdateDateTime = DateTime.Now.Date,
                        UpdateAdministratorId = administratorId
                    });
                    if (efftedRows > 0)
                        return RedirectToAction("Index", new
                        {
                            searchKeyWord = searchKeyWord,
                            pageNumber = pageNumber,
                            mess = SuccessMessages.SW002
                        });
                    else
                        return RedirectToAction("Index", new
                        {
                            searchKeyWord = searchKeyWord,
                            pageNumber = pageNumber,
                            mess = ErrorMessages.EW0502
                        });
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomExtention)
                {
                    ViewData["ErrorMessage"] = ex.Message;
                }
                else
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW500;
                }
                var error = Convert.ToString(ViewData["ErrorMessage"]);
                return RedirectToAction("Index", new
                {
                    searchKeyWord = searchKeyWord,
                    pageNumber = pageNumber,
                    mess = error
                });
            }
        }

        private List<MInterruptReasonModel> GetListInterruptReason(string searchKey, string dbName)
        {
            var interruptReasonModels = new List<MInterruptReasonModel>();
            var listInterruptReason = new List<MInterruptReasonModel>();
            using (var db = new DbSqlKata(dbName))
            {
                listInterruptReason = db.Query("MInterruptReason as a")
                .Select(
                        "InterruptReasonId",
                        "InterruptReasonCode",
                        "InterruptReasonName",
                        "a.Remark",
                        "a.IsNotUse",
                        "CreateDateTime",
                        "a.CreateAdministratorId",
                        "UpdateDateTime",
                        "a.UpdateAdministratorId",
                        "b.AdministratorLoginId as AdministratorIdCreate",
                        "b.AdministratorName as AdministratorNameCreate",
                        "c.AdministratorLoginId as AdministratorIdUpdate",
                        "c.AdministratorName as AdministratorNameUpdate"
                    )
                .LeftJoin("MAdministrator as b", "a.CreateAdministratorId", "b.AdministratorId")
                .LeftJoin("MAdministrator as c", "a.UpdateAdministratorId", "c.AdministratorId")
                .OrderBy("a.InterruptReasonId")
                .Get<MInterruptReasonModel>().ToList();
                listInterruptReason = listInterruptReason.Where(x => x.AdministratorNameCreate != null && x.AdministratorNameUpdate != null).ToList();
            }

            if (listInterruptReason.Count == 0)
            {
                throw new CustomExtention(ErrorMessages.EW0101);
            }

            searchKey = (searchKey ?? "").Trim();

            if (listInterruptReason.Count > 0 && searchKey.Length > 0)
            {
                // 検索キーワードが存在する場合
                interruptReasonModels = listInterruptReason.Where(x => x.InterruptReasonCode.ToString().Contains(searchKey) || x.InterruptReasonName.Contains(searchKey)).ToList();
                if (interruptReasonModels.Count == 0)
                {
                    throw new CustomExtention(ErrorMessages.EW0102);
                }
            }
            else
            {
                interruptReasonModels = listInterruptReason;
            }

            return interruptReasonModels;
        }
    }
}
