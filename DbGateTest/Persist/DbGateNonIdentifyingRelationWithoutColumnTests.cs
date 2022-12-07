using System;
using System.Data;
using System.Linq;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.Persist.Support.NonIdentifyingRelationWithoutColumn;
using log4net;
using Xunit;

namespace DbGate.Persist
{
    [Collection("Sequential")]
    public class DbGateNonIdentifyingRelationWithoutColumnTests : AbstractDbGateTestBase, IDisposable
    {
        private const string DbName = "testing-non-identifying-relatin-without-column";

        public DbGateNonIdentifyingRelationWithoutColumnTests()
        {
            TestClass = typeof(DbGateFeatureIntegrationTest);
            BeginInit(DbName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.VerifyOnWriteStrategy = VerifyOnWriteStrategy.DoNotVerify;
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
        }
        public void Dispose()
        {
            CleanupDb(DbName);
            FinalizeDb(DbName);
        }

        private IDbConnection SetupTables()
        {
            RegisterClassForDbPatching(typeof(Currency), DbName);
            RegisterClassForDbPatching(typeof(Product), DbName);
            EndInit(DbName);

            return Connection;
        }

        [Fact]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                var connection = SetupTables();
                var tx = CreateTransaction(connection);

                var currencyId = 45;
                var productId = 55;

                var currency = new Currency();
                currency.CurrencyId = currencyId;
                currency.Code = "LKR";
                currency.Persist(tx);

                var product = new Product();
                product.ProductId = productId;
                product.Price = 300;
                product.Currency = currency;
                product.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                var loaded = LoadProductWithId(tx, productId);
                Assert.NotNull(loaded);
                Assert.NotNull(loaded.Currency);
                Assert.Equal(loaded.Currency.CurrencyId, currency.CurrencyId);
                Assert.Equal(loaded.Currency.Code, currency.Code);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_UpdateShouldBeSameAsPersisted()
        {
            try
            {
                var connection = SetupTables();
                var tx = CreateTransaction(connection);

                var currencyAId = 35;
                var currencyBId = 45;
                var productId = 55;

                var currencyA = new Currency();
                currencyA.CurrencyId = currencyAId;
                currencyA.Code = "LKR";
                currencyA.Persist(tx);

                var currencyB = new Currency();
                currencyB.CurrencyId = currencyBId;
                currencyB.Code = "USD";
                currencyB.Persist(tx);

                var product = new Product();
                product.ProductId = productId;
                product.Price = 300;
                product.Currency = currencyA;
                product.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                var loaded = LoadProductWithId(tx,productId);
                loaded.Currency = currencyB;
                loaded.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                loaded = LoadProductWithId(tx,productId);
                Assert.NotNull(loaded);
                Assert.NotNull(loaded.Currency);
                Assert.Equal(loaded.Currency.CurrencyId,currencyB.CurrencyId);
                Assert.Equal(loaded.Currency.Code, currencyB.Code);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Fact]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_DeleteShouldBeSameAsPersisted()
        {
            try
            {
                var connection = SetupTables();
                var tx = CreateTransaction(connection);

                var currencyId = 45;
                var productId = 55;

                var currency = new Currency();
                currency.CurrencyId = currencyId;
                currency.Code = "LKR";
                currency.Persist(tx);

                var product = new Product();
                product.ProductId = productId;
                product.Price = 300;
                product.Currency = currency;
                product.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                var loaded = LoadProductWithId(tx, productId);
                loaded.Currency = null;
                loaded.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                loaded = LoadProductWithId(tx, productId);
                Assert.NotNull(loaded);
                Assert.Null(loaded.Currency);
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        private Product LoadProductWithId(ITransaction transaction, int id)
        {
            var query = new SelectionQuery()
                .From(QueryFrom.EntityType(typeof (Product)))
                .Where(QueryCondition.Expression(ConditionExpr.Build().Field(typeof(Product), "ProductId").Eq().Value(id)))
                .Select(QuerySelection.EntityType(typeof (Product))).ToList(transaction);

            return query.First() as Product;
        }
    }
}