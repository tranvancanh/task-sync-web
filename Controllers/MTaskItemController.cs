using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Data;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MTaskItemController : BaseController
    {
        [HttpGet]
        public IActionResult Index(MTaskItemViewModel viewModel, Enums.GetState command = Enums.GetState.Default)
        {
            try
            {
                var taskUserViewModel = GetListTaskItemModel(viewModel.SearchKeyWord);
                switch (command)
                {
                    //検索処理
                    case Enums.GetState.Default:
                    case Enums.GetState.Search:
                        {
                            var listPaged = taskUserViewModel.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                            // page the list
                            viewModel.TaskItemModels = listPaged;
                            return View(viewModel);
                        }
                    case Enums.GetState.ExcelOutput:
                        {
                            var excelHeaderStyle = new ExcelHeaderStyleModel();
                            excelHeaderStyle.FirstColorBackgroundColorColumnNumber = new int[1] { 1 };
                            excelHeaderStyle.SecondColorBackgroundColorColumnNumber = new int[8] { 2, 3, 4, 5, 6, 7, 8, 9 };
                            var memoryStream = ExcelFile<MTaskItemModel>.ExcelCreate(taskUserViewModel, true, 1, 1, excelHeaderStyle);
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
                var taskUserViewModel = GetListTaskItemModel(viewModel.SearchKeyWord);
                var listPaged = taskUserViewModel.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                // page the list
                viewModel.TaskItemModels = listPaged;

                // 妥当性チェック
                totalErrorList = ValidateCheck(viewModel.File);
                if (totalErrorList.Any())
                {
                    TempData["ErrorMessage"] = totalErrorList;
                    return RedirectToAction("Index", redirectParam);
                }
                var dataTable = await ExcelFile<MTaskItemModel>.ReadExcelToDataTable(viewModel.File);

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

                dataTable = ExcelFile<MTaskItemModel>.ToWithFormat(dataTable);
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    var model = new MTaskItemModel();
                    var rowErrorList = new List<string>();
                    var modifyFlag = Convert.ToString(dataTable.Rows[i]["ModifiedFlag"]);

                    var taskItemId = Convert.ToString(dataTable.Rows[i]["TaskItemId"]);
                    var messCheckItemId = CheckTaskItemId(modifyFlag, taskItemId);
                    if (string.IsNullOrWhiteSpace(messCheckItemId))
                    {
                        rowErrorList.Add(messCheckItemId);
                    }

                    var taskItemCode = Convert.ToString(dataTable.Rows[i]["TaskItemCode"]);
                    var messCheckItemCode = CheckTaskItemCode(modifyFlag, taskItemCode);
                    if (!string.IsNullOrWhiteSpace(messCheckItemCode))
                    {
                        rowErrorList.Add(messCheckItemCode);
                    }

                    var taskItemCategory = Convert.ToString(dataTable.Rows[i]["TaskItemCategory"]);
                    if (taskItemCategory != null && taskItemCategory.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業項目分類", "10"));

                    var taskPrimaryItem = Convert.ToString(dataTable.Rows[i]["TaskPrimaryItem"]);
                    if (taskPrimaryItem != null && taskPrimaryItem.Length > 10)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業大項目", "10"));

                    var taskSecondryItem = Convert.ToString(dataTable.Rows[i]["TaskSecondryItem"]);
                    if (taskSecondryItem != null && taskSecondryItem.Length > 30)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業中項目", "30"));

                    var taskTertiaryItem = Convert.ToString(dataTable.Rows[i]["TaskTertiaryItem"]);
                    if (taskTertiaryItem != null && taskTertiaryItem.Length > 30)
                        rowErrorList.Add(string.Format(ErrorMessages.EW0002, "作業小項目", "30"));

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

                var listData = dataTable.ToList<MTaskItemModel>();
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

        private string CheckTaskItemId(string flag, string taskItemId)
        {
            flag = (flag ?? "").Trim();
            // 新規登録チェック
            if (flag.Equals("1"))
            {
                //using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                //{
                //    var result = db.Query("MTaskItem")
                //        .WhereIn("TaskItemCode", taskItemCode)
                //        .Get<MTaskItemModel>()
                //        .FirstOrDefault();
                //    if (result != null)
                //        return string.Format(ErrorMessages.EW1204, "作業者ログインID");
                //}
            }
            // 更新チェック
            else if (flag.Equals("2"))
            {
                if(!int.TryParse(taskItemId, out int val))
                {
                    return string.Format(ErrorMessages.EW0009, "作業項目ID");
                }
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

        private string CheckTaskItemCode(string flag, string taskItemCode)
        {
            flag = (flag ?? "").Trim();

            if (string.IsNullOrWhiteSpace(taskItemCode))
            {
                return string.Format(ErrorMessages.EW0001, "作業項目コード");
            }
            if (!int.TryParse(taskItemCode, out int val))
            {
                return string.Format(ErrorMessages.EW0009, "作業項目コード");
            }

            // 新規登録チェック
            if (flag.Equals("1"))
            {
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = db.Query("MTaskItem")
                        .WhereIn("TaskItemCode", taskItemCode)
                        .Get<MTaskItemModel>()
                        .FirstOrDefault();
                    if (result != null)
                        return string.Format(ErrorMessages.EW1204, "作業項目コード");
                }
            }
            // 更新チェック
            else if (flag.Equals("2"))
            {
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var result = db.Query("MTaskItem")
                        .WhereIn("TaskItemCode", taskItemCode)
                        .Get<MTaskItemModel>()
                        .FirstOrDefault();
                    if (result == null)
                        return string.Format(ErrorMessages.EW1205, "作業項目コード");
                }
            }

            return string.Empty;
        }

        private int SaveChangeData(List<MTaskItemModel> insertData, List<MTaskItemModel> modifyData)
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
                        var result = db.Query("MTaskItem")
                            .InsertGetId<int>(new
                            {
                                TaskItemCode = data.TaskItemCode,
                                TaskItemCategory = data.TaskItemCategory,
                                TaskPrimaryItem = data.TaskPrimaryItem,
                                TaskSecondryItem = data.TaskSecondryItem,
                                TaskTertiaryItem = data.TaskTertiaryItem,
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
                        var result = db.Query("MTaskItem")
                            .Where("TaskItemCode", data.TaskItemCode)
                            .Update(new
                            {
                                TaskItemCode = data.TaskItemCode,
                                TaskItemCategory = data.TaskItemCategory,
                                TaskPrimaryItem = data.TaskPrimaryItem,
                                TaskSecondryItem = data.TaskSecondryItem,
                                TaskTertiaryItem = data.TaskTertiaryItem,
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

        private List<MTaskItemModel> GetListTaskItemModel(string searchKey)
        {
            var listMTaskItemModel = new List<MTaskItemModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                listMTaskItemModel = db.Query("MTaskItem as a")
                .Select(
                        "a.TaskItemId",
                        "a.TaskItemCode",
                        "a.TaskItemCategory",
                        "a.TaskPrimaryItem",
                        "a.TaskSecondryItem",
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
                .Get<MTaskItemModel>()
                .ToList();
            }

            if (listMTaskItemModel.Count == 0)
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
                    || x.TaskSecondryItem.Contains(searchKey)
                    || x.TaskTertiaryItem.Contains(searchKey)
                    || x.Remark.Contains(searchKey)
                    ).ToList();
                if (listMTaskItemModel.Count == 0)
                {
                    throw new CustomExtention(ErrorMessages.EW0102);
                }
            }

            return listMTaskItemModel;
        }
    }
}
