using System;
using System.Data;
using DbGate.Persist.Support.InheritanceTest;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateInheritancePersistTests : AbstractDbGateTestBase, IDisposable
    {
        public const int TypeAttribute = 1;
        public const int TypeField = 2;
        public const int TypeExternal = 3;

        private const string DbName = "unit-testing-inheritance-persist";

        public DbGateInheritancePersistTests()
        {
            TestClass = typeof(DbGateInheritancePersistTests);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
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
            var sql = "Create table inheritance_test_super (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);
            
            sql = "Create table inheritance_test_suba (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname_a Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table inheritance_test_subb (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname_b Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            EndInit(DbName);
            return Connection;
        }

        private void ClearTables(IDbConnection connection)
        {
            try
            {
                var transaction = CreateTransaction(connection);

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM inheritance_test_super";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "delete from inheritance_test_suba";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "delete from inheritance_test_subb";
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (System.Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test cleanup.", ex);
            }
        }


        private void RegisterForExternal()
        {
            var objType = typeof (InheritanceTestSuperEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof (InheritanceTestSubEntityAExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof (InheritanceTestSubEntityBExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));
        }

        [Fact]
        public void Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded()
        {
            try
            {
                var types = new[] {TypeAttribute, TypeExternal, TypeField};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                var connection = SetupTables();

                for (var i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    var idA = idAs[i];
                    var idB = idBs[i];

                    switch (type)
                    {
                        case TypeAttribute:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With attributes");
                            break;
                        case TypeExternal:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With externals");
                            break;
                        case TypeField:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With fields");
                            break;
                    }
                    ClearTables(connection);

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TypeExternal)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    var transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    IInheritanceTestSuperEntity loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSuperEntity loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, loadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, loadedEntityB, idB);
                    transaction.Commit();

                    var compareResult = CompareEntities(entityA, loadedEntityA);
                    Assert.True(compareResult);
                    compareResult = CompareEntities(entityB, loadedEntityB);
                    Assert.True(compareResult);
                }
                connection.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded()
        {
            try
            {
                var types = new[] {TypeAttribute, TypeExternal, TypeField};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                var connection = SetupTables();

                for (var i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    var idA = idAs[i];
                    var idB = idBs[i];

                    switch (type)
                    {
                        case TypeAttribute:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With attributes");
                            break;
                        case TypeExternal:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With externals");
                            break;
                        case TypeField:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With fields");
                            break;
                    }

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TypeExternal)
                    {
                        RegisterForExternal();
                    }

                    ClearTables(connection);

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    var transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    var loadedEntityA = CreateObjectEmptyTypeA(type);
                    var loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, loadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, loadedEntityB, idB);

                    loadedEntityA.Name = "typeA-changed-name";
                    loadedEntityA.NameA = "changed-nameA";
                    loadedEntityA.Status = EntityStatus.Modified;

                    loadedEntityB.Name = "typeB-changed-name";
                    loadedEntityB.NameB = "changed-nameB";
                    loadedEntityB.Status = EntityStatus.Modified;

                    loadedEntityA.Persist(transaction);
                    loadedEntityB.Persist(transaction);

                    var reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    var reLoadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, reLoadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, reLoadedEntityB, idB);
                    transaction.Commit();

                    var compareResult = CompareEntities(loadedEntityA, reLoadedEntityA);
                    Assert.True(compareResult);
                    compareResult = CompareEntities(loadedEntityB, reLoadedEntityB);
                    Assert.True(compareResult);
                }

                connection.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete()
        {
            try
            {
                var types = new[] {TypeAttribute, TypeExternal, TypeField};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                var connection = SetupTables();

                for (var i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    var idA = idAs[i];
                    var idB = idBs[i];

                    switch (type)
                    {
                        case TypeAttribute:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With attributes");
                            break;
                        case TypeExternal:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With externals");
                            break;
                        case TypeField:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With fields");
                            break;
                    }

                    ClearTables(connection);

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TypeExternal)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    var transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    var loadedEntityA = CreateObjectEmptyTypeA(type);
                    var loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, loadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, loadedEntityB, idB);

                    loadedEntityA.Name = "typeA-changed-name";
                    loadedEntityA.NameA = "changed-nameA";
                    loadedEntityA.Status = EntityStatus.Deleted;

                    loadedEntityB.Name = "typeB-changed-name";
                    loadedEntityB.NameB = "changed-nameB";
                    loadedEntityB.Status = EntityStatus.Deleted;

                    loadedEntityA.Persist(transaction);
                    loadedEntityB.Persist(transaction);

                    var reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    var reLoadedEntityB = CreateObjectEmptyTypeB(type);

                    var reLoadedA = LoadEntityWithTypeA(transaction, reLoadedEntityA, idA);
                    var existesSuperA = ExistsSuper(transaction, idA);
                    var existesSubA = ExistsSubA(transaction, idA);
                    var reLoadedB = LoadEntityWithTypeB(transaction, reLoadedEntityB, idB);
                    var existesSuperB = ExistsSuper(transaction, idB);
                    var existesSubB = ExistsSubB(transaction, idB);

                    Assert.False(reLoadedA);
                    Assert.False(existesSuperA);
                    Assert.False(existesSubA);
                    Assert.False(reLoadedB);
                    Assert.False(existesSuperB);
                    Assert.False(existesSubB);
                    transaction.Commit();
                }

                connection.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithTypeA(ITransaction transaction, IInheritanceTestSuperEntity loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_suba where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, transaction);
                loaded = true;
            }

            return loaded;
        }

        private bool LoadEntityWithTypeB(ITransaction transaction, IInheritanceTestSuperEntity loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_subb where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, transaction);
                loaded = true;
            }

            return loaded;
        }

        private bool ExistsSuper(ITransaction transaction, int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_super where id_col = ?";

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

        private bool ExistsSubA(ITransaction transaction, int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_suba where id_col = ?";

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

        private bool ExistsSubB(ITransaction transaction, int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_subb where id_col = ?";

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

        private IInheritanceTestSubEntityA CreateObjectWithDataTypeA(int id, int type)
        {
            IInheritanceTestSubEntityA entity = null;
            entity = (type == TypeAttribute)
                         ? new InheritanceTestSubEntityAAttribute()
                         : (type == TypeField)
                               ? (IInheritanceTestSubEntityA) new InheritanceTestSubEntityAFields()
                               : new InheritanceTestSubEntityAExt();
            entity.IdCol = id;
            entity.Name = "typeA-name";
            entity.NameA = "typeA-nameA";

            return entity;
        }

        private IInheritanceTestSubEntityB CreateObjectWithDataTypeB(int id, int type)
        {
            IInheritanceTestSubEntityB entity = null;
            entity = (type == TypeAttribute)
                         ? new InheritanceTestSubEntityBAttributes()
                         : (type == TypeField)
                               ? (IInheritanceTestSubEntityB) new InheritanceTestSubEntityBFields()
                               : new InheritanceTestSubEntityBExt();
            entity.IdCol = id;
            entity.Name = "typeB-name";
            entity.NameB = "typeB-nameB";

            return entity;
        }

        private IInheritanceTestSubEntityA CreateObjectEmptyTypeA(int type)
        {
            IInheritanceTestSubEntityA entity = null;
            entity = (type == TypeAttribute)
                         ? new InheritanceTestSubEntityAAttribute()
                         : (type == TypeField)
                               ? (IInheritanceTestSubEntityA) new InheritanceTestSubEntityAFields()
                               : new InheritanceTestSubEntityAExt();
            return entity;
        }

        private IInheritanceTestSubEntityB CreateObjectEmptyTypeB(int type)
        {
            IInheritanceTestSubEntityB entity = null;
            entity = (type == TypeAttribute)
                         ? new InheritanceTestSubEntityBAttributes()
                         : (type == TypeField)
                               ? (IInheritanceTestSubEntityB) new InheritanceTestSubEntityBFields()
                               : new InheritanceTestSubEntityBExt();
            return entity;
        }

        private bool CompareEntities(IInheritanceTestSuperEntity entityA, IInheritanceTestSuperEntity entityB)
        {
            var result = entityA.IdCol == entityB.IdCol;
            result &= entityA.Name.Equals(entityB.Name);

            if (entityA is IInheritanceTestSubEntityA
                && entityB is IInheritanceTestSubEntityA)
            {
                result &=
                    ((IInheritanceTestSubEntityA) entityA).NameA.Equals(((IInheritanceTestSubEntityA) entityB).NameA);
            }
            else if (entityA is IInheritanceTestSubEntityB
                     && entityB is IInheritanceTestSubEntityB)
            {
                result &=
                    ((IInheritanceTestSubEntityB) entityA).NameB.Equals(((IInheritanceTestSubEntityB) entityB).NameB);
            }
            else
            {
                return false;
            }

            return result;
        }
    }
}