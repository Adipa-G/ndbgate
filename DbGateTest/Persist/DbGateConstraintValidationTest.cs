using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions;
using DbGate.Persist.Support.Constraint;
using log4net;
using NUnit.Framework;

namespace DbGate.Persist
{
    public class DbGateConstraintValidationTest : AbstractDbGateTestBase
    {
        private const string DBName = "unit-testing-constraint";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateConstraintValidationTest);
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
            string sql = "Create table constraint_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table constraint_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DBName);

            sql = "Create table constraint_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);
            EndInit(DBName);
            
            return Connection;
        }

        [Test]
        public void ConstraintValidation_DeleteOneToOneChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                loadedEntity.One2OneEntity.Status =EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2OneChild(transaction, id);
                bool hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsTrue(hasOneToOne);
                Assert.IsTrue(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ConstraintValidation_DeleteOneToManyChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol= id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                IEnumerator<ConstraintTestOne2ManyEntity> childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                ConstraintTestOne2ManyEntity loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToMany = ExistsOne2ManyChild(transaction, id);
                bool hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsTrue(hasOneToMany);
                Assert.IsTrue(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ConstraintValidation_DeleteRootWithOneToOneChild_WithReverseRelationShip_ShouldThrowException()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction transaction = CreateTransaction(connection);
                
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2OneChild(transaction,id);
                bool hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.IsTrue(hasOneToOne);
                Assert.IsFalse(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ConstraintValidation_DeleteRootWithOneToManyChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToMany = ExistsOne2ManyChild(transaction,id);
                bool hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.IsTrue(hasOneToMany);
                Assert.IsFalse(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ConstraintValidation_DeleteRootWithOneToOneChild_WithRestrictConstraint_ShouldThrowException()
        {
            IDbConnection connection = SetupTables();

            ITransaction transaction = CreateTransaction(connection);
            int id = 45;
            ConstraintTestDeleteRestrictRootEntity entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol =id;
            entity.Name = "Org-Name";
            entity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
            one2OneEntity.IdCol = id;
            one2OneEntity.Name = "Child-Org-Name";
            one2OneEntity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            ConstraintTestDeleteRestrictRootEntity loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(transaction,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            loadedEntity.Persist(transaction);
            transaction.Commit();
            connection.Close();
        }
    
        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ConstraintValidation_DeleteRootWithOneToManyChild_WithRestrictConstraint_ShouldThrowException()
        {
            IDbConnection connection = SetupTables();

            ITransaction transaction = CreateTransaction(connection);
            int id = 45;
            ConstraintTestDeleteRestrictRootEntity entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
            one2ManyEntity.IdCol = id;
            one2ManyEntity.Name = "Child-Org-Name";
            one2ManyEntity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            ConstraintTestDeleteRestrictRootEntity loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(transaction,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            loadedEntity.Persist(transaction);
            transaction.Commit();
            connection.Close();
        }
    
        [Test]
        public void ConstraintValidation_DeleteOneToManyChild_WithCascadeConstraint_ShouldDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                IEnumerator<ConstraintTestOne2ManyEntity> childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                ConstraintTestOne2ManyEntity loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToMany = ExistsOne2ManyChild(transaction,id);
                bool hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToMany);
                Assert.IsTrue(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }
    
        [Test]
        public void ConstraintValidation_DeleteOneToOneChild_WithCascadeConstraint_ShouldDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2OneEntity one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2ManyChild(transaction,id);
                bool hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToOne);
                Assert.IsTrue(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ConstraintValidation_DeleteOneToManyRoot_WithCascadeConstraint_ShouldDeleteBoth()
        {
            try
            {
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToMany = ExistsOne2ManyChild(transaction,id);
                bool hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToMany);
                Assert.IsFalse(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ConstraintValidation_DeleteOneToOneRoot_WithCascadeConstraint_ShouldDeleteBoth()
        {
            try
            {
                IDbConnection connection = SetupTables();

                ITransaction transaction = CreateTransaction(connection);
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestOne2OneEntity one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                bool hasOneToOne = ExistsOne2ManyChild(transaction, id);
                bool hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.IsFalse(hasOneToOne);
                Assert.IsFalse(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestDeleteCascadeRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestDeleteRestrictRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestReverseRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool ExistsRoot(ITransaction transaction,int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool ExistsOne2OneChild(ITransaction transaction,int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_one2one where id_col = ?";

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

        private bool ExistsOne2ManyChild(ITransaction transaction,int id)
        {
            bool exists = false;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_one2many where id_col = ?";

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
    }
}