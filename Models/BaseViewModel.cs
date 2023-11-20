namespace task_sync_web.Models
{
    public class BaseViewModel
    {
        ///// <summary>
        ///// 画面名
        ///// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 初期一覧表示のページ番号
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// １ページに表示する行数
        /// </summary>
        public int PageRowCount { get; set; }

        public BaseViewModel()
        {
            PageNumber = 1;
            PageRowCount = 100;
        }
    }
}
