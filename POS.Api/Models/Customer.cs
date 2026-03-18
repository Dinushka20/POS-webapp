using POS.Api.Enums;

namespace POS.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int LoyaltyPoints { get; set; }
        public CustomerTier Tier { get; set; } = CustomerTier.Silver;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
