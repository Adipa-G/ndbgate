using System;
using System.Collections.Generic;
using System.Data;
using dbgate;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgatetestapp.dbgate.one2oneexample.entities;

namespace dbgatetestapp.dbgate.one2oneexample
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

        public void Patch(IDbConnection con) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(One2OneParentEntity));
            entityTypes.Add(typeof(One2OneChildEntityA));
            entityTypes.Add(typeof(One2OneChildEntityB));
            IDbTransaction transaction = con.BeginTransaction();
            ErLayer.GetSharedInstance().PatchDataBase(con,entityTypes,false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con, One2OneParentEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public One2OneParentEntity Retrieve(IDbConnection con,int id)
        {
            IDbCommand cmd = con.CreateCommand();
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
                entity.Retrieve(reader, con);
            }
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            One2OneExample example = new One2OneExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            One2OneParentEntity entityA = example.CreateEntityWithChildA();
            example.Persist(con, entityA);
            One2OneParentEntity entityB = example.CreateEntityWithChildB();
            example.Persist(con, entityB);

            entityA = example.Retrieve(con, One2OneExample.IdA);
            Console.WriteLine("Entity Name = " + entityA.Name);
            Console.WriteLine("Entity Child Name = " + entityA.ChildEntity.Name);

            entityB = example.Retrieve(con, One2OneExample.IdB);
            Console.WriteLine("Entity Name = " + entityB.Name);
            Console.WriteLine("Entity Child Name = " + entityB.ChildEntity.Name);

            entityA.Name = "Updated Entity A";
            entityA.ChildEntity.Name = "Updated Child Entity A";
            example.Persist(con, entityA);

            entityB.Name = "Updated Entity B";
            entityB.ChildEntity.Name = "Updated Child Entity B";
            example.Persist(con, entityB);

            entityA = example.Retrieve(con, One2OneExample.IdA);
            Console.WriteLine("Entity Name = " + entityA.Name);
            Console.WriteLine("Entity Child Name = " + entityA.ChildEntity.Name);

            entityB = example.Retrieve(con, One2OneExample.IdB);
            Console.WriteLine("Entity Name = " + entityB.Name);
            Console.WriteLine("Entity Child Name = " + entityB.ChildEntity.Name);

            entityA.Status = EntityStatus.Deleted;
            example.Persist(con, entityA);
            entityB.Status = EntityStatus.Deleted;
            example.Persist(con, entityB);

            entityA = example.Retrieve(con, One2OneExample.IdA);
            Console.WriteLine("Entity A = " + entityA);
            entityB = example.Retrieve(con, One2OneExample.IdB);
            Console.WriteLine("Entity B = " + entityB);

            ExampleBase.CloseDb();
        }
    }
}

