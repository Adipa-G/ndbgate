using System;
using System.Data;
using dbgate.ermanagement.context;
using dbgate.ermanagement.context.impl;
using dbgate.ermanagement.impl;

namespace dbgate.ermanagement
{
    public class DefaultServerRoDbClass : IServerRoDbClass
    {
        private IEntityContext _context;

        public DefaultServerRoDbClass()
        {
            _context = new EntityContext();
        }

        public void Retrieve(IDataReader reader, IDbConnection con)
        {
            ErLayer.GetSharedInstance().Load(this,reader,con);
        }

        public IEntityContext Context
        {
            get { return _context; }
        }
    }
}