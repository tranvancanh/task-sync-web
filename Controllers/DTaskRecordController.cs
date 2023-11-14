using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;
using static task_sync_web.Commons.Enums;

namespace task_sync_web.Controllers
{
    public class DTaskRecordController : BaseController
    {
        public string[] DateTimeFormatSupport = new[] { "yyyy/MM/dd", "yyyy-MM-dd", "yyyy.MM.dd", "yyyyMMdd" };
        public string[] TimeSpanFormatSupport = new[] { @"hh\:mm\:ss", @"hh\:mm" };

        [HttpGet]
        public async Task<IActionResult> Index(DTaskRecordViewModel viewModel, Enums.GetState command = Enums.GetState.Default)
        {
            try
            {
                var validateInputForm = ValidateInputForm(viewModel);
                if (validateInputForm.Any())
                {
                    ViewData["ErrorMessage"] = validateInputForm;
                    return View(viewModel);
                }

                var taskUserViewModel = await GetListTaskRecordModel(viewModel);
                switch (command)
                {
                    //検索処理
                    case GetState.Default:
                    case GetState.Search:
                        {
                            var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);
                            foreach (var item in listPaged)
                            {
                                var startTime = Convert.ToDateTime(item.TaskStartDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                                var stopTime = Convert.ToDateTime(item.TaskEndDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                                item.TaskTime = stopTime - startTime; //作業時間
                                var totalTime = TimeSpan.FromSeconds(item.TaskInterruptTotalTime);
                                item.PureTaskTime = stopTime - startTime - totalTime; //純作業時間
                                item.TaskTimeMinute = item.TaskTime.Minutes; //作業時間(分)
                                item.PureTaskTimeMinute = item.PureTaskTime.Minutes; //純作業時間(分)
                            }

                            // page the list
                            viewModel.TaskRecordModels = listPaged;
                            return View(viewModel);
                        }
                    case GetState.ExcelOutput:
                        {
                            var excelHeaderStyle = new ExcelHeaderStyleModel();
                            excelHeaderStyle.FirstColorBackgroundColorColumnNumber = new int[1] { 1 };
                            excelHeaderStyle.SecondColorBackgroundColorColumnNumber = new int[23] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,19, 20, 21, 22, 23, 24 };
                            var memoryStream = ExcelFile<DTaskRecordModel>.ExcelCreate(taskUserViewModel, true, 1, 1, excelHeaderStyle);
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

        private List<string> ValidateInputForm(DTaskRecordViewModel viewModel)
        {
            var errorList = new List<string>();
            DateTime taskStartDateTime = new DateTime();
            DateTime taskEndDateTime = new DateTime();
            try
            {
                taskStartDateTime = DateTime.ParseExact(viewModel.TaskStartDateTime, DateTimeFormatSupport, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                errorList.Add(ErrorMessages.EW1300);
            }

            try
            {
                taskEndDateTime = DateTime.ParseExact(viewModel.TaskEndDateTime, DateTimeFormatSupport, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                errorList.Add(ErrorMessages.EW1301);
            }
            if (errorList.Any())
                return errorList;
            else
            {
                var userIdOrName = (viewModel.TaskUserLoginIdName ?? string.Empty).Split(':');
                if (userIdOrName.Length > 1)
                    viewModel.TaskUserLoginIdName = userIdOrName[0];

                viewModel.TaskStartDateTime = taskStartDateTime.ToString("yyyy/MM/dd");
                viewModel.TaskEndDateTime = taskEndDateTime.ToString("yyyy/MM/dd");
            }

            return errorList;
        }


        [HttpGet]
        public async Task<List<dynamic>> GetUserAutoComplete(string userInfor)
        {
            var recordList = new List<dynamic>();
            userInfor = (userInfor ?? string.Empty).Trim();

            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業大項目を取得
                var query = db.Query("MTaskUser")
                .Select(
                        "TaskUserLoginId", // 作業者ログインID
                        "TaskUserName"     // 作業者名
                    )
                .WhereContains("TaskUserName", $"{userInfor}");
                if ((userInfor.Length <= 8) && int.TryParse(userInfor, out int val))
                {
                    query = query.OrWhereContains("TaskUserLoginId", $"{userInfor}");
                }
                query = query.GroupBy("TaskUserLoginId", "TaskUserName")
                             .OrderBy("TaskUserLoginId", "TaskUserName");
                recordList = (await query.GetAsync<dynamic>()).ToList();
            }

            return recordList;
        }



        //////////////////////////////////////////////////////////////////////// 中断時間モダール Start  /////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public async Task<IActionResult> Interrupt(DTaskInterruptRecordViewModel recordViewModel)
        {
            try
            {
                var taskRecordId = recordViewModel.TaskRecordId;
                recordViewModel = await GetDTaskRecord(recordViewModel);
                recordViewModel.TaskTrackTotalTime = CalculationTimeMinutesTimeByRoundupSeconds(recordViewModel.TaskStartDateTrackTime, recordViewModel.TaskEndDateTrackTime);
                var listInterruptRecord = await GetDTaskInterruptRecords(taskRecordId);
                foreach (var item in listInterruptRecord)
                {
                    var startTime = Convert.ToDateTime(item.TaskInterruptEndDateTrackTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    var stopTime = Convert.ToDateTime(item.TaskInterruptStartDateTrackTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    item.TaskInterruptTime = CalculationTimeMinutesTimeByRoundupSeconds(startTime, stopTime);
                }
                recordViewModel.InterruptRecords.AddRange(listInterruptRecord);
            }
            catch (Exception)
            {
                throw;
            }

            return PartialView("~/Views/DTaskRecord/_Interrupt.cshtml", recordViewModel);
        }

        private async Task<DTaskInterruptRecordViewModel> GetDTaskRecord(DTaskInterruptRecordViewModel recordViewModel)
        {
            var taskRecordId = recordViewModel.TaskRecordId;
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業日付(始)"), 作業日付(終)")が存在する場合
                recordViewModel = (await db.Query("DTaskRecord as taskrecord")
                .Select(
                        "taskrecord.TaskRecordId",            // 作業実績ID
                        "taskrecord.TaskUserId",              // 作業者ID
                        "taskUser.TaskUserLoginId",              // 作業者ログインID   
                        "taskUser.TaskUserName",              // 作業者名   
                        "taskrecord.TaskStartDateTrackTime",      // 作業開始時刻(実績)
                        "taskrecord.TaskEndDateTrackTime",         // 作業終了時刻(実績)
                        "taskrecord.TaskInterruptTrackTotalTime"   // 中断時間(実績)
                    )
                .LeftJoin("MTaskUser as taskUser", "taskrecord.TaskUserId", "taskUser.TaskUserId")
                .Where("taskrecord.TaskRecordId", taskRecordId)
                .WhereNotNull("taskUser.TaskUserName")
                .GetAsync<DTaskInterruptRecordViewModel>())
                .FirstOrDefault();
            }
            if (recordViewModel == null)
                throw new CustomExtention(ErrorMessages.EW0101);
            return recordViewModel;
        }

        private async Task<List<DTaskInterruptRecordModel>> GetDTaskInterruptRecords(int taskRecordId)
        {
            var recordModel = new List<DTaskInterruptRecordModel>();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業日付(始)"), 作業日付(終)")が存在する場合
                recordModel = (await db.Query("DTaskInterruptRecord as interruptRecord")
                .Select(
                        "interruptRecord.TaskInterruptRecordId",                 // 作業中断実績ID

                        "interruptRecord.TaskRecordId",                          //作業実績ID
                        "interruptRecord.TaskInterruptStartDateTrackTime",       // 作業中断開始時間
                        "interruptRecord.TaskInterruptEndDateTrackTime",         // 作業中断終了時間
                        "interruptRecord.TaskInterruptReasonCode",               // 作業中断理由コード
                        "interruptRecord.TaskInterruptReasonName",               // 作業中断理由名

                        "interruptRecord.CreateDateTime",        // 作成日時
                        "interruptRecord.CreateTaskUserId",      // 作成作業者ID
                        "interruptRecord.UpdateDateTime",        // 更新日時
                        "interruptRecord.UpdateTaskUserId"       // 更新作業者ID
                    )
                .Where("TaskRecordId", taskRecordId)
                .GetAsync<DTaskInterruptRecordModel>())
                .ToList();
            }
            return recordModel;
        }

        //////////////////////////////////////////////////////////////////////// 中断時間モダール Stop  /////////////////////////////////////////////////////////////////////////////



        //////////////////////////////////////////////////////////////////////// 修正モダール Start  ///////////////////////////////////////////////////////////////////////////////


        [HttpGet]
        public async Task<IActionResult> Edit(int taskRecordId)
        {
            var model = new DTaskInterruptModalEditViewModel();
            try
            {
                model = await GetDTaskRecord(taskRecordId);
                model.TaskTrackTotalTime = CalculationTimeMinutesTimeByRoundupSeconds(model.TaskStartDateTrackTime, model.TaskEndDateTrackTime);

            }
            catch (Exception)
            {
                throw;
            }

            return PartialView("~/Views/DTaskRecord/_Edit.cshtml", model);
        }

        [HttpGet]
        public async Task<List<dynamic>> TaskItemAutoComplete(string taskItemCode)
        {
            var recordList = new List<dynamic>();
            taskItemCode = (taskItemCode ?? string.Empty).Trim();
            if ((taskItemCode.Length > 8) || !int.TryParse(taskItemCode, out int val))
            {
                return recordList;
            }

            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業大項目を取得
                recordList = (await db.Query("MTaskItem")
                    .Select(
                            "TaskItemCode",      // 作業項目コード
                            "TaskPrimaryItem",   // 作業大項目
                            "TaskSecondaryItem", // 作業中項目 
                            "TaskTertiaryItem"   // 作業小項目
                        )
                    .WhereLike("TaskItemCode", $"%{taskItemCode}%")
                    .GroupBy("TaskItemCode",
                            "TaskPrimaryItem",
                            "TaskSecondaryItem",
                            "TaskTertiaryItem")
                    .OrderBy("TaskItemCode",
                            "TaskPrimaryItem",
                            "TaskSecondaryItem",
                            "TaskTertiaryItem")
                    .GetAsync<dynamic>())
                    .ToList();
                ;
            }

            return recordList;
        }


        [HttpPost]
        public async Task<IActionResult> Edit(DTaskInterruptModalEditViewModel editViewModel)
        {
            var errorList = new List<string>();
            try
            {
                errorList = ValidateForm(editViewModel);
                if (errorList.Any())
                    return Json(new { Result = "NG", Mess = errorList });

                var efftedRows = await SaveChangeData(editViewModel);
                if(efftedRows > 0)
                    return Json(new { Result = "OK", Mess = SuccessMessages.SW002 });
                else
                {
                    errorList.Add(ErrorMessages.EW1207);
                    return Json(new { Result = "NG", Mess = errorList });
                }
            }
            catch(Exception ex)
            {
                errorList.Add(ex.Message);
                return Json(new { Result = "NG", Mess = errorList });
            }
        }

        private List<string> ValidateForm(DTaskInterruptModalEditViewModel editViewModel)
        {
            var errorList = new List<string>();
            var taskStartDate = editViewModel.TaskStartDate;
            var taskStartTime = editViewModel.TaskStartTime;
            var taskEndDate = editViewModel.TaskEndDate;
            var taskEndTime = editViewModel.TaskEndTime;
            var taskInterruptTotalTime = editViewModel.TaskInterruptTotalTime;
            var taskItem = editViewModel.TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem;
            var remark = (editViewModel.Remark ?? string.Empty).Trim();
            try
            {
                taskStartDate = DateTime.ParseExact(taskStartDate, DateTimeFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(DateTimeFormatSupport[0]);
            }
            catch (Exception)
            {
                errorList.Add(string.Format(ErrorMessages.EW1302, "作業開始日付"));
            }
            try
            {
                taskStartTime = TimeSpan.ParseExact(taskStartTime, TimeSpanFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(TimeSpanFormatSupport[0]);
            }
            catch (Exception)
            {
                errorList.Add(string.Format(ErrorMessages.EW1303, "作業開始時間"));
            }

            try
            {
                taskEndDate = DateTime.ParseExact(taskEndDate, DateTimeFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(DateTimeFormatSupport[0]);
            }
            catch (Exception)
            {
                errorList.Add(string.Format(ErrorMessages.EW1302, "作業終了日付"));
            }
            try
            {
                taskEndTime = TimeSpan.ParseExact(taskEndTime, TimeSpanFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(TimeSpanFormatSupport[0]);
            }
            catch (Exception)
            {
                errorList.Add(string.Format(ErrorMessages.EW1303, "作業終了時間"));
            }
            if (taskInterruptTotalTime < 0)
                errorList.Add(string.Format(ErrorMessages.EW0009, "中断時間(分)"));
            if (string.IsNullOrWhiteSpace(taskItem))
                errorList.Add(string.Format(ErrorMessages.EW0001, "作業項目"));
            else
            {
                if (taskItem.Split('-').Length != 4)
                    errorList.Add(string.Format(ErrorMessages.EW1304));
            }
            if (remark.Length > 200)
                errorList.Add(string.Format(ErrorMessages.EW0002, "備考", "200"));

            return errorList;
        }

        private async Task<int> SaveChangeData(DTaskInterruptModalEditViewModel editViewModel)
        {
            var efftedRows = -1;
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                var tran = db.Begin();
                try
                {
                    var taskRecordId = editViewModel.TaskRecordId;
                    var taskStartDate = Convert.ToDateTime(editViewModel.TaskStartDate).Date;
                    var taskStartTime = TimeSpan.ParseExact(editViewModel.TaskStartTime, TimeSpanFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(@"hh\:mm");
                    var taskEndDate = Convert.ToDateTime(editViewModel.TaskEndDate).Date;
                    var taskEndTime = TimeSpan.ParseExact(editViewModel.TaskEndTime, TimeSpanFormatSupport, System.Globalization.CultureInfo.InvariantCulture).ToString(@"hh\:mm");
                    var taskInterruptTotalTime = editViewModel.TaskInterruptTotalTime;
                    var taskItem = editViewModel.TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem.Split('-');
                    var taskItemCode = taskItem[0];
                    var taskPrimaryItem = taskItem[1];
                    var taskSecondaryItem = taskItem[2];
                    var taskTertiaryItem = taskItem[3];
                    var remark = (editViewModel.Remark ?? string.Empty).Trim();
                    var isDelete = editViewModel.IsDelete;

                    efftedRows = 0;
                    // 更新処理
                    var result = await db.Query("DTaskRecord")
                        .Where("TaskRecordId", taskRecordId)
                        .UpdateAsync(new
                        {
                            TaskItemCode = taskItemCode,
                            TaskPrimaryItem = taskPrimaryItem,
                            TaskSecondaryItem = taskSecondaryItem,
                            TaskTertiaryItem = taskTertiaryItem,
                            TaskStartDateTime = taskStartDate.Add(TimeSpan.Parse(taskStartTime)),
                            TaskEndDateTime = taskEndDate.Add(TimeSpan.Parse(taskEndTime)),
                            TaskInterruptTotalTime = taskInterruptTotalTime,
                            Remark = remark,
                            IsDelete = isDelete,
                            UpdateDateTime = DateTime.Now,
                            UpdateAdministratorId = LoginUser.AdministratorId
                        }, tran);
                    if (result > 0)
                        efftedRows = efftedRows + result;

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

        private async Task<DTaskInterruptModalEditViewModel> GetDTaskRecord(int taskRecordId)
        {
            var recordViewModel = new DTaskInterruptModalEditViewModel();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                recordViewModel = (await db.Query("DTaskRecord as taskrecord")
                .Select(
                        "taskrecord.TaskRecordId",            // 作業実績ID
                        "taskrecord.TaskUserId",              // 作業者ID
                        "taskUser.TaskUserLoginId",           // 作業者ログインID
                        "taskUser.TaskUserName",              // 作業者名
                        "taskrecord.TaskStartDateTrackTime",       // 作業開始時刻(記録)
                        "taskrecord.TaskEndDateTrackTime",         // 作業終了時刻(記録)
                        "taskrecord.TaskInterruptTrackTotalTime",  // 中断記録(記録)
                        "taskrecord.TaskItemCode",                 //作業項目コード
                        "taskrecord.TaskPrimaryItem",              // 作業大項目
                        "taskrecord.TaskSecondaryItem",            // 作業中項目
                        "taskrecord.TaskTertiaryItem",             // 作業小項目
                        "taskrecord.Remark"                       // 備考
                        )
                //.SelectRaw("CONCAT(CAST([taskrecord].[TaskItemCode] AS varchar), '-', [taskrecord].[TaskPrimaryItem], '-', [taskrecord].[TaskSecondaryItem], '-' [taskrecord].[TaskTertiaryItem]) as TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem") // 作業項目選択
                .SelectRaw("FORMAT([taskrecord].[TaskStartDateTime], 'yyyy/MM/dd') as TaskStartDate") // 作業開始日付(実績)
                .SelectRaw("FORMAT([taskrecord].[TaskStartDateTime], 'hh:mm:ss') as TaskStartTime")   // 作業開始時間(実績)
                .SelectRaw("FORMAT([taskrecord].[TaskEndDateTime], 'yyyy/MM/dd') as TaskEndDate")     // 作業終了日付(実績)
                .SelectRaw("FORMAT([taskrecord].[TaskEndDateTime], 'hh:mm:ss') as TaskEndTime")       // 作業終了時間(実績)
                .Select("taskrecord.TaskInterruptTotalTime")                                          // 中断記録(実績)(分)
                .LeftJoin("MTaskUser as taskUser", "taskrecord.TaskUserId", "taskUser.TaskUserId")
                .Where("taskrecord.TaskRecordId", taskRecordId)
                .WhereNotNull("taskUser.TaskUserName")
                .GetAsync<DTaskInterruptModalEditViewModel>())
                .FirstOrDefault();
            }
            if (recordViewModel == null)
                throw new CustomExtention(ErrorMessages.EW0101);
            else
                recordViewModel.TaskItemCode_PrimaryItem_SecondaryItem_TertiaryItem = recordViewModel.TaskItemCode + "-" + recordViewModel.TaskPrimaryItem + "-" + recordViewModel.TaskSecondaryItem + "-" + recordViewModel.TaskTertiaryItem;
            return recordViewModel;
        }


        //////////////////////////////////////////////////////////////////////// 修正モダール Stop  /////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// 時間の差を分に変換。30秒以上は切り上げ１分・30未満は切り捨て0分で計算
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public int CalculationTimeMinutesTimeByRoundupSeconds(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                int totalMinutes = 0;

                // 作業時間の差を計算
                TimeSpan timeSpan = endDateTime - startDateTime;

                // 時間の部分のみ取得
                var hours = timeSpan.Hours;
                // 時間が0以上なら、60を掛けて分に変換する
                totalMinutes += (hours > 0) ? (hours * 60) : 0;

                // 分の部分のみ取得
                var minutes = timeSpan.Minutes;
                totalMinutes += minutes;

                // 秒の部分のみ取得
                var seconds = timeSpan.Seconds;
                // 秒単位は、30秒以上なら1分、30秒未満なら0分で四捨五入して分に変換する
                totalMinutes += (seconds >= 30) ? 1 : 0;

                return totalMinutes;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<List<DTaskRecordModel>> GetListTaskRecordModel(DTaskRecordViewModel viewModel, bool command = true)
        {
            var listDTaskRecordModel = new List<DTaskRecordModel>();
            var taskStartDateTime = Convert.ToDateTime(viewModel.TaskStartDateTime).Date;
            var taskEndDateTime = Convert.ToDateTime(viewModel.TaskEndDateTime).AddDays(1).Date;
            var taskUserLoginId = viewModel.TaskUserLoginIdName;
            var isDelete = viewModel.IsDelete;
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業日付(始)"), 作業日付(終)")が存在する場合
                var query = db.Query("DTaskRecord as taskrecord")
                .Select(
                        "taskrecord.TaskRecordId",            // 作業実績ID
                        "login.LoginDateTime",                // ログイン時刻
                        "taskrecord.TaskDate",                // 作業日付
                        "taskUser.TaskUserDepartmentName",    // 所属名
                        "taskUser.TaskUserGroupName",         // グループ名
                        "taskrecord.LoginTaskUserRecordId",   // 作業ログインID
                        "taskUser.TaskUserName",              // 作業者名

                        "taskrecord.TaskItemId",              //作業項目ID
                        "taskrecord.TaskItemCode",            // 作業項目コード
                        "taskrecord.TaskPrimaryItem",         // 作業大項目
                        "taskrecord.TaskSecondaryItem",       // 作業中項目
                        "taskrecord.TaskTertiaryItem",        // 作業小項目
                        "taskItem.TaskItemCategory",          // 作業分類

                        "taskrecord.TaskStartDateTrackTime",  // 作業開始時刻(記録)
                        "taskrecord.TaskEndDateTrackTime",    // 作業終了時刻(記録)
                        "taskrecord.TaskInterruptTrackTotalTime", // 中断記録(記録)

                         "taskrecord.TaskStartDateTime",      // 作業開始時刻(実績)
                        "taskrecord.TaskEndDateTime",         // 作業終了時刻(実績)
                        "taskrecord.TaskInterruptTotalTime",  // 中断記録(実績)

                        "taskrecord.Remark",                  // 備考
                        "taskrecord.IsDelete",                // 削除フラグ

                        "taskrecord.CreateDateTime",
                        "b.AdministratorLoginId as CreateAdministratorId", // 作成管理者ログインID
                        "b.AdministratorName as CreateAdministratorName", // 作成管理者名
                        "taskrecord.UpdateDateTime",
                        "c.AdministratorLoginId as UpdateAdministratorId", // 更新管理者ログインID
                        "c.AdministratorName as UpdateAdministratorName", // 更新管理者名
                        "e.TaskUserLoginId as CreateTaskUserId", // 作成作業者ログインID
                        "e.TaskUserName as CreateTaskUserName", // 作成作業者名
                        "f.TaskUserLoginId as UpdateTaskUserId", // 更新作業者ログインID
                        "f.TaskUserName as UpdateCreateTaskUserName" // 更新作業者名
                    )
                .LeftJoin("DLoginTaskUserRecord as login", "taskrecord.LoginTaskUserRecordId", "login.LoginTaskUserRecordId")
                .LeftJoin("MTaskUser as taskUser", "taskrecord.TaskUserId", "taskUser.TaskUserId")
                .LeftJoin("MTaskItem as taskItem", "taskrecord.TaskItemId", "taskItem.TaskItemId")
                .LeftJoin("MAdministrator as b", "taskrecord.CreateAdministratorId", "b.AdministratorId")
                .LeftJoin("MAdministrator as c", "taskrecord.UpdateAdministratorId", "c.AdministratorId")
                .LeftJoin("MTaskUser as e", "taskrecord.CreateTaskUserId", "e.TaskUserId")
                .LeftJoin("MTaskUser as f", "taskrecord.UpdateTaskUserId", "f.TaskUserId")
                .Where("TaskStartDateTime", ">=", taskStartDateTime)
                .Where("TaskEndDateTime", "<", taskEndDateTime);
                if (!string.IsNullOrWhiteSpace(taskUserLoginId))
                    query = query.Where("TaskUserLoginId", taskUserLoginId);
                if (!isDelete)
                    query = query.WhereIn("IsDelete", new[] { false });
                else
                    query = query.WhereIn("IsDelete", new[] { true, false });
                query = query.WhereNotNull("login.LoginDateTime");
                query = query.OrderByDesc("login.LoginDateTime");
                var sql = db.Compiler.Compile(query).Sql;
                listDTaskRecordModel = (await query.GetAsync<DTaskRecordModel>()).ToList();
            }

            if (listDTaskRecordModel.Count <= 0 && command)
            {
                throw new CustomExtention(ErrorMessages.EW0101);
            }

            return listDTaskRecordModel;
        }

    }
}
