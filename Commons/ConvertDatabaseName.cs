namespace task_sync_web.Commons
{
    public partial class ConvertDatabaseName
    {
        public string ComapnyDatabeseName { get; set; }

        public ConvertDatabaseName(string webPath)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            ComapnyDatabeseName = configuration.GetSection("companyDatabaseName").GetValue<string>(webPath);
        }
    }
}