namespace DbGate
{
    public interface IColumn : IField
    {
        ColumnType ColumnType { get; set; }

        int Size { get; set; }

        string ColumnName { get; set; }

        bool Key { get; set; }

        //to avoid read column twice to prevent errors in un supported databases
        bool SubClassCommonColumn { get; set; }

        bool Nullable { get; set; }

        bool ReadFromSequence { get; set; }

        ISequenceGenerator SequenceGenerator { get; set; }
    }
}