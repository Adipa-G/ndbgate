using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions;
using DbGate.Persist.Support.Constraint;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateConstraintValidationTest : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-constraint";

        public DbGateConstraintValidationTest()
        {
            TestClass = typeof(DbGateConstraintValidationTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Manual;
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }
        
        private IDbConnection SetupTables()
        {
            var sql = "Create table constraint_test_root (\n" +
                      "\tid_col Int NOT NULL,\n" +
                      "\tname Varchar(20) NOT NULL,\n" +
                      " Primary Key (id_col))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table constraint_test_one2many (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tindex_no Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col,index_no))";
            CreateTableFromSql(sql, DbName);

            sql = "Create table constraint_test_one2one (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  " Primary Key (id_col))";
            CreateTableFromSql(sql,DbName);
            EndInit(DbName);
            
            return Connection;
        }

        [Fact]
        public void ConstraintValidation_DeleteOneToOneChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                loadedEntity.One2OneEntity.Status =EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2OneChild(transaction, id);
                var hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.True(hasOneToOne);
                Assert.True(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteOneToManyChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol= id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                var childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                var loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToMany = ExistsOne2ManyChild(transaction, id);
                var hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.True(hasOneToMany);
                Assert.True(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteRootWithOneToOneChild_WithReverseRelationShip_ShouldThrowException()
        {
            try
            {
                var connection = SetupTables();
                var transaction = CreateTransaction(connection);
                
                var id = 45;
                var entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2OneEntity = new ConstraintTestOne2OneEntity();
                one2OneEntity.IdCol = id;
                one2OneEntity.Name = "Child-Org-Name";
                one2OneEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2OneChild(transaction,id);
                var hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.True(hasOneToOne);
                Assert.False(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteRootWithOneToManyChild_WithReverseRelationShip_ShouldNotDeleteChild()
        {
            try
            {
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestReverseRootEntity();
                entity.IdCol = id;
                entity.Name = "Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol = id;
                one2ManyEntity.IndexNo = 1;
                one2ManyEntity.Name = "Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestReverseRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToMany = ExistsOne2ManyChild(transaction,id);
                var hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.True(hasOneToMany);
                Assert.False(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteRootWithOneToOneChild_WithRestrictConstraint_ShouldThrowException()
        {
            var connection = SetupTables();

            var transaction = CreateTransaction(connection);
            var id = 45;
            var entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol =id;
            entity.Name = "Org-Name";
            entity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            var one2OneEntity = new ConstraintTestOne2OneEntity();
            one2OneEntity.IdCol = id;
            one2OneEntity.Name = "Child-Org-Name";
            one2OneEntity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            var loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(transaction,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            Assert.Throws<PersistException>(() => loadedEntity.Persist(transaction));
            transaction.Commit();
            connection.Close();
        }
    
        [Fact]
        public void ConstraintValidation_DeleteRootWithOneToManyChild_WithRestrictConstraint_ShouldThrowException()
        {
            var connection = SetupTables();

            var transaction = CreateTransaction(connection);
            var id = 45;
            var entity = new ConstraintTestDeleteRestrictRootEntity();
            entity.IdCol = id;
            entity.Name = "Org-Name";
            entity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            var one2ManyEntity = new ConstraintTestOne2ManyEntity();
            one2ManyEntity.IdCol = id;
            one2ManyEntity.Name = "Child-Org-Name";
            one2ManyEntity.Persist(transaction);
            transaction.Commit();

            transaction = CreateTransaction(connection);
            var loadedEntity = new ConstraintTestDeleteRestrictRootEntity();
            LoadEntityWithId(transaction,loadedEntity,id);
            loadedEntity.Status = EntityStatus.Deleted;
            Assert.Throws<PersistException>(() => loadedEntity.Persist(transaction));
            transaction.Commit();
            connection.Close();
        }
    
        [Fact]
        public void ConstraintValidation_DeleteOneToManyChild_WithCascadeConstraint_ShouldDeleteChild()
        {
            try
            {
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                var childEnumarator = loadedEntity.One2ManyEntities.GetEnumerator();
                childEnumarator.MoveNext();
                var loadedOne2ManyEntity = childEnumarator.Current;
                loadedOne2ManyEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToMany = ExistsOne2ManyChild(transaction,id);
                var hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToMany);
                Assert.True(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }
    
        [Fact]
        public void ConstraintValidation_DeleteOneToOneChild_WithCascadeConstraint_ShouldDeleteChild()
        {
            try
            {
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.One2OneEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2ManyChild(transaction,id);
                var hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToOne);
                Assert.True(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteOneToManyRoot_WithCascadeConstraint_ShouldDeleteBoth()
        {
            try
            {
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2ManyEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.IndexNo=1;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction,loadedEntity,id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToMany = ExistsOne2ManyChild(transaction,id);
                var hasRoot = ExistsRoot(transaction,id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToMany);
                Assert.False(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void ConstraintValidation_DeleteOneToOneRoot_WithCascadeConstraint_ShouldDeleteBoth()
        {
            try
            {
                var connection = SetupTables();

                var transaction = CreateTransaction(connection);
                var id = 45;
                var entity = new ConstraintTestDeleteCascadeRootEntity();
                entity.IdCol =id;
                entity.Name ="Org-Name";
                entity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var one2ManyEntity = new ConstraintTestOne2OneEntity();
                one2ManyEntity.IdCol =id;
                one2ManyEntity.Name ="Child-Org-Name";
                one2ManyEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var loadedEntity = new ConstraintTestDeleteCascadeRootEntity();
                LoadEntityWithId(transaction, loadedEntity, id);
                loadedEntity.Status = EntityStatus.Deleted;
                loadedEntity.Persist(transaction);
                transaction.Commit();

                transaction = CreateTransaction(connection);
                var hasOneToOne = ExistsOne2ManyChild(transaction, id);
                var hasRoot = ExistsRoot(transaction, id);
                transaction.Commit();
                connection.Close();

                Assert.False(hasOneToOne);
                Assert.False(hasRoot);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateConstraintValidationTest)).Fatal(e.Message,e);
                Assert.Fail(e.Message);
            }
        }

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestDeleteCascadeRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestDeleteRestrictRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool LoadEntityWithId(ITransaction transaction, ConstraintTestReverseRootEntity loadEntity,int id)
        {
            var loaded = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool ExistsRoot(ITransaction transaction,int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_root where id_col = ?";

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

        private bool ExistsOne2OneChild(ITransaction transaction,int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_one2one where id_col = ?";

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

        private bool ExistsOne2ManyChild(ITransaction transaction,int id)
        {
            var exists = false;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from constraint_test_one2many where id_col = ?";

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
    }
}