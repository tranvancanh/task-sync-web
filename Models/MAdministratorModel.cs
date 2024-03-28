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

        /// <summary>
        /// ���p�J�n��
        /// </summary>
        public DateTime LoginAdministratorEnableStartDate { get; set; }

        /// <summary>
        /// ���p�I����
        /// </summary>
        public DateTime LoginAdministratorEnableEndDate { get; set; }

        //[Display(Name = "���p��~�t���O")]
        //public bool IsNotUse { get; set; }

        [Key]
        public int AdministratorId { get; set; }

        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsLogin { get; set; }

    }
}