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
                //検索処理
                if (viewModel.IsModalStatus == null)
                {
                    if (!listInterruptReason.Any())
                    {
                        ViewData["ErrorMessage"] = ErrorMessages.EW0102;
                        return View(viewModel);
                    }
                }
                else if(viewModel.IsModalStatus == true)
                {
                    //新規登録処理
                    ModelState.Clear();
                    viewModel.ModalModel = new MInterruptReasonModel();
                }
                else
                {
                    //更新処理
                    ModelState.Clear();
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
                    ViewData["ErrorMessage"] = ErrorMessages.EW500;
                    return View(viewModel);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(MInterruptReasonViewModel viewModel, bool? isModalStatus)
        {
            ViewData["SuccessMessage"] = null;
            ViewData["ErrorMessageModal"] = null;

            try
            {
                if (string.IsNullOrWhiteSpace(viewModel.ModalModel.InterruptReasonCode))
                    viewModel.ModalModel.InterruptReasonCode = string.Empty;

                if (string.IsNullOrWhiteSpace(viewModel.ModalModel.InterruptReasonName))
                    viewModel.ModalModel.InterruptReasonName = string.Empty;

                if (string.IsNullOrWhiteSpace(viewModel.ModalModel.Remark))
                    viewModel.ModalModel.Remark = string.Empty;

                var efftedRows = -1;
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    if (isModalStatus == true)
                    {
                        //新規登録処理
                        efftedRows = db.Query("MInterruptReason").Insert(
                                    new[] { "InterruptReasonCode", "InterruptReasonName", "Remark", "IsNotUse", "CreateDateTime", "CreateAdministratorId", "UpdateDateTime", "UpdateAdministratorId" },
                                    new[] 
                                    {
                                        new object[] { viewModel.ModalModel.InterruptReasonCode, viewModel.ModalModel.InterruptReasonName, viewModel.ModalModel.Remark, viewModel.ModalModel.IsNotUse, DateTime.Now.Date, LoginUser.AdministratorId, "1900/01/01", LoginUser.AdministratorId }
                                    });
                    }
                    else if (isModalStatus == false)
                    {
                        //更新処理
                        efftedRows = db.Query("MInterruptReason").Where("InterruptReasonId", viewModel.ModalModel.InterruptReasonId).Update(new
                        {
                            InterruptReasonCode = viewModel.ModalModel.InterruptReasonCode,
                            InterruptReasonName = viewModel.ModalModel.InterruptReasonName,
                            Remark = viewModel.ModalModel.Remark,
                            IsNotUse = viewModel.ModalModel.IsNotUse,
                            UpdateDateTime = DateTime.Now.Date,
                            UpdateAdministratorId = LoginUser.AdministratorId
                        });
                    }
                }

                if (efftedRows > 0)
                {
                    viewModel.IsModalStatus = null;
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
                    ViewData["ErrorMessageModal"] = ErrorMessages.EW500;
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
