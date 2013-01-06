using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.exceptions;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.persistant.version;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementVersionTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementVersionTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementVersionTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (ErManagementVersionTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErLayer.GetSharedInstance().ClearCache();
            }
            ErLayer.GetSharedInstance().Config.UpdateChangedColumnsOnly = false;
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM version_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table version_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM version_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table version_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM version_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table version_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            String sql = "Create table version_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         "\tversion Int NOT NULL,\n" +
                         " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table version_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table version_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        [Test]
        public void ERLayer_persistTwice_WithVersionColumnEntity_shouldNotThrowException()
        {
            try
            {
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 35;
                VersionColumnTestRootEntity entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                entity.Persist(connection);
                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_persistTwice_WithoutVersionColumnEntity_shouldNotThrowException()
        {
            try
            {
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                entity.Persist(connection);
                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_persistWithTwoChanges_WithoutUpdateChangedColumnsOnly_shouldThrowException()
        {
            IDbConnection connection = SetupTables();

            IDbTransaction transaction = connection.BeginTransaction();
            int id = 45;
            VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Version = 1;
            entity.Persist(connection);
            transaction.Commit();
            
            transaction = connection.BeginTransaction();
            VersionGeneralTestRootEntity loadedEntityA = new VersionGeneralTestRootEntity();
            VersionGeneralTestRootEntity loadedEntityB = new VersionGeneralTestRootEntity();
            LoadWithoutVersionColumnEntityWithId(connection,loadedEntityA, entity.IdCol);
            LoadWithoutVersionColumnEntityWithId(connection,loadedEntityB, entity.IdCol);
            transaction.Commit();

            transaction = connection.BeginTransaction();
            loadedEntityA.Name ="Mod Name";
            loadedEntityA.Persist(connection);

            loadedEntityB.Version = loadedEntityB.Version + 1;
            loadedEntityB.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        public void ERLayer_persistWithTwoChanges_WithUpdateChangedColumnsOnly_shouldNotThrowException()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.UpdateChangedColumnsOnly = true;
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Version = 1;
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionGeneralTestRootEntity loadedEntityA = new VersionGeneralTestRootEntity();
                VersionGeneralTestRootEntity loadedEntityB = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(connection, loadedEntityA, entity.IdCol);
                LoadWithoutVersionColumnEntityWithId(connection, loadedEntityB, entity.IdCol);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                loadedEntityA.Name = "Mod Name";
                loadedEntityA.Persist(connection);

                loadedEntityB.Version = loadedEntityB.Version + 1;
                loadedEntityB.Persist(connection);
                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_rootUpdateFromAnotherTransaction_WithVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionColumnTestRootEntity entity = null;

            try
            {
                transaction = connection.BeginTransaction();
                int id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(connection,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            entity.Name ="New Name2";;
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_rootUpdateFromAnotherTransaction_WithOutVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 65;
                transaction = connection.BeginTransaction();
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(connection,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            entity.Name ="New Name2";;
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_one2oneChildUpdateFromAnotherTransaction_WithVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionColumnTestRootEntity entity = null;

            try
            {
                int id = 55;
                transaction = connection.BeginTransaction();
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionColumnTestOne2OneEntity one2OneEntity = new VersionColumnTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            entity.One2OneEntity.Name ="Modified2 One2One";
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_one2oneChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 55;
                transaction = connection.BeginTransaction();
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionGeneralTestOne2OneEntity one2OneEntity = new VersionGeneralTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity = one2OneEntity;
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            entity.One2OneEntity.Name = "Modified2 One2One";
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_one2manyChildUpdateFromAnotherTransaction_WithVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionColumnTestRootEntity entity = null;

            try
            {
                int id = 55;
                transaction = connection.BeginTransaction();
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionColumnTestOne2ManyEntity one2ManyEntityOrg = new VersionColumnTestOne2ManyEntity();
                one2ManyEntityOrg.Name = "One2Many";
                one2ManyEntityOrg.IndexNo = 1; ;
                entity.One2ManyEntities.Add(one2ManyEntityOrg);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(connection,loadedEntity,id);
                IEnumerator<VersionColumnTestOne2ManyEntity> loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                VersionColumnTestOne2ManyEntity loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name ="Modified One2Many";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            IEnumerator<VersionColumnTestOne2ManyEntity> orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            VersionColumnTestOne2ManyEntity orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ERLayer_one2manyChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_shouldThrowException()
        {
            IDbConnection connection = SetupTables();
            IDbTransaction transaction = null;
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 55;
                transaction = connection.BeginTransaction();
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionGeneralTestOne2ManyEntity orgOne2ManyEntityOrg = new VersionGeneralTestOne2ManyEntity();
                orgOne2ManyEntityOrg.Name = "One2Many";
                orgOne2ManyEntityOrg.IndexNo = 1;
                entity.One2ManyEntities.Add(orgOne2ManyEntityOrg);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(connection,loadedEntity,id);

                IEnumerator<VersionGeneralTestOne2ManyEntity> loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                VersionGeneralTestOne2ManyEntity loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name = "Modified One2Many";
                loadedEntity.Persist(connection);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = connection.BeginTransaction();
            IEnumerator<VersionGeneralTestOne2ManyEntity> orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            VersionGeneralTestOne2ManyEntity orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            entity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }

        private bool LoadWithVersionColumnEntityWithId(IDbConnection connection, VersionColumnTestRootEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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

        private bool LoadWithoutVersionColumnEntityWithId(IDbConnection connection, VersionGeneralTestRootEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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
