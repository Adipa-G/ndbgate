using System;
using System.Data;
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
        public const int DbSqlServer = 7;

        private readonly IDbGate dbGate;
        private readonly int dbType;
        private readonly Func<IDbConnection> connectionFactory;

        public DefaultTransactionFactory(Func<IDbConnection> connectionFactory,
            int dbType)
        {
            this.dbType = dbType;
            dbGate = new ErManagement.ErMapper.DbGate(this.dbType);
            this.connectionFactory = connectionFactory;
        }

        public ITransaction CreateTransaction()
        {
            IDbConnection conn = null;
            IDbTransaction tx = null;
            try
            {
                conn = connectionFactory.Invoke();
                conn.Open();
                tx = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new TransactionCreationFailedException("Failed to create a transaction ", ex);
            }
            return new Transaction(this,tx);
        }

        public IDbGate DbGate => dbGate;

        #region IDisposable Members
        public void Dispose()
        {
            LogManager.GetLogger(typeof (DefaultTransactionFactory)).Info("finalize pool");
        }
        #endregion
    }
}