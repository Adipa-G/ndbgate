using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions;
using DbGate.Persist.Support.Version;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateVersionTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-version";
        public DbGateVersionTest()
        {
            TestClass = typeof(DbGateVersionTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.Verify;
            TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;
        }
        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
       
        private IDbConnection SetupTables()
        {
            var sql = "Create table version_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      "\tversion Int NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table version_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table version_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            EndInit(DbName);
            return Connection;
        }

        [Fact]
        public void Version_PersistTwice_WithVersionColumnEntity_ShouldNotThrowException()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 35;
                var entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con); 
                entity.Persist(transaction);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Version_PersistTwice_WithoutVersionColumnEntity_ShouldNotThrowException()
        {
            try
            {
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 45;
                var entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con); 
                entity.Persist(transaction);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Version_PersistWithTwoChanges_WithoutUpdateChangedColumnsOnly_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);

            var id = 45;
            var entity = new VersionGeneralTestRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Version = 1;
            entity.Persist(transaction);
            transaction.Commit();
            
            transaction = CreateTransaction(con);
            var loadedEntityA = new VersionGeneralTestRootEntity();
            var loadedEntityB = new VersionGeneralTestRootEntity();
            LoadWithoutVersionColumnEntityWithId(transaction,loadedEntityA, entity.IdCol);
            LoadWithoutVersionColumnEntityWithId(transaction,loadedEntityB, entity.IdCol);
            transaction.Commit();

            transaction = CreateTransaction(con);
            loadedEntityA.Name ="Mod Name";
            loadedEntityA.Persist(transaction);
            transaction.Commit();

            loadedEntityB.Version = loadedEntityB.Version + 1;
            Assert.Throws<PersistException>(() => loadedEntityB.Persist(transaction));
        }

        [Fact]
        public void Version_PersistWithTwoChanges_WithUpdateChangedColumnsOnly_ShouldNotThrowException()
        {
            try
            {
                TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.ChangedColumns;
                var con = SetupTables();
                var transaction = CreateTransaction(con);

                var id = 45;
                var entity = new VersionGeneralTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Version = 1;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntityA = new VersionGeneralTestRootEntity();
                var loadedEntityB = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction, loadedEntityA, entity.IdCol);
                LoadWithoutVersionColumnEntityWithId(transaction, loadedEntityB, entity.IdCol);
                transaction.Commit();

                transaction = CreateTransaction(con);
                loadedEntityA.Name = "Mod Name";
                loadedEntityA.Persist(transaction);

                loadedEntityB.Version = loadedEntityB.Version + 1;
                loadedEntityB.Persist(transaction);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void Version_RootUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                var id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.Name ="New Name2";;
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        [Fact]
        public void Version_RootUpdateFromAnotherTransaction_WithOutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                var id = 65;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.Name ="New Name2";;
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        [Fact]
        public void Version_One2oneChildUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                var id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                var one2OneEntity = new VersionColumnTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.One2OneEntity.Name ="Modified2 One2One";
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        [Fact]
        public void Version_One2oneChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                var id = 55;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                var one2OneEntity = new VersionGeneralTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity = one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.One2OneEntity.Name = "Modified2 One2One";
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        [Fact]
        public void Version_One2manyChildUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                var id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                var one2ManyEntityOrg = new VersionColumnTestOne2ManyEntity();
                one2ManyEntityOrg.Name = "One2Many";
                one2ManyEntityOrg.IndexNo = 1; 
                entity.One2ManyEntities.Add(one2ManyEntityOrg);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                var loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                var loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name ="Modified One2Many";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction =  CreateTransaction(con);
            var orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            var orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        [Fact]
        public void Version_One2manyChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            var transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                var id = 55;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                var orgOne2ManyEntityOrg = new VersionGeneralTestOne2ManyEntity();
                orgOne2ManyEntityOrg.Name = "One2Many";
                orgOne2ManyEntityOrg.IndexNo = 1;
                entity.One2ManyEntities.Add(orgOne2ManyEntityOrg);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                var loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);

                var loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                var loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name = "Modified One2Many";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            var orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            var orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            Assert.Throws<PersistException>(() => entity.Persist(transaction));
            transaction.Commit();
            con.Close();
        }

        private bool LoadWithVersionColumnEntityWithId(ITransaction transaction, VersionColumnTestRootEntity loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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

        private bool LoadWithoutVersionColumnEntityWithId(ITransaction transaction, VersionGeneralTestRootEntity loadEntity, int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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
    }
}
