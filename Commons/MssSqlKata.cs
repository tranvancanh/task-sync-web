using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SqlClient;

namespace task_sync_web.Commons.MssSqlKata
{
    public class MssSqlKata : QueryFactory, IDisposable
    {
        private static readonly string connectionString = new GetConnectString("tasksync_0_test").ConnectionString;

        private IDbTransaction _transaction = null;

        public MssSqlKata() 
        {
            var connection = new SqlConnection(connectionString);
            var compiler = new SqlServerCompiler();
            this.Connection = connection;
            this.Compiler = compiler;
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
        }

    }
}
