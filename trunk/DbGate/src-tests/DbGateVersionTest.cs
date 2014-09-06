using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.Exceptions;
using DbGate.Support.Persistant.Version;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateVersionTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-version";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateVersionTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.Verify;
            TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }
        
        private IDbConnection SetupTables()
        {
            string sql = "Create table version_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         "\tversion Int NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table version_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table version_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tversion Int NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);

            EndInit(DBName);
            return Connection;
        }

        [Test]
        public void Version_PersistTwice_WithVersionColumnEntity_ShouldNotThrowException()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 35;
                VersionColumnTestRootEntity entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con); 
                entity.Persist(transaction);
                transaction.Commit();
                con.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Version_PersistTwice_WithoutVersionColumnEntity_ShouldNotThrowException()
        {
            try
            {
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 45;
                VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con); 
                entity.Persist(transaction);
                transaction.Commit();
                con.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_PersistWithTwoChanges_WithoutUpdateChangedColumnsOnly_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);

            int id = 45;
            VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Version = 1;
            entity.Persist(transaction);
            transaction.Commit();
            
            transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity loadedEntityA = new VersionGeneralTestRootEntity();
            VersionGeneralTestRootEntity loadedEntityB = new VersionGeneralTestRootEntity();
            LoadWithoutVersionColumnEntityWithId(transaction,loadedEntityA, entity.IdCol);
            LoadWithoutVersionColumnEntityWithId(transaction,loadedEntityB, entity.IdCol);
            transaction.Commit();

            transaction = CreateTransaction(con);
            loadedEntityA.Name ="Mod Name";
            loadedEntityA.Persist(transaction);
            transaction.Commit();

            loadedEntityB.Version = loadedEntityB.Version + 1;
            loadedEntityB.Persist(transaction);
            transaction.Commit();
        }

        [Test]
        public void Version_PersistWithTwoChanges_WithUpdateChangedColumnsOnly_ShouldNotThrowException()
        {
            try
            {
                TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.ChangedColumns;
                var con = SetupTables();
                ITransaction transaction = CreateTransaction(con);

                int id = 45;
                VersionGeneralTestRootEntity entity = new VersionGeneralTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Version = 1;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionGeneralTestRootEntity loadedEntityA = new VersionGeneralTestRootEntity();
                VersionGeneralTestRootEntity loadedEntityB = new VersionGeneralTestRootEntity();
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
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_RootUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                int id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.Name ="New Name2";;
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_RootUpdateFromAnotherTransaction_WithOutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 65;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Name ="New Name";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.Name ="New Name2";;
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_One2oneChildUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                int id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionColumnTestOne2OneEntity one2OneEntity = new VersionColumnTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.One2OneEntity.Name ="Modified2 One2One";
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_One2oneChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 55;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionGeneralTestOne2OneEntity one2OneEntity = new VersionGeneralTestOne2OneEntity();
                one2OneEntity.Name ="One2One";
                entity.One2OneEntity = one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Name ="Modified One2One";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            entity.One2OneEntity.Name = "Modified2 One2One";
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_One2manyChildUpdateFromAnotherTransaction_WithVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionColumnTestRootEntity entity = null;

            try
            {
                int id = 55;
                entity = new VersionColumnTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionColumnTestOne2ManyEntity one2ManyEntityOrg = new VersionColumnTestOne2ManyEntity();
                one2ManyEntityOrg.Name = "One2Many";
                one2ManyEntityOrg.IndexNo = 1; 
                entity.One2ManyEntities.Add(one2ManyEntityOrg);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionColumnTestRootEntity loadedEntity = new VersionColumnTestRootEntity();
                LoadWithVersionColumnEntityWithId(transaction,loadedEntity,id);
                IEnumerator<VersionColumnTestOne2ManyEntity> loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                VersionColumnTestOne2ManyEntity loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name ="Modified One2Many";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction =  CreateTransaction(con);
            IEnumerator<VersionColumnTestOne2ManyEntity> orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            VersionColumnTestOne2ManyEntity orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void Version_One2manyChildUpdateFromAnotherTransaction_WithoutVersionColumnEntity_ShouldThrowException()
        {
            var con = SetupTables();
            ITransaction transaction = CreateTransaction(con);
            VersionGeneralTestRootEntity entity = null;

            try
            {
                int id = 55;
                entity = new VersionGeneralTestRootEntity();
                entity.IdCol =id;
                entity.Name = "Org-Name";
                VersionGeneralTestOne2ManyEntity orgOne2ManyEntityOrg = new VersionGeneralTestOne2ManyEntity();
                orgOne2ManyEntityOrg.Name = "One2Many";
                orgOne2ManyEntityOrg.IndexNo = 1;
                entity.One2ManyEntities.Add(orgOne2ManyEntityOrg);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                VersionGeneralTestRootEntity loadedEntity = new VersionGeneralTestRootEntity();
                LoadWithoutVersionColumnEntityWithId(transaction,loadedEntity,id);

                IEnumerator<VersionGeneralTestOne2ManyEntity> loadedEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                loadedEnumerator.MoveNext();
                VersionGeneralTestOne2ManyEntity loadedOne2ManyEntity = loadedEnumerator.Current;
                loadedOne2ManyEntity.Name = "Modified One2Many";
                loadedEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateVersionTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }

            transaction = CreateTransaction(con);
            IEnumerator<VersionGeneralTestOne2ManyEntity> orgEnumerator = entity.One2ManyEntities.GetEnumerator();
            orgEnumerator.MoveNext();
            VersionGeneralTestOne2ManyEntity orgOne2ManyEntity = orgEnumerator.Current;
            orgOne2ManyEntity.Name = "Modified2 One2Many";
            entity.Persist(transaction);
            transaction.Commit();
            con.Close();
        }

        private bool LoadWithVersionColumnEntityWithId(ITransaction transaction, VersionColumnTestRootEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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

        private bool LoadWithoutVersionColumnEntityWithId(ITransaction transaction, VersionGeneralTestRootEntity loadEntity, int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from version_test_root where id_col = ?";

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
    }
}
