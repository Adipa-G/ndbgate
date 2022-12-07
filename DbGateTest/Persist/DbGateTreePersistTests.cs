using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Persist.Support.TreeTest;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateTreePersistTests : AbstractDbGateTestBase, IDisposable
    {
        public const int TypeAttribute = 1;
        public const int TypeField = 2;
        public const int TypeExternal = 3;

        private const string DbName = "unit-testing-tree-persist";

        public DbGateTreePersistTests()
        {
            TestClass = typeof(DbGateTreePersistTests);
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

        private void RegisterForExternal()
        {
            var objType = typeof (TreeTestRootEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableInfo(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));

            objType = typeof (TreeTestOne2ManyEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableInfo(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));

            objType = typeof (TreeTestOne2OneEntityExt);
            TransactionFactory.DbGate.RegisterEntity(objType,
                                                      TreeTestExtFactory.GetTableInfo(objType),
                                                      TreeTestExtFactory.GetFieldInfo(objType));
        }

        private IDbConnection SetupTables()
        {
            var sql = "Create table tree_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table tree_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table tree_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);
            
            EndInit(DbName);
            return Connection;
        }

        [Fact]
        public void TreePersist_Insert_WithAttributesDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);
                
                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeAttribute);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAttributes();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Insert_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeField);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Insert_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);
               
                RegisterForExternal();

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeExternal);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Insert_WithAttributesNullOneToOneChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeAttribute);
                rootEntity.One2OneEntity = null;
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAttributes();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Insert_WithFieldsNullOneToOneChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeField);
                rootEntity.One2OneEntity = null;
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Insert_WithExtsNullOneToOneChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                RegisterForExternal();

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeExternal);
                rootEntity.One2OneEntity = null;
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();
                con.Close();

                var compareResult = CompareEntities(rootEntity, loadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Update_WithAttributesDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);
               
                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeAttribute);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAttributes();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Modified;
                loadedEntity.One2OneEntity.Name = "changed-one2one";
                loadedEntity.One2OneEntity.Status = EntityStatus.Modified;

                IEnumerator<ITreeTestOne2ManyEntity> enumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                enumerator.MoveNext();

                var one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityAttributes();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Update_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeField);
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

                var one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Update_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                RegisterForExternal();

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeExternal);
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

                var one2ManyEntity = enumerator.Current;
                one2ManyEntity.Name = "changed-one2many";
                one2ManyEntity.Status = EntityStatus.Modified;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, reLoadedEntity, id);
                con.Close();

                var compareResult = CompareEntities(loadedEntity, reLoadedEntity);
                Assert.True(compareResult);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Delete_WithAttributesDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeAttribute);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityAttributes();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityAttributes();
                var loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                var existsOne2One = ExistsOne2ManyChild(transaction, id);
                var existsOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.False(loaded);
                Assert.False(existsOne2One);
                Assert.False(existsOne2Many);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Delete_WithFieldsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeField);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityFields();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityFields();
                var loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                var existsOne2One = ExistsOne2ManyChild(transaction, id);
                var existsOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.False(loaded);
                Assert.False(existsOne2One);
                Assert.False(existsOne2Many);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void TreePersist_Delete_WithExtsDifferentTypeOfChildren_ShouldEqualWhenLoaded()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                RegisterForExternal();

                var id = 35;
                var rootEntity = CreateFullObjectTree(id, TypeExternal);
                rootEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ITreeTestRootEntity loadedEntity = new TreeTestRootEntityExt();
                LoadEntityWithId(transaction, loadedEntity, id);

                loadedEntity.Name = "changed-name";
                loadedEntity.Status = EntityStatus.Deleted;

                loadedEntity.Persist(transaction);

                ITreeTestRootEntity reLoadedEntity = new TreeTestRootEntityExt();
                var loaded = LoadEntityWithId(transaction, reLoadedEntity, id);
                var existsOne2One = ExistsOne2ManyChild(transaction, id);
                var existsOne2Many = ExistsOne2ManyChild(transaction, id);
                transaction.Close();

                Assert.False(loaded);
                Assert.False(existsOne2One);
                Assert.False(existsOne2Many);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateTreePersistTests)).Fatal("Exception during test", e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, ITreeTestRootEntity loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from tree_test_root where id_col = ?";

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

        private bool ExistsOne2ManyChild(ITransaction transaction, int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from tree_test_one2many where id_col = ?";

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

        private ITreeTestRootEntity CreateFullObjectTree(int id, int type)
        {
            ITreeTestRootEntity entity = null;
            entity = (type == TypeAttribute)
                         ? new TreeTestRootEntityAttributes()
                         : (type == TypeField)
                               ? (ITreeTestRootEntity) new TreeTestRootEntityFields()
                               : new TreeTestRootEntityExt();
            entity.IdCol = id;
            entity.Name = "root";

            ITreeTestOne2OneEntity one2OneEntity = null;
            one2OneEntity = (type == TypeAttribute)
                                ? new TreeTestOne2OneEntityAttributes()
                                : (type == TypeField)
                                      ? (ITreeTestOne2OneEntity) new TreeTestOne2OneEntityFields()
                                      : new TreeTestOne2OneEntityExt();
            one2OneEntity.IdCol = id;
            one2OneEntity.Name = "one2one";
            entity.One2OneEntity = one2OneEntity;

            ITreeTestOne2ManyEntity one2ManyEntity = null;
            one2ManyEntity = (type == TypeAttribute)
                                 ? new TreeTestOne2ManyEntityAttributes()
                                 : (type == TypeField)
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
            var result = rootA.IdCol == rootB.IdCol;
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
                foreach (var one2ManyEntityA in rootA.One2ManyEntities)
                {
                    var found = false;
                    foreach (var one2ManyEntityB in rootB.One2ManyEntities)
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