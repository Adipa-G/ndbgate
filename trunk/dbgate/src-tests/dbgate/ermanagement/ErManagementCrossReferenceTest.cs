using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.persistant.crossreference;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementCrossReferenceTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementCrossReferenceTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementCrossReferenceTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (ErManagementCrossReferenceTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                DbGate.GetSharedInstance().ClearCache();
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
                command.CommandText = "DELETE FROM cross_reference_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table cross_reference_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM cross_reference_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table cross_reference_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM cross_reference_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table cross_reference_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementCrossReferenceTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            string sql = "Create table cross_reference_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table cross_reference_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table cross_reference_test_one2one (\n" +
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
        public void CrossReference_PersistWithOne2OneChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                CrossReferenceTestRootEntity entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                CrossReferenceTestOne2OneEntity one2OneEntity = new CrossReferenceTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Entity";
                one2OneEntity.RootEntity = entity;
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(connection);
                transaction.Commit();

                CrossReferenceTestRootEntity loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                Assert.IsNotNull(loadedEntity);
                Assert.IsNotNull(loadedEntity.One2OneEntity);
                Assert.IsNotNull(loadedEntity.One2OneEntity.RootEntity);
                Assert.IsTrue(loadedEntity == loadedEntity.One2OneEntity.RootEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementCrossReferenceTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CrossReference_PersistWithOne2ManyChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                DbGate.GetSharedInstance().Config.AutoTrackChanges = true;
            
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                CrossReferenceTestRootEntity entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                CrossReferenceTestOne2ManyEntity one2ManyEntity = new CrossReferenceTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo =1;
                one2ManyEntity.Name = "Child-Entity";
                one2ManyEntity.RootEntity = entity;
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(connection);
                transaction.Commit();

                CrossReferenceTestRootEntity loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                Assert.IsNotNull(loadedEntity);
                Assert.IsTrue(loadedEntity.One2ManyEntities.Count == 1);
                IEnumerator<CrossReferenceTestOne2ManyEntity> childEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumerator.MoveNext();
                CrossReferenceTestOne2ManyEntity childOne2ManyEntity = childEnumerator.Current;
                Assert.IsNotNull(childOne2ManyEntity);
                Assert.IsTrue(loadedEntity == childOne2ManyEntity.RootEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementCrossReferenceTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(IDbConnection connection, CrossReferenceTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from cross_reference_test_root where id_col = ?";

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