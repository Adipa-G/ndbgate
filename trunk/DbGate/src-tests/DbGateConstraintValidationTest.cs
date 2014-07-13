using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.Exceptions;
using DbGate.Support.Persistant.Constraint;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateConstraintValidationTest
    {
        private static ITransactionFactory _transactionFactory;

        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateConstraintValidationTest)).Info("Starting in-memory database for unit tests");
                _transactionFactory = new DefaultTransactionFactory("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DefaultTransactionFactory.DbSqllite);
                Assert.IsNotNull(_transactionFactory.CreateTransaction());
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateConstraintValidationTest)).Fatal("Exception during database startup.", ex);
            }
        }

        [TestFixtureTearDown]
        public static void After()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateConstraintValidationTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            _transactionFactory.DbGate.ClearCache();
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction();

                IDbCommand command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table constraint_test_root";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table constraint_test_one2many";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_one2one";
                command.ExecuteNonQuery();

                command = transaction.CreateCommand();
                command.CommandText = "drop table constraint_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            ITransaction transaction = _transactionFactory.CreateTransaction();
            IDbConnection connection = transaction.Connection;

            string sql = "Create table constraint_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table constraint_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table constraint_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            cmd = transaction.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        private ITransaction CreateTransaction(IDbConnection connection)
        {
            return new Transaction(_transactionFactory, connection.BeginTransaction());
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
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