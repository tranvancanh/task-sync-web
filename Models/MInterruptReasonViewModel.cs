using System.ComponentModel.DataAnnotations;
using X.PagedList;
using static task_sync_web.Commons.Enums;

namespace task_sync_web.Models
{
    public class MInterruptReasonViewModel : BaseViewModel
    {
        [Display(Name = "検索キーワード")]
        [StringLength(50, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IPagedList<MInterruptReasonModel> InterruptReasonModels { get; set; }

        public ModalType ModalType { get; set; }

        public MInterruptReasonModel ModalModel { get; set; }

        public MInterruptReasonViewModel()
        {
            DisplayName = "中断理由マスター";
            InterruptReasonModels = new List<MInterruptReasonModel>().ToPagedList(1, PageRowCount);
            ModalModel = new MInterruptReasonModel();
        }
    }
}