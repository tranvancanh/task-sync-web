using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MInterruptReasonViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        [StringLength(50, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IPagedList<MInterruptReasonModel> InterruptReasonModels { get; set; }

        public bool? IsModalState { get; set; } = null; // null: 検索処理、true: 新規登録、flase: 更新

        public MInterruptReasonModel ModalModel { get; set; }

        public MInterruptReasonViewModel()
        {
            DisplayName = "中断理由マスター";
            PageRowCount = 50;
            InterruptReasonModels = new List<MInterruptReasonModel>().ToPagedList(1, PageRowCount);
            ModalModel = new MInterruptReasonModel();
        }
    }
}
