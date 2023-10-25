using Microsoft.AspNetCore.Mvc;
using SpreadsheetLight;
using SqlKata.Execution;
using System.Data;
using System.Diagnostics;
using task_sync_web.Commons;
using task_sync_web.Commons.MssSqlKata;
using task_sync_web.Models;
using task_sync_web.Models.Commons;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MAdministratorController : Controller
    {
        private readonly ILogger<MAdministratorController> _logger;

        public MAdministratorController(ILogger<MAdministratorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初期一覧表示orキーワード検索
        /// </summary>
        /// <param name="searchKeyWord"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(string searchKeyWord, int? pageNumber)
        {
            var viewModel = new MAdministratorViewModel() { SearchKeyWord = searchKeyWord };
            var listUser = await GetListMAdministrator(searchKeyWord);

            // page the list
            var listPaged = listUser.ToPagedList(pageNumber ?? 1, viewModel.PageViewModel.PageRowCount);
            viewModel.AdministratorModels = listPaged;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchKeyWord)
        {
            int? pageNumber = null;
            var viewModel = new MAdministratorViewModel() { SearchKeyWord = searchKeyWord };
            var listUser = await GetListMAdministrator(searchKeyWord);

            // page the list
            var listPaged = listUser.ToPagedList(pageNumber ?? 1, viewModel.PageViewModel.PageRowCount);
            viewModel.AdministratorModels = listPaged;

            return View(viewModel);
        }

        /// <summary>
        /// Excel出力
        /// </summary>
        /// <param name="searchKeyWord"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExcelOutput(string searchKeyWord) 
        {
            var viewModel = new MAdministratorViewModel() { SearchKeyWord = searchKeyWord };
            var listUser = await GetListMAdministrator(searchKeyWord);
            var memoryStream = this.ExcelCreate(viewModel.ExcelHeaderList, listUser);
            await Task.CompletedTask;
            // ファイル名
            var fileName = viewModel.DisplayName + DateTime.Now.ToString("yyyyMMddHHmmss");
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
        }

        /// <summary>
        /// データリストを取得
        /// </summary>
        /// <param name="keySearch"></param>
        /// <returns></returns>
        private async Task<List<MAdministratorModel>> GetListMAdministrator(string keySearch)
        {
            var userModel = new List<MAdministratorModel>();
            using (var db = new MssSqlKata())
            {
                if (string.IsNullOrWhiteSpace(keySearch))
                {
                    userModel = (await db.Query("MAdministrator")
                                        .GetAsync<MAdministratorModel>()).ToList();
                }
                else
                {
                    userModel = (await db.Query("MAdministrator")
                                        .WhereLike("AdministratorLoginId", $"%{keySearch}%")
                                        .OrWhereLike("AdministratorName", $"%{keySearch}%")
                                        .OrWhereLike("AdministratorNameKana", $"%{keySearch}%")
                                        .GetAsync<MAdministratorModel>()).ToList();
                }
            }
            return userModel;
        }

        public MemoryStream ExcelCreate(List<string> headerList, List<MAdministratorModel> administratorList)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (SLDocument sl = new SLDocument())
                {
                    SLStyle keyStyle = sl.CreateStyle();

                    // 太字
                    keyStyle.SetFontBold(true);

                    // 1行目：ヘッダーをセット
                    for (int i = 1; i < (headerList.Count + 1); ++i)
                    {
                        sl.SetCellStyle(1, i, keyStyle);
                        sl.SetCellValue(1, i, headerList[i - 1]);
                    }

                    // 2行目～：値をセット
                    var data = administratorList;
                    if (data != null && data.Count() > 0)
                    {
                        for (int col = 2; col < (data.Count() + 2); ++col)
                        {
                            int row = 0;
                            int _col = col - 2;
                            sl.SetCellValue(col, ++row, data[_col].AdministratorLoginId);
                            sl.SetCellValue(col, ++row, data[_col].AdministratorName);
                            sl.SetCellValue(col, ++row, data[_col].AdministratorNameKana);
                            sl.SetCellValue(col, ++row, data[_col].IsNotUse);
                        }
                    }

                    sl.AutoFitColumn(0, headerList.Count);

                    sl.SaveAs(ms);
                }

                ms.Position = 0;

                return ms;

            }
            catch (Exception)
            {
                throw;
            }

        }

        private List<MAdministratorModel> GetList(string SearchKeyWord)
        {
            var administratorViewModels = new List<MAdministratorModel>();

            try
            {
                // DBからデータ一覧取得
                var db = Utils.GetQueryFactory("tasksync_0_test");

                var administratorList = db.queryFactory.Query("MAdministrator").Get<MAdministratorModel>().ToList();
                if (administratorList.Count == 0)
                {
                    throw new CustomExtention(ErrorMessages.EW101);
                }
                
                SearchKeyWord = SearchKeyWord.Trim();
                if (administratorList.Count > 0 && SearchKeyWord.Length > 0)
                {
                    // キーワード検索
                    administratorViewModels = administratorList.Where(x => x.AdministratorLoginId.Contains(SearchKeyWord) || x.AdministratorName.Contains(SearchKeyWord) || x.AdministratorNameKana.Contains(SearchKeyWord)).ToList();
                    if (administratorViewModels.Count == 0)
                    {
                        throw new CustomExtention(ErrorMessages.EW102);
                    }

                    // キーワードをセッションにセット
                    SessionSet(SearchKeyWord);
                }
                else
                {
                    administratorViewModels = administratorList;
                }

                return administratorViewModels;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SessionSet(string SearchKeyWord)
        {
            HttpContext.Session.SetString(MAdministratorViewModel.Session_SearchKeyWord, SearchKeyWord);
        }

        private string SessionGet()
        {
            var SearchKeyWord = HttpContext.Session.GetString(MAdministratorViewModel.Session_SearchKeyWord) ?? "";
            return SearchKeyWord;

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(MAdministratorViewModel.Session_SearchKeyWord);
        }
    }
}