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

    }

}