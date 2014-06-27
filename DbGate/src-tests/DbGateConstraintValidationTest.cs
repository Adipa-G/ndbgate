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
        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGateConstraintValidationTest)).Info("Starting in-memory database for unit tests");
                var dbConnector = new DbConnector("Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;foreign_keys = ON", DbConnector.DbSqllite);
				Assert.IsNotNull(dbConnector.Connection);
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
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGateConstraintValidationTest)).Fatal("Exception during test cleanup.", ex);
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            if (DbConnector.GetSharedInstance() != null)
            {
                ErManagement.ErMapper.DbGate.GetSharedInstance().ClearCache();
            }
        }

        [TearDown]
        public void AfterEach()
        {
            try
            {
                IDbConnection connection = DbConnector.GetSharedInstance().Connection;
                IDbTransaction transaction = connection.BeginTransaction();

                IDbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table constraint_test_root";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table constraint_test_one2many";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "DELETE FROM constraint_test_one2one";
                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText = "drop table constraint_test_one2one";
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal("Exception during test cleanup.", ex);
            }
        }
        
        private IDbConnection SetupTables()
        {
            IDbConnection connection = DbConnector.GetSharedInstance().Connection;
            IDbTransaction transaction = connection.BeginTransaction();

            string sql = "Create table constraint_test_root (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table constraint_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            sql = "Create table constraint_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();

            transaction.Commit();
            return connection;
        }

        [Test]
        public void ConstraintValidation_DeleteOneToOneChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Status =EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2OneChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol= id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                IEnumerator<ConstraintTestOne2ManyEntity> childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                ConstraintTestOne2ManyEntity loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToMany = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2OneChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestReverseRootEntity entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestReverseRootEntity loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToMany = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

            IDbTransaction transaction = connection.BeginTransaction();
            int id = 45;
            ConstraintTestDeleteRestrictRootEntity entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol =id;
            entity.Name = "Org-Name";
            entity.Persist(connection);
            transaction.Commit();

            transaction = connection.BeginTransaction();
            ConstraintTestOne2OneEntity one2OneEntity = new ConstraintTestOne2OneEntity();
            one2OneEntity.IdCol = id;
            one2OneEntity.Name = "Child-Org-Name";
            one2OneEntity.Persist(connection);
            transaction.Commit();

            transaction = connection.BeginTransaction();
            ConstraintTestDeleteRestrictRootEntity loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(connection,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            loadedEntity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }
    
        [Test]
        [ExpectedException(typeof(PersistException))]
        public void ConstraintValidation_DeleteRootWithOneToManyChild_WithRestrictConstraint_ShouldThrowException()
        {
            IDbConnection connection = SetupTables();

            IDbTransaction transaction = connection.BeginTransaction();
            int id = 45;
            ConstraintTestDeleteRestrictRootEntity entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Persist(connection);
            transaction.Commit();

            transaction = connection.BeginTransaction();
            ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
            one2ManyEntity.IdCol = id;
            one2ManyEntity.Name = "Child-Org-Name";
            one2ManyEntity.Persist(connection);
            transaction.Commit();

            transaction = connection.BeginTransaction();
            ConstraintTestDeleteRestrictRootEntity loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(connection,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            loadedEntity.Persist(connection);
            transaction.Commit();
            connection.Close();
        }
    
        [Test]
        public void ConstraintValidation_DeleteOneToManyChild_WithCascadeConstraint_ShouldDeleteChild()
        {
            try
            {
                IDbConnection connection = SetupTables();

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                IEnumerator<ConstraintTestOne2ManyEntity> childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                ConstraintTestOne2ManyEntity loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToMany = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2OneEntity one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.One2OneEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2ManyEntity one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToMany = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

                IDbTransaction transaction = connection.BeginTransaction();
                int id = 45;
                ConstraintTestDeleteCascadeRootEntity entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestOne2OneEntity one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(connection);
                transaction.Commit();

                transaction = connection.BeginTransaction();
                ConstraintTestDeleteCascadeRootEntity loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(connection,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(connection);
                transaction.Commit();

                bool hasOneToOne = ExistsOne2ManyChild(connection,id);
                bool hasRoot = ExistsRoot(connection,id);
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

        private bool LoadEntityWithId(IDbConnection connection, ConstraintTestDeleteCascadeRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }

        private bool LoadEntityWithId(IDbConnection connection, ConstraintTestDeleteRestrictRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }

        private bool LoadEntityWithId(IDbConnection connection, ConstraintTestReverseRootEntity loadEntity,int id)
        {
            bool loaded = false;

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = id;

            IDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.Read())
            {
                loadEntity.Retrieve(dataReader, connection);
                loaded = true;
            }

            return loaded;
        }

        private bool ExistsRoot(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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

        private bool ExistsOne2OneChild(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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

        private bool ExistsOne2ManyChild(IDbConnection connection,int id)
        {
            bool exists = false;

            IDbCommand cmd = connection.CreateCommand();
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