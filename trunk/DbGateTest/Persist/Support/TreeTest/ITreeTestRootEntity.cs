using System.Collections.Generic;

namespace DbGate.Persist.Support.TreeTest
{
    public interface ITreeTestRootEntity : IEntity
    {
        int IdCol { get; set; }

        string Name { get; set; }

        List<ITreeTestOne2ManyEntity> One2ManyEntities { get; set; }

        ITreeTestOne2OneEntity One2OneEntity { get; set; }
    }
}