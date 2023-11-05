using System.ComponentModel.DataAnnotations;
using X.PagedList;
using static task_sync_web.Commons.Enums;

namespace task_sync_web.Models
{
    public class MTaskUserViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        public string SearchKeyWord { get; set; }

        public IFormFile File { get; set; }

        public CollapseState IsState { get; set; } = CollapseState.Hide;

        public IPagedList<MTaskUserModel> TaskUserModelModels { get; set; }

        public MTaskUserViewModel()
        {
            DisplayName = "作業者マスター";
            TaskUserModelModels = new List<MTaskUserModel>().ToPagedList(1, PageRowCount);
        }
    }
}
