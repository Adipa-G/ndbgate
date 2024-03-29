using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.ErManagement.Query;
using DbGate.Utility;
using DbGateTestApp.DocGenerate;
using DbGateTestApp.SimpleExample.Entities;
using System.Linq;

namespace DbGateTestApp.SimpleExample
{
    [WikiCodeBlock("simple_example")]
    public class SimpleExample
    {
        private const int Id = 43;

        public SimpleEntity CreateEntity()
        {
            var entity = new SimpleEntity();
            entity.Id = Id;
            entity.Name = "Entity";
            return entity;
        }

        public void Patch(ITransaction tx) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(SimpleEntity));
            tx.DbGate.PatchDataBase(tx, entityTypes, false);
        }

        public void Persist(ITransaction tx,SimpleEntity entity)
        {
            entity.Persist(tx);
        }

        public SimpleEntity RetrieveWithQuery(ITransaction tx)
        {
            var query = new SelectionQuery()
                .From(QueryFrom.EntityType(typeof(SimpleEntity)))
                .Select(QuerySelection.EntityType(typeof(SimpleEntity)));

            var result = query.ToList(tx).FirstOrDefault();
            if (result == null)
                return null;

            var retrieved = (SimpleEntity)result;
            return retrieved;
        }

        public static void DoTest()
        {
            var example = new SimpleExample();
            var tx = ExampleBase.SetupDb();
            example.Patch(tx);

            var entity = example.CreateEntity();
            example.Persist(tx, entity);

            entity = example.RetrieveWithQuery(tx);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Name = "Updated";
            example.Persist(tx, entity);

            entity = example.RetrieveWithQuery(tx);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Status = EntityStatus.Deleted;
            example.Persist(tx, entity);

            entity = example.RetrieveWithQuery(tx);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}
