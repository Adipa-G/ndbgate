using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Castle.DynamicProxy;
using DbGate.Support.Persistant.Lazy;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateLazyTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-lazy";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateLazyTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }
        
        private IDbConnection SetupTables()
        {
            string sql = "Create table lazy_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table lazy_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table lazy_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);

            EndInit(DBName);
            return Connection;
        }

        [Test]
        public void Lazy_PersistAndLoad_WithEmptyLazyFieldsWithLazyOn_ShouldHaveProxiesForLazyFields()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();
                IDbConnection con = SetupTables();

                ITransaction transaction = CreateTransaction(con);
                int id = 45;
                LazyTestRootEntity entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction,entityReloaded,id);
                transaction.Commit();
                con.Close();


                bool isProxyOneToMany = ProxyUtil.IsProxyType(entityReloaded.One2ManyEntities.GetType());
                bool isProxyOneToOne = ProxyUtil.IsProxyType(entityReloaded.One2OneEntity.GetType());
                Assert.IsTrue(isProxyOneToMany);
                Assert.IsTrue(isProxyOneToOne);
                Assert.IsTrue(TransactionFactory.DbGate.Statistics.SelectQueryCount == 0);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Lazy_PersistAndLoad_WithLazyOnWithValuesInLazyFields_ShouldRetrieveLazyFieldsInSameConnection()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();
                
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                
                int id = 45;
                LazyTestRootEntity entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                LazyTestOne2ManyEntity one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                LazyTestOne2ManyEntity one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                LazyTestOne2OneEntity one2One = new LazyTestOne2OneEntity();
                one2One.Name ="One2One";
                entity.One2OneEntity =one2One;

                entity.Persist(transaction);
                transaction.Commit();
                
                transaction = CreateTransaction(con);
                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                
                Assert.IsTrue(entityReloaded.One2ManyEntities.Count == 2);
                IEnumerator<LazyTestOne2ManyEntity> enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.IsTrue(entityReloaded.One2OneEntity != null);
                Assert.IsTrue(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.IsTrue(TransactionFactory.DbGate.Statistics.SelectQueryCount == 2);
                
                transaction.Commit();
                con.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Lazy_PersistAndLoad_WithLazyOnWithValuesInLazyFields_ShouldRetrieveLazyFieldsInAnotherConnection()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();

                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                
                int id = 45;
                LazyTestRootEntity entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                LazyTestOne2ManyEntity one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                LazyTestOne2ManyEntity one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                LazyTestOne2OneEntity one2One = new LazyTestOne2OneEntity();
                one2One.Name = "One2One";
                entity.One2OneEntity = one2One;

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                

                Assert.IsTrue(entityReloaded.One2ManyEntities.Count == 2);
                IEnumerator<LazyTestOne2ManyEntity> enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.IsTrue(entityReloaded.One2OneEntity != null);
                Assert.IsTrue(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.IsTrue(TransactionFactory.DbGate.Statistics.SelectQueryCount == 2);

                transaction.Commit();
                con.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Lazy_LoadAndPersist_WithLazyOnWithoutFetchingLazyFields_ShouldNotLoadLazyLoadingQueries()
        {
            try
            {
                TransactionFactory.DbGate.Config.EnableStatistics = true;
                TransactionFactory.DbGate.Statistics.Reset();

                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                
                int id = 45;
                LazyTestRootEntity entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                LazyTestOne2ManyEntity one2Many1 = new LazyTestOne2ManyEntity();
                one2Many1.IndexNo = 1;
                one2Many1.Name = "One2Many1";

                LazyTestOne2ManyEntity one2Many2 = new LazyTestOne2ManyEntity();
                one2Many2.IndexNo = 2;
                one2Many2.Name = "One2Many2";
                entity.One2ManyEntities.Add(one2Many1);
                entity.One2ManyEntities.Add(one2Many2);

                LazyTestOne2OneEntity one2One = new LazyTestOne2OneEntity();
                one2One.Name = "One2One";
                entity.One2OneEntity = one2One;

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(transaction, entityReloaded, id);
                entity.Persist(transaction);
                transaction.Commit();

                Assert.IsTrue(TransactionFactory.DbGate.Statistics.SelectQueryCount == 0);
                con.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, LazyTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from lazy_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, transaction);
                loaded = true;
            }

            return loaded;
        }
    }
}
