using System.Collections.Generic;

namespace dbgate.ermanagement.support.persistant.treetest
{
    public interface ITreeTestRootEntity  : IServerDbClass
    {
        int IdCol { get; set; }

        string Name { get; set; }

        List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }

        ITreeTestOne2OneEntity One2OneEntity { get; set; }
    }
}
