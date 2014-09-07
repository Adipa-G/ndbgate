using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.DocGenerate;
using DbGateTestApp.One2ManyExample.Entities;

namespace DbGateTestApp.One2ManyExample
{
    [WikiCodeBlock("one_2_many_example")]
    public class One2ManyExample
    {
        public const int Id = 43;

        public One2ManyParentEntity CreateEntity()
        {
            One2ManyParentEntity entity = new One2ManyParentEntity();
            entity.Id = Id;
            entity.Name = "Parent";

            One2ManyChildEntityA childEntityA = new One2ManyChildEntityA();
            childEntityA.IndexNo = 0;
            childEntityA.Name = "Child A";
            entity.ChildEntities.Add(childEntityA);

            One2ManyChildEntityB childEntityB = new One2ManyChildEntityB();
            childEntityB.IndexNo = 1;
            childEntityB.Name = "Child B";
            entity.ChildEntities.Add(childEntityB);

            return entity;
        }

        public void Patch(ITransaction tx) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(One2ManyParentEntity));
            entityTypes.Add(typeof(One2ManyChildEntityA));
            entityTypes.Add(typeof(One2ManyChildEntityB));
            tx.DbGate.PatchDataBase(tx,entityTypes,false);
        }

        public void Persist(ITransaction tx, One2ManyParentEntity entity)
        {
            entity.Persist(tx);
        }

        public One2ManyParentEntity Retrieve(ITransaction tx)
        {
            IDbCommand cmd = tx.CreateCommand();
            cmd.CommandText = "select * from parent_entity where id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = Id;

            One2ManyParentEntity entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new One2ManyParentEntity();
                entity.Retrieve(reader, tx);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            One2ManyExample example = new One2ManyExample();
            ITransaction tx = ExampleBase.SetupDb();
            example.Patch(tx);

            One2ManyParentEntity entity = example.CreateEntity();
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity Name = " + entity.Name);
            foreach (One2ManyChildEntity childEntity in entity.ChildEntities)
            {
                Console.WriteLine("Entity Child Name = " + childEntity.Name);
            }

            entity.Name = "Updated Entity A";
            foreach (One2ManyChildEntity childEntity in entity.ChildEntities)
            {
                childEntity.Name += " Updated";
            }
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity Name = " + entity.Name);
            foreach (One2ManyChildEntity childEntity in entity.ChildEntities)
            {
                Console.WriteLine("Entity Child Name = " + childEntity.Name);
            }

            entity.Status = EntityStatus.Deleted;
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}


