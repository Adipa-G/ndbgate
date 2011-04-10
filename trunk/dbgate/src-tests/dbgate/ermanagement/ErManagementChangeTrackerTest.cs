using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.persistant.changetracker;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementChangeTrackerTest
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementChangeTrackerTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementChangeTrackerTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (ErManagementChangeTrackerTest)).Fatal("Exception during test cleanup.", ex);
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
                command.CommandText = "DELETE FROM change_tracker_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table change_tracker_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM change_tracker_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table change_tracker_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM change_tracker_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table change_tracker_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            String sql = "Create table change_tracker_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table change_tracker_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table change_tracker_test_one2one (\n" +
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
        public void ERLayer_changeField_WithAutoTrackChangesOn_shouldUpdateTheEntityInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);

                transaction = connection.BeginTransaction();
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reloadedEntity,id);
                connection.Close();

                Assert.AreEqual(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_changeField_WithAutoTrackChangesOff_shouldNotUpdateTheEntityInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);

                transaction = connection.BeginTransaction();
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reloadedEntity,id);
                connection.Close();

                Assert.AreEqual(entity.Name, reloadedEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_removeOneToOneChild_WithAutoTrackChangesOn_shouldDeleteChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2OneChild(connection,id);
                connection.Close();

                Assert.IsFalse(hasOneToOne);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_removeOneToOneChild_WithAutoTrackChangesOff_DeleteChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2OneChild(connection,id);
                connection.Close();

                Assert.IsFalse(hasOneToOne);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_changeOneToOneChild_WithAutoTrackChangesOn_shouldUpdateChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reLoadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                Assert.AreEqual(loadedEntity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_changeOneToOneChild_WithAutoTrackChangesOff_shouldNotUpdateChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reLoadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                Assert.AreEqual(entity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_removeOneToManyChild_WithAutoTrackChangesOn_shouldDeleteChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOne2Many = ExistsOne2ManyChild(connection,id);
                connection.Close();

                Assert.IsFalse(hasOne2Many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_removeOneToManyChild_WithAutoTrackChangesOff_shouldDeleteChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOne2Many = ExistsOne2ManyChild(connection,id);
                connection.Close();

                Assert.IsFalse(hasOne2Many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_changeOneToManyChild_WithAutoTrackChangesOn_shouldUpdateChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                IEnumerator<ChangeTrackerTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reloadedEntity,id);
                enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity reLoadedChild = enumerator.Current;
                connection.Close();

                Assert.AreEqual(loadedChild.Name, reLoadedChild.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_changeOneToManyChild_WithAutoTrackChangesOff_shouldNotUpdateChildInDb()
        {
            try
            {
                ErLayer.GetSharedInstance().Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                IEnumerator<ChangeTrackerTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(connection);
                transaction.Commit();

                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(connection,reloadedEntity,id);
                enumerator = reloadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity reLoadedChild = enumerator.Current;
                connection.Close();

                Assert.AreEqual(reLoadedChild.Name, one2ManyEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(ErManagementChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(IDbConnection connection, ChangeTrackerTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from change_tracker_test_root where id_col = ?";

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

        private bool ExistsOne2OneChild(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from change_tracker_test_one2one where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                exists = true;
            }
            return exists;
        }

        private bool ExistsOne2ManyChild(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from change_tracker_test_one2many where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                exists = true;
            }
            return exists;
        }
    }
}
