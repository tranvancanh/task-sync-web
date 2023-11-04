using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        public string SearchKeyWord { get; set; }

        public IPagedList<MAdministratorModel> AdministratorModels { get; set; }

        public MAdministratorViewModel()
        {
            DisplayName = "管理者マスター";
        }
    }
}