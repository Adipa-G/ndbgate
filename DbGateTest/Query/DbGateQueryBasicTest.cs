using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbGate.ErManagement.Query;
using DbGate.ErManagement.Query.Expr;
using DbGate.Query.Support.BasicTest;
using log4net;
using NUnit.Framework;

namespace DbGate.Query
{
    [TestFixture]
    public class DbGateQueryBasicTest : AbstractDbGateTestBase
    {
        private int[] basicEntityIds;
        private string[] basicEntityNames;
        private bool[] hasOverrideChildren;

        private ICollection<QueryBasicEntity> basicEntities;
        private ICollection<QueryBasicDetailsEntity> detailedEntities;

        private const string DBName = "init-testing-query-basic";

        [OneTimeSetUp]
        public static void Before()
        {
            TestClass = typeof(DbGateQueryBasicTest);
        }

        [SetUp]
        public void BeforeEach()
        {
            BeginInit(DBName);
            TransactionFactory.DbGate.ClearCache();
            TransactionFactory.DbGate.Config.DirtyCheckStrategy = DirtyCheckStrategy.Automatic;
        }

        [TearDown]
        public void AfterEach()
        {
            CleanupDb(DBName);
            FinalizeDb(DBName);
        }

        private IDbConnection SetupTables()
        {
            string sql = "Create table query_basic (\n" +
                         "\tid_col Int NOT NULL,\n" +
                         "\tname Varchar(20) NOT NULL,\n" +
                         " Primary Key (id_col))";
            CreateTableFromSql(sql,DBName);

            sql = "Create table query_basic_details (\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\tdescription Varchar(50) NOT NULL )";
            CreateTableFromSql(sql, DBName);

            sql = "Create table query_basic_join (\n" +
                  "\tid_col Int NOT NULL,\n" +
                  "\tname Varchar(20) NOT NULL,\n" +
                  "\toverride_description Varchar(50) NOT NULL )";
            CreateTableFromSql(sql, DBName);

            EndInit(DBName);
            return Connection;
        }

        private QueryBasicEntity GetById(int id)
        {
            foreach (QueryBasicEntity basicEntity in basicEntities)
            {
                if (basicEntity.IdCol == id)
                    return basicEntity;
            }
            return null;
        }

        private QueryBasicDetailsEntity GetDescriptionForName(String name)
        {
            foreach (QueryBasicDetailsEntity detailsEntity in detailedEntities)
            {
                if (detailsEntity.Name.Equals(name))
                    return detailsEntity;
            }
            return null;
        }

        private bool HasIds(ICollection<object> list, params int[] idList)
        {
            if (list.Count != idList.Length)
                return false;

            foreach (int id in idList)
            {
                bool found = false;
                foreach (object listItem in list)
                {
                    if (listItem is QueryBasicEntity)
                    {
                        found = found || ((QueryBasicEntity) listItem).IdCol == id;
                    }
                    else if (listItem is object[])
                    {
                        var items = (object[]) listItem;
                        foreach (object item in items)
                        {
                            if (item is int)
                            {
                                found = found || ((int) item) == id;
                            }
                        }
                    }
                    else
                    {
                        found = found || ((int) listItem) == id;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private void CreateTestData(ITransaction transaction)
        {
            basicEntityIds = new[] {35, 45, 55, 65};
            basicEntityNames = new[] {"Org-NameA", "Org-NameA", "Org-NameA", "Org-NameB"};
            hasOverrideChildren = new[] {true, false, false, true};

            basicEntities = new List<QueryBasicEntity>();
            detailedEntities = new List<QueryBasicDetailsEntity>();

            for (int i = 0, basicEntityIdsLength = basicEntityIds.Length; i < basicEntityIdsLength; i++)
            {
                int basicEntityId = basicEntityIds[i];
                var entity = new QueryBasicEntity();
                entity.IdCol = basicEntityId;
                entity.Name = basicEntityNames[i];
                if (hasOverrideChildren[i])
                {
                    var joinEntity = new QueryBasicJoinEntity();
                    joinEntity.OverrideDescription = entity.Name + "Details";
                    entity.JoinEntity = joinEntity;
                }
                entity.Persist(transaction);
                basicEntities.Add(entity);
            }

            foreach (string basicEntityName in basicEntityNames)
            {
                bool found = false;
                foreach (QueryBasicDetailsEntity entity in detailedEntities)
                {
                    if (entity.Name.Equals(basicEntityName))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    continue;
                }
                var detailsEntity = new QueryBasicDetailsEntity();
                detailsEntity.Name = basicEntityName;
                detailsEntity.Description = basicEntityName + "Details";
                detailsEntity.Persist(transaction);
                detailedEntities.Add(detailsEntity);
            }
        }

        [Test]
        public void QueryBasic_Basic_WithSql_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Select(QuerySelection.RawSql("id_col"))
                    .Select(QuerySelection.RawSql("name as name_col"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
                {
                    var resultArray = (object[]) result;
                    var id = (int) resultArray[0];
                    var name = (string) resultArray[1];

                    QueryBasicEntity entity = GetById(id);
                    Assert.AreEqual(entity.Name, name);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Distinct_ShouldSelectDistinct()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.RawSql("name as name_col"))
                    .Distinct();

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 2);
                int count = 0;
                foreach (object result in results)
                {
                    var name = result.ToString();
                    foreach (QueryBasicEntity basicEntity in basicEntities)
                    {
                        if (basicEntity.Name.Equals(name))
                            count++;
                    }
                }
                Assert.IsTrue(count == 4);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Skip_ShouldSkip()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>())
                    .Skip(1);

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds.Skip(1).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Fetch_ShouldFetch()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>())
                    .Fetch(2);

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds.Take(2).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_SkipAndFetch_ShouldSkipAndFetch()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>())
                    .Skip(1).Fetch(2);

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds.Skip(1).Take(2).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithTypeSelection_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
                {
                    var loadedEntity = (QueryBasicEntity)result;

                    QueryBasicEntity orgEntity = GetById(loadedEntity.IdCol);
                    Assert.AreEqual(loadedEntity.Name, orgEntity.Name);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithTypeSelectionGeneric_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
                {
                    var loadedEntity = (QueryBasicEntity)result;

                    QueryBasicEntity orgEntity = GetById(loadedEntity.IdCol);
                    Assert.AreEqual(loadedEntity.Name, orgEntity.Name);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithSubQuerySelection_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery descriptionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicDetailsEntity>("qbd1"))
                    .Where(QueryCondition.RawSql("qbd1.name = qb1.name"))
                    .Select(QuerySelection.RawSql("qbd1.description")).Fetch(1);

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>())
                    .Select(QuerySelection.Query(descriptionQuery, "description"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                foreach (object result in results)
                {
                    var resultArray = (object[]) result;
                    var entity = (QueryBasicEntity) resultArray[0];
                    var description = (string) resultArray[1];

                    QueryBasicDetailsEntity detailsEntity = GetDescriptionForName(entity.Name);
                    Assert.AreEqual(detailsEntity.Description, description);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithFieldSelectionWithClass_ShouldSelectColumn()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Field(typeof (QueryBasicEntity), "Name", "name1"));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                int index = 0;
                foreach (object result in results)
                {
                    var name = result.ToString();
                    Assert.IsTrue(basicEntityNames[index++].Equals(name));
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithFieldSelectionWithoutClass_ShouldSelectColumn()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Field("Name", "name1"));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                int index = 0;
                foreach (object result in results)
                {
                    var name = result.ToString();
                    Assert.IsTrue(basicEntityNames[index++].Equals(name));
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithFieldWithExpression_ShouldSelectColumn()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Field<QueryBasicEntity>(r => r.Name));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                int index = 0;
                foreach (object result in results)
                {
                    var name = result.ToString();
                    Assert.IsTrue(basicEntityNames[index++].Equals(name));
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithSumSelection_ShouldSelectSum()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Sum<QueryBasicEntity>(r => r.IdCol, "id_sum"));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 1);
                int sum = 0;
                foreach (QueryBasicEntity entity in basicEntities)
                {
                    sum += entity.IdCol;
                }
                foreach (Object result in results)
                {
                    var resultSum = (long)result;
                    Assert.IsTrue(sum == resultSum);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithCountSelection_ShouldSelectCount()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Count<QueryBasicEntity>(r => r.IdCol, "id_count"));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 1);
                foreach (Object result in results)
                {
                    var resultCount = (long)result;
                    Assert.IsTrue(resultCount == 4);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Select_WithCustomFunctionCountAsExampleSelection_ShouldSelectCount()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.CustFunction<QueryBasicEntity>("COUNT",f => f.IdCol, "id_count"));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 1);
                foreach (Object result in results)
                {
                    var resultCount = (long)result;
                    Assert.IsTrue(resultCount == 4);
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_From_WithTypeFrom_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicEntity),"qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_From_WithTypeFromGeneric_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof(DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_From_WithQueryFrom_ShouldSelectAll()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery fromQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType< QueryBasicEntity>());

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.Query(fromQuery, "qb1"))
                    .Select(QuerySelection.EntityType< QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_From_WithQueryUnionFrom_ShouldSelectUnion()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery fromBasic = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Select(QuerySelection.RawSql("name as name1"));

                ISelectionQuery fromDetails = new SelectionQuery()
                    .From(QueryFrom.EntityType< QueryBasicDetailsEntity>())
                    .Select(QuerySelection.RawSql("name as name1"));

                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.QueryUnion(true, new[] {fromBasic, fromDetails}))
                    .Select(QuerySelection.RawSql("name1"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 6);
                int index = 0;
                foreach (object result in results)
                {
                    var name = result.ToString();
                    if (index < 4)
                    {
                        Assert.IsTrue(basicEntityNames[index++].Equals(name));
                    }
                    else
                    {
                        QueryBasicDetailsEntity detailsEntity = detailedEntities.ToArray()[index++ - 4];
                        Assert.IsTrue(detailsEntity.Name.Equals(name));
                    }
                }
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSql_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Where(QueryCondition.RawSql("id_col = 35"))
                    .Where(QueryCondition.RawSql("name like 'Org-NameA'"))
                    .Select(QuerySelection.RawSql("id_col,name"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, 35);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithEqExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Eq().Value(
                            ColumnType.Integer, 35)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNEqExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Neq().Value(
                            ColumnType.Integer, 35)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Where(id => id != 35).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithGtExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Gt().Value(
                            ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Where(id => id > 45).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithGeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Ge().Value(
                                                             ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Where(id => id >= 45).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLtExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Lt().Value(
                                                             ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Where(id => id < 45).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Le().Value(
                                                             ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Where(id => id <= 45).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithLikeExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.Name).Like().Value(
                                                             ColumnType.Varchar, "Org-NameA")))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 45, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNeqExpression_WithField_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicDetailsEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicDetailsEntity>(r => r.Name).Neq()
                        .Field<QueryBasicDetailsEntity>(r => r.Description)))
                    .Select(QuerySelection.EntityType<QueryBasicDetailsEntity>());

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 2);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithGtExpression_WithQuery_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qbd1"))
                    .OrderBy(QueryOrderBy.RawSql("id_col"))
                    .Select(QuerySelection.Field<QueryBasicEntity>(r => r.IdCol, "id_col")).Fetch(1);

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Gt().Query(subQuery)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds.Skip(1).ToArray());
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithBetweenExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).Between().Values(
                            ColumnType.Integer, 35, 55)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 45, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithInExpression_WithValue_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).In().Values(
                            ColumnType.Integer, 35, 55)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithInExpression_WithQuery_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Select(QuerySelection.Field<QueryBasicEntity>(r => r.IdCol, null));

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(r => r.IdCol).In().Query(subQuery)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithExistsExpression_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType(typeof(QueryBasicDetailsEntity), "qbd1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicDetailsEntity>(r => r.Description, "qbd1").Eq()
                        .Field<QueryBasicEntity>(r => r.Name, "qb1")))
                    .Select(QuerySelection.EntityType(typeof(QueryBasicDetailsEntity)));

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Query(subQuery).Exists()))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithNotExistsExpression_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery subQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicDetailsEntity>("qbd1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicDetailsEntity>(f => f.Name, "qbd1").Eq()
                        .Field<QueryBasicEntity>(f => f.Name, "qb1")))
                    .Select(QuerySelection.EntityType<QueryBasicDetailsEntity>());

                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Query(subQuery).NotExists()))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 0);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSimpleMergeExpressionSingleAnd_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 35, 55)
                        .And().Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 45, 55)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSimpleMergeExpressionTwoOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 35, 55)
                        .Or().Field<QueryBasicEntity>(f => f.IdCol).In().Value(
                            ColumnType.Integer, 55)
                        .Or().Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 45, 55)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 45, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithSimpleMergeExpressionSingleAndSingleOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 35, 55)
                        .Or().Field<QueryBasicEntity>(f => f.IdCol).In().Value(
                            ColumnType.Integer, 55)
                        .And().Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                            ColumnType.Integer, 45, 55)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithComplexMergeExpressionCombinedTwoAndsWithOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .And(
                            ConditionExpr.Build().Field<QueryBasicEntity>(f => f.IdCol).In()
                                .Values(ColumnType.Integer, 35, 55),
                            ConditionExpr.Build().Field<QueryBasicEntity>(f => f.IdCol).Eq()
                                .Value(ColumnType.Integer, 55)).Or().Field<QueryBasicEntity>(f => f.IdCol).Eq()
                        .Value(ColumnType.Integer, 45))).Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 45, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Condition_WithComplexMergeExpressionCombinedTwoOrsWithOr_ShouldSelectMatching()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>())
                    .Where(QueryCondition.Expression(ConditionExpr.Build()
                        .Or(ConditionExpr.Build()
                                .Field<QueryBasicEntity>(f => f.IdCol).In().Values(
                                    ColumnType.Integer, 35, 55)
                            , ConditionExpr.Build()
                                .Field<QueryBasicEntity>(f => f.IdCol).Eq().Value
                                (ColumnType.Integer, 55))
                        .Or().Field<QueryBasicEntity>(f => f.IdCol).Eq().Value(
                            ColumnType.Integer, 45)))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = query.ToList(transaction);
                HasIds(results, 35, 45, 55);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithBasicSql_ShouldJoin()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .Join(QueryJoin.RawSql("inner join query_basic_details qbd1 on qb1.name = qbd1.name"))
                    .OrderBy(QueryOrderBy.RawSql("qb1.name"))
                    .Select(QuerySelection.EntityType(typeof (QueryBasicEntity)));

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinDirectionOfDefinition_ShouldJoin()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Join(QueryJoin.EntityType<QueryBasicEntity, QueryBasicJoinEntity>("qbj1"))
                    .Select(QuerySelection.Field<QueryBasicEntity>(f => f.IdCol, null));

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, 35, 65);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinDirectionOppositeToDefinition_ShouldJoin()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicJoinEntity>("qbj1"))
                    .Join(QueryJoin.EntityType<QueryBasicJoinEntity, QueryBasicEntity>("qb1"))
                    .Select(QuerySelection.Field<QueryBasicJoinEntity>(f => f.IdCol, null));

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, 35, 65);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithEntityDefinedJoinWithOuterJoin_ShouldJoin()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicJoinEntity>("qbj1"))
                    .Join(QueryJoin.EntityType<QueryBasicJoinEntity, QueryBasicEntity>("qb1", QueryJoinType.Left))
                    .Select(QuerySelection.Field<QueryBasicJoinEntity>(f => f.IdCol, null));

                ICollection<object> results = selectionQuery.ToList(transaction);
                HasIds(results, basicEntityIds);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Join_WithoutEntityDefinedJoinWithExpression_ShouldJoin()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb1"))
                    .Join(QueryJoin.EntityType<QueryBasicEntity, QueryBasicDetailsEntity>(
                        JoinExpr.Build().Field<QueryBasicEntity>(f => f.Name).Eq()
                            .Field<QueryBasicDetailsEntity>(f => f.Name), "qbd1"))
                    .Select(QuerySelection.Field<QueryBasicDetailsEntity>(f => f.Description, null));


                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.AreEqual(4,results.Count);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Group_WithBasicSql_ShouldGroup()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .GroupBy(QueryGroup.RawSql("Name"))
                    .Select(QuerySelection.RawSql("Name"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 2);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_Group_WithExpression_ShouldGroup()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb"))
                    .GroupBy(QueryGroup.Field<QueryBasicEntity>(f => f.Name))
                    .Select(QuerySelection.Field<QueryBasicEntity>(f => f.Name, null));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 2);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ExecuteWithGroupCondition_WithBasicSqlQuery_shouldLoadTarget()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .GroupBy(QueryGroup.RawSql("name"))
                    .Having(QueryGroupCondition.RawSql("count(id_col)>1"))
                    .Select(QuerySelection.RawSql("name"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 1);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_GroupCondition_WithExpressionCount_ShouldSelectMatchingGroups()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery query = new SelectionQuery()
                    .From(QueryFrom.EntityType<QueryBasicEntity>("qb"))
                    .Select(QuerySelection.Field<QueryBasicEntity>(f => f.Name, null))
                    .GroupBy(QueryGroup.Field<QueryBasicEntity>(f => f.Name))
                    .Having(QueryGroupCondition.Expression(GroupConditionExpr.Build()
                        .Field<QueryBasicEntity>(f => f.Name).Count().Gt().Value(ColumnType.Integer, 1)));

                ICollection<object> results = query.ToList(transaction);
                Assert.IsTrue(results.Count == 1);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void ExecuteWithOrderBy_WithBasicSqlQuery_shouldLoadTarget()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .OrderBy(QueryOrderBy.RawSql("name"))
                    .Select(QuerySelection.RawSql("name"));

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void QueryBasic_OrderBy_WithExpression_ShouldSelectOrdered()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .OrderBy(QueryOrderBy.Field<QueryBasicEntity>(f => f.Name))
                    .OrderBy(QueryOrderBy.Field<QueryBasicEntity>(f => f.IdCol))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }

        public void QueryBasic_OrderBy_WithExpressionDesc_ShouldSelectOrdered()
        {
            try
            {
                IDbConnection con = SetupTables();
                ITransaction transaction = CreateTransaction(con);
                CreateTestData(transaction);
                transaction.Commit();

                transaction = CreateTransaction(con);
                ISelectionQuery selectionQuery = new SelectionQuery()
                    .From(QueryFrom.RawSql("query_basic qb1"))
                    .OrderBy(QueryOrderBy.Field<QueryBasicEntity>(f => f.Name, QueryOrderType.Descend))
                    .OrderBy(QueryOrderBy.Field<QueryBasicEntity>(f => f.IdCol, QueryOrderType.Descend))
                    .Select(QuerySelection.EntityType<QueryBasicEntity>());

                ICollection<object> results = selectionQuery.ToList(transaction);
                Assert.IsTrue(results.Count == 4);
                transaction.Commit();
                con.Close();
            }
            catch (System.Exception e)
            {
                LogManager.GetLogger(typeof (DbGateQueryBasicTest)).Fatal(e.Message, e);
                Assert.Fail(e.Message);
            }
        }
    }
}