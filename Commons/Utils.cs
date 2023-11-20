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

        public static void WhitespaceNotTake(DataTable dataTable)
        {
            for (var row = 0; row < dataTable.Rows.Count; row++)
            {
                for (var col = 0; col < dataTable.Columns.Count; col++)
                {
                    var val = Convert.ToString(dataTable.Rows[row][col]);
                    if (string.IsNullOrWhiteSpace(val))
                        dataTable.Rows[row][col] = string.Empty;
                    else
                        dataTable.Rows[row][col] = val.Trim();
                }
            }
        }

    }

}