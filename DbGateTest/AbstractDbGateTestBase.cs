using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.ErManagement.ErMapper;
using log4net;
using Xunit;

namespace DbGate
{
    public class AbstractDbGateTestBase
    {
        protected ITransactionFactory TransactionFactory;
        protected IDbConnection Connection;
        
        private readonly Dictionary<string,ICollection<string>> dbTableNameMap = new Dictionary<string, ICollection<string>>();
        private readonly Dictionary<string,ICollection<Type>> dbEntityTypeMap = new Dictionary<string, ICollection<Type>>();
        
        protected static Type TestClass = typeof(AbstractDbGateTestBase);

        protected void BeginInit(string dbName)
        {
            try
            {
                if (TransactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(TestClass).Info("Starting in-memory database for unit tests");
                    TransactionFactory = new DefaultTransactionFactory(
                        () => new SQLiteConnection(
                            "Data Source=:memory:;Version=3;Mode=Memory;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON"),
                        DefaultTransactionFactory.DbSqllite);
                }
                
                var transaction = TransactionFactory.CreateTransaction();
                Assert.NotNull(transaction);
                
                Connection = transaction.Connection;
                Assert.NotNull(Connection);
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal(string.Format("Exception during database {0} startup.",dbName), ex);
            }
        }

        protected ITransaction CreateTransaction(IDbConnection con = null)
        {
            if (con != null)
            {
                return new Transaction(TransactionFactory, con.BeginTransaction());
            }
            return new Transaction(TransactionFactory, Connection.BeginTransaction());
        }

        protected void CreateTableFromSql(string sql,string dbName,IDbConnection con = null)
        {
            try
            {
                var tx = CreateTransaction(con);
                var cmd = tx.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                tx.Commit();

                AddTableNameFromSql(sql,dbName);
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal(string.Format("Exception creating the table with sql {0} in database {1}.",sql,dbName),ex);
            }
        }

        protected void RegisterClassForDbPatching(Type entity,string dbName)
        {
            ICollection<Type> entityTypes;
            if (dbEntityTypeMap.ContainsKey(dbName))
            {
                entityTypes = dbEntityTypeMap[dbName];
            }
            else
            {
                entityTypes = new List<Type>();
                dbEntityTypeMap.Add(dbName,entityTypes);
            }
            entityTypes.Add(entity);
        }

        private void AddTableNameFromSql(string sql,string dbName)
        {
            var match = Regex.Match(sql,@"(create)([\\s]*)(table)([\\s]*)([^\\s]*)",RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var tableName = match.Groups[match.Groups.Count].Value;
                AddTableName(tableName,dbName);
            }
        }

        protected void EndInit(string dbName,IDbConnection con = null)
        {
            try
            {
                var tx = CreateTransaction(con);
                if (dbEntityTypeMap.ContainsKey(dbName))
                {
                    var typeList = dbEntityTypeMap[dbName];
                    if (typeList.Count > 0)
                    {
                        tx.DbGate.PatchDataBase(tx,typeList,true);
                        tx.Commit();
                    }

                    foreach (var aType in typeList)
                    {
                        AddTableNameFromEntity(aType,dbName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal(string.Format("Exception patching the database {0}.",dbName),ex);
            }
        }

        private void AddTableNameFromEntity(Type entityType,string dbName)
        {
            var entityInfo = CacheManager.GetEntityInfo(entityType);
            while (entityInfo != null)
            {
                AddTableName(entityInfo.TableInfo.TableName,dbName);
                entityInfo = entityInfo.SuperEntityInfo;
            }
        }

        private void AddTableName(string tableName,string dbName)
        {
            ICollection<string> tableNames;
            if (dbTableNameMap.ContainsKey(dbName))
            {
                tableNames = dbTableNameMap[dbName];
            }
            else
            {
                tableNames = new List<string>();
                dbTableNameMap.Add(dbName, tableNames);
            }
            if (!tableNames.Contains(tableName))
            {
                tableNames.Add(tableName);
            }
        }

        protected void CleanupDb(string dbName)
        {
            try
            {
                if (dbTableNameMap.ContainsKey(dbName))
                {
                    var tableNames = dbTableNameMap[dbName];
                    if (tableNames.Count > 0)
                    {
                        var tx = TransactionFactory.CreateTransaction();
                        foreach (var tableName in tableNames)
                        {
                            var cmd = tx.CreateCommand();
                            cmd.CommandText = string.Format("DELETE FROM {0}",tableName);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal(string.Format("Exception cleaning the database {0}.",dbName),ex);
            }
        }

        protected void FinalizeDb(string dbName)
        {
            LogManager.GetLogger(TestClass).Info(string.Format("Stopping in-memory database {0}.",dbName));

            try
            {
                Connection.Close();
                Connection = null;
                TransactionFactory = null;
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal("Exception during test cleanup.", ex);
            }

            dbEntityTypeMap.Clear();
            dbTableNameMap.Clear();
        }
    }
}
