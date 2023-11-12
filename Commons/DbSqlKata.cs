using Dapper;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SqlClient;

namespace task_sync_web.Commons
{
    public class DbSqlKata : QueryFactory, IDisposable
    {
        private static readonly string masterConnectionString = new GetConnectString("tasksync_0_master").ConnectionString;

        private IDbTransaction _transaction = null;

        public DbSqlKata(string dbName = "") 
        {
            string connectionString;
            if (dbName == "")
            {
                // マスターDBの接続文字列をセット
                connectionString = masterConnectionString;
            }
            else
            {
                // 各会社DBの接続文字列をセット
                connectionString = new GetConnectString(dbName).ConnectionString;
            }
            var connection = new SqlConnection(connectionString);
            if(connection.State != ConnectionState.Open)
                connection.Open();
            var compiler = new SqlServerCompiler();
            this.Connection = connection;
            this.Compiler = compiler;
        }

        public DataTable GetDataTable(Query query)
        {
            var table = new DataTable();
            var conStr = this.Connection.ConnectionString;
            var connection = new SqlConnection(conStr);
            var compiler = new SqlServerCompiler();
            using (var queryFac = new QueryFactory(connection, compiler))
            {
                var sqlStr = queryFac.Compiler.Compile(query).Sql;
                var reader = this.Connection.ExecuteReader(sqlStr, _transaction, commandType: CommandType.Text);
                table.Load(reader);
            }
            return table;
        }

        public async Task<DataTable> GetDataTableAsync(Query query)
        {
            var table = new DataTable();
            var conStr = this.Connection.ConnectionString;
            var connection = new SqlConnection(conStr);
            var compiler = new SqlServerCompiler();
            using (var queryFac = new QueryFactory(connection, compiler))
            {
                var sqlStr = queryFac.Compiler.Compile(query).Sql;
                var reader = await this.Connection.ExecuteReaderAsync(sqlStr, _transaction, commandType: CommandType.Text);
                table.Load(reader);
            }
            return table;
        }

        public IEnumerable<T> ExecQueryData<T>(string T_SQL, object parametter = null)
        {
            return Connection.Query<T>(T_SQL, parametter, _transaction, commandType: CommandType.Text);
        }

        public Task<IEnumerable<T>> ExecQueryDataAsync<T>(string T_SQL, object parametter = null)
        {
            return Connection.QueryAsync<T>(T_SQL, parametter, _transaction, commandType: CommandType.Text);
        }

        public IDbTransaction Begin()
        {
            if (this.Connection is not null)
                return _transaction = this.Connection.BeginTransaction();
            else
                throw new ArgumentNullException("Connection is null");
        }

        public void Commit()
        {
            try
            {
                if (_transaction is not null)
                    _transaction.Commit();
                else
                    throw new ArgumentNullException("Transaction is null");
            }
            catch (Exception)
            {
                throw;
            }
            return;
        }

        public void Rollback()
        {
            if (_transaction is not null)
                _transaction.Rollback();
            else
                throw new ArgumentNullException("Transaction is null");
            return;
        }

        public new void Dispose()
        {
            base.Dispose();
            if(_transaction is not null)
            {
                _transaction.Dispose();
            }
            _transaction = null;
        }

    }
}
