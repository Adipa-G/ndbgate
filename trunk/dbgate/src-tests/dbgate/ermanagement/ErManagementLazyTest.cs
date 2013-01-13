using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Castle.DynamicProxy;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.persistant.lazy;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementLazyTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementLazyTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementLazyTest)).Fatal("Exception during database startup.", ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementLazyTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErLayer.GetSharedInstance().ClearCache();
            }
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table lazy_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table lazy_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM lazy_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table lazy_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementLazyTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            String sql = "Create table lazy_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table lazy_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table lazy_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }
    
        [Test]
        public void ERLayer_persistAndLoad_WithEmptyLazyFieldsWithLazyOn_shouldHaveProxiesForLazyFields()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.EnableStatistics = true;
                ErLayer.GetSharedInstance().Statistics.Reset();
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                LazyTestRootEntity entity = new LazyTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(connection,entityReloaded,id);
                connection.Close();


                bool isProxyOneToMany = ProxyUtil.IsProxyType(entityReloaded.One2ManyEntities.GetType());
                bool isProxyOneToOne = ProxyUtil.IsProxyType(entityReloaded.One2OneEntity.GetType());
                Assert.IsTrue(isProxyOneToMany);
                Assert.IsTrue(isProxyOneToOne);
                Assert.IsTrue(ErLayer.GetSharedInstance().Statistics.SelectQueryCount == 0);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementLazyTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_persistAndLoad_WithLazyOnWithValuesInLazyFields_shouldRetrieveLazyFieldsInSameConnection()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.EnableStatistics = true;
                ErLayer.GetSharedInstance().Statistics.Reset();
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
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

                entity.Persist(connection);
                transaction.Commit();

                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                
                Assert.IsTrue(entityReloaded.One2ManyEntities.Count == 2);
                IEnumerator<LazyTestOne2ManyEntity> enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.IsTrue(entityReloaded.One2OneEntity != null);
                Assert.IsTrue(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.IsTrue(ErLayer.GetSharedInstance().Statistics.SelectQueryCount == 2);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_persistAndLoad_WithLazyOnWithValuesInLazyFields_shouldRetrieveLazyFieldsInAnotherConnection()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.EnableStatistics = true;
                ErLayer.GetSharedInstance().Statistics.Reset();
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
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

                entity.Persist(connection);
                transaction.Commit();

                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);
                connection.Close();

                Assert.IsTrue(entityReloaded.One2ManyEntities.Count == 2);
                IEnumerator<LazyTestOne2ManyEntity> enumerator = entityReloaded.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many1.Name));
                enumerator.MoveNext();
                Assert.IsTrue(enumerator.Current.Name.Equals(one2Many2.Name));
                Assert.IsTrue(entityReloaded.One2OneEntity != null);
                Assert.IsTrue(entityReloaded.One2OneEntity.Name.Equals(one2One.Name));
                Assert.IsTrue(ErLayer.GetSharedInstance().Statistics.SelectQueryCount == 2);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_loadAndPersist_WithLazyOnWithoutFetchingLazyFields_shouldNotLoadLazyLoadingQueries()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.EnableStatistics = true;
                ErLayer.GetSharedInstance().Statistics.Reset();
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
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

                entity.Persist(connection);
                transaction.Commit();

                LazyTestRootEntity entityReloaded = new LazyTestRootEntity();
                LoadEntityWithId(connection, entityReloaded, id);

                transaction = connection.BeginTransaction();
                entity.Persist(connection);
                transaction.Commit();

                Assert.IsTrue(ErLayer.GetSharedInstance().Statistics.SelectQueryCount == 0);
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementLazyTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(IDbConnection connection, LazyTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from lazy_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }
    }
}
