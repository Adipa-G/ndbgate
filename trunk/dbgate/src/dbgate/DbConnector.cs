using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using log4net;
//for windows
//for linux
//using Mono.Data.Sqlite;

namespace dbgate
{
    public class DbConnector : IDisposable
    {
        public const int DbOracle = 1;
        public const int DbPostgre = 2;
        public const int DbAccess = 3;
        public const int DbSqllite = 4;
        public const int DbDerby = 5;
        public const int DbMysql = 6;

        private static DbConnector _staticInstance;
        private readonly string _connectionString;

        public DbConnector(String connectionString, int dbType)
        {
            _connectionString = connectionString;
            DbType = dbType;
            _staticInstance = this;
        }

        public IDbConnection Connection
        {
            get
            {
                IDbConnection conn = null;
                try
                {
                    if (DbType == DbSqllite)
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
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger(typeof (DbConnector)).Fatal("Error creating connection", ex);
                }
                return conn;
            }
        }

        public int DbType { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            LogManager.GetLogger(typeof (DbConnector)).Info("finalize pool");
            _staticInstance = null;
        }

        #endregion

        public static DbConnector GetSharedInstance()
        {
            return _staticInstance;
        }
    }
}