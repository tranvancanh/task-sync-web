using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorViewModel : BaseViewModel
    {
        public string? SearchKeyWord { get; set; }

        public int AdministratorId { get; set; }

        [Display(Name = "ŠÇ—ÒƒƒOƒCƒ“ID")]
        public string AdministratorLoginId { get; set; }

        [Display(Name = "ŠÇ—Ò–¼")]
        [Required(ErrorMessageResourceName = "EW001", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string AdministratorName { get; set; }

        [Display(Name = "ŠÇ—Ò–¼‚©‚È")]
        public string AdministratorNameKana { get; set; }

        public IPagedList<MAdministratorViewModel> AdministratorViewModels { get; set; }

    }
}