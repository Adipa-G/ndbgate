using System;
using System.Data;
using System.IO;
using DbGate.Support.Persistant.ColumnTest;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateColumnPersistTests
    {
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateColumnPersistTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateColumnPersistTests)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (DbGatePatchEmptyDbTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().ClearCache();
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
                command.CommandText = "DELETE FROM column_test_entity";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table column_test_entity";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            string sql = "Create table column_test_entity (\n" +
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
                        " Primary Key (id_col))";


            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        [Test]
        public void ColumnPersist_Insert_WithFieldsDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(entity);

                entity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Insert_WithExtsDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(entity);

                entity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Insert_WithAnnotationsDifferentTypesWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityAnnotations();
                
                CreateEntityWithNonNullValues(entity);
                entity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Insert_WithFieldsDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityFields();
                
                CreateEntityWithNullValues(entity);

                entity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Insert_WithExtsDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));
 
                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityExts();
                
                CreateEntityWithNullValues(entity);

                entity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Insert_WithAnnotationsDifferentTypesWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity entity = new ColumnTestEntityAnnotations();
                CreateEntityWithNullValues(entity);

                entity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(entity,loadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id =(int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status =EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof (ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
               
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);
               
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status =EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
               
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithAnnotationsDifferentTypesStartWithoutNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityAnnotations();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
               
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);

                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithAnnotationsDifferentTypesStartWithoutNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityAnnotations();
                CreateEntityWithNonNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);
               
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithAnnotationsDifferentTypesStartWithNullEndWithoutNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityAnnotations();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);
                
                UpdateEntityWithNonNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();
                
                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithFieldsDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                int id =(int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);

                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithExtsDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);

                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Update_WithAnnotationsDifferentTypesStartWithNullEndWithNull_ShouldEqualWhenLoaded()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityAnnotations();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);

                UpdateEntityWithNullValues(loadedEntity);
                loadedEntity.Status = EntityStatus.Modified;

                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                AssertTwoEntitiesEquals(loadedEntity,reLoadedEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Delete_WithFieldsDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                IDbConnection connection = SetupTables();
                 IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityFields();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityFields();
                LoadEntityWithId(connection,loadedEntity,id);

                loadedEntity.Status = EntityStatus.Deleted;
                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityFields();
                bool  loaded = LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                Assert.IsFalse(loaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Delete_WithExtsDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                Type type = typeof(ColumnTestEntityExts);
                ErManagement.ErMapper.DbGate.GetSharedInstance().RegisterEntity(type, ColumnTestExtFactory.GetTableNames(type),
                                                           ColumnTestExtFactory.GetFieldInfo(type));

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityExts();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityExts();
                LoadEntityWithId(connection,loadedEntity,id);

                loadedEntity.Status = EntityStatus.Deleted;
                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityExts();
                bool loaded = LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                Assert.IsFalse(loaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ColumnPersist_Delete_WithAnnotationsDifferentTypesStartWithNullEndWithNull_ShouldDelete()
        {
            try
            {
                IDbConnection connection = SetupTables();
                IDbTransaction transaction = connection.BeginTransaction();

                int id = (int)new PrimaryKeyGenerator().GetNextSequenceValue(connection);
                IColumnTestEntity newEntity = new ColumnTestEntityAnnotations();
                CreateEntityWithNullValues(newEntity);
                newEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity loadedEntity = new ColumnTestEntityAnnotations();
                LoadEntityWithId(connection,loadedEntity,id);

                loadedEntity.Status = EntityStatus.Deleted;
                transaction = connection.BeginTransaction();
                loadedEntity.Persist(connection);
                transaction.Commit();

                IColumnTestEntity reLoadedEntity = new ColumnTestEntityAnnotations();
                bool  loaded = LoadEntityWithId(connection,reLoadedEntity,id);
                connection.Close();

                Assert.IsFalse(loaded);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGatePatchEmptyDbTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(IDbConnection connection, IColumnTestEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from column_test_entity where id_col = ?";

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
        }

        private void AssertTwoEntitiesEquals(IColumnTestEntity entityA, IColumnTestEntity entityB)
        {
            Assert.AreEqual(entityA.IdCol,entityB.IdCol);

            Assert.AreEqual(entityA.CharNotNull,entityB.CharNotNull);
            Assert.AreEqual(entityA.CharNull,entityB.CharNull);
            Assert.AreEqual(entityA.DateNotNull,entityB.DateNotNull);
            Assert.AreEqual(entityA.DateNull,entityB.DateNull);
            Assert.AreEqual(entityA.DoubleNotNull,entityB.DoubleNotNull);
            Assert.AreEqual(entityA.DoubleNull,entityB.DoubleNull);
            Assert.AreEqual(entityA.FloatNotNull,entityB.FloatNotNull);
            Assert.AreEqual(entityA.FloatNull,entityB.FloatNull);
            Assert.AreEqual(entityA.IntNotNull,entityB.IntNotNull);
            Assert.AreEqual(entityA.IntNull,entityB.IntNull);
            Assert.AreEqual(entityA.LongNotNull,entityB.LongNotNull);
            Assert.AreEqual(entityA.LongNull,entityB.LongNull);
            Assert.AreEqual(entityA.TimestampNotNull,entityB.TimestampNotNull);
            Assert.AreEqual(entityA.TimestampNull,entityB.TimestampNull);
            Assert.AreEqual(entityA.VarcharNotNull,entityB.VarcharNotNull);
            Assert.AreEqual(entityA.VarcharNull,entityB.VarcharNull);
        }
    }
}