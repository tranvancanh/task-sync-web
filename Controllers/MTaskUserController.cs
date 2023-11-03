using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Data;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MTaskUserController : BaseController
    {
        [HttpGet]
        public IActionResult Index(MTaskUserViewModel viewModel, string command = null)
        {
            try
            {
                var taskUserViewModel = GetListMTaskUserModel(viewModel.SearchKeyWord);
                switch (command)
                {
                    //検索処理
                    case null:
                    case "Search":
                        {
                            var listPaged = taskUserViewModel.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                            // page the list
                            viewModel.TaskUserModelModels = listPaged;

                            return View(viewModel);
                        }
                    case "ExcelOutput":
                        {
                            var memoryStream = ExcelFile<MTaskUserModel>.ExcelCreate(taskUserViewModel, true);
                            // ファイル名
                            var fileName = viewModel.DisplayName + DateTime.Now.ToString("yyyyMMddHHmmss");
                            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
                        }
                    default:
                        {
                            return View("_NotFound");
                        }
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomExtention)
                    ViewData["ErrorMessage"] = ex.Message;
                else
                    ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View("Index", viewModel);
            }
        }

        [HttpPost]
        public IActionResult Index(MTaskUserViewModel viewModel)
        {

            return View(viewModel);
        }

        private List<MTaskUserModel> GetListMTaskUserModel(string searchKey)
        {
            var listMTaskUserModel = new List<MTaskUserModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                 listMTaskUserModel = db.Query("MTaskUser as a")
                 .Select(
                         "a.TaskUserLoginId",
                         "a.TaskUserName",
                         "a.TaskUserNameKana",
                         "a.TaskUserDepartmentName",
                         "a.TaskUserGroupName",
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
                 .OrderBy("a.TaskUserLoginId")
                 .Get<MTaskUserModel>()
                 .ToList(); ;
            }

            if (listMTaskUserModel.Count == 0)
            {
                throw new CustomExtention(ErrorMessages.EW0101);
            }

            searchKey = (searchKey ?? string.Empty).Trim();


            if (listMTaskUserModel.Count > 0 && searchKey.Length > 0)
            {
                // 検索キーワードが存在する場合
                listMTaskUserModel = listMTaskUserModel.Where(
                    x => x.TaskUserLoginId.ToString().Contains(searchKey) 
                    || x.TaskUserName.Contains(searchKey)
                    || x.TaskUserNameKana.Contains(searchKey)
                    || x.TaskUserDepartmentName.Contains(searchKey)
                    || x.TaskUserGroupName.Contains(searchKey)
                    || x.Remark.Contains(searchKey)
                    ).ToList();
                if (listMTaskUserModel.Count == 0)
                {
                    throw new CustomExtention(ErrorMessages.EW0102);
                }
            }
            
            return listMTaskUserModel;
        }


    }
}
