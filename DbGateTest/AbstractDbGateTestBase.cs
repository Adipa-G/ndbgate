using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using DbGate.Caches;
using DbGate.Caches.Impl;
using DbGate.ErManagement.ErMapper;
using NUnit.Framework;
using log4net;

namespace DbGate
{
    public class AbstractDbGateTestBase
    {
        protected static ITransactionFactory TransactionFactory;
        protected static IDbConnection Connection;
        
        private static readonly Dictionary<string,ICollection<string>> DBTableNameMap = new Dictionary<string, ICollection<string>>();
        private static readonly Dictionary<string,ICollection<Type>> DBEntityTypeMap = new Dictionary<string, ICollection<Type>>();
        
        protected static Type TestClass = typeof(AbstractDbGateTestBase);

        protected static void BeginInit(string dbName)
        {
            try
            {
                if (TransactionFactory == null)
                {
                    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

                    LogManager.GetLogger(TestClass).Info("Starting in-memory database for unit tests");
                    TransactionFactory = new DefaultTransactionFactory("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DefaultTransactionFactory.DbSqllite);
                }
                
                var transaction = TransactionFactory.CreateTransaction();
                Assert.IsNotNull(transaction);
                
                Connection = transaction.Connection;
                Assert.IsNotNull(Connection);
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(TestClass).Fatal(string.Format("Exception during database {0} startup.",dbName), ex);
            }
        }

        protected static ITransaction CreateTransaction(IDbConnection con = null)
        {
            if (con != null)
            {
                return new Transaction(TransactionFactory, con.BeginTransaction());
            }
            return new Transaction(TransactionFactory, Connection.BeginTransaction());
        }

        protected static void CreateTableFromSql(string sql,string dbName,IDbConnection con = null)
        {
            try
            {
                ITransaction tx = CreateTransaction(con);
                IDbCommand cmd = tx.CreateCommand();
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

        protected static void RegisterClassForDbPatching(Type entity,string dbName)
        {
            ICollection<Type> entityTypes;
            if (DBEntityTypeMap.ContainsKey(dbName))
            {
                entityTypes = DBEntityTypeMap[dbName];
            }
            else
            {
                entityTypes = new List<Type>();
                DBEntityTypeMap.Add(dbName,entityTypes);
            }
            entityTypes.Add(entity);
        }

        private static void AddTableNameFromSql(string sql,string dbName)
        {
            var match = Regex.Match(sql,@"(create)([\\s]*)(table)([\\s]*)([^\\s]*)",RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string tableName = match.Groups[match.Groups.Count].Value;
                AddTableName(tableName,dbName);
            }
        }

        protected static void EndInit(string dbName,IDbConnection con = null)
        {
            try
            {
                ITransaction tx = CreateTransaction(con);
                if (DBEntityTypeMap.ContainsKey(dbName))
                {
                    ICollection<Type> typeList = DBEntityTypeMap[dbName];
                    if (typeList.Count > 0)
                    {
                        tx.DbGate.PatchDataBase(tx,typeList,true);
                        tx.Commit();
                    }

                    foreach (Type aType in typeList)
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

        private static void AddTableNameFromEntity(Type entityType,string dbName)
        {
            EntityInfo entityInfo = CacheManager.GetEntityInfo(entityType);
            while (entityInfo != null)
            {
                AddTableName(entityInfo.TableInfo.TableName,dbName);
                entityInfo = entityInfo.SuperEntityInfo;
            }
        }

        private static void AddTableName(string tableName,string dbName)
        {
            ICollection<string> tableNames;
            if (DBTableNameMap.ContainsKey(dbName))
            {
                tableNames = DBTableNameMap[dbName];
            }
            else
            {
                tableNames = new List<string>();
                DBTableNameMap.Add(dbName, tableNames);
            }
            if (!tableNames.Contains(tableName))
            {
                tableNames.Add(tableName);
            }
        }

        protected static void CleanupDb(string dbName)
        {
            try
            {
                if (DBTableNameMap.ContainsKey(dbName))
                {
                    ICollection<string> tableNames = DBTableNameMap[dbName];
                    if (tableNames.Count > 0)
                    {
                        ITransaction tx = TransactionFactory.CreateTransaction();
                        foreach (string tableName in tableNames)
                        {
                            IDbCommand cmd = tx.CreateCommand();
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

        protected static void FinalizeDb(string dbName)
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

            DBEntityTypeMap.Clear();
            DBTableNameMap.Clear();
        }
    }
}
