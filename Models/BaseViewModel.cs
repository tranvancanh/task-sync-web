using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task_sync_web.Models
{
    public class BaseViewModel
    {
        ///// <summary>
        ///// 画面名
        ///// </summary>
        public string DisplayName { get; set; } = string.Empty;

        public List<string> ExcelHeaderList { get; set; }= new List<string>();

        public PageViewModel PageViewModel { get; set; } = new PageViewModel();
    }
}
