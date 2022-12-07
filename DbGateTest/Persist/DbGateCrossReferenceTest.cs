using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Persist.Support.CrossReference;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateCrossReferenceTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-cross-reference";

        public DbGateCrossReferenceTest()
        {
            TestClass = typeof(DbGateCrossReferenceTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }
        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
        private IDbConnection SetupTables()
        {
            var sql = "Create table cross_reference_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);

            sql = "Create table cross_reference_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table cross_reference_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void CrossReference_PersistWithOne2OneChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                var connection = SetupTables();
                
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                var one2OneEntity = new CrossReferenceTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Entity";
                one2OneEntity.RootEntity = entity;
                entity.One2OneEntity =one2OneEntity;
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                transaction.Commit();
                connection.Close();

                Assert.NotNull(loadedEntity);
                Assert.NotNull(loadedEntity.One2OneEntity);
                Assert.NotNull(loadedEntity.One2OneEntity.RootEntity);
                Assert.True(loadedEntity == loadedEntity.One2OneEntity.RootEntity);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateCrossReferenceTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void CrossReference_PersistWithOne2ManyChild_WithCrossReference_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);

                var id = 45;
                var entity = new CrossReferenceTestRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                var one2ManyEntity = new CrossReferenceTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo =1;
                one2ManyEntity.Name = "Child-Entity";
                one2ManyEntity.RootEntity = entity;
                entity.One2ManyEntities.Add(one2ManyEntity);
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new CrossReferenceTestRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                
                Assert.NotNull(loadedEntity);
                Assert.True(loadedEntity.One2ManyEntities.Count == 1);
                var childEnumerator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumerator.MoveNext();
                var childOne2ManyEntity = childEnumerator.Current;
                Assert.NotNull(childOne2ManyEntity);
                Assert.True(loadedEntity == childOne2ManyEntity.RootEntity);

                transaction.Commit();
                connection.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateCrossReferenceTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, CrossReferenceTestRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from cross_reference_test_root where id_col = ?";

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