using System.ComponentModel.DataAnnotations.Schema;

namespace PerformanceTest.EF.Entities.Product
{
    public abstract class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ItemId { get; set; }

        public string Name { get; set; }
    }
}
