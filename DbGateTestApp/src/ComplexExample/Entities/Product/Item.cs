using DbGate;
using DbGateTestApp.DocGenerate;

namespace DbGateTestApp.ComplexExample.Entities.Product
{
    [WikiCodeBlock("complex_example_product_item")]
    [TableInfo("product_item")]
    public abstract class Item : DefaultEntity
    {
        [ColumnInfo(ColumnType.Integer, Key = true, SubClassCommonColumn = true)]
        public int ItemId { get; set; }

        [ColumnInfo(ColumnType.Varchar)]
        public string Name { get; set; }
    }
}