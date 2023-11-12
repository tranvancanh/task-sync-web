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
