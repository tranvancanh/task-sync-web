using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorViewModel : BaseViewModel
    {
        public string? SearchKeyWord { get; set; }

        public int AdministratorId { get; set; }

        [Display(Name = "�Ǘ��҃��O�C��ID")]
        public string AdministratorLoginId { get; set; }

        [Display(Name = "�Ǘ��Җ�")]
        [Required(ErrorMessageResourceName = "EW001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string AdministratorName { get; set; }

        [Display(Name = "�Ǘ��Җ�����")]
        public string AdministratorNameKana { get; set; }

        public IPagedList<MAdministratorViewModel> AdministratorViewModels { get; set; }

    }
}