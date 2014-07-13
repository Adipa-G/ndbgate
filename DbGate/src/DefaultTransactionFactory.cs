using System;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using DbGate.ErManagement.ErMapper;
using DbGate.Exceptions.Common;
using log4net;

//for windows
//for linux
//using Mono.Data.Sqlite;

namespace DbGate
{
    public class DefaultTransactionFactory : ITransactionFactory
    {
        public const int DbOracle = 1;
        public const int DbPostgre = 2;
        public const int DbAccess = 3;
        public const int DbSqllite = 4;
        public const int DbDerby = 5;
        public const int DbMysql = 6;

        private readonly IDbGate _dbGate;
        private readonly int _dbType;
        private readonly string _connectionString;

        public DefaultTransactionFactory(String connectionString, int dbType)
        {
            _connectionString = connectionString;
            _dbType = dbType;
            _dbGate = new ErManagement.ErMapper.DbGate(_dbType);
        }

        public ITransaction CreateTransaction()
        {
            IDbConnection conn = null;
            IDbTransaction tx = null;
            try
            {
                if (_dbType == DbSqllite)
                {
                    //for windows
                    conn = new SQLiteConnection(_connectionString);
                    //for linux
                    //conn = new SqliteConnection(_connectionString);
                }
                else
                {
                    conn = new SqlConnection(_connectionString);
                }
                conn.Open();
                tx = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new TransactionCreationFailedException(
                    String.Format("Failed to create a transaction for connection string {0}", _connectionString), ex);
            }
            return new Transaction(this,tx);
        }

        public IDbGate DbGate
        {
            get { return _dbGate; }
        }

        #region IDisposable Members
        public void Dispose()
        {
            LogManager.GetLogger(typeof (DefaultTransactionFactory)).Info("finalize pool");
        }
        #endregion
    }
}