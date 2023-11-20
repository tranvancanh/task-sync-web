using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlKata.Execution;
using System.Data;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MTaskItemController : BaseController
    {
        private IWebHostEnvironment _environment;
        private static string DisplayName = "MTaskItem";

        public MTaskItemController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(MTaskItemViewModel viewModel, Enums.GetState command = Enums.GetState.Default)
        {
            try
            {
                var taskUserViewModel = await GetListTaskItemModel(viewModel.SearchKeyWord);
                switch (command)
                {
                    //検索処理
                    case Enums.GetState.Default:
                    case Enums.GetState.Search:
                        {
                            var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);
                            // page the list
                            viewModel.TaskItemModels = listPaged;
                            return View(viewModel);
                        }
                    case Enums.GetState.ExcelOutput:
                        {
                            var excelHeaderStyle = new ExcelHeaderStyleModel();
                            excelHeaderStyle.FirstColorBackgroundColorColumnNumber = new int[1] { 1 };
                            excelHeaderStyle.SecondColorBackgroundColorColumnNumber = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
                            var memoryStream = ExcelFile<MTaskItemModel>.ExcelCreate(taskUserViewModel, true, 1, 1, excelHeaderStyle);
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
        public async Task<IActionResult> Import(MTaskItemViewModel viewModel)
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
                var taskUserViewModel = await GetListTaskItemModel(viewModel.SearchKeyWord, false);
                var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);
                // page the list
                viewModel.TaskItemModels = listPaged;

                // 妥当性チェック
                totalErrorList = ValidateCheck(viewModel.File);
                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }
                await ExcelFile<bool>.SaveFileImportAndDelete(viewModel.File, _environment.WebRootPath, DisplayName);

                var dataTable = await ExcelFile<MTaskItemModel>.ReadExcelToDataTable(viewModel.File);
                // ファイルのフォーマットをチェック
                var isFormat = FileFormatCheck(dataTable);
                if (!isFormat)
                {
                    totalErrorList.Add(ErrorMessages.EW1212);
                    if (totalErrorList.Any())
                    {
                        TempData["ErrorMessage"] = totalErrorList;
                        return RedirectToAction("Index", redirectParam);
                    }
                }

                var listErrDataCheck = new Dictionary<string, string>();
                dataTable = ExcelFile<MTaskItemModel>.ToWithFormat(dataTable);
                Utils.WhitespaceNotTake(dataTable);
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var model = new MTaskItemModel();
                    var rowErrorList = new List<string>();
                    var modifyFlag = Convert.ToString(dataTable.Rows[i]["ModifiedFlag"]);
                    modifyFlag = (modifyFlag ?? string.Empty).Trim();
                    dataTable.Rows[i]["ModifiedFlag"] = modifyFlag;
                    if (!string.IsNullOrWhiteSpace(modifyFlag) && !modifyFlag.Equals("1") && !modifyFlag.Equals("2"))
                        rowErrorList.Add(string.Format(ErrorMessages.EW1201, "登録修正フラグ"));
                    if (!modifyFlag.Equals("1") && !modifyFlag.Equals("2"))
                        goto STEP;

                    var taskItemId = Convert.ToString(dataTable.Rows[i]["TaskItemId"]);
                    var messCheckItemId = CheckTaskItemId(modifyFlag, ref taskItemId);
                    if (!string.IsNullOrWhiteSpace(messCheckItemId))
                        rowErrorList.Add(messCheckItemId);
                    else
                        dataTable.Rows[i]["TaskItemId"] = taskItemId;

                    var taskItemCode = Convert.ToString(dataTable.Rows[i]["TaskItemCode"]);
                    var messCheckItemCode = CheckTaskItemCode(taskItemCode);
                    if (!string.IsNullOrWhiteSpace(messCheckItemCode))
                        rowErrorList.Add(messCheckItemCode);

                    var taskPrimaryItem = Convert.ToString(dataTable.Rows[i]["TaskPrimaryItem"]);
                    if (string.IsNullOrWhiteSpace(taskPrimaryItem))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業大項目"));
                    else if (taskPrimaryItem.Length > 15)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業大項目", "15"));

                    var taskSecondaryItem = Convert.ToString(dataTable.Rows[i]["TaskSecondaryItem"]);
                    if (string.IsNullOrWhiteSpace(taskSecondaryItem))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業中項目"));
                    else if (taskSecondaryItem.Length > 30)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業中項目", "30"));

                    var taskTertiaryItem = Convert.ToString(dataTable.Rows[i]["TaskTertiaryItem"]);
                    if (string.IsNullOrWhiteSpace(taskTertiaryItem))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業小項目"));
                    else if (taskTertiaryItem.Length > 30)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業小項目", "30"));

                    var taskItemCategory = Convert.ToString(dataTable.Rows[i]["TaskItemCategory"]);
                    if (string.IsNullOrWhiteSpace(taskItemCategory))
                        rowErrorList.Add(string.Format(ErrorMessages.EW0001, "作業項目分類"));
                    else if (taskItemCategory.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業項目分類", "10"));

                    var remark = Convert.ToString(dataTable.Rows[i]["Remark"]);
                    if (remark != null && remark.Length > 200)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "備考", "200"));

                    var isNotUse = Convert.ToString(dataTable.Rows[i]["IsNotUse"]);
                    isNotUse = (string.IsNullOrWhiteSpace(isNotUse) ? "0" : isNotUse).Trim();
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

                var insertData = insertDt.ToList<MTaskItemModel>();
                var modifyData = modifyDt.ToList<MTaskItemModel>();

                var resultCheck = CheckUnique(insertData, modifyData);
                if (resultCheck.Any())
                    totalErrorList.AddRange(resultCheck);

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

        private List<string> CheckUnique(List<MTaskItemModel> insertData, List<MTaskItemModel> modifyData)
        {
            var localErr = new List<string>();
            var anyDuplicateItemId = modifyData.GroupBy(x => x.TaskItemId).Any(g => g.Count() > 1);
            if(anyDuplicateItemId)
                localErr.Add(string.Format(ErrorMessages.EW1208, "作業項目ID"));

            //var anyDuplicateInsert = insertData.GroupBy(x => x.TaskItemCode).Any(g => g.Count() > 1);
            //var anyDuplicateModify = modifyData.GroupBy(x => x.TaskItemCode).Any(g => g.Count() > 1);
            //if (anyDuplicateModify)
            //    localErr.Add(string.Format(ErrorMessages.EW1208, "作業項目コード"));
            //if (anyDuplicateInsert)
            //    localErr.Add(string.Format(ErrorMessages.EW1209, "作業項目コード"));

            //var results = from p in insertData
            //              join c in modifyData
            //              on p.TaskItemCode equals c.TaskItemCode
            //              select new { insertData };
            //if (results.Any())
            //    localErr.Add(string.Format(ErrorMessages.EW1210, "作業項目コード"));

            return localErr;
        }

        private bool FileFormatCheck(DataTable dataTable)
        {
            try
            {
                var columnNames = dataTable.Rows[0].ItemArray.ToList();
                var properties = Utils.GetModelProperties<MTaskItemModel>();
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

        private string CheckTaskItemId(string flag, ref string taskItemId)
        {
            taskItemId = (taskItemId ?? string.Empty).Trim();
            // 新規登録チェック
            if (flag.Equals("1"))
            {
                // 登録対象行の作業項目IDが空白であるか
                if (!string.IsNullOrWhiteSpace(taskItemId))
                    return string.Format(ErrorMessages.EW1211, "作業項目ID");
            }
            // 更新チェック
            else if (flag.Equals("2"))
            {
                if(string.IsNullOrWhiteSpace(taskItemId))
                    return string.Format(ErrorMessages.EW0001, "作業項目ID");
                //else if (!int.TryParse(taskItemId, out int val))
                //    return string.Format(ErrorMessages.EW0009, "作業項目ID");

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = db.Query("MTaskItem")
                        .WhereIn("TaskItemId", taskItemId)
                        .Get<MTaskItemModel>()
                        .FirstOrDefault();
                    if (result == null)
                        return string.Format(ErrorMessages.EW1205, "作業項目ID");
                }
            }

            return string.Empty;
        }

        private string CheckTaskItemCode(string taskItemCode)
        {
            if (string.IsNullOrWhiteSpace(taskItemCode))
                return string.Format(ErrorMessages.EW0001, "作業項目コード");
            if (!int.TryParse(taskItemCode, out int val))
                return string.Format(ErrorMessages.EW0009, "作業項目コード");
            else
            {
                if(taskItemCode.Length > 8)
                    return string.Format(ErrorMessages.EW0002, "作業項目コード", "8");
            }
            return string.Empty;
        }

        private async Task<int> SaveChangeDataAsync(List<MTaskItemModel> insertData, List<MTaskItemModel> modifyData)
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
                        var result = await db.Query("MTaskItem")
                            .InsertAsync(new
                            {
                                TaskItemCode = data.TaskItemCode,
                                TaskItemCategory = data.TaskItemCategory,
                                TaskPrimaryItem = data.TaskPrimaryItem,
                                TaskSecondaryItem = data.TaskSecondaryItem,
                                TaskTertiaryItem = data.TaskTertiaryItem,
                                Remark = data.Remark,
                                IsNotUse = data.IsNotUse,
                                CreateDateTime = DateTime.Now,
                                CreateAdministratorId = LoginUser.AdministratorId,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId
                            }, tran);
                        if (result > 0)
                            efftedRows = efftedRows + result;
                    }

                    // 更新処理
                    foreach (var data in modifyData)
                    {
                        var result = await db.Query("MTaskItem")
                            .Where("TaskItemId", data.TaskItemId)
                            .UpdateAsync(new
                            {
                                TaskItemCode = data.TaskItemCode,
                                TaskItemCategory = data.TaskItemCategory,
                                TaskPrimaryItem = data.TaskPrimaryItem,
                                TaskSecondaryItem = data.TaskSecondaryItem,
                                TaskTertiaryItem = data.TaskTertiaryItem,
                                Remark = data.Remark,
                                IsNotUse = data.IsNotUse,
                                UpdateDateTime = DateTime.Now,
                                UpdateAdministratorId = LoginUser.AdministratorId
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
            }
            return efftedRows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="command">search: true, 以外：false</param>
        /// <returns></returns>
        /// <exception cref="CustomExtention"></exception>
        private async Task<List<MTaskItemModel>> GetListTaskItemModel(string searchKey, bool command = true)
        {
            var listMTaskItemModel = new List<MTaskItemModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                listMTaskItemModel = (await db.Query("MTaskItem as a")
                .Select(
                        "a.TaskItemId",
                        "a.TaskItemCode",
                        "a.TaskItemCategory",
                        "a.TaskPrimaryItem",
                        "a.TaskSecondaryItem",
                        "a.TaskTertiaryItem",
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
                .OrderBy("a.TaskItemCode")
                .GetAsync<MTaskItemModel>())
                .ToList();
            }

            if (listMTaskItemModel.Count == 0 && command)
            {
                throw new CustomExtention(ErrorMessages.EW0101);
            }

            searchKey = (searchKey ?? string.Empty).Trim();


            if (listMTaskItemModel.Count > 0 && searchKey.Length > 0)
            {
                // 検索キーワードが存在する場合
                listMTaskItemModel = listMTaskItemModel.Where(
                    x => x.TaskItemCode.ToString().Contains(searchKey)
                    || x.TaskItemCategory.Contains(searchKey)
                    || x.TaskPrimaryItem.Contains(searchKey)
                    || x.TaskSecondaryItem.Contains(searchKey)
                    || x.TaskTertiaryItem.Contains(searchKey)
                    || x.Remark.Contains(searchKey)
                    ).ToList();
                if (listMTaskItemModel.Count == 0 && command)
                {
                    throw new CustomExtention(ErrorMessages.EW0102);
                }
            }

            return listMTaskItemModel;
        }
    }
}
