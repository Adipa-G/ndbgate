using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using DbGate.Context;
using DbGate.Support.Persistant.ChangeTracker;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateChangeTrackerTest
    {
        private static ITransactionFactory _transactionFactory;

        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateChangeTrackerTest)).Info("Starting in-memory database for unit tests");
                _transactionFactory = new DefaultTransactionFactory("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DefaultTransactionFactory.DbSqllite);
                Assert.IsNotNull(_transactionFactory.CreateTransaction());
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateChangeTrackerTest)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (DbGateChangeTrackerTest)).Fatal("Exception during test cleanup.", ex);
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
                command.CommandText = "DELETE FROM change_tracker_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table change_tracker_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM change_tracker_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table change_tracker_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM change_tracker_test_one2one";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table change_tracker_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            ITransaction transaction = _transactionFactory.CreateTransaction();
            IDbConnection connection = transaction.Connection;

            string sql = "Create table change_tracker_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table change_tracker_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table change_tracker_test_one2one (\n" +
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
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOn_ShouldUpdateTheEntityInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOnAndClearTracker_ShouldUpdateTheEntityInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();

                var changeTracker = loadedEntity.Context.ChangeTracker;
                changeTracker.GetType().GetField("_fields",BindingFlags.Instance|BindingFlags.NonPublic)
                    .SetValue(changeTracker,new ReadOnlyCollection<EntityFieldValue>(new List<EntityFieldValue>()));
                changeTracker.GetType().GetField("_childEntityRelationKeys",BindingFlags.Instance|BindingFlags.NonPublic)
                    .SetValue(changeTracker,new ReadOnlyCollection<ITypeFieldValueList>(new List<ITypeFieldValueList>()));
                
                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOff_ShouldNotUpdateTheEntityInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(entity.Name, reloadedEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2OneChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToOne);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOff_DeleteChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2OneChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToOne);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reLoadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new ChangeTrackerTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reLoadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(entity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOne2Many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOff_ShouldDeleteChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOne2Many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                IEnumerator<ChangeTrackerTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedChild.Name, reLoadedChild.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                _transactionFactory.DbGate.Config.AutoTrackChanges = false;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ChangeTrackerTestRootEntity entity = new ChangeTrackerTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                ChangeTrackerTestOne2ManyEntity one2ManyEntity = new ChangeTrackerTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity loadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                IEnumerator<ChangeTrackerTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ChangeTrackerTestRootEntity reloadedEntity = new ChangeTrackerTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = reloadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                ChangeTrackerTestOne2ManyEntity reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(reLoadedChild.Name, one2ManyEntity.Name);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateChangeTrackerTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, ChangeTrackerTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from change_tracker_test_root where id_col = ?";

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

        private bool ExistsOne2OneChild(ITransaction transaction,int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
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

        private bool ExistsOne2ManyChild(ITransaction transaction,int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
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
