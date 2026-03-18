using POS.Api.Enums;

namespace POS.Api.Models
{
    public class StockAuditLog
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Adjustment { get; set; }
        public StockAdjustReason Reason { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public virtual Product Product { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
    }
}
