using System;

namespace dbgate.ermanagement.support.persistant.columntest
{
    public interface IColumnTestEntity : IServerDbClass
    {
        int IdCol { get; set; }

        long LongNotNull { get; set; }

        long? LongNull { get; set; }

        bool BooleanNotNull { get; set; }

        bool? BooleanNull { get; set; }

        char CharNotNull { get; set; }

        char? CharNull { get; set; }

        int IntNotNull { get; set; }

        int? IntNull { get; set; }

        DateTime DateNotNull { get; set; }

        DateTime? DateNull { get; set; }

        double DoubleNotNull { get; set; }

        double? DoubleNull { get; set; }

        float FloatNotNull { get; set; }

        float? FloatNull { get; set; }

        DateTime TimestampNotNull { get; set; }

        DateTime? TimestampNull { get; set; }

        string VarcharNotNull { get; set; }

        string VarcharNull { get; set; }
    }
}
