using System;
using System.Collections.Generic;
using System.Data;
using dbgate;
using dbgate.dbutility;
using dbgate.ermanagement.impl;
using dbgatetestapp.dbgate.inheritanceexample.entities;
using dbgatetestapp.dbgate.simpleexample.entities;

namespace dbgatetestapp.dbgate.inheritanceexample
{
    public class InheritanceExample
    {
        private const int Id = 43;

        public SubEntity CreateEntity()
        {
            SubEntity entity = new SubEntity();
            entity.Id = Id;
            entity.SuperName = "Super";
            entity.MiddleName = "Middle";
            entity.SubName = "Sub";
            return entity;
        }

        public void Patch(IDbConnection con) 
        {
            ICollection<IServerDbClass> entities = new List<IServerDbClass>();
            entities.Add(CreateEntity());
            IDbTransaction transaction = con.BeginTransaction();
            ErLayer.GetSharedInstance().PatchDataBase(con,entities,false);
            transaction.Commit();
        }

        public void Persist(IDbConnection con, SubEntity entity)
        {
            IDbTransaction transaction = con.BeginTransaction();
            entity.Persist(con);
            transaction.Commit();
        }

        public SubEntity Retrieve(IDbConnection con)
        {
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from sub_entity where id = ?";

            IDbDataParameter parameter = cmd.CreateParameter();
            cmd.Parameters.Add(parameter);
            parameter.DbType = DbType.Int32;
            parameter.Value = Id;

            SubEntity entity = null;
            IDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                entity = new SubEntity();
                entity.Retrieve(reader, con);
            }
            DbMgmtUtility.Close(reader);
            DbMgmtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            InheritanceExample example = new InheritanceExample();
            IDbConnection con = ExampleBase.SetupDb();
            example.Patch(con);

            SubEntity entity = example.CreateEntity();
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

            entity.Status = DbClassStatus.Deleted;
            example.Persist(con, entity);

            entity = example.Retrieve(con);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}


