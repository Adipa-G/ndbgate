using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.InheritanceExample.Entities;

namespace DbGateTestApp.InheritanceExample
{
    public class InheritanceExample
    {
        private const int Id = 43;

        public BottomEntity CreateEntity()
        {
            BottomEntity entity = new BottomEntity();
            entity.Id = Id;
            entity.SuperName = "Super";
            entity.MiddleName = "Middle";
            entity.SubName = "Sub";
            return entity;
        }

        public void Patch(IDbConnection con) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(BottomEntity));
            IDbTransaction transaction = con.BeginTransaction();
            DbGate._transactionFactory.DbGate.PatchDataBase(con, entityTypes, false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con, BottomEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public BottomEntity Retrieve(IDbConnection con)
        {
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from bottom_entity where id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = Id;

            BottomEntity entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new BottomEntity();
                entity.Retrieve(reader, con);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            InheritanceExample example = new InheritanceExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            BottomEntity entity = example.CreateEntity();
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity Super Name = " + entity.SuperName);
            Console.WriteLine("Entity Middle Name = " + entity.MiddleName);
            Console.WriteLine("Entity Sub Name = " + entity.SubName);

            entity.SuperName = "Updated Super";
            entity.MiddleName = "Updated Middle";
            entity.SubName = "Updated Sub";
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity Super Name = " + entity.SuperName);
            Console.WriteLine("Entity Middle Name = " + entity.MiddleName);
            Console.WriteLine("Entity Sub Name = " + entity.SubName);

            entity.Status = EntityStatus.Deleted;
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}


