using POS.Api.Enums;

namespace POS.Api.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int BranchId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Completed;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
