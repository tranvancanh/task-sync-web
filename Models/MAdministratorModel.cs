using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorModel
    {
        [Display(Name = "�Ǘ��҃��O�C��ID")]
        public string AdministratorLoginId { get; set; }

        [Display(Name = "�Ǘ��Җ�")]
        public string AdministratorName { get; set; }

        [Display(Name = "�Ǘ��Җ�����")]
        public string AdministratorNameKana { get; set; }

        [Display(Name = "���p��~�t���O")]
        public bool IsNotUse { get; set; }

    }
}