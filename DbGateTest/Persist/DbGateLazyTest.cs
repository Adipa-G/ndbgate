using System;
using System.Collections.Generic;
using System.Data;
using Castle.DynamicProxy;
using DbGate.Persist.Support.Lazy;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateLazyTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-fetchstrategy";

        public DbGateLazyTest()
        {
            TestClass = typeof(DbGateLazyTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
       
        private IDbConnection SetupTables()
        {
            var sql = "Create table lazy_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table lazy_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table lazy_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            EndInit(DbName);
            return Connection;
        }

        [Fact]
        public void Lazy_PersistAndLoad_WithEmptyLazyFieldsWithLazyOn_ShouldHaveProxiesForLazyFields()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();
                var con = SetupTables();

                var transaction = CreateTransaction(con);
                var id = 45;
                var entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction,entityReloaded,id);
                transaction.Commit();
                con.Close();


                var isProxyOneToMany = ProxyUtil.IsProxyType(entityReloaded.One2ManyEntities.GetType());
                var isProxyOneToOne = ProxyUtil.IsProxyType(entityReloaded.One2OneEntity.GetType());
                Assert.True(isProxyOneToMany);
                Assert.True(isProxyOneToOne);
                Assert.True(TransactionFactory.DbGate.Statistics.SelectQueryCount == 0);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateLazyTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Lazy_PersistAndLoad_WithLazyOnWithValuesInLazyFields_ShouldRetrieveLazyFieldsInSameConnection()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();
                
                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 45;
                var entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                var one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                var one2One = new LazyTestOne2OneEntity();
                one2One.Name ="One2One";
                entity.One2OneEntity =one2One;

                entity.Persist(transaction);
                transaction.Commit();
                
                transaction = CreateTransaction(con);
                var entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                
                Assert.True(entityReloaded.One2ManyEntities.Count == 2);
                var enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.True(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.True(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.True(entityReloaded.One2OneEntity != null);
                Assert.True(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.True(TransactionFactory.DbGate.Statistics.SelectQueryCount == 2);
                
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Lazy_PersistAndLoad_WithLazyOnWithValuesInLazyFields_ShouldRetrieveLazyFieldsInAnotherConnection()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();

                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 45;
                var entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                var one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                var one2One = new LazyTestOne2OneEntity();
                one2One.Name = "One2One";
                entity.One2OneEntity = one2One;

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                

                Assert.True(entityReloaded.One2ManyEntities.Count == 2);
                var enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.True(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.True(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.True(entityReloaded.One2OneEntity != null);
                Assert.True(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.True(TransactionFactory.DbGate.Statistics.SelectQueryCount == 2);

                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Lazy_LoadAndPersist_WithLazyOnWithoutFetchingLazyFields_ShouldNotLoadLazyLoadingQueries()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();

                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 45;
                var entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                var one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                var one2One = new LazyTestOne2OneEntity();
                one2One.Name = "One2One";
                entity.One2OneEntity = one2One;

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                entity.Persist(transaction);
                transaction.Commit();

                Assert.True(TransactionFactory.DbGate.Statistics.SelectQueryCount == 0);
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, LazyTestRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from lazy_test_root where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            var dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, transaction);
                loaded = true;
            }

            return loaded;
        }
    }
}
