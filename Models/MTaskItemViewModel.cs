using static task_sync_web.Commons.Enums;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MTaskItemViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        public string SearchKeyWord { get; set; }

        public IFormFile File { get; set; }

        public CollapseState IsState { get; set; } = CollapseState.Hide;

        public IPagedList<MTaskItemModel> TaskItemModels { get; set; }

        public MTaskItemViewModel()
        {
            DisplayName = "作業項目マスター";
            TaskItemModels = new List<MTaskItemModel>().ToPagedList(1, PageRowCount);
        }
    }
}
