﻿using Microsoft.AspNetCore.Mvc;
using SpreadsheetLight;
using SqlKata.Execution;
using System.Data;
using System.Diagnostics;
using task_sync_web.Commons;
using task_sync_web.Models;
using X.PagedList;

namespace task_sync_web.Controllers
{
    public class MAdministratorController : BaseController
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
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(MAdministratorViewModel viewModel)
        {
            try
            {
                var listUser = GetListMAdministrator(viewModel.SearchKeyWord);

                // page the list
                var administratorModels = listUser.ToPagedList(viewModel.PageNumber, viewModel.PageRowCount);
                viewModel.AdministratorModels = administratorModels;

                return View(viewModel);
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(viewModel);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW0500;
                return View(viewModel);
            }
        }

        /// <summary>
        /// Excel出力
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExcelOutput(MAdministratorViewModel viewModel)
        {
            try
            {
                var administratorModels = GetListMAdministrator(viewModel.SearchKeyWord);
                var memoryStream = this.ExcelCreate(administratorModels);

                // ファイル名
                var fileName = viewModel.DisplayName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
            }
            catch (CustomExtention ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View("Index", viewModel);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = ErrorMessages.EW0500;
                return View("Index", viewModel);
            }
        }

        /// <summary>
        /// 管理者マスターを取得
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        private List<MAdministratorModel> GetListMAdministrator(string searchKey)
        {
            try
            {
                var administratorModels = new List<MAdministratorModel>();
                var today = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd 00:00:00"));

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    // DBからデータ一覧を取得
                    var administratorList = db.Query("MAdministrator")
                        .WhereFalse("IsNotUse") // 利用停止フラグがFalse
                        .WhereNot("AdministratorId", 1) // 東山システムは表示しない
                        .Where("LoginAdministratorEnableStartDate", "<=", today) // 利用開始日を過ぎている
                        .Where("LoginAdministratorEnableEndDate", ">=", today) // 利用終了日を過ぎていない
                        .OrderBy("AdministratorId")
                        .Get<MAdministratorModel>().ToList();
                    if (administratorList.Count == 0)
                    {
                        throw new CustomExtention(ErrorMessages.EW0101);
                    }

                    searchKey = (searchKey ?? "").Trim();
                    if (administratorList.Count > 0 && searchKey.Length > 0)
                    {
                        // 検索キーワードが存在する場合
                        administratorModels = administratorList.Where(x => x.AdministratorLoginId.Contains(searchKey) || x.AdministratorName.Contains(searchKey) || x.AdministratorNameKana.Contains(searchKey)).ToList();
                        if (administratorModels.Count == 0)
                        {
                            throw new CustomExtention(ErrorMessages.EW0102);
                        }
                    }
                    else
                    {
                        administratorModels = administratorList;
                    }
                }
                return administratorModels;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public MemoryStream ExcelCreate(List<MAdministratorModel> administratorList)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (SLDocument sl = new SLDocument())
                {
                    SLStyle keyStyle = sl.CreateStyle();

                    // 太字
                    keyStyle.SetFontBold(true);

                    // ModelのProperty一覧を取得
                    var properties = Utils.GetModelProperties<MAdministratorModel>();

                    // 1行目：ヘッダーをセット
                    for (int i = 1; i < (properties.Count()+ 1); ++i)
                    {
                        sl.SetCellStyle(1, i, keyStyle);
                        sl.SetCellValue(1, i, properties[i - 1].DisplayName);
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
                            //sl.SetCellValue(col, ++row, data[_col].IsNotUse == false ? 0 : 1);
                        }
                    }

                    sl.AutoFitColumn(0, properties.Count);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}