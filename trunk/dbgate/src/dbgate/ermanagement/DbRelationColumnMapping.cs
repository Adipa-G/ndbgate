namespace dbgate.ermanagement
{
    public class DbRelationColumnMapping
    {
        public DbRelationColumnMapping(string fromField, string toField)
        {
            FromField = fromField;
            ToField = toField;
        }

        public string FromField { get; set; }

        public string ToField { get; set; }
    }
}