using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.One2OneExample.Entities;

namespace DbGateTestApp.One2OneExample
{
    public class One2OneExample
    {
        public const int IdA = 43;
        public const int IdB = 44;

        public One2OneParentEntity CreateEntityWithChildA()
        {
            One2OneParentEntity entity = new One2OneParentEntity();
            entity.Id = IdA;
            entity.Name = "Parent A";
            entity.ChildEntity = new One2OneChildEntityA();
            entity.ChildEntity.Name = "Child A";
            return entity;
        }


        public One2OneParentEntity CreateEntityWithChildB()
        {
            One2OneParentEntity entity = new One2OneParentEntity();
            entity.Id = IdB;
            entity.Name = "Parent B";
            entity.ChildEntity = new One2OneChildEntityB();
            entity.ChildEntity.Name = "Child B";
            return entity;
        }

        public void Patch(ITransaction tx) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(One2OneParentEntity));
            entityTypes.Add(typeof(One2OneChildEntityA));
            entityTypes.Add(typeof(One2OneChildEntityB));
            tx.DbGate.PatchDataBase(tx, entityTypes, false);
        }

        public void Persist(ITransaction tx, One2OneParentEntity entity)
        {
            entity.Persist(tx);
        }

        public One2OneParentEntity Retrieve(ITransaction tx,int id)
        {
            IDbCommand cmd = tx.CreateCommand();
            cmd.CommandText = "select * from parent_entity where id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = id;

            One2OneParentEntity entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new One2OneParentEntity();
                entity.Retrieve(reader, tx);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            One2OneExample example = new One2OneExample();
            ITransaction tx = ExampleBase.SetupDb();
            example.Patch(tx);

            One2OneParentEntity entityA = example.CreateEntityWithChildA();
            example.Persist(tx, entityA);
            One2OneParentEntity entityB = example.CreateEntityWithChildB();
            example.Persist(tx, entityB);

            entityA = example.Retrieve(tx, One2OneExample.IdA);
            Console.WriteLine("Entity Name = " + entityA.Name);
            Console.WriteLine("Entity Child Name = " + entityA.ChildEntity.Name);

            entityB = example.Retrieve(tx, One2OneExample.IdB);
            Console.WriteLine("Entity Name = " + entityB.Name);
            Console.WriteLine("Entity Child Name = " + entityB.ChildEntity.Name);

            entityA.Name = "Updated Entity A";
            entityA.ChildEntity.Name = "Updated Child Entity A";
            example.Persist(tx, entityA);

            entityB.Name = "Updated Entity B";
            entityB.ChildEntity.Name = "Updated Child Entity B";
            example.Persist(tx, entityB);

            entityA = example.Retrieve(tx, One2OneExample.IdA);
            Console.WriteLine("Entity Name = " + entityA.Name);
            Console.WriteLine("Entity Child Name = " + entityA.ChildEntity.Name);

            entityB = example.Retrieve(tx, One2OneExample.IdB);
            Console.WriteLine("Entity Name = " + entityB.Name);
            Console.WriteLine("Entity Child Name = " + entityB.ChildEntity.Name);

            entityA.Status = EntityStatus.Deleted;
            example.Persist(tx, entityA);
            entityB.Status = EntityStatus.Deleted;
            example.Persist(tx, entityB);

            entityA = example.Retrieve(tx, One2OneExample.IdA);
            Console.WriteLine("Entity A = " + entityA);
            entityB = example.Retrieve(tx, One2OneExample.IdB);
            Console.WriteLine("Entity B = " + entityB);

            ExampleBase.CloseDb();
        }
    }
}

