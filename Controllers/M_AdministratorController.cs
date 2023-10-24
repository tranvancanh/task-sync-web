using Dapper;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Data;
using System.Diagnostics;
using task_sync_web.Commons;
using task_sync_web.Models;
using task_sync_web.Models.Commons;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly ILogger<AdministratorController> _logger;

        /// <summary>
        /// 1ページに表示する行数の設定
        /// </summary>
        private const int pageRowCount = 50;

        /// <summary>
        /// 検索条件セッションキーの設定
        /// </summary>
        private const string SESSIONKEY_SearchKeyWord = "SearchKeyWord";

        public AdministratorController(ILogger<AdministratorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初期一覧表示orキーワード検索
        /// </summary>
        /// <param name="SearchKeyWord"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(M_AdministratorViewModel administratorViewModel, bool IsExcelOutput)
        {
            SessionReset();

            try
            {
                var administratorList = GetList(administratorViewModel.SearchKeyWord ?? "");

                // ページング関連のデータセット
                int pageNumber = 1; // 初期表示は１ページ目
                administratorViewModel.PageViewModel = Utils.SetPaging(pageNumber, pageRowCount, administratorList.Count);
                administratorViewModel.AdministratorViewModels = administratorList.ToPagedList(pageNumber, pageRowCount);

                return View(administratorViewModel);
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(administratorViewModel);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW500;
                return View(administratorViewModel);
            }
        }

        /// <summary>
        /// ページング
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult PageChange(int pageNumber = 1)
        {
            var viewModel = new M_AdministratorViewModel();

            try
            {
                // 前回の検索条件をセッションから取得
                var SearchKeyWord = SessionGet();

                // データを取得
                var administratorList = GetList(SearchKeyWord);

                // ページング関連のデータセット
                viewModel.PageViewModel = Utils.SetPaging(pageNumber, pageRowCount, administratorList.Count);
                viewModel.AdministratorViewModels = administratorList.ToPagedList(pageNumber, pageRowCount);

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

        private List<M_AdministratorViewModel> GetList(string SearchKeyWord)
        {
            var administratorViewModels = new List<M_AdministratorViewModel>();

            try
            {
                // DBからデータ一覧取得
                var db = Utils.GetQueryFactory("tasksync_0_test");

                var administratorList = db.queryFactory.Query("M_Administrator").Get<M_AdministratorViewModel>().ToList();
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
            HttpContext.Session.SetString(SESSIONKEY_SearchKeyWord, SearchKeyWord);
        }

        private string SessionGet()
        {
            var SearchKeyWord = (HttpContext.Session.GetString(SESSIONKEY_SearchKeyWord) ?? "").Trim();
            return SearchKeyWord;

        }
        private void SessionReset()
        {
            HttpContext.Session.Remove(SESSIONKEY_SearchKeyWord);
        }
    }
}