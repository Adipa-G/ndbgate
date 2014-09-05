﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.Exceptions;
using DbGate.DbUtility;
using DbGate.ErManagement.ErMapper;
using DbGate.Support.Persistant.NonIdentifyingRelationWithoutColumn;
using log4net;
using NUnit.Framework;

namespace DbGate
{
    public class DbGateNonIdentifyingRelationWithoutColumnTests : AbstractDbGateTestBase
    {
        private const string DBName = "testing-non-identifying-relatin-without-column";

        [TestFixtureSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateFeatureIntegrationTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        private IDbConnection SetupTables()
        {
            RegisterClassForDbPatching(typeof(Currency), DBName);
            RegisterClassForDbPatching(typeof(Product), DBName);
            EndInit(DBName);

            return Connection;
        }

        [Test]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_LoadedShouldBeSameAsPersisted()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction tx = CreateTransaction(connection);

                int currencyId = 45;
                int productId = 55;

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
                Product loaded = LoadProductWithId(tx, productId);
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.Currency);
                Assert.AreEqual(loaded.Currency.CurrencyId, currency.CurrencyId);
                Assert.AreEqual(loaded.Currency.Code, currency.Code);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_UpdateShouldBeSameAsPersisted()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction tx = CreateTransaction(connection);

                int currencyAId = 35;
                int currencyBId = 45;
                int productId = 55;

                Currency currencyA = new Currency();
                currencyA.CurrencyId = currencyAId;
                currencyA.Code = "LKR";
                currencyA.Persist(tx);

                Currency currencyB = new Currency();
                currencyB.CurrencyId = currencyBId;
                currencyB.Code = "USD";
                currencyB.Persist(tx);

                Product product = new Product();
                product.ProductId = productId;
                product.Price = 300;
                product.Currency = currencyA;
                product.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                Product loaded = LoadProductWithId(tx,productId);
                loaded.Currency = currencyB;
                loaded.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                loaded = LoadProductWithId(tx,productId);
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.Currency);
                Assert.AreEqual(loaded.Currency.CurrencyId,currencyB.CurrencyId);
                Assert.AreEqual(loaded.Currency.Code, currencyB.Code);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(DbGateFeatureIntegrationTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void NonIdentifyingRelationWithoutColumn_Persist_WithRelation_DeleteShouldBeSameAsPersisted()
        {
            try
            {
                IDbConnection connection = SetupTables();
                ITransaction tx = CreateTransaction(connection);

                int currencyId = 45;
                int productId = 55;

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
                Product loaded = LoadProductWithId(tx, productId);
                loaded.Currency = null;
                loaded.Persist(tx);
                tx.Commit();

                tx = CreateTransaction(connection);
                loaded = LoadProductWithId(tx, productId);
                Assert.IsNotNull(loaded);
                Assert.IsNull(loaded.Currency);
            }
            catch (Exception e)
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