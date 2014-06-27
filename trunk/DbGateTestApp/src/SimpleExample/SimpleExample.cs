using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.SimpleExample.Entities;

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

        public SimpleEntity Retrieve(IDbConnection con)
        {
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from simple_entity where id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = Id;

            SimpleEntity entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new SimpleEntity();
                entity.Retrieve(reader, con);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            SimpleExample example = new SimpleExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            SimpleEntity entity = example.CreateEntity();
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Name = "Updated";
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity Name = " + entity.Name);

            entity.Status = EntityStatus.Deleted;
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}
