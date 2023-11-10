using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlKata.Execution;
using System.Data;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MTaskUserController : BaseController
    {
        private IWebHostEnvironment _environment;
        private static string DisplayName = "MTaskUser";

        public MTaskUserController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(MTaskUserViewModel viewModel, Enums.GetState command = Enums.GetState.Default)
        {
            try
            {
                var taskUserViewModel = await GetListMTaskUserModel(viewModel.SearchKeyWord);
                switch (command)
                {
                    //検索処理
                    case Enums.GetState.Default:
                    case Enums.GetState.Search:
                        {
                            var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);
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
                            var fileName = viewModel.DisplayName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
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
                var taskUserViewModel = await GetListMTaskUserModel(viewModel.SearchKeyWord, false);
                var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);
                // page the list
                viewModel.TaskUserModelModels = listPaged;

                // 妥当性チェック
                totalErrorList = ValidateCheck(viewModel.File);
                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }
                await ExcelFile<bool>.SaveFileImportAndDelete(viewModel.File, _environment.WebRootPath, DisplayName);

                var dataTable = await ExcelFile<MTaskUserModel>.ReadExcelToDataTable(viewModel.File);
                // ファイルのフォーマットをチェック
                var isFormat = FileFormatCheck(dataTable);
                if (!isFormat)
                {
                    totalErrorList.Add(ErrorMessages.EW1202);
                    if (totalErrorList.Any())
                    {
                        TempData["ErrorMessage"] = totalErrorList;
                        return RedirectToAction("Index", redirectParam);
                    }
                }
                var listErrDataCheck = new Dictionary<string, string>();
                dataTable = ExcelFile<MTaskUserModel>.ToWithFormat(dataTable);
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var model = new MTaskUserModel();
                    var rowErrorList = new List<string>();
                    var modifyFlag = Convert.ToString(dataTable.Rows[i]["ModifiedFlag"]);
                    modifyFlag = (modifyFlag ?? string.Empty).Trim();
                    dataTable.Rows[i]["ModifiedFlag"] = modifyFlag;
                    if (!string.IsNullOrWhiteSpace(modifyFlag) && !modifyFlag.Equals("1") && !modifyFlag.Equals("2"))
                        rowErrorList.Add(string.Format(ErrorMessages.EW1201, "登録修正フラグ"));
                    if (!modifyFlag.Equals("1") && !modifyFlag.Equals("2"))
                        goto STEP;

                    var taskUserId = Convert.ToString(dataTable.Rows[i]["TaskUserId"]);
                    var taskUserLoginId = Convert.ToString(dataTable.Rows[i]["TaskUserLoginId"]);
                    var resultCheck = await CheckTaskUserIdAndTaskUserLoginId(modifyFlag, taskUserId, taskUserLoginId);
                    if (resultCheck.Any())
                        rowErrorList.AddRange(resultCheck);

                    var taskUserName = Convert.ToString(dataTable.Rows[i]["TaskUserName"]);
                    if (string.IsNullOrWhiteSpace(taskUserName))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業者名"));
                    else if (taskUserName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業者名", "10"));

                    var taskUserNameKana = Convert.ToString(dataTable.Rows[i]["TaskUserNameKana"]);
                    if (string.IsNullOrWhiteSpace(taskUserNameKana))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業者名かな"));
                    else if (taskUserNameKana.Length > 50)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業者名かな", "50"));

                    var taskUserDepartmentName = Convert.ToString(dataTable.Rows[i]["TaskUserDepartmentName"]);
                    if (string.IsNullOrWhiteSpace(taskUserDepartmentName))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "所属名"));
                    else if (taskUserDepartmentName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "所属名", "10"));

                    var taskUserGroupName = Convert.ToString(dataTable.Rows[i]["TaskUserGroupName"]);
                    if (taskUserGroupName != null && taskUserGroupName.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "グループ名", "10"));

                    var remark = Convert.ToString(dataTable.Rows[i]["Remark"]);
                    if (remark != null && remark.Length > 200)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "備考", "200"));

                    var isNotUse = Convert.ToString(dataTable.Rows[i]["IsNotUse"]);
                    if (string.IsNullOrWhiteSpace(isNotUse))
                        isNotUse = "0";
                    else
                        isNotUse = isNotUse.Trim();
                    dataTable.Rows[i]["IsNotUse"] = isNotUse;
                    if (!string.IsNullOrWhiteSpace(isNotUse) && !isNotUse.Equals("0") && !isNotUse.Equals("1"))
                        rowErrorList.Add(string.Format(ErrorMessages.EW1206, "利用停止フラグ", "0", "1"));

                    STEP:
                    if (rowErrorList.Count > 0)
                        listErrDataCheck.Add($"{i + 2}行目", JsonConvert.SerializeObject(rowErrorList));
                }

                if (listErrDataCheck.Any())
                {
                    TempData["DictErrorMessage"] = listErrDataCheck;
                    return RedirectToAction("Index", redirectParam);
                }

                var queryInsert =
                     from row in dataTable.AsEnumerable()
                     where row.Field<string>("ModifiedFlag").Equals("1")
                     select row;
                DataTable insertDt;
                if (queryInsert.Any())
                    insertDt = queryInsert.CopyToDataTable();
                else
                    insertDt = dataTable.Clone();

                var queryModify =
                     from row in dataTable.AsEnumerable()
                     where row.Field<string>("ModifiedFlag").Equals("2")
                     select row;
                DataTable modifyDt;
                if (queryModify.Any())
                    modifyDt = queryModify.CopyToDataTable();
                else
                    modifyDt = dataTable.Clone();

                var insertData = insertDt.ToList<MTaskUserModel>();
                var modifyData = modifyDt.ToList<MTaskUserModel>();

                var result = CheckUnique(insertData, modifyData);
                if (result.Any())
                    totalErrorList.AddRange(result);

                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }

                var efftedRows = await SaveChangeDataAsync(insertData, modifyData);
                if (efftedRows > 0)
                {
                    TempData["SuccessMessage"] = SuccessMessages.SW002;
                }
                else
                {
                    totalErrorList.Add(ErrorMessages.EW1207);
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

        private List<string> CheckUnique(List<MTaskUserModel> insertData, List<MTaskUserModel> modifyData)
        {
            var localErr = new List<string>();
            var anyDuplicateInsert = insertData.GroupBy(x => x.TaskUserLoginId).Any(g => g.Count() > 1);
            var anyDuplicateModify = modifyData.GroupBy(x => x.TaskUserLoginId).Any(g => g.Count() > 1);
            if (anyDuplicateModify)
                localErr.Add(string.Format(ErrorMessages.EW1208, "作業者ログインID"));
            if (anyDuplicateInsert)
                localErr.Add(string.Format(ErrorMessages.EW1209, "作業者ログインID"));

            var results = from p in insertData
                          join c in modifyData
                          on p.TaskUserLoginId.Trim() equals c.TaskUserLoginId.Trim()
                          select new { insertData };
            if (results.Any())
                localErr.Add(string.Format(ErrorMessages.EW1210, "作業者ログインID"));

            return localErr;
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

        private async Task<List<string>> CheckTaskUserIdAndTaskUserLoginId(string flag, string taskUserId, string taskUserLoginId)
        {
            var errorList = new List<string>();
            if (!(string.IsNullOrWhiteSpace(flag) || flag.Equals("1") || flag.Equals("2")))
                return errorList;
           
            // 新規登録チェック
            if (flag.Equals("1"))
            {
                if (string.IsNullOrWhiteSpace(taskUserLoginId))
                {
                    errorList.Add(string.Format(ErrorMessages.EW0001, "作業者ログインID"));
                    return errorList;
                }

                if (taskUserLoginId.Length > 8)
                    errorList.Add(string.Format(ErrorMessages.EW0002, "作業者ログインID", "8"));

                if (errorList.Any())
                    return errorList;

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = (await db.Query("MTaskUser")
                        .WhereIn("TaskUserLoginId", taskUserLoginId)
                        .GetAsync<MTaskUserModel>())
                        .FirstOrDefault();
                    if (result != null)
                        errorList.Add(string.Format(ErrorMessages.EW1204, "作業者ログインID"));
                }
            }
            // 更新チェック
            else if (flag.Equals("2"))
            {
                if (string.IsNullOrWhiteSpace(taskUserId))
                {
                    errorList.Add(string.Format(ErrorMessages.EW0001, "作業者ID"));
                    return errorList;
                }
                if (string.IsNullOrWhiteSpace(taskUserLoginId))
                {
                    errorList.Add(string.Format(ErrorMessages.EW0001, "作業者ログインID"));
                    return errorList;
                }

                if (taskUserId.Length > 8)
                    errorList.Add(string.Format(ErrorMessages.EW0002, "作業者ID", "8"));
                else
                {
                    if (!int.TryParse(taskUserId, out int val))
                        errorList.Add(string.Format(ErrorMessages.EW0009, "作業者ID"));
                }

                if (taskUserLoginId.Length > 8)
                    errorList.Add(string.Format(ErrorMessages.EW0002, "作業者ログインID", "8"));

                if (errorList.Any())
                    return errorList;

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = (await db.Query("MTaskUser")
                    .Where(new
                    {
                        TaskUserId = taskUserId,
                        TaskUserLoginId = taskUserLoginId
                    })
                    .GetAsync<MTaskUserModel>())
                    .FirstOrDefault();
                    if (result == null)
                        errorList.Add(string.Format(ErrorMessages.EW1205, "作業者及び作業者ログインID"));
                }
            }

            return errorList;
        }


        private async Task<int> SaveChangeDataAsync(List<MTaskUserModel> insertData, List<MTaskUserModel> modifyData)
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
                        var result = await db.Query("MTaskUser")
                            .InsertAsync(new
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
                        var result = await db.Query("MTaskUser")
                            .Where(new
                            {
                                TaskUserId = data.TaskUserId,
                                TaskUserLoginId = data.TaskUserLoginId
                            })
                            .UpdateAsync(new
                            {
                                TaskUserName = data.TaskUserName,
                                TaskUserNameKana = data.TaskUserNameKana,
                                TaskUserDepartmentName = data.TaskUserDepartmentName,
                                TaskUserGroupName = data.TaskUserGroupName,
                                Remark = data.Remark,
                                IsNotUse = data.IsNotUse,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId,
                            }, tran);
                        if (result > 0)
                            efftedRows = efftedRows + result;
                    }

                    db.Commit();
                }
                catch (Exception)
                {
                    db.Rollback();
                    throw;
                }

                return efftedRows;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="command">search: true, 以外：false</param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private async Task<List<MTaskUserModel>> GetListMTaskUserModel(string searchKey, bool command = true)
        {
            var listMTaskUserModel = new List<MTaskUserModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                listMTaskUserModel = (await db.Query("MTaskUser as a")
                .Select(
                        "a.TaskUserId",
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
                .GetAsync<MTaskUserModel>())
                .ToList();
            }

            if (listMTaskUserModel.Count == 0 && command)
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
                if (listMTaskUserModel.Count == 0 && command)
                {
                    throw new CustomExtention(ErrorMessages.EW0102);
                }
            }

            return listMTaskUserModel;
        }


    }
}
