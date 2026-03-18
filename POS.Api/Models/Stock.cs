namespace POS.Api.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int Quantity { get; set; }
        public int LowStockThreshold { get; set; } = 10;

        public virtual Product Product { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
    }
}
