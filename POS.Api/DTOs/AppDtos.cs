using System.ComponentModel.DataAnnotations;

namespace POS.Api.DTOs
{
    // ==================== AUTH DTOs ====================

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int BranchId { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int BranchId { get; set; }
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // ==================== PRODUCT DTOs ====================

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999)]
        public decimal Price { get; set; }

        [Required]
        public decimal CostPrice { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Barcode { get; set; }

        [Range(0.01, 999999)]
        public decimal? Price { get; set; }

        public decimal? CostPrice { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    // ==================== STOCK DTOs ====================

    public class StockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public int Quantity { get; set; }
        public int LowStockThreshold { get; set; }
        public string Status { get; set; } = string.Empty; // InStock, Low, OutOfStock
    }

    public class AdjustStockDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        [Range(-10000, 10000)]
        public int Adjustment { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    public class TransferStockDto
    {
        [Required]
        public int FromBranchId { get; set; }

        [Required]
        public int ToBranchId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    // ==================== CUSTOMER DTOs ====================

    public class CustomerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int LoyaltyPoints { get; set; }
        public string Tier { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }
    }

    public class UpdateCustomerDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    // ==================== ORDER DTOs ====================

    public class CreateOrderDto
    {
        [Required]
        public int BranchId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        public string PaymentMode { get; set; } = string.Empty;

        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public bool RedeemPoints { get; set; }

        [Required]
        [MinLength(1)]
        public List<OrderItemInputDto> Items { get; set; } = new();
    }

    public class OrderItemInputDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? CustomerName { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ==================== REPORT DTOs ====================

    public class SummaryReportDto
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class DailyReportDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQtySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class HourlyReportDto
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
