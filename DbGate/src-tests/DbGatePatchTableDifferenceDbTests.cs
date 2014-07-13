using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using DbGate.Support.Patch.PatchTableDifferences;
using NUnit.Framework;
using log4net;
using log4net.Config;

namespace DbGate
{
    public class DbGatePatchTableDifferenceDbTests
    {
        private static ITransactionFactory _transactionFactory;

        [TestFixtureSetUp]
        public static void Before()
        {
            try
            {
                XmlConfigurator.Configure(new FileInfo("log4net.config"));

                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Info(
                    "Starting in-memory database for unit tests");
                _transactionFactory =
                    new DefaultTransactionFactory(
                        "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=4;foreign_keys = ON",
                        DefaultTransactionFactory.DbSqllite);

                ITransaction transaction = _transactionFactory.CreateTransaction();
                transaction.Close();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(
                    "Exception during database startup.", ex);
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
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(
                    "Exception during test cleanup.", ex);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnAdded_ShouldAddColumn()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction();
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                _transactionFactory.DbGate.PatchDataBase(transaction, types, true);
                var connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                transaction = _transactionFactory.CreateTransaction();
                types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                _transactionFactory.DbGate.PatchDataBase(transaction, types, false);
                connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                int id = 35;
                transaction = _transactionFactory.CreateTransaction();
                FourColumnEntity columnEntity = CreateFourColumnEntity(id);
                columnEntity.Persist(transaction);
                columnEntity = LoadFourColumnEntityWithId(transaction, id);
                connection = transaction.Connection;
                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnDeleted_ShouldDeleteColumn()
        {
            try
            {
                ITransaction transaction = _transactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (FourColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                var connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                transaction = _transactionFactory.CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                //Sqllite does not support dropping columns, so this test does not work
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void PatchDifference_PatchDB_WithTableColumnChanged_ShouldUpdateColumn()
        {
            try
            {
                var longStr = new string('A', 220);

                ITransaction transaction = _transactionFactory.CreateTransaction(); 
                ICollection<Type> types = new List<Type>();
                types.Add(typeof (ThreeColumnEntity));
                transaction.DbGate.PatchDataBase(transaction, types, true);
                var connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                int id = 34;
                transaction = _transactionFactory.CreateTransaction(); 
                ThreeColumnEntity columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                try
                {
                    columnEntity.Persist(transaction);
                }
                catch (AssertionException)
                {
                    throw;
                }
                catch (Exception)
                {
                }
                transaction.Close();

                transaction = _transactionFactory.CreateTransaction(); 
                types = new List<Type>();
                types.Add(typeof (ThreeColumnTypeDifferentEntity));
                transaction.DbGate.PatchDataBase(transaction, types, false);
                connection = transaction.Connection;
                transaction.Commit();
                connection.Close();

                id = 35;
                transaction = _transactionFactory.CreateTransaction(); 
                columnEntity = CreateThreeColumnEntity(id);
                columnEntity.Name = longStr;
                columnEntity.Persist(transaction);
                connection = transaction.Connection;
                transaction.Commit();
                connection.Close();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof (DbGatePatchTableDifferenceDbTests)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private FourColumnEntity LoadFourColumnEntityWithId(ITransaction transaction, int id)
        {
            FourColumnEntity loadedEntity = null;

            IDbCommand cmd = transaction.CreateCommand();
            cmd.CommandText = "select * from table_change_test_entity where id_col = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            IDataReader rs = cmd.ExecuteReader();
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