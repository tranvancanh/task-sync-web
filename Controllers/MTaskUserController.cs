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
        public IActionResult Index(MTaskUserViewModel viewModel, Enums.GetState command = Enums.GetState.Default)
        {
            try
            {
                var taskUserViewModel = GetListMTaskUserModel(viewModel.SearchKeyWord);
                switch (command)
                {
                    //検索処理
                    case Enums.GetState.Default:
                    case Enums.GetState.Search:
                        {
                            var listPaged = taskUserViewModel.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                            // page the list
                            viewModel.TaskUserModelModels = listPaged;
                            return View(viewModel);
                        }
                    case Enums.GetState.ExcelOutput:
                        {
                            var excelHeaderStyle = new ExcelHeaderStyleModel();
                            excelHeaderStyle.FirstColorBackgroundColorColumnNumber = new int[1] { 1 };
                            excelHeaderStyle.SecondColorBackgroundColorColumnNumber = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
                            var memoryStream = ExcelFile<MTaskUserModel>.ExcelCreate(taskUserViewModel, true, 1, 1, excelHeaderStyle);
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
                    ViewData["ErrorMessage"] = ErrorMessages.EW9000;
                return View("Index", viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Import(MTaskUserViewModel viewModel)
        {
            var totalErrorList = new List<string>();
            var redirectParam = new
            {
                SearchKeyWord = viewModel.SearchKeyWord,
                command = Enums.GetState.Search,
                PageNumber = viewModel.PageNumber,
                IsState = Enums.CollapseState.Show
            };
            try
            {
                var taskUserViewModel = GetListMTaskUserModel(viewModel.SearchKeyWord);
                var listPaged = taskUserViewModel.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                // page the list
                viewModel.TaskUserModelModels = listPaged;

                // 妥当性チェック
                totalErrorList = ValidateCheck(viewModel.File);
                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }
                var dataTable = await ExcelFile<MTaskUserModel>.ReadExcelToDataTable(viewModel.File);

                // ファイルのフォーマットをチェック
                var isFormat = FileFormatCheck(dataTable);
                if (!isFormat)
                {
                    totalErrorList.Add(ErrorMessages.EW1207);
                    if (totalErrorList.Any())
                    {
                        TempData["ErrorMessage"] = totalErrorList;
                        return RedirectToAction("Index", redirectParam);
                    }
                }

                dataTable = ExcelFile<MTaskUserModel>.ToWithFormat(dataTable);
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var model = new MTaskUserModel();
                    var rowErrorList = new List<string>();
                    var modifyFlag = Convert.ToString(dataTable.Rows[i]["ModifiedFlag"]);

                    var taskUserLoginId = Convert.ToString(dataTable.Rows[i]["TaskUserLoginId"]);
                    if (taskUserLoginId != null && taskUserLoginId.Length > 8)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業者ログインID", "8"));

                    if (!string.IsNullOrWhiteSpace(CheckTaskUserLoginId(modifyFlag, taskUserLoginId)))
                    {
                        rowErrorList.Add(CheckTaskUserLoginId(modifyFlag, taskUserLoginId));
                    }

                    var taskUserName = Convert.ToString(dataTable.Rows[i]["TaskUserName"]);
                    if (taskUserName != null && taskUserName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業者名", "10"));

                    var taskUserNameKana = Convert.ToString(dataTable.Rows[i]["TaskUserNameKana"]);
                    if (taskUserNameKana != null && taskUserNameKana.Length > 50)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業者名かな", "50"));

                    var taskUserDepartmentName = Convert.ToString(dataTable.Rows[i]["TaskUserDepartmentName"]);
                    if (taskUserDepartmentName != null && taskUserDepartmentName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "所属名", "10"));

                    var taskUserGroupName = Convert.ToString(dataTable.Rows[i]["TaskUserGroupName"]);
                    if (taskUserGroupName != null && taskUserGroupName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "グループ名", "10"));

                    var remark = Convert.ToString(dataTable.Rows[i]["Remark"]);
                    if (remark != null && remark.Length > 200)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "備考", "200"));

                    var isNotUse = Convert.ToString(dataTable.Rows[i]["IsNotUse"]);
                    if (string.IsNullOrWhiteSpace(isNotUse) || (!isNotUse.Equals("0") && !isNotUse.Equals("1")))
                        rowErrorList.Add(string.Format(ErrorMessages.EW1206, "利用停止フラグ", "0", "1"));

                    if (rowErrorList.Count > 0)
                        totalErrorList.Add($"{i + 1}行目 : " + string.Join(" ", rowErrorList));
                }

                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }

                var listData = dataTable.ToList<MTaskUserModel>();
                var insertData = listData.Where(x => x.ModifiedFlag.ToString().Trim().Equals("1")).ToList();
                var modifyData = listData.Where(x => x.ModifiedFlag.ToString().Trim().Equals("2")).ToList();

                var efftedRows = SaveChangeData(insertData, modifyData);
                if (efftedRows > 0)
                {
                    TempData["SuccessMessage"] = SuccessMessages.SW002;
                }
                else
                {
                    totalErrorList.Add(ErrorMessages.EW0502);
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomExtention)
                    totalErrorList.Add(ex.Message);
                else
                    totalErrorList.Add(ex.Message);

                TempData["ErrorMessage"] = totalErrorList;
                return RedirectToAction("Index", redirectParam);
            }

            return RedirectToAction("Index", redirectParam);
        }

        private List<string> ValidateCheck(IFormFile file)
        {
            var errorList = new List<string>();
            if (file == null || file.Length == 0)
                errorList.Add(ErrorMessages.EW1203);
            else
            {
                // Get file
                var fileInfor = new FileInfo(file.FileName);
                var fileExtension = fileInfor.Extension;

                // Check if file is an Excel File
                if (!fileExtension.ToLower().EndsWith(".xlsx"))
                    errorList.Add(ErrorMessages.EW1202);
            }

            return errorList;
        }

        private bool FileFormatCheck(DataTable dataTable)
        {
            try
            {
                var columnNames = dataTable.Rows[0].ItemArray.ToList();
                var properties = Utils.GetModelProperties<MTaskUserModel>();
                for (var i = 0; i < properties.Count; i++)
                {
                    var propertie = properties[i];
                    if (propertie.DisplayName == Convert.ToString(columnNames[i]))
                        continue;
                     else
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
             return true;
        }

        private string CheckTaskUserLoginId(string flag, string taskUserLoginId)
        {
            flag = (flag ?? "").Trim();
            // 新規登録チェック
            if (flag.Equals("1"))
            {
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = db.Query("MTaskUser")
                        .WhereIn("TaskUserLoginId", taskUserLoginId)
                        .Get<MTaskUserModel>()
                        .FirstOrDefault();
                    if (result != null)
                        return string.Format(ErrorMessages.EW1204, "作業者ログインID");
                }
            }
            // 更新チェック
            else if (flag.Equals("2"))
            {
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = db.Query("MTaskUser")
                        .WhereIn("TaskUserLoginId", taskUserLoginId)
                        .Get<MTaskUserModel>()
                        .FirstOrDefault();
                    if (result == null)
                        return string.Format(ErrorMessages.EW1205, "作業者ログインID");
                }
            }

            return string.Empty;
        }

        private int SaveChangeData(List<MTaskUserModel> insertData, List<MTaskUserModel> modifyData)
        {
            var efftedRows = -1;
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                var tran = db.Begin();
                try
                {
                    efftedRows = 0;
                    // 新規登録処理
                    foreach (var data in insertData)
                    {
                        var result = db.Query("MTaskUser")
                            .InsertGetId<int>(new
                            {
                                TaskUserLoginId = data.TaskUserLoginId,
                                TaskUserName = data.TaskUserName,
                                TaskUserNameKana = data.TaskUserNameKana,
                                TaskUserDepartmentName = data.TaskUserDepartmentName,
                                TaskUserGroupName = data.TaskUserGroupName,
                                Remark = data.Remark,
                                IsNotUse = data.IsNotUse,
                                CreateDateTime = DateTime.Now,
                                CreateAdministratorId = LoginUser.AdministratorId,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId,
                            }, tran);
                        if (result > 0)
                            efftedRows = efftedRows + result;
                    }

                    // 更新処理
                    foreach (var data in modifyData)
                    {
                        var result = db.Query("MTaskUser")
                            .Where("TaskUserLoginId", data.TaskUserLoginId)
                            .Update(new
                            {
                                TaskUserLoginId = data.TaskUserLoginId,
                                TaskUserName = data.TaskUserName,
                                TaskUserNameKana = data.TaskUserNameKana,
                                TaskUserDepartmentName = data.TaskUserDepartmentName,
                                TaskUserGroupName = data.TaskUserGroupName,
                                Remark = data.Remark,
                                IsNotUse = data.IsNotUse,
                                CreateDateTime = DateTime.Now,
                                CreateAdministratorId = LoginUser.AdministratorId,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId,
                            }, tran);
                        if (result > 0)
                            efftedRows = efftedRows + result;
                    }

                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }

                return efftedRows;
            }
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
