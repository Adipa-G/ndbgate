using System;
using System.Data;
using DbGate.Patch;
using DbGate.Persist.Support.ColumnTest;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateColumnPersistTests :  AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-column-persist";

        public DbGateColumnPersistTests()
        {
            TestClass = typeof(DbGateColumnPersistTests);
            BeginInit(DbName);
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
        
        private IDbConnection SetupTables()
        {
            var sql = "Create table column_test_entity (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tlong_not_null Bigint NOT NULL,\n" +
                      "\tlong_null Bigint,\n" +
                      "\tboolean_not_null SmallInt NOT NULL,\n" +
                      "\tboolean_null SmallInt,\n" +
                      "\tchar_not_null Char(1) NOT NULL,\n" +
                      "\tchar_null Char(1),\n" +
                      "\tint_not_null Int NOT NULL,\n" +
                      "\tint_null Int,\n" +
                      "\tdate_not_null Date NOT NULL,\n" +
                      "\tdate_null Date,\n" +
                      "\tdouble_not_null Double NOT NULL,\n" +
                      "\tdouble_null Double,\n" +
                      "\tfloat_not_null Float NOT NULL,\n" +
                      "\tfloat_null Float,\n" +
                      "\ttimestamp_not_null Timestamp NOT NULL,\n" +
                      "\ttimestamp_null Timestamp,\n" +
                      "\tvarchar_not_null Varchar(20) NOT NULL,\n" +
                      "\tvarchar_null Varchar(20),\n" +
                      "\tguid_not_null Varchar(36) NOT NULL,\n" +
                      "\tguid_null Varchar(36),\n" +
                      " Primary Key (id_col))";
            
            CreateTableFromSql(sql,DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void ColumnPersist_Insert_WithFieldsDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(entity);

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Insert_WithExtsDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(entity);

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Insert_WithAttributesDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityAttribute();
                
                CreateEntityWithNonNullValues(entity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Insert_WithFieldsDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityFields();
                
                CreateEntityWithNullValues(entity);

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Insert_WithExtsDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));
 
                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityExts();
                
                CreateEntityWithNullValues(entity);

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Insert_WithAttributesDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity entity = new ColumnTestEntityAttribute();
                CreateEntityWithNullValues(entity);

                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id =(int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status =EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var type = typeof (ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status =EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithAttributesDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityAttribute();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithAttributesDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityAttribute();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithAttributesDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityAttribute();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var id =(int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Update_WithAttributesDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityAttribute();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Delete_WithFieldsDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                var connection = SetupTables();
                 var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                var  loaded = LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.False(loaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Delete_WithExtsDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var type = typeof(ColumnTestEntityExts);
                TransactionFactory.DbGate.RegisterEntity(type, ColumnTestExtFactory.GetTableInfo(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                var loaded = LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.False(loaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ColumnPersist_Delete_WithAttributesDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(transaction);
                IColumnTestEntity newEntity = new ColumnTestEntityAttribute();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity loadedEntity = new ColumnTestEntityAttribute();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAttribute();
                var  loaded = LoadEntityWithId(transaction,reLoadedEntity,id);
                transaction.Commit();
                connection.Close();

                Assert.False(loaded);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, IColumnTestEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from column_test_entity where id_col = ?";

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

        private void CreateEntityWithNonNullValues(IColumnTestEntity entity)
        {
            entity.BooleanNotNull=true;
            entity.BooleanNull=true;
            entity.CharNotNull='A';
            entity.CharNull='B';
            entity.DateNotNull= DateTime.Now;
            entity.DateNull=DateTime.Now;
            entity.DoubleNotNull=5D;
            entity.DoubleNull=6D;
            entity.FloatNotNull=20F;
            entity.FloatNull=20F;
            entity.IntNotNull=24;
            entity.IntNull=23;
            entity.LongNotNull=356L;
            entity.LongNull=326L;
            entity.TimestampNotNull=DateTime.Now;
            entity.TimestampNull=DateTime.Now;
            entity.VarcharNotNull="notNull";
            entity.VarcharNull="null";
            entity.GuidNotNull = Guid.NewGuid();
            entity.GuidNull = Guid.NewGuid();
        }

        private void CreateEntityWithNullValues(IColumnTestEntity entity)
        {
            entity.BooleanNotNull=true;
            entity.BooleanNull=null;
            entity.CharNotNull='A';
            entity.CharNull=null;
            entity.DateNotNull=DateTime.Now;
            entity.DateNull=null;
            entity.DoubleNotNull=5D;
            entity.DoubleNull=null;
            entity.FloatNotNull=20F;
            entity.FloatNull=null;
            entity.IntNotNull=24;
            entity.IntNull=null;
            entity.LongNotNull=356L;
            entity.LongNull=null;
            entity.TimestampNotNull=DateTime.Now;
            entity.TimestampNull=null;
            entity.VarcharNotNull="notNull";
            entity.VarcharNull=null;
            entity.GuidNotNull = Guid.NewGuid();
            entity.GuidNull = null;
        }

        private void UpdateEntityWithNonNullValues(IColumnTestEntity entity)
        {
            entity.BooleanNotNull=false;
            entity.BooleanNull=false;
            entity.CharNotNull='C';
            entity.CharNull='D';
            entity.DateNotNull=DateTime.Now;
            entity.DateNull=DateTime.Now;
            entity.DoubleNotNull=53D;
            entity.DoubleNull=65D;
            entity.FloatNotNull=20465F;
            entity.FloatNull=32420F;
            entity.IntNotNull=35424;
            entity.IntNull=46723;
            entity.LongNotNull=3565535L;
            entity.LongNull=2245326L;
            entity.TimestampNotNull=DateTime.Now;
            entity.TimestampNull = DateTime.Now;
            entity.VarcharNotNull="notNull string";
            entity.VarcharNull="null string";
            entity.GuidNotNull = Guid.NewGuid();
            entity.GuidNull = Guid.NewGuid();
        }

        private void UpdateEntityWithNullValues(IColumnTestEntity entity)
        {
            entity.BooleanNotNull=false;
            entity.BooleanNull=null;
            entity.CharNotNull='C';
            entity.CharNull=null;
            entity.DateNotNull = DateTime.Now;
            entity.DateNull=null;
            entity.DoubleNotNull=53D;
            entity.DoubleNull=null;
            entity.FloatNotNull=20465F;
            entity.FloatNull=null;
            entity.IntNotNull=35424;
            entity.IntNull=null;
            entity.LongNotNull=3565535L;
            entity.LongNull=null;
            entity.TimestampNotNull = DateTime.Now;
            entity.TimestampNull=null;
            entity.VarcharNotNull="notNull string";
            entity.VarcharNull=null;
            entity.GuidNotNull = Guid.NewGuid();
            entity.GuidNull = null;
        }

        private void AssertTwoEntitiesEquals(IColumnTestEntity entityA, IColumnTestEntity entityB)
        {
            Assert.Equal(entityA.IdCol,entityB.IdCol);

            Assert.Equal(entityA.CharNotNull,entityB.CharNotNull);
            Assert.Equal(entityA.CharNull,entityB.CharNull);
            Assert.Equal(entityA.DateNotNull,entityB.DateNotNull);
            Assert.Equal(entityA.DateNull,entityB.DateNull);
            Assert.Equal(entityA.DoubleNotNull,entityB.DoubleNotNull);
            Assert.Equal(entityA.DoubleNull,entityB.DoubleNull);
            Assert.Equal(entityA.FloatNotNull,entityB.FloatNotNull);
            Assert.Equal(entityA.FloatNull,entityB.FloatNull);
            Assert.Equal(entityA.IntNotNull,entityB.IntNotNull);
            Assert.Equal(entityA.IntNull,entityB.IntNull);
            Assert.Equal(entityA.LongNotNull,entityB.LongNotNull);
            Assert.Equal(entityA.LongNull,entityB.LongNull);
            Assert.Equal(entityA.TimestampNotNull,entityB.TimestampNotNull);
            Assert.Equal(entityA.TimestampNull,entityB.TimestampNull);
            Assert.Equal(entityA.VarcharNotNull,entityB.VarcharNotNull);
            Assert.Equal(entityA.VarcharNull,entityB.VarcharNull);
            Assert.Equal(entityA.GuidNotNull,entityB.GuidNotNull);
            Assert.Equal(entityA.GuidNull,entityB.GuidNull);
        }
    }
}