using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MTaskUserViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        [StringLength(50, ErrorMessageResourceName = "EW002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IFormFile File { get; set; }

        public IPagedList<MTaskUserModel> TaskUserModelModels { get; set; }

        public MTaskUserViewModel()
        {
            DisplayName = "作業者マスター";
            PageRowCount = 50;
            TaskUserModelModels = new List<MTaskUserModel>().ToPagedList(1, PageRowCount);
        }
    }
}
