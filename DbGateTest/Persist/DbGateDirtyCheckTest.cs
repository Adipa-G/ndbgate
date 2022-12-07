using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using DbGate.Context;
using DbGate.Persist.Support.DirtyCheck;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateDirtyCheckTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-dirty-check";

        public DbGateDirtyCheckTest()
        {
            TestClass = typeof(DbGateDirtyCheckTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);   
        }
        
        private IDbConnection SetupTables()
        {
            var sql = "Create table dirty_check_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table dirty_check_test_one2many (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tindex_no Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table dirty_check_test_one2one (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOn_ShouldUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.Equal(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOnAndClearTracker_ShouldUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();

                var changeTracker = loadedEntity.Context.ChangeTracker;
                changeTracker.GetType().GetField("fields",BindingFlags.Instance|BindingFlags.NonPublic)
                    .SetValue(changeTracker,new ReadOnlyCollection<EntityFieldValue>(new List<EntityFieldValue>()));
                changeTracker.GetType().GetField("childEntityRelationKeys",BindingFlags.Instance|BindingFlags.NonPublic)
                    .SetValue(changeTracker,new ReadOnlyCollection<ITypeFieldValueList>(new List<ITypeFieldValueList>()));
                
                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction, reloadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.Equal(loadedEntity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeField_WithAutoTrackChangesOff_ShouldNotUpdateTheEntityInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                loadedEntity.Name = "Changed-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.Equal(entity.Name, reloadedEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2OneChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToOne);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_RemoveOneToOneChild_WithAutoTrackChangesOff_DeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity = null;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2OneChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToOne);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reLoadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.Equal(loadedEntity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeOneToOneChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.One2OneEntity =new DirtyCheckTestOne2OneEntity();
                entity.One2OneEntity.Name = "Child-Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reLoadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.Equal(entity.One2OneEntity.Name,reLoadedEntity.One2OneEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOn_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOne2Many);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_RemoveOneToManyChild_WithAutoTrackChangesOff_ShouldDeleteChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2ManyEntities.Clear();
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOne2Many);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOn_ShouldUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                var enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                var loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                var reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.Equal(loadedChild.Name, reLoadedChild.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ChangeTracker_ChangeOneToManyChild_WithAutoTrackChangesOff_ShouldNotUpdateChildInDb()
        {
            try
            {
                TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new DirtyCheckTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";

                var one2ManyEntity = new DirtyCheckTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                var enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                var loadedChild = enumerator.Current;
                loadedChild.Name = "Child-Upd-Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var reloadedEntity = new DirtyCheckTestRootEntity();
                LoadEntityWithId(transaction,reloadedEntity,id);
                enumerator = reloadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();
                var reLoadedChild = enumerator.Current;
                transaction.Commit();
                connection.Close();

                Assert.Equal(reLoadedChild.Name, one2ManyEntity.Name);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateDirtyCheckTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, DirtyCheckTestRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from dirty_check_test_root where id_col = ?";

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

        private bool ExistsOne2OneChild(ITransaction transaction,int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from dirty_check_test_one2one where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                exists = true;
            }
            return exists;
        }

        private bool ExistsOne2ManyChild(ITransaction transaction,int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from dirty_check_test_one2many where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                exists = true;
            }
            return exists;
        }
    }
}
