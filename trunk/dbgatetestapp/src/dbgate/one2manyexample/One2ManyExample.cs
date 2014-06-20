using System;
using System.Collections.Generic;
using System.Data;
using dbgate;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgate.utility;
using dbgatetestapp.dbgate.one2manyexample.entities;

namespace dbgatetestapp.dbgate.one2manyexample
{
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

        public void Patch(IDbConnection con) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(One2ManyParentEntity));
            entityTypes.Add(typeof(One2ManyChildEntityA));
            entityTypes.Add(typeof(One2ManyChildEntityB));
            IDbTransaction transaction = con.BeginTransaction();
            DbGate.GetSharedInstance().PatchDataBase(con,entityTypes,false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con, One2ManyParentEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public One2ManyParentEntity Retrieve(IDbConnection con)
        {
            IDbCommand cmd = con.CreateCommand();
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
                entity.Retrieve(reader, con);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            One2ManyExample example = new One2ManyExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            One2ManyParentEntity entity = example.CreateEntity();
            example.Persist(con, entity);

            entity = example.Retrieve(con);
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
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity Name = " + entity.Name);
            foreach (One2ManyChildEntity childEntity in entity.ChildEntities)
            {
                Console.WriteLine("Entity Child Name = " + childEntity.Name);
            }

            entity.Status = EntityStatus.Deleted;
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}


