using System.ComponentModel.DataAnnotations.Schema;

namespace PerformanceTest.EF.Entities.Product
{
    public class Service : Item
    {
        public double HourlyRate { get; set; }
    }
}
