namespace task_sync_web.Models
{
    public class BaseViewModel
    {
        ///// <summary>
        ///// 画面名
        ///// </summary>
        public string DisplayName { get; set; } = string.Empty;

        public int PageNumber { get; set; }

        public int PageRowCount { get; set; }

        public BaseViewModel()
        {
            PageNumber = 1;
            PageRowCount = 50;
        }
    }
}
