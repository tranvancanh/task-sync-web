﻿using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Diagnostics;
using task_sync_web.Commons;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeViewModel();
            try
            {
                // 会社情報を取得
                var company = GetCompany();

                // システム管理者からのメッセージを取得
                viewModel.CompanyMessageBySystem = company == null ? "" : company.CompanyMessageBySystem;

                // スマホアプリの最新Ver.取得
                viewModel.SmartphoneAppMinVersion = company == null ? "" : company.SmartphoneAppMinVersion.ToString();

                // スマホアプリのダウンロードコードを取得
                viewModel.SmartphoneAppDownloadCode = company == null ? "" : company.SmartphoneAppDownloadCode;

                // 利用デバイス状況の利用中データを取得
                viewModel.UseDeviceStatusModels = GetListUseDeviceStatus();

                // 利用デバイスマスター一覧を取得
                var useDeviceModels = GetListUseDevice();
                viewModel.UseDeviceEnableCount = useDeviceModels.Count;

                // 利用デバイスマスターの情報を、利用デバイス状況にマージして表示
                foreach (var useDeviceModel in useDeviceModels)
                {
                    var useDeviceStatusModel = viewModel.UseDeviceStatusModels.Where(x => x.UseDeviceId == useDeviceModel.UseDeviceId).FirstOrDefault();
                    var today = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd 00:00:00"));

                    if (useDeviceStatusModel == null) // 利用中ではない場合
                    {
                        var addUseDeviceStatusModel = new DUseDeviceStatusViewModel();
                        addUseDeviceStatusModel.UseDeviceStatusId = 0;
                        addUseDeviceStatusModel.UseDeviceId = useDeviceModel.UseDeviceId;
                        addUseDeviceStatusModel.DeviceName = "";
                        addUseDeviceStatusModel.Model = "";
                        addUseDeviceStatusModel.Manufacturer = "";
                        addUseDeviceStatusModel.RegistDateTimeString = "";
                        addUseDeviceStatusModel.UseDeviceEnableDateString = useDeviceModel.UseDeviceEnableStartDate.ToString("yyyy/MM/dd");
                        addUseDeviceStatusModel.UseDeviceStatus = useDeviceModel.UseDeviceEnableStartDate > today ? Enums.UseDeviceStatus.利用開始前 : Enums.UseDeviceStatus.待機中;

                        viewModel.NotUseDeviceCount += 1;
                        viewModel.UseDeviceStatusModels.Add(addUseDeviceStatusModel);
                    }
                    else // 利用中の場合
                    {
                        viewModel.UseDeviceCount += 1;
                        useDeviceStatusModel.RegistDateTimeString = useDeviceStatusModel.RegistDateTime.ToString("yyyy/MM/dd HH:mm");
                        useDeviceStatusModel.UseDeviceEnableDateString = useDeviceModel.UseDeviceEnableStartDate.ToString("yyyy/MM/dd");
                        useDeviceStatusModel.UseDeviceStatus = Enums.UseDeviceStatus.利用中;
                    }
                }

                viewModel.UseDeviceStatusModels.OrderByDescending(x => x.UseDeviceStatusId);

                // メッセージがTempデータにある場合は代入
                if (TempData["ErrorMessage"] != null)
                {
                    ViewData["ErrorMessage"] = TempData["ErrorMessage"].ToString();
                }
                else if (TempData["SuccessMessage"] != null)
                {
                    ViewData["SuccessMessage"] = TempData["SuccessMessage"].ToString();
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
                ViewData["ErrorMessage"] = ErrorMessages.EW0500;
                return View(viewModel);
            }
        }

        /// <summary>
        /// 利用デバイス状況を取得
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private List<DUseDeviceStatusViewModel> GetListUseDeviceStatus()
        {
            try
            {
                var useDeviceStatusModels = new List<DUseDeviceStatusViewModel>();

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    // DBからデータ一覧を取得
                    var useDeviceStatusList = db.Query("DUseDeviceStatus")
                        .WhereFalse("IsCancel")
                        .Get<DUseDeviceStatusViewModel>()
                        .ToList();

                    useDeviceStatusModels = useDeviceStatusList;
                }
                return useDeviceStatusModels;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 利用デバイスマスターを取得
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private List<MUseDeviceModel> GetListUseDevice()
        {
            try
            {
                var useDeviceModels = new List<MUseDeviceModel>();

                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    // DBからデータ一覧を取得
                    var useDeviceList = db.Query("MUseDevice")
                        .Where("UseDeviceEnableEndDate", ">=", DateTime.Now.Date) // 利用デバイス有効日付以内のデータ
                        .Get<MUseDeviceModel>()
                        .ToList();

                    useDeviceModels = useDeviceList;
                }
                return useDeviceModels;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult Index(int cancelUseDeviceStatusId)
        {
            try
            {
                // Update
                using (var db = new DbSqlKata(LoginUser.CompanyDatabaseName))
                {
                    var efftedRows = db.Query("DUseDeviceStatus")
                        .Where("UseDeviceStatusId", cancelUseDeviceStatusId)
                        .Update(new
                        {
                            IsCancel = true,
                            CancelAdministratorDateTime = DateTime.Now,
                            CancelAdministratorId = LoginUser.AdministratorId
                        });

                    if (efftedRows == 0)
                    {
                        TempData["ErrorMessage"] = ErrorMessages.EW0502;
                    }
                    else
                    {
                        TempData["SuccessMessage"] = SuccessMessages.SW002;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ErrorMessages.EW0502;
            }
            return RedirectToAction("Index");
        }

        public MCompanyModel GetCompany()
        {
            var companyModel = new MCompanyModel();
            try
            {

                using (var db = new DbSqlKata())
                {
                    // DBからデータ一覧を取得
                    companyModel = db.Query("MCompany")
                        .Where("CompanyWebPath", LoginUser.CompanyWebPath)
                        .Get<MCompanyModel>().FirstOrDefault();
                }
            }
            catch (Exception)
            {
                // 何もしない
            }

            return companyModel;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}