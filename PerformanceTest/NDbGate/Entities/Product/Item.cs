using DbGate;

namespace PerformanceTest.NDbGate.Entities.Product
{
    [TableInfo("dbgate_product_item")]
    public abstract class Item : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int ItemId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}