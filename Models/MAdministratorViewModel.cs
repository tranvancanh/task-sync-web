using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorViewModel : BaseViewModel
    {
        /// <summary>
        /// セッションキーの設定
        /// </summary>
        public const string Session_SearchKeyWord = "SearchKeyWord";

        [Display(Name = "検索キーワード")]
        [StringLength(50, ErrorMessageResourceName = "EW002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IPagedList<MAdministratorModel> AdministratorModels { get; set; }

        public MAdministratorViewModel()
        {
            DisplayName = "管理者マスター";
            PageViewModel.PageRowCount = 50;
            ExcelHeaderList = new List<string> { "管理者ログインID", "管理者名" , "管理者名かな" , "利用停止フラグ" };
        }
    }
}