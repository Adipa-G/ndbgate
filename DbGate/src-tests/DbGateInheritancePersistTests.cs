using System;
using System.Data;
using System.IO;
using DbGate.ErManagement.ErMapper;
using DbGate.Support.Persistant.InheritanceTest;
using NUnit.Framework;
using log4net;
using log4net.Config;

namespace DbGate
{
    public class DbGateInheritancePersistTests : AbstractDbGateTestBase
    {
        public const int TYPE_ANNOTATION = 1;
        public const int TYPE_FIELD = 2;
        public const int TYPE_EXTERNAL = 3;

        private const string DBName = "unit-testing-inheritance-persist";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateInheritancePersistTests);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        private IDbConnection SetupTables()
        {
            string sql = "Create table inheritance_test_super (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);
            
            sql = "Create table inheritance_test_suba (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname_a Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table inheritance_test_subb (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname_b Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);

            EndInit(DBName);
            return Connection;
        }

        private void ClearTables(IDbConnection connection)
        {
            try
            {
                ITransaction transaction = CreateTransaction(connection);

                IDbCommand command = connection.CreateCommand();
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
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test cleanup.", ex);
            }
        }


        private void RegisterForExternal()
        {
            Type objType = typeof (InheritanceTestSuperEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof (InheritanceTestSubEntityAExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof (InheritanceTestSubEntityBExt);
            TransactionFactory.DbGate.RegisterEntity(objType, InheritanceTestExtFactory.GetTableInfo(objType),
                                                      InheritanceTestExtFactory.GetFieldInfo(objType));
        }

        [Test]
        public void Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded()
        {
            try
            {
                var types = new[] {TYPE_ANNOTATION, TYPE_EXTERNAL, TYPE_FIELD};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Insert_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With fields");
                            break;
                    }
                    ClearTables(connection);

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    ITransaction transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    IInheritanceTestSuperEntity loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSuperEntity loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, loadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, loadedEntityB, idB);
                    transaction.Commit();

                    bool compareResult = CompareEntities(entityA, loadedEntityA);
                    Assert.IsTrue(compareResult);
                    compareResult = CompareEntities(entityB, loadedEntityB);
                    Assert.IsTrue(compareResult);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded()
        {
            try
            {
                var types = new[] {TYPE_ANNOTATION, TYPE_EXTERNAL, TYPE_FIELD};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Update_WithAllModesWithBothSubClasses_ShouldEqualWhenLoaded With fields");
                            break;
                    }

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    ClearTables(connection);

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    ITransaction transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    IInheritanceTestSubEntityA loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB loadedEntityB = CreateObjectEmptyTypeB(type);
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

                    IInheritanceTestSubEntityA reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB reLoadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(transaction, reLoadedEntityA, idA);
                    LoadEntityWithTypeB(transaction, reLoadedEntityB, idB);
                    transaction.Commit();

                    bool compareResult = CompareEntities(loadedEntityA, reLoadedEntityA);
                    Assert.IsTrue(compareResult);
                    compareResult = CompareEntities(loadedEntityB, reLoadedEntityB);
                    Assert.IsTrue(compareResult);
                }

                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete()
        {
            try
            {
                var types = new[] {TYPE_ANNOTATION, TYPE_EXTERNAL, TYPE_FIELD};
                var idAs = new[] {35, 45, 55};
                var idBs = new[] {36, 46, 56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Info(
                                "Inheritance_Delete_WithAllModesWithBothSubClasses_ShouldDelete With fields");
                            break;
                    }

                    ClearTables(connection);

                    TransactionFactory.DbGate.ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA, type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB, type);

                    ITransaction transaction = CreateTransaction(connection);
                    entityA.Persist(transaction);
                    entityB.Persist(transaction);
                    transaction.Commit();

                    transaction = CreateTransaction(connection);
                    IInheritanceTestSubEntityA loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB loadedEntityB = CreateObjectEmptyTypeB(type);
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

                    IInheritanceTestSubEntityA reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB reLoadedEntityB = CreateObjectEmptyTypeB(type);

                    bool reLoadedA = LoadEntityWithTypeA(transaction, reLoadedEntityA, idA);
                    bool existesSuperA = ExistsSuper(transaction, idA);
                    bool existesSubA = ExistsSubA(transaction, idA);
                    bool reLoadedB = LoadEntityWithTypeB(transaction, reLoadedEntityB, idB);
                    bool existesSuperB = ExistsSuper(transaction, idB);
                    bool existesSubB = ExistsSubB(transaction, idB);

                    Assert.IsFalse(reLoadedA);
                    Assert.IsFalse(existesSuperA);
                    Assert.IsFalse(existesSubA);
                    Assert.IsFalse(reLoadedB);
                    Assert.IsFalse(existesSuperB);
                    Assert.IsFalse(existesSubB);
                    transaction.Commit();
                }

                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithTypeA(ITransaction transaction, IInheritanceTestSuperEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_suba where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, transaction);
                loaded = true;
            }

            return loaded;
        }

        private bool LoadEntityWithTypeB(ITransaction transaction, IInheritanceTestSuperEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_subb where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, transaction);
                loaded = true;
            }

            return loaded;
        }

        private bool ExistsSuper(ITransaction transaction, int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_super where id_col = ?";

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

        private bool ExistsSubA(ITransaction transaction, int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_suba where id_col = ?";

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

        private bool ExistsSubB(ITransaction transaction, int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_subb where id_col = ?";

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

        private IInheritanceTestSubEntityA CreateObjectWithDataTypeA(int id, int type)
        {
            IInheritanceTestSubEntityA entity = null;
            entity = (type == TYPE_ANNOTATION)
                         ? new InheritanceTestSubEntityAAnnotations()
                         : (type == TYPE_FIELD)
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
            entity = (type == TYPE_ANNOTATION)
                         ? new InheritanceTestSubEntityBAnnotations()
                         : (type == TYPE_FIELD)
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
            entity = (type == TYPE_ANNOTATION)
                         ? new InheritanceTestSubEntityAAnnotations()
                         : (type == TYPE_FIELD)
                               ? (IInheritanceTestSubEntityA) new InheritanceTestSubEntityAFields()
                               : new InheritanceTestSubEntityAExt();
            return entity;
        }

        private IInheritanceTestSubEntityB CreateObjectEmptyTypeB(int type)
        {
            IInheritanceTestSubEntityB entity = null;
            entity = (type == TYPE_ANNOTATION)
                         ? new InheritanceTestSubEntityBAnnotations()
                         : (type == TYPE_FIELD)
                               ? (IInheritanceTestSubEntityB) new InheritanceTestSubEntityBFields()
                               : new InheritanceTestSubEntityBExt();
            return entity;
        }

        private bool CompareEntities(IInheritanceTestSuperEntity entityA, IInheritanceTestSuperEntity entityB)
        {
            bool result = entityA.IdCol == entityB.IdCol;
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