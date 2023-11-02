using System.ComponentModel.DataAnnotations;
using System.Data;
using task_sync_web.Models;

namespace task_sync_web.Commons
{
    // 共通関数
    // 参考：https://gist.github.com/Buravo46/49c34e77ff1a75177340

    public static class Utils
    {

        public static List<PropertyModel> GetModelProperties<T>() 
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.IsDefined(typeof(DisplayAttribute), false))
                .Select(p => new PropertyModel
                {
                    PropertyName = p.Name,
                    DisplayName = p.GetCustomAttributes(typeof(DisplayAttribute),
                            false).Cast<DisplayAttribute>().Single().Name
                });
            return properties.ToList();
        }

        //public static (QueryFactory queryFactory, SqlServerCompiler compiler, SqlConnection connection) GetQueryFactory(string databeseName)
        //{
        //    var connectionString = new GetConnectString(databeseName).ConnectionString;
        //    // Setup the connection and compiler
        //    var connection = new SqlConnection(connectionString);
        //    var compiler = new SqlServerCompiler();
        //    var db = new QueryFactory(connection, compiler);

        //    return (db, compiler, connection);
        //}

        //public static PageViewModel SetPaging(int pageNumber, int pageRowCount, int dataAllCount)
        //{
        //    var pageViewModel = new PageViewModel();
        //    var endNo = pageNumber * pageRowCount;
        //    var startNo = endNo - (pageRowCount - 1);
        //    if (endNo > dataAllCount)
        //    {
        //        endNo = dataAllCount;
        //    }
        //    pageViewModel.PageRowStartNumber = startNo;
        //    pageViewModel.PageRowEndNumber = endNo;
        //    pageViewModel.PageRowCount = dataAllCount;

        //    return pageViewModel;
        //}
    }

}