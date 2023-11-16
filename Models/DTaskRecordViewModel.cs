using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace task_sync_web.Models
{
    public class DTaskRecordViewModel : BaseViewModel
    {
        [Display(Name = "作業開始日付(実績)")]
        public string TaskStartDateTime { get; set; }

        [Display(Name = "作業終了日付(実績)")]
        public string TaskEndDateTime { get; set; }

        [Display(Name = "作業者")]
        [StringLength(50, ErrorMessageResourceName = "EW0002", ErrorMessageResourceType = typeof(ErrorMessages))]
        public string TaskUserLoginIdName { get; set; }

        [Display(Name = "削除データを含める")]
        public bool IsDelete { get; set; }


        public IPagedList<DTaskRecordModel> TaskRecordModels { get; set; }

        public DTaskRecordViewModel()
        {
            DisplayName = "作業実績";
            TaskStartDateTime = DateTime.Today.ToString("yyyy/MM/dd");
            TaskEndDateTime = DateTime.Today.ToString("yyyy/MM/dd");
            TaskUserLoginIdName = string.Empty;
            IsDelete = false;
            TaskRecordModels = new List<DTaskRecordModel>().ToPagedList(1, PageRowCount);
        }
    }
}
