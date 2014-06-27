namespace DbGate
{
    public class RelationColumnMapping
    {
        public RelationColumnMapping(string fromField, string toField)
        {
            FromField = fromField;
            ToField = toField;
        }

        public string FromField { get; set; }

        public string ToField { get; set; }
    }
}