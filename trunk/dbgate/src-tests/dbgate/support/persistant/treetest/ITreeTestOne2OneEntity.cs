﻿namespace dbgate.support.persistant.treetest
{
    public interface ITreeTestOne2OneEntity : IEntity
    {
        int IdCol { get; set; }

        string Name { get; set; }
    }
}