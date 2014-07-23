using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.ErManagement.ErMapper;
using DbGate.Support.Persistant.TreeTest;
using NUnit.Framework;
using log4net;
using log4net.Config;

namespace DbGate
{
    public class DbGateTreePersistTests : AbstractDbGateTestBase
    {
        public const int TYPE_ANNOTATION = 1;
        public const int TYPE_FIELD = 2;
        public const int TYPE_EXTERNAL = 3;

        private const string DBName = "unit-testing-tree-persist";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateTreePersistTests);
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

        private void RegisterForExternal()
        {
            Type objType = typeof (TreeTestRootEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableNames(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));

            objType = typeof (TreeTestOne2ManyEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableNames(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));

            objType = typeof (TreeTestOne2OneEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableNames(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));
        }

        private IDbConnection SetupTables()
        {
            string sql = "Create table tree_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table tree_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table tree_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);
            
            EndInit(DBName);
            return Connection;
        }

        [Test]
        public void TreePersist_Insert_WithAnnotationsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                
                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_ANNOTATION);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAnnotations();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                bool compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Insert_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_FIELD);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                bool compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Insert_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
               
                RegisterForExternal();

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_EXTERNAL);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();
                con.Close();

                bool compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Update_WithAnnotationsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
               
                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_ANNOTATION);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAnnotations();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.One2OneEntity.Name = "changed-one2one";
                loadedEntity.One2OneEntity.Status = EntityStatus.Modified;

                IEnumerator<ITreeTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();

                ITreeTestOne2ManyEntity one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityAnnotations();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                bool compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Update_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_FIELD);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.One2OneEntity.Name = "changed-one2one";
                loadedEntity.One2OneEntity.Status = EntityStatus.Modified;

                IEnumerator<ITreeTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();

                ITreeTestOne2ManyEntity one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                bool compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Update_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                RegisterForExternal();

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_EXTERNAL);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.One2OneEntity.Name = "changed-one2one";
                loadedEntity.One2OneEntity.Status = EntityStatus.Modified;

                IEnumerator<ITreeTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();

                ITreeTestOne2ManyEntity one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                bool compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.IsTrue(compareResult);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Delete_WithAnnotationsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_ANNOTATION);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAnnotations();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityAnnotations();
                bool loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                bool existsOne2one = ExistsOne2ManyChild(transaction, id);
                bool existsOne2many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.IsFalse(loaded);
                Assert.IsFalse(existsOne2one);
                Assert.IsFalse(existsOne2many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Delete_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_FIELD);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityFields();
                bool loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                bool existsOne2one = ExistsOne2ManyChild(transaction, id);
                bool existsOne2many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.IsFalse(loaded);
                Assert.IsFalse(existsOne2one);
                Assert.IsFalse(existsOne2many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TreePersist_Delete_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                RegisterForExternal();

                int id = 35;
                ITreeTestRootEntity rootEntity = CreateFullObjectTree(id, TYPE_EXTERNAL);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityExt();
                bool loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                bool existsOne2one = ExistsOne2ManyChild(transaction, id);
                bool existsOne2many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.IsFalse(loaded);
                Assert.IsFalse(existsOne2one);
                Assert.IsFalse(existsOne2many);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, ITreeTestRootEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from tree_test_root where id_col = ?";

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

        private bool ExistsOne2ManyChild(ITransaction transaction, int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from tree_test_one2many where id_col = ?";

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

        private ITreeTestRootEntity CreateFullObjectTree(int id, int type)
        {
            ITreeTestRootEntity entity = null;
            entity = (type == TYPE_ANNOTATION)
                         ? new TreeTestRootEntityAnnotations()
                         : (type == TYPE_FIELD)
                               ? (ITreeTestRootEntity) new TreeTestRootEntityFields()
                               : new TreeTestRootEntityExt();
            entity.IdCol = id;
            entity.Name = "root";

            ITreeTestOne2OneEntity one2OneEntity = null;
            one2OneEntity = (type == TYPE_ANNOTATION)
                                ? new TreeTestOne2OneEntityAnnotations()
                                : (type == TYPE_FIELD)
                                      ? (ITreeTestOne2OneEntity) new TreeTestOne2OneEntityFields()
                                      : new TreeTestOne2OneEntityExt();
            one2OneEntity.IdCol = id;
            one2OneEntity.Name = "one2one";
            entity.One2OneEntity = one2OneEntity;

            ITreeTestOne2ManyEntity one2ManyEntity = null;
            one2ManyEntity = (type == TYPE_ANNOTATION)
                                 ? new TreeTestOne2ManyEntityAnnotations()
                                 : (type == TYPE_FIELD)
                                       ? (ITreeTestOne2ManyEntity) new TreeTestOne2ManyEntityFields()
                                       : new TreeTestOne2ManyEntityExt();
            one2ManyEntity.IdCol = id;
            one2ManyEntity.IndexNo = 0;
            one2ManyEntity.Name = "one2many";
            entity.One2ManyEntities = new List<ITreeTestOne2ManyEntity>();
            entity.One2ManyEntities.Add(one2ManyEntity);

            return entity;
        }

        private bool CompareEntities(ITreeTestRootEntity rootA, ITreeTestRootEntity rootB)
        {
            bool result = rootA.IdCol == rootB.IdCol;
            result &= rootA.Name == rootB.Name;

            if (rootA.One2OneEntity != null
                && rootB.One2OneEntity != null)
            {
                result &= rootA.One2OneEntity.IdCol == rootB.One2OneEntity.IdCol;
                result &= rootA.One2OneEntity.Name.Equals(rootB.One2OneEntity.Name);
            }
            else if (rootA.One2OneEntity == null
                     && rootB.One2OneEntity == null)
            {
            }
            else
            {
                return false;
            }
            if (!result)
            {
                return false;
            }

            if (rootA.One2ManyEntities != null
                && rootB.One2ManyEntities != null)
            {
                if (rootA.One2ManyEntities.Count != rootB.One2ManyEntities.Count)
                {
                    return false;
                }
                foreach (ITreeTestOne2ManyEntity one2ManyEntityA in rootA.One2ManyEntities)
                {
                    bool found = false;
                    foreach (ITreeTestOne2ManyEntity one2ManyEntityB in rootB.One2ManyEntities)
                    {
                        found = one2ManyEntityA.IdCol == one2ManyEntityB.IdCol;
                        found &= one2ManyEntityA.IndexNo == one2ManyEntityB.IndexNo;
                        found &= one2ManyEntityA.Name.Equals(one2ManyEntityB.Name);
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
            }
            else if (rootA.One2ManyEntities == null
                     && rootB.One2ManyEntities == null)
            {
            }
            else
            {
                return false;
            }

            return result;
        }
    }
}