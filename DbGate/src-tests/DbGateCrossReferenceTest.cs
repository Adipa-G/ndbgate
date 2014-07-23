using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.Support.Persistant.CrossReference;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateCrossReferenceTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-cross-reference";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateCrossReferenceTest);
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

        private IDbConnection SetupTables()
        {
            string sql = "Create table cross_reference_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table cross_reference_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table cross_reference_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);
            EndInit(DBName);

            return Connection;
        }

        [Test]
        public void CrossReference_PersistWithOne2OneChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                TransactionFactory.DbGate.Config.AutoTrackChanges = true;
                IDbConnection connection = SetupTables();
                
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                CrossReferenceTestRootEntity entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                CrossReferenceTestOne2OneEntity one2OneEntity = new CrossReferenceTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Entity";
                one2OneEntity.RootEntity = entity;
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                CrossReferenceTestRootEntity loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.IsNotNull(loadedEntity);
                Assert.IsNotNull(loadedEntity.One2OneEntity);
                Assert.IsNotNull(loadedEntity.One2OneEntity.RootEntity);
                Assert.IsTrue(loadedEntity == loadedEntity.One2OneEntity.RootEntity);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateCrossReferenceTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void CrossReference_PersistWithOne2ManyChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                TransactionFactory.DbGate.Config.AutoTrackChanges = true;
            
                IDbConnection connection = SetupTables();
                ITransaction transaction = CreateTransaction(connection);

                int id = 45;
                CrossReferenceTestRootEntity entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                CrossReferenceTestOne2ManyEntity one2ManyEntity = new CrossReferenceTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo =1;
                one2ManyEntity.Name = "Child-Entity";
                one2ManyEntity.RootEntity = entity;
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                CrossReferenceTestRootEntity loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                
                Assert.IsNotNull(loadedEntity);
                Assert.IsTrue(loadedEntity.One2ManyEntities.Count == 1);
                IEnumerator<CrossReferenceTestOne2ManyEntity> childEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumerator.MoveNext();
                CrossReferenceTestOne2ManyEntity childOne2ManyEntity = childEnumerator.Current;
                Assert.IsNotNull(childOne2ManyEntity);
                Assert.IsTrue(loadedEntity == childOne2ManyEntity.RootEntity);

                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateCrossReferenceTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, CrossReferenceTestRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from cross_reference_test_root where id_col = ?";

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