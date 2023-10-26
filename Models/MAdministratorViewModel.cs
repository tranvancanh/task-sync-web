using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class MAdministratorViewModel : BaseViewModel
    {
        [Display(Name = "�����L�[���[�h")]
        [StringLength(50, ErrorMessageResourceName = "EW002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string SearchKeyWord { get; set; }

        public IPagedList<MAdministratorModel> AdministratorModels { get; set; }

        public MAdministratorViewModel()
        {
            DisplayName = "�Ǘ��҃}�X�^�[";
            PageRowCount = 2;
            ExcelHeaderList = new List<string> { "�Ǘ��҃��O�C��ID", "�Ǘ��Җ�" , "�Ǘ��Җ�����" , "���p��~�t���O" };
        }
    }
}