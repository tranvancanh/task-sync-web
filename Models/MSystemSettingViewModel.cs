using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MSystemSettingViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        [StringLength(50, ErrorMessageResourceName = "EW002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IPagedList<MSystemSettingModel> SystemSettingModels { get; set; }

        public int EditSystemSettingId { get; set; }
        public MSystemSettingModel SystemSettingEditModel { get; set; }

        public MSystemSettingViewModel()
        {
            DisplayName = "システム設定";
            PageRowCount = 50;
            SystemSettingModels = new List<MSystemSettingModel>().ToPagedList(1, PageRowCount);
            SystemSettingEditModel = new MSystemSettingModel();
        }
    }
}
