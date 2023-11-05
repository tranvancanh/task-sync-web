using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{

    public class MInterruptReasonController : BaseController
    {
        [HttpGet]
        public IActionResult Index(MInterruptReasonViewModel viewModel)
        {
            try
            {
                var listInterruptReason = GetListInterruptReason(viewModel.SearchKeyWord);
                ModelState.Clear();

                // 検索処理
                if (viewModel.ModalType == Enums.ModalType.None)
                {
                    if (!listInterruptReason.Any())
                    {
                        ViewData["ErrorMessage"] = ErrorMessages.EW0102;
                        return View(viewModel);
                    }
                }
                else if(viewModel.ModalType == Enums.ModalType.Create)
                {
                    // 新規登録処理
                    viewModel.ModalModel = new MInterruptReasonModel() { InterruptReasonId = viewModel.ModalModel.InterruptReasonId};
                }
                else
                {
                    // 更新処理
                    var modalVal = listInterruptReason.Where(x => x.InterruptReasonId == viewModel.ModalModel.InterruptReasonId).FirstOrDefault();
                    if (modalVal != null)
                        viewModel.ModalModel = modalVal;
                    else
                    {
                        ViewData["ErrorMessage"] = ErrorMessages.EW0102;
                        return View(viewModel);
                    }
                }

                // page the list
                var listPaged = listInterruptReason.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                viewModel.InterruptReasonModels = listPaged;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (ex is CustomExtention)
                {
                    ViewData["ErrorMessage"] = ex.Message;
                    return View(viewModel);
                }
                else
                {
                    ViewData["ErrorMessage"] = ErrorMessages.EW9000;
                    return View(viewModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(MInterruptReasonViewModel viewModel, Enums.ModalType modalType)
        {
            try
            {
                var efftedRows = -1;
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    if (modalType == Enums.ModalType.Create)
                    {
                        // 新規登録処理
                        efftedRows = db.Query("MInterruptReason")
                            .InsertGetId<int>(new
                            {
                                viewModel.ModalModel.InterruptReasonCode,
                                InterruptReasonName = (viewModel.ModalModel.InterruptReasonName ?? "").Trim(),
                                Remark = (viewModel.ModalModel.Remark ?? "").Trim(),
                                viewModel.ModalModel.IsNotUse,
                                CreateDateTime = DateTime.Now,
                                CreateAdministratorId = LoginUser.AdministratorId,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId,
                            });
                    }
                    else if (modalType == Enums.ModalType.Edit)
                    {
                        // 更新処理
                        efftedRows = db.Query("MInterruptReason")
                            .Where("InterruptReasonId", viewModel.ModalModel.InterruptReasonId)
                            .Update(new
                            {
                                viewModel.ModalModel.InterruptReasonCode,
                                InterruptReasonName = (viewModel.ModalModel.InterruptReasonName ?? "").Trim(),
                                Remark = (viewModel.ModalModel.Remark ?? "").Trim(),
                                viewModel.ModalModel.IsNotUse,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId
                            });
                    }
                }

                if (efftedRows > 0)
                {
                    viewModel.ModalType = Enums.ModalType.None;
                    ViewData["SuccessMessage"] = SuccessMessages.SW002;
                }
                else
                {
                    ViewData["ErrorMessageModal"] = ErrorMessages.EW0502;
                }

                var listInterruptReason = GetListInterruptReason(viewModel.SearchKeyWord);
                var listPaged = listInterruptReason.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                viewModel.InterruptReasonModels = listPaged;
                return View("Index", viewModel);
            }
            catch(Exception ex)
            {
                if (ex is CustomExtention)
                    ViewData["ErrorMessageModal"] = ex.Message;
                else
                    ViewData["ErrorMessageModal"] = ErrorMessages.EW9000;
                return View("Index", viewModel);
            }
        }

        private List<MInterruptReasonModel> GetListInterruptReason(string searchKey)
        {
            var interruptReasonModels = new List<MInterruptReasonModel>();
            var listInterruptReason = new List<MInterruptReasonModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                listInterruptReason = db.Query("MInterruptReason as a")
                .Select(
                        "InterruptReasonId",
                        "InterruptReasonCode",
                        "InterruptReasonName",
                        "a.Remark",
                        "a.IsNotUse",
                        "a.CreateDateTime",
                        "b.AdministratorLoginId as CreateAdministratorLoginId",
                        "b.AdministratorName as CreateAdministratorName",
                        "a.UpdateDateTime",
                        "c.AdministratorLoginId as UpdateAdministratorLoginId",
                        "c.AdministratorName as UpdateAdministratorName"
                    )
                .LeftJoin("MAdministrator as b", "a.CreateAdministratorId", "b.AdministratorId")
                .LeftJoin("MAdministrator as c", "a.UpdateAdministratorId", "c.AdministratorId")
                .OrderBy("a.InterruptReasonCode")
                .Get<MInterruptReasonModel>().ToList();
            }

            if (listInterruptReason.Count == 0)
            {
                throw new CustomExtention(ErrorMessages.EW0101);
            }

            searchKey = (searchKey ?? "").Trim();

            if (listInterruptReason.Count > 0 && searchKey.Length > 0)
            {
                // 検索キーワードが存在する場合
                interruptReasonModels = listInterruptReason
                    .Where(x => x.InterruptReasonCode.ToString().Contains(searchKey) || x.InterruptReasonName.Contains(searchKey) || x.Remark.Contains(searchKey))
                    .ToList();
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
