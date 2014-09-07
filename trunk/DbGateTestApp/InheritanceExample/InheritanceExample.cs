using System;
using System.Collections.Generic;
using System.Data;
using DbGate;
using DbGate.Utility;
using DbGateTestApp.DocGenerate;
using DbGateTestApp.InheritanceExample.Entities;

namespace DbGateTestApp.InheritanceExample
{
    [WikiCodeBlock("inheritance_example")]
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

        public void Patch(ITransaction tx) 
        {
            ICollection<Type> entityTypes = new List<Type>();
            entityTypes.Add(typeof(BottomEntity));
            tx.DbGate.PatchDataBase(tx, entityTypes, false);
        }

        public void Persist(ITransaction tx, BottomEntity entity)
        {
            entity.Persist(tx);
        }

        public BottomEntity Retrieve(ITransaction tx)
        {
            IDbCommand cmd = tx.CreateCommand();
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
                entity.Retrieve(reader, tx);
            }
            DbMgtUtility.Close(reader);
            DbMgtUtility.Close(cmd);
            return entity;
        }

        public static void DoTest()
        {
            InheritanceExample example = new InheritanceExample();
            ITransaction tx = ExampleBase.SetupDb();
            example.Patch(tx);

            BottomEntity entity = example.CreateEntity();
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity Super Name = " + entity.SuperName);
            Console.WriteLine("Entity Middle Name = " + entity.MiddleName);
            Console.WriteLine("Entity Sub Name = " + entity.SubName);

            entity.SuperName = "Updated Super";
            entity.MiddleName = "Updated Middle";
            entity.SubName = "Updated Sub";
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity Super Name = " + entity.SuperName);
            Console.WriteLine("Entity Middle Name = " + entity.MiddleName);
            Console.WriteLine("Entity Sub Name = " + entity.SubName);

            entity.Status = EntityStatus.Deleted;
            example.Persist(tx, entity);

            entity = example.Retrieve(tx);
            Console.WriteLine("Entity = " + entity);

            ExampleBase.CloseDb();
        }
    }
}


