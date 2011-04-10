﻿using System.Collections.Generic;

namespace dbgate.utility.support
{
    public class RootEntity : IDbClass
    {
        public DbClassStatus Status { get; set; }
        public LeafEntity LeafEntityNotNull { get; set; }
        public LeafEntity LeafEntityNull { get; set; }
        public List<LeafEntity> LeafEntities { get; set; }

        public RootEntity()
        {
            LeafEntities = new List<LeafEntity>();
            Status = DbClassStatus.Unmodified;
        }
    }
}
