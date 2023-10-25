using Dapper;
using Microsoft.AspNetCore.Mvc;
using SpreadsheetLight;
using SqlKata.Execution;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using task_sync_web.Commons;
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
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(MAdministratorViewModel viewModel, bool isExcelOutput)
        {
            SessionReset();

            try
            {
                var administratorList = GetList(viewModel.SearchKeyWord ?? "");

                if (!isExcelOutput)
                {
                    // ページング関連のデータセット
                    int pageNumber = 1; // 初期表示は１ページ目
                    viewModel.PageViewModel = Utils.SetPaging(pageNumber, viewModel.PageViewModel.PageRowCount, administratorList.Count);
                    viewModel.AdministratorModels = administratorList.ToPagedList(pageNumber, viewModel.PageViewModel.PageRowCount);
                }
                else
                {
                    var memoryStream = ExcelCreate(viewModel.ExcelHeaderList, administratorList);

                    // ファイル名
                    var fileName = viewModel.DisplayName + DateTime.Now.ToString("yyyyMMddHHmmss");
                    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
                }

                return View(viewModel);
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View(viewModel);
            }
        }

        ///// <summary>
        ///// Excel出力
        ///// </summary>
        ///// <param name="viewModel"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult ExcelOutput(MAdministratorViewModel viewModel)
        //{
        //    SessionReset();

        //    try
        //    {
        //        var administratorList = GetList(viewModel.SearchKeyWord ?? "");

        //        var memoryStream = ExcelCreate(viewModel.ExcelHeaderList, administratorList);

        //        // ファイル名
        //        var fileName = viewModel.DisplayName + DateTime.Now.ToString("yyyyMMddHHmmss");
        //        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");

        //    }
        //    catch (CustomExtention ex)
        //    {
        //        ViewData["ErrorMessage"] = ex.Message;
        //        return View("Index", viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewData["ErrorMessage"] = ErrorMessages.EW500;
        //        return View("Index", viewModel);
        //    }
        //}

        /// <summary>
        /// ページング
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PageChange(int pageNumber = 1)
        {
            var viewModel = new MAdministratorViewModel();

            try
            {
                // 前回の検索条件をセッションから取得
                var SearchKeyWord = SessionGet();

                // データを取得
                var administratorList = GetList(SearchKeyWord);

                // ページング関連のデータセット
                viewModel.PageViewModel = Utils.SetPaging(pageNumber, viewModel.PageViewModel.PageRowCount, administratorList.Count);
                viewModel.AdministratorModels = administratorList.ToPagedList(pageNumber, viewModel.PageViewModel.PageRowCount);

                return View("Index", viewModel);
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View(viewModel);
            }

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

                    sl.SaveAs(ms);
                }

                ms.Position = 0;

                return ms;

            }
            catch (Exception ex)
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