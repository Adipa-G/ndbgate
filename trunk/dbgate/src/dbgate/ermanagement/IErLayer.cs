﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using dbgate.ermanagement.query;

namespace dbgate.ermanagement
{
    public interface IErLayer
    {
        void Load(IServerRoDbClass serverRoDbClass, IDataReader reader, IDbConnection con);

        void Save(IServerDbClass serverDbClass, IDbConnection con);

        ICollection<Object> Select(ISelectionQuery query,IDbConnection con );

        void PatchDataBase(IDbConnection con, ICollection<IServerDbClass> dbClasses, bool dropAll);

        void ClearCache();

        void RegisterTable(Type type, string tableName);

        void RegisterFields(Type type, ICollection<IField> fields);

        IErLayerConfig Config { get; }

        IErLayerStatistics Statistics { get; }
    }
}