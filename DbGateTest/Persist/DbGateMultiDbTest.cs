using System.Data;
using DbGate.Persist.Support.MultiDb;
using NUnit.Framework;

namespace DbGate.Persist
{
    [TestFixture]
    public class DbGateMultiDbTest : AbstractDbGateTestBase
    {
        private const string DB1Name = "unit-testing-multi-db-1";
        private const string DB2Name = "unit-testing-multi-db-2";

        protected static ITransactionFactory TransactionFactoryDb1;
        protected static IDbConnection ConnectionDb1;

        [OneTimeSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateMultiDbTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DB1Name);
            TransactionFactoryDb1 = TransactionFactory;
            ConnectionDb1 = Connection;
            
            BeginInit(DB2Name);
            
            TransactionFactoryDb1.DbGate.ClearCache();
            TransactionFactoryDb1.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;

            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.UpdateStrategy = UpdateStrategy.AllColumns;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DB1Name);
            CleanupDb(DB2Name);

            ConnectionDb1.Close();
            ConnectionDb1 = null;
            TransactionFactoryDb1 = null;

            FinalizeDb(DB2Name);
        }
        
        private void SetupTables()
        {
            string sql = "Create table multi_db_test_root (\n" +
                "\tid_col Int NOT NULL,\n" +
                "\tname Varchar(100) NOT NULL,\n" +
                " Primary Key (id_col))";
            
            CreateTableFromSql(sql,DB1Name,ConnectionDb1);
            CreateTableFromSql(sql,DB2Name,Connection);

            EndInit(DB1Name,ConnectionDb1);
            EndInit(DB2Name,Connection);
        }

        [Test]
        public void MultipleDatabaseSupport_PersistTwoEntitiesInTwoDbs_AndFetchThem_ShouldFetch()
        {
            try
            {
                SetupTables();

                ITransaction txDb1 = CreateTransaction(ConnectionDb1);
                int id = 35;
                var db1Entity = new MultiDbEntity();
                db1Entity.IdCol = id;
                db1Entity.Name = DB1Name;
                
                db1Entity.Persist(txDb1);
                txDb1.Commit();
 
                ITransaction txDb2 = CreateTransaction(Connection);
                var db2Entity = new MultiDbEntity();
                db2Entity.IdCol = id;
                db2Entity.Name = DB2Name;
                db2Entity.Persist(txDb2);
                txDb2.Commit();

                txDb1 = CreateTransaction(ConnectionDb1);
                var db1LoadedEntity = new MultiDbEntity();
                LoadWithId(txDb1,db1LoadedEntity,id);
                Assert.AreEqual(db1LoadedEntity.Name,DB1Name);
                txDb1.Commit();

                txDb2 = CreateTransaction(Connection);
                var db2LoadedEntity = new MultiDbEntity();
                LoadWithId(txDb2,db2LoadedEntity,id);
                Assert.AreEqual(db2LoadedEntity.Name,DB2Name);
            }
            catch (System.Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private bool LoadWithId(ITransaction tx, MultiDbEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = tx.CreateCommand();
            cmd.CommandText = "select * from multi_db_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                loadEntity.Retrieve(reader, tx);
                loaded = true;
            }

            return loaded;
        }
    }
}
