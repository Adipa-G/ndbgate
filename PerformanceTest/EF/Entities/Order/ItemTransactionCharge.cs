namespace PerformanceTest.EF.Entities.Order
{
    public class ItemTransactionCharge
    {
        public int TransactionId { get; set; }

        public int IndexNo { get; set; }

        public int ChargeIndex { get; set; }

        public string ChargeCode { get; set; }

        public Transaction Transaction { get; set; }

        public ItemTransaction ItemTransaction { get; set; }
    }
}
