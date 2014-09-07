using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using DbGate.Context;
using DbGate.Persist.Support.DirtyCheck;
using log4net;
using NUnit.Framework;

namespace DbGate.Persist
{
    public class DbGateDirtyCheckTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-dirty-check";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateDirtyCheckTest);
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
            string sql = "Create table dirty_check_test_root (\n" +
                             "\tid_col Int NOT NULL,\n" +
                             "\tname Varchar(20) NOT NULL,\n" +
                             " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table dirty_check_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table dirty_check_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);
            EndInit(DBName);

            return Connection;
        }

        [Test]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOn_ShouldUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOnAndClearTracker_ShouldUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
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
                DirtyCheckTestRootEntity reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOff_ShouldNotUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(entity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
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
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOff_DeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
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
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reLoadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedEntity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reLoadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(entity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                DirtyCheckTestOne2ManyEntity one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
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
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOff_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                DirtyCheckTestOne2ManyEntity one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
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
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                DirtyCheckTestOne2ManyEntity one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                IEnumerator<DirtyCheckTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                DirtyCheckTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                DirtyCheckTestOne2ManyEntity reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(loadedChild.Name, reLoadedChild.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                DirtyCheckTestRootEntity entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                DirtyCheckTestOne2ManyEntity one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                IEnumerator<DirtyCheckTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                DirtyCheckTestOne2ManyEntity loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                DirtyCheckTestRootEntity reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = reloadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                DirtyCheckTestOne2ManyEntity reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.AreEqual(reLoadedChild.Name, one2ManyEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, DirtyCheckTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from dirty_check_test_root where id_col = ?";

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
            cmd.CommandText = "select * from dirty_check_test_one2one where id_col = ?";

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
            cmd.CommandText = "select * from dirty_check_test_one2many where id_col = ?";

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
