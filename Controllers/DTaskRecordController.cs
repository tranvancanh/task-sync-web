using DocumentFormat.OpenXml.Drawing.Charts;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class DTaskRecordController : BaseController
    {
        private string[] DateTimeFormatSupport = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "yyyyMMdd" };

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
                    case Enums.GetState.Default:
                    case Enums.GetState.Search:
                        {
                            var listPaged = await taskUserViewModel.ToPagedListAsync(viewModel.PageNumber, viewModel.PageRowCount);

                            // page the list
                            viewModel.TaskRecordModels = listPaged;
                            return View(viewModel);
                        }
                    case Enums.GetState.ExcelOutput:
                        {
                            var excelHeaderStyle = new ExcelHeaderStyleModel();
                            excelHeaderStyle.FirstColorBackgroundColorColumnNumber = new int[1] { 1 };
                            excelHeaderStyle.SecondColorBackgroundColorColumnNumber = new int[7] { 2, 3, 4, 5, 6, 7, 8 };
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
                viewModel.TaskStartDateTime = taskStartDateTime.ToString("yyyy/MM/dd");
                viewModel.TaskEndDateTime = taskEndDateTime.ToString("yyyy/MM/dd");
            }

            return errorList;
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
                        "taskrecord.TaskInterruptTrackTotalTime"   // 中断記録(実績)
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

                await Task.CompletedTask;
            }
            catch (Exception)
            {
                throw;
            }

            return PartialView("~/Views/DTaskRecord/_Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DTaskInterruptModalEditViewModel editViewModel)
        {
            var model = new DTaskInterruptModalEditViewModel();
            try
            {

                await Task.CompletedTask;
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { Result = "NG", Mess = "" });
        }

        private async Task<DTaskInterruptModalEditViewModel> GetDTaskRecord(int taskRecordId)
        {
            var recordViewModel = new DTaskInterruptModalEditViewModel();
            using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
            {
                // 作業日付(始)"), 作業日付(終)")が存在する場合
                recordViewModel = (await db.Query("DTaskRecord as taskrecord")
                .Select(
                        "taskrecord.TaskRecordId",            // 作業実績ID
                        "taskrecord.TaskUserId",              // 作業者ID
                        "taskUser.TaskUserLoginId",           // 作業者ログインID   
                        "taskUser.TaskUserName",              // 作業者名   
                        "taskrecord.TaskStartDateTrackTime",      // 作業開始時刻(記録)
                        "taskrecord.TaskEndDateTrackTime",        // 作業終了時刻(記録)
                        "taskrecord.TaskInterruptTrackTotalTime"  // 中断記録(記録)
                    )
                .LeftJoin("MTaskUser as taskUser", "taskrecord.TaskUserId", "taskUser.TaskUserId")
                .Where("taskrecord.TaskRecordId", taskRecordId)
                .WhereNotNull("taskUser.TaskUserName")
                .GetAsync<DTaskInterruptModalEditViewModel>())
                .FirstOrDefault();
            }
            if (recordViewModel == null)
                throw new CustomExtention(ErrorMessages.EW0101);
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

                        "taskrecord.TaskMemo",                // 作業メモ
                        "taskrecord.IsDelete",                // 削除フラグ

                        "taskrecord.CreateDateTime",          // 作成日時
                        "taskrecord.CreateAdministratorId",   // 作成管理者
                        "taskrecord.CreateTaskUserId",        // 作成作業者
                        "taskrecord.UpdateDateTime",          // 更新日時
                        "taskrecord.UpdateAdministratorId",   // 更新管理者
                        "taskrecord.UpdateTaskUserId",        // 更新作業者

                        "taskrecord.CreateDateTime",
                        "b.AdministratorLoginId as CreateAdministratorId",
                        "b.AdministratorName as CreateAdministratorName",
                        "taskrecord.UpdateDateTime",
                        "c.AdministratorLoginId as UpdateAdministratorLoginId",
                        "c.AdministratorName as UpdateAdministratorName"
                    )
                .LeftJoin("DLoginTaskUserRecord as login", "taskrecord.LoginTaskUserRecordId", "login.LoginTaskUserRecordId")
                .LeftJoin("MTaskUser as taskUser", "taskrecord.TaskUserId", "taskUser.TaskUserId")
                .LeftJoin("MTaskItem as taskItem", "taskrecord.TaskItemId", "taskItem.TaskItemId")
                .LeftJoin("MAdministrator as b", "taskrecord.CreateAdministratorId", "b.AdministratorId")
                .LeftJoin("MAdministrator as c", "taskrecord.UpdateAdministratorId", "c.AdministratorId")
                .Where("TaskStartDateTime", ">=", taskStartDateTime)
                .Where("TaskEndDateTime", "<", taskEndDateTime);
                if (!string.IsNullOrWhiteSpace(taskUserLoginId))
                    query = query.Where("TaskUserLoginId", taskUserLoginId);
                if (!viewModel.IsDelete)
                    query = query.Where("IsDelete", isDelete);
                query = query.WhereNotNull("login.LoginDateTime");
                query = query.OrderByDesc("login.LoginDateTime");

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
