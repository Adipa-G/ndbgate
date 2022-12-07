using System;
using System.Data;
using DbGate.Persist.Support.MultiDb;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateMultiDbTest : AbstractDbGateTestBase, IDisposable
    {
        private const string Db1Name = "unit-testing-multi-db-1";
        private const string Db2Name = "unit-testing-multi-db-2";

        protected static ITransactionFactory TransactionFactoryDb1;
        protected static IDbConnection ConnectionDb1;

        public DbGateMultiDbTest()
        {
            TestClass = typeof(DbGateMultiDbTest);
            BeginInit(Db1Name);
            TransactionFactoryDb1 = TransactionFactory;
            ConnectionDb1 = Connection;
            
            BeginInit(Db2Name);
            
            TransactionFactoryDb1.DbGate.ClearCache();
            TransactionFactoryDb1.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;

            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;
        }

        public void Dispose()
        {
            CleanupDb(Db1Name);
            CleanupDb(Db2Name);

            ConnectionDb1.Close();
            ConnectionDb1 = null;
            TransactionFactoryDb1 = null;

            FinalizeDb(Db2Name);
        }
       
        private void SetupTables()
        {
            var sql = "Create table multi_db_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(100) NOT NULL,\n" +
                      " Primary Key (id_col))";
            
            CreateTableFromSql(sql,Db1Name,ConnectionDb1);
            CreateTableFromSql(sql,Db2Name,Connection);

            EndInit(Db1Name,ConnectionDb1);
            EndInit(Db2Name,Connection);
        }

        [Fact]
        public void MultipleDatabaseSupport_PersistTwoEntitiesInTwoDbs_AndFetchThem_ShouldFetch()
        {
            try
            {
                SetupTables();

                var txDb1 = CreateTransaction(ConnectionDb1);
                var id = 35;
                var db1Entity = new MultiDbEntity();
                db1Entity.IdCol = id;
                db1Entity.Name = Db1Name;
                
                db1Entity.Persist(txDb1);
                txDb1.Commit();
 
                var txDb2 = CreateTransaction(Connection);
                var db2Entity = new MultiDbEntity();
                db2Entity.IdCol = id;
                db2Entity.Name = Db2Name;
                db2Entity.Persist(txDb2);
                txDb2.Commit();

                txDb1 = CreateTransaction(ConnectionDb1);
                var db1LoadedEntity = new MultiDbEntity();
                LoadWithId(txDb1,db1LoadedEntity,id);
                Assert.Equal(Db1Name,db1LoadedEntity.Name);
                txDb1.Commit();

                txDb2 = CreateTransaction(Connection);
                var db2LoadedEntity = new MultiDbEntity();
                LoadWithId(txDb2,db2LoadedEntity,id);
                Assert.Equal(Db2Name,db2LoadedEntity.Name);
            }
            catch (System.Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private bool LoadWithId(ITransaction tx, MultiDbEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = tx.CreateCommand();
            cmd.CommandText = "select * from multi_db_test_root where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, tx);
                loaded = true;
            }

            return loaded;
        }
    }
}
