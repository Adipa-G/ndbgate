using System;
using System.Data;
using System.IO;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.ermanagement.support.persistant.inheritancetest;
using log4net;
using NUnit.Framework;

namespace dbgate.ermanagement
{
    public class ErManagementInheritancePersistTests
    {
        public const int TYPE_ANNOTATION = 1;
        public const int TYPE_FIELD = 2;
        public const int TYPE_EXTERNAL = 3;

        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Fatal("Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Fatal("Exception during test cleanup.", ex);
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
                command.CommandText = "drop table inheritance_test_super";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table inheritance_test_suba";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table inheritance_test_subb";
                command.ExecuteNonQuery();
                transaction.Commit();

                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ErManagementInheritancePersistTests)).Fatal("Exception during test cleanup.", ex);
            }
        }

        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            string sql = "Create table inheritance_test_super (\n" +
                            "\tid_col Int NOT NULL,\n" +
                            "\tname Varchar(20) NOT NULL,\n" +
                            " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table inheritance_test_suba (\n" +
                        "\tid_col Int NOT NULL,\n" +
                        "\tname_a Varchar(20) NOT NULL,\n" +
                        " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table inheritance_test_subb (\n" +
                    "\tid_col Int NOT NULL,\n" +
                    "\tname_b Varchar(20) NOT NULL,\n" +
                    " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        private void ClearTables(IDbConnection connection)
        {
            try
            {
                IDbTransaction transaction = connection.BeginTransaction();
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
                LogManager.GetLogger(typeof(ErManagementInheritancePersistTests)).Fatal("Exception during test cleanup.", ex);
            }
        }


        private void RegisterForExternal()
        {
            Type objType = typeof(InheritanceTestSuperEntityExt);
            ErLayer.GetSharedInstance().RegisterTable(objType,InheritanceTestExtFactory.GetTableNames(objType));
            ErLayer.GetSharedInstance().RegisterFields(objType,InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof(InheritanceTestSubEntityAExt);
            ErLayer.GetSharedInstance().RegisterTable(objType,InheritanceTestExtFactory.GetTableNames(objType));
            ErLayer.GetSharedInstance().RegisterFields(objType,InheritanceTestExtFactory.GetFieldInfo(objType));

            objType = typeof(InheritanceTestSubEntityBExt);
            ErLayer.GetSharedInstance().RegisterTable(objType,InheritanceTestExtFactory.GetTableNames(objType));
            ErLayer.GetSharedInstance().RegisterFields(objType,InheritanceTestExtFactory.GetFieldInfo(objType));
        }

        [Test]
        public void ERLayer_insert_withAllModesWithBothSubClasses_shouldEqualWhenLoaded()
        {
            try
            {
                int[] types = new int[]{TYPE_ANNOTATION,TYPE_EXTERNAL,TYPE_FIELD};
                int[] idAs = new int[]{35,45,55};
                int[] idBs = new int[]{36,46,56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_insert_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_insert_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_insert_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With fields");
                            break;
                    }
                    ClearTables(connection);
                    
                    ErLayer.GetSharedInstance().ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA,type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB,type);

                    IDbTransaction transaction = connection.BeginTransaction();
                    entityA.Persist(connection);
                    entityB.Persist(connection);
                    transaction.Commit();

                    IInheritanceTestSuperEntity loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSuperEntity loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(connection,loadedEntityA,idA);
                    LoadEntityWithTypeB(connection,loadedEntityB,idB);
                    

                    bool compareResult = CompareEntities(entityA,loadedEntityA);
                    Assert.IsTrue(compareResult);
                    compareResult = CompareEntities(entityB,loadedEntityB);
                    Assert.IsTrue(compareResult);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_update_withAllModesWithBothSubClasses_shouldEqualWhenLoaded()
        {
            try
            {
                int[] types = new int[]{TYPE_ANNOTATION,TYPE_EXTERNAL,TYPE_FIELD};
                int[] idAs = new int[]{35,45,55};
                int[] idBs = new int[]{36,46,56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_update_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_update_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_update_withAllModesWithBothSubClasses_shouldEqualWhenLoaded With fields");
                            break;
                    }

                    ErLayer.GetSharedInstance().ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    ClearTables(connection);

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA,type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB,type);

                    IDbTransaction transaction = connection.BeginTransaction();
                    entityA.Persist(connection);
                    entityB.Persist(connection);
                    transaction.Commit();

                    IInheritanceTestSubEntityA loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(connection,loadedEntityA,idA);
                    LoadEntityWithTypeB(connection,loadedEntityB,idB);

                    loadedEntityA.Name = "typeA-changed-name";
                    loadedEntityA.NameA = "changed-nameA";
                    loadedEntityA.Status = DbClassStatus.Modified;

                    loadedEntityB.Name ="typeB-changed-name";
                    loadedEntityB.NameB= "changed-nameB";
                    loadedEntityB.Status = DbClassStatus.Modified;

                    loadedEntityA.Persist(connection);
                    loadedEntityB.Persist(connection);

                    IInheritanceTestSubEntityA reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB reLoadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(connection,reLoadedEntityA,idA);
                    LoadEntityWithTypeB(connection,reLoadedEntityB,idB);

                    bool compareResult = CompareEntities(loadedEntityA,reLoadedEntityA);
                    Assert.IsTrue(compareResult);
                    compareResult = CompareEntities(loadedEntityB,reLoadedEntityB);
                    Assert.IsTrue(compareResult);
                }

                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ERLayer_delete_withAllModesWithBothSubClasses_shouldDelete()
        {
            try
            {
                int[] types = new int[]{TYPE_ANNOTATION,TYPE_EXTERNAL,TYPE_FIELD};
                int[] idAs = new int[]{35,45,55};
                int[] idBs = new int[]{36,46,56};

                IDbConnection connection = SetupTables();

                for (int i = 0; i < types.Length; i++)
                {
                    int type = types[i];
                    int idA = idAs[i];
                    int idB = idBs[i];

                    switch (type)
                    {
                        case TYPE_ANNOTATION:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_delete_withAllModesWithBothSubClasses_shouldDelete With annotations");
                            break;
                        case TYPE_EXTERNAL:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_delete_withAllModesWithBothSubClasses_shouldDelete With externals");
                            break;
                        case TYPE_FIELD:
                            LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Info("ERLayer_delete_withAllModesWithBothSubClasses_shouldDelete With fields");
                            break;
                    }

                    ClearTables(connection);

                    ErLayer.GetSharedInstance().ClearCache();
                    if (type == TYPE_EXTERNAL)
                    {
                        RegisterForExternal();
                    }

                    IInheritanceTestSuperEntity entityA = CreateObjectWithDataTypeA(idA,type);
                    IInheritanceTestSuperEntity entityB = CreateObjectWithDataTypeB(idB,type);

                    IDbTransaction transaction = connection.BeginTransaction();
                    entityA.Persist(connection);
                    entityB.Persist(connection);
                    transaction.Commit();

                    IInheritanceTestSubEntityA loadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB loadedEntityB = CreateObjectEmptyTypeB(type);
                    LoadEntityWithTypeA(connection,loadedEntityA,idA);
                    LoadEntityWithTypeB(connection,loadedEntityB,idB);

                    loadedEntityA.Name = "typeA-changed-name";
                    loadedEntityA.NameA = "changed-nameA";
                    loadedEntityA.Status = DbClassStatus.Deleted;

                    loadedEntityB.Name = "typeB-changed-name";
                    loadedEntityB.NameB ="changed-nameB";
                    loadedEntityB.Status = DbClassStatus.Deleted;

                    loadedEntityA.Persist(connection);
                    loadedEntityB.Persist(connection);

                    IInheritanceTestSubEntityA reLoadedEntityA = CreateObjectEmptyTypeA(type);
                    IInheritanceTestSubEntityB reLoadedEntityB = CreateObjectEmptyTypeB(type);

                    bool reLoadedA = LoadEntityWithTypeA(connection,reLoadedEntityA,idA);
                    bool existesSuperA = ExistsSuper(connection,idA);
                    bool existesSubA = ExistsSubA(connection,idA);
                    bool reLoadedB = LoadEntityWithTypeB(connection,reLoadedEntityB,idB);
                    bool existesSuperB = ExistsSuper(connection,idB);
                    bool existesSubB = ExistsSubB(connection,idB);

                    Assert.IsFalse(reLoadedA);
                    Assert.IsFalse(existesSuperA);
                    Assert.IsFalse(existesSubA);
                    Assert.IsFalse(reLoadedB);
                    Assert.IsFalse(existesSuperB);
                    Assert.IsFalse(existesSubB);
                }

                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (ErManagementInheritancePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithTypeA(IDbConnection connection, IInheritanceTestSuperEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_suba where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader,connection);
                loaded = true;
            }

            return loaded;
        }

        private bool LoadEntityWithTypeB(IDbConnection connection, IInheritanceTestSuperEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from inheritance_test_subb where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader,connection);
                loaded = true;
            }

            return loaded;
        }

        private bool ExistsSuper(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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

        private bool ExistsSubA(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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

        private bool ExistsSubB(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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

        private IInheritanceTestSubEntityA CreateObjectWithDataTypeA(int id,int type)
        {
            IInheritanceTestSubEntityA entity = null;
            entity = (type == TYPE_ANNOTATION)?new InheritanceTestSubEntityAAnnotations()
                                              :(type==TYPE_FIELD)? (IInheritanceTestSubEntityA) new InheritanceTestSubEntityAFields()
                                                                 : new InheritanceTestSubEntityAExt();
            entity.IdCol = id;
            entity.Name = "typeA-name";
            entity.NameA = "typeA-nameA";

            return entity;
        }

        private IInheritanceTestSubEntityB CreateObjectWithDataTypeB(int id,int type)
        {
            IInheritanceTestSubEntityB entity = null;
            entity = (type == TYPE_ANNOTATION)?new InheritanceTestSubEntityBAnnotations()
                                              :(type==TYPE_FIELD)?(IInheritanceTestSubEntityB) new InheritanceTestSubEntityBFields()
                                                                 : new InheritanceTestSubEntityBExt();
            entity.IdCol = id;
            entity.Name = "typeB-name";
            entity.NameB = "typeB-nameB";

            return entity;
        }

        private IInheritanceTestSubEntityA CreateObjectEmptyTypeA(int type)
        {
            IInheritanceTestSubEntityA entity = null;
            entity = (type == TYPE_ANNOTATION)?new InheritanceTestSubEntityAAnnotations()
                                              :(type==TYPE_FIELD)?(IInheritanceTestSubEntityA) new InheritanceTestSubEntityAFields()
                                                                 : new InheritanceTestSubEntityAExt();
            return entity;
        }

        private IInheritanceTestSubEntityB CreateObjectEmptyTypeB(int type)
        {
            IInheritanceTestSubEntityB entity = null;
            entity = (type == TYPE_ANNOTATION)?new InheritanceTestSubEntityBAnnotations()
                                              :(type==TYPE_FIELD)?(IInheritanceTestSubEntityB) new InheritanceTestSubEntityBFields()
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
                result &= ((IInheritanceTestSubEntityA) entityA).NameA.Equals(((IInheritanceTestSubEntityA)entityB).NameA);
            }
            else if (entityA is IInheritanceTestSubEntityB
                    && entityB is IInheritanceTestSubEntityB)
            {
                result &= ((IInheritanceTestSubEntityB) entityA).NameB.Equals(((IInheritanceTestSubEntityB)entityB).NameB);
            }
            else
            {
                return false;
            }
        
            return result;
        }
    }
}