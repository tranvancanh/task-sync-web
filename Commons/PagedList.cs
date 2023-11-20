using X.PagedList;

namespace task_sync_web.Commons
{
    public class PagedList<T> : List<T>, IPagedList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="source">The source list of elements containing all elements to be paged over.</param>
        /// <param name="currentPage">The current page number (1 based).</param>
        /// <param name="pageSize">Size of a page (number of items per page).</param>
        public PagedList(IEnumerable<T> items, int currentPage, int itemsPerPage, int totalRows)
        {
            this.TotalItemCount = totalRows;
            this.TotalItems = totalRows;
            this.ItemsPerPage = itemsPerPage;
            this.CurrentPage = Math.Min(Math.Max(1, currentPage), TotalPages);
            this.AddRange(items);
        }
        public int CurrentPage { get; private set; }
        public int ItemsPerPage { get; private set; }
        public bool HasPreviousPage { get { return (CurrentPage > 1); } }
        public bool HasNextPage { get { return (CurrentPage * ItemsPerPage) < TotalItems; } }
        public int TotalPages { get { return (int)Math.Ceiling((double)TotalItems / ItemsPerPage); } }
        public int TotalItems { get; private set; }

        public int PageCount => 123;

        public int TotalItemCount;

        public int PageNumber => throw new NotImplementedException();

        public int PageSize => throw new NotImplementedException();

        public bool IsFirstPage => throw new NotImplementedException();

        public bool IsLastPage => throw new NotImplementedException();

        public int FirstItemOnPage => throw new NotImplementedException();

        public int LastItemOnPage => throw new NotImplementedException();

        int IPagedList.TotalItemCount => throw new NotImplementedException();
    }
}
