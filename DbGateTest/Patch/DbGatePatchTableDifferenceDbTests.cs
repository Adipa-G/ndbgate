using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Patch.Support.PatchTableDifferences;
using log4net;
using Xunit;

namespace DbGate.Patch
{
    [Collection("Sequential")]
    public class DbGatePatchTableDifferenceDbTests : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "unit-testing-metadata-table-difference";

        public DbGatePatchTableDifferenceDbTests()
        {
            TestClass = typeof(DbGatePatchTableDifferenceDbTests);
            BeginInit(DbName);
        }

        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }

        [Fact]
        public void PatchDifference_PatchDB_WithTableColumnAdded_ShouldAddColumn()
        {
            try
            {
                var transaction = TransactionFactory.CreateTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                TransactionFactory.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                TransactionFactory.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();

                var id = 35;
                transaction = CreateTransaction(); 
                var columnEntity = CreateFourColumnEntity(id);
                columnEntity.Persist(transaction);
                columnEntity = LoadFourColumnEntityWithId(transaction, id);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void PatchDifference_PatchDB_WithTableColumnDeleted_ShouldDeleteColumn()
        {
            try
            {
                var transaction = TransactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();
 
                //Sqllite does not support dropping columns, so this test does not work
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void PatchDifference_PatchDB_WithTableColumnChanged_ShouldUpdateColumn()
        {
            try
            {
                var longStr = new string('A', 220);

                var transaction = TransactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                transaction.Commit();
 
                var id = 34;
                transaction = CreateTransaction(); 
                var columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                try
                {
                    columnEntity.Persist(transaction);
                }
                catch (System.Exception)
                {
                }

                transaction = CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnTypeDifferentEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                transaction.Commit();
 
                id = 35;
                transaction = CreateTransaction();  
                columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                columnEntity.Persist(transaction);
                transaction.Commit();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private FourColumnEntity LoadFourColumnEntityWithId(ITransaction transaction, int id)
        {
            FourColumnEntity loadedEntity = null;

            var cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from table_change_test_entity where id_col = ?";

            var parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            var rs = cmd.ExecuteReader();
            if (rs.Read())
            {
                loadedEntity = new FourColumnEntity();
                loadedEntity.Retrieve(rs, transaction);
            }

            return loadedEntity;
        }

        private FourColumnEntity CreateFourColumnEntity(int id)
        {
            var entity = new FourColumnEntity();
            entity.IdCol = id;
            entity.Code = "4C";
            entity.Name = "4Col";
            entity.IndexNo = 0;
            return entity;
        }

        private ThreeColumnEntity CreateThreeColumnEntity(int id)
        {
            var entity = new ThreeColumnEntity();
            entity.IdCol = id;
            entity.Name = "3Col";
            entity.IndexNo = 0;
            return entity;
        }
    }
}