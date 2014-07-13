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
    public class DbGateLazyTest
    {
        private static ITransactionFactory _transactionFactory;

        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Info("Starting in-memory database for unit tests");
                _transactionFactory = new DefaultTransactionFactory("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DefaultTransactionFactory.DbSqllite);
                Assert.IsNotNull(_transactionFactory.CreateTransaction());
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Fatal("Exception during database startup.", ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateSuperEntityRefTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            _transactionFactory.DbGate.ClearCache();
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table lazy_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table lazy_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_one2one";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table lazy_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGateSuperEntityRefTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            ITransaction transaction = _transactionFactory.CreateTransaction();
            IDbConnection connection = transaction.Connection;

            string sql = "Create table lazy_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table lazy_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table lazy_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        private ITransaction CreateTransaction(IDbConnection connection)
        {
            return new Transaction(_transactionFactory, connection.BeginTransaction());
        }
    
        [Test]
        public void Lazy_PersistAndLoad_WithEmptyLazyFieldsWithLazyOn_ShouldHaveProxiesForLazyFields()
        {
            try
            {
                _transactionFactory.DbGate.Config.EnableStatistics = true;
                _transactionFactory.DbGate.Statistics.Reset();
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
                Assert.IsTrue(_transactionFactory.DbGate.Statistics.SelectQueryCount == 0);
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
                _transactionFactory.DbGate.Config.EnableStatistics = true;
                _transactionFactory.DbGate.Statistics.Reset();
                
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
                Assert.IsTrue(_transactionFactory.DbGate.Statistics.SelectQueryCount == 2);
                
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
                _transactionFactory.DbGate.Config.EnableStatistics = true;
                _transactionFactory.DbGate.Statistics.Reset();

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
                Assert.IsTrue(_transactionFactory.DbGate.Statistics.SelectQueryCount == 2);

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
                _transactionFactory.DbGate.Config.EnableStatistics = true;
                _transactionFactory.DbGate.Statistics.Reset();

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

                Assert.IsTrue(_transactionFactory.DbGate.Statistics.SelectQueryCount == 0);
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
