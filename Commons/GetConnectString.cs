/// <summary>
/// DBに接続するための文字列を取得
/// </summary>
namespace task_sync_web.Commons
{
    public class GetMasterConnectString
    {
        public string ConnectionString { get; set; }

        public GetMasterConnectString()
        {
            var databaseName = "tasksync_master";

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            ConnectionString = configuration.GetSection("connectionString").GetValue<string>(databaseName);
        }

    }

    public class GetConnectString
    {
        public string ConnectionString { get; set; }

        public GetConnectString(string databaseName)
        {
            var connectionString1 = "tasksync_company_before";
            var connectionString2 = "tasksync_company_after";

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();
            ConnectionString = configuration.GetSection("connectionString").GetValue<string>(connectionString1) + databaseName + configuration.GetSection("connectionString").GetValue<string>(connectionString2);

        }

    }
}
