using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.ErManagement.Query;
using DbGate.Utility;
using DbGateTestApp.SimpleExample.Entities;
using System.Linq;

namespace DbGateTestApp.SimpleExample
{
    public class SimpleExample
    {
        private const int Id = 43;

        public SimpleEntity CreateEntity()
        {
            SimpleEntity entity = new SimpleEntity();
            entity.Id = Id;
            entity.Name = "Entity";
            return entity;
        }

        public void Patch(IDbConnection con) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(SimpleEntity));
            IDbTransaction transaction = con.BeginTransaction();
            DbGate.ErManagement.ErMapper.DbGate.GetSharedInstance().PatchDataBase(con, entityTypes, false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con,SimpleEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public SimpleEntity RetrieveWithQuery(IDbConnection con)
        {
            ISelectionQuery query = new SelectionQuery()
                .From(QueryFrom.EntityType(typeof(SimpleEntity)))
                .Select(QuerySelection.EntityType(typeof(SimpleEntity)));

            var result = query.ToList(con).FirstOrDefault();
            if (result == null)
                return null;

            var retrieved = (SimpleEntity)((object[])result)[0];
            return retrieved;
        }

        public static void DoTest()
        {
            SimpleExample example = new SimpleExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            SimpleEntity entity = example.CreateEntity();
            example.Persist(con, entity);

            entity = example.RetrieveWithQuery(con);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Name = "Updated";
            example.Persist(con, entity);

            entity = example.RetrieveWithQuery(con);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Status = EntityStatus.Deleted;
            example.Persist(con, entity);

            entity = example.RetrieveWithQuery(con);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}
