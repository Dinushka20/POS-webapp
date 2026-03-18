using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POS.Api.Data;
using POS.Api.DTOs;
using POS.Api.Enums;
using POS.Api.Hubs;
using POS.Api.Models;
using POS.Api.Repositories;

namespace POS.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IStockRepository _stockRepository;
        private readonly IHubContext<SalesHub> _hubContext;

        public OrderService(AppDbContext context, IStockRepository stockRepository, IHubContext<SalesHub> hubContext)
        {
            _context = context;
            _stockRepository = stockRepository;
            _hubContext = hubContext;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(userId) 
                    ?? throw new UnauthorizedAccessException("User not found.");

                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();
                var productNames = new Dictionary<int, string>();

                // 1. Validate every item & Calculate Subtotal
                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId) 
                        ?? throw new InvalidOperationException($"Product not found: {item.ProductId}");

                    if (!product.IsActive)
                        throw new InvalidOperationException($"Product is inactive: {product.Name}");

                    var stock = await _stockRepository.GetByProductAndBranchAsync(item.ProductId, dto.BranchId);
                    if (stock == null || stock.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                    }

                    productNames[product.Id] = product.Name;
                    var lineTotal = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                    totalAmount += lineTotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount
                    });
                }

                int pointsRedeemed = 0;
                decimal discountAmount = 0;

                Customer? customer = null;
                // 3. Redeem Points
                if (dto.CustomerId.HasValue)
                {
                    customer = await _context.Customers.FindAsync(dto.CustomerId.Value);
                    if (customer == null) throw new InvalidOperationException("Customer not found.");

                    if (dto.RedeemPoints && customer.LoyaltyPoints >= 100)
                    {
                        pointsRedeemed = (customer.LoyaltyPoints / 100) * 100; // Multiples of 100
                        discountAmount = (pointsRedeemed / 100) * 10; // 100 points = LKR 10
                        totalAmount -= discountAmount;
                        customer.LoyaltyPoints -= pointsRedeemed;
                    }
                }

                // 4. Create Order entity and OrderItems
                var paymentModeEnum = Enum.Parse<PaymentMode>(dto.PaymentMode);
                int pointsEarned = 0;

                if (customer != null)
                {
                    pointsEarned = (int)(totalAmount / 100);
                    customer.LoyaltyPoints += pointsEarned;

                    // Update Tier
                    if (customer.LoyaltyPoints >= 5000) customer.Tier = CustomerTier.Platinum;
                    else if (customer.LoyaltyPoints >= 1000) customer.Tier = CustomerTier.Gold;
                    else customer.Tier = CustomerTier.Silver;

                    _context.Customers.Update(customer);
                }

                var order = new Order
                {
                    UserId = userId,
                    CustomerId = dto.CustomerId,
                    BranchId = dto.BranchId,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    PaymentMode = paymentModeEnum,
                    CashAmount = dto.CashAmount,
                    CardAmount = dto.CardAmount,
                    PointsEarned = pointsEarned,
                    PointsRedeemed = pointsRedeemed,
                    Status = OrderStatus.Completed,
                    Items = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 5. Deduct stock for each item via IStockRepository.AdjustAsync
                foreach (var item in orderItems)
                {
                    await _stockRepository.AdjustAsync(
                        item.ProductId, 
                        dto.BranchId, 
                        -item.Quantity, 
                        userId, 
                        StockAdjustReason.Adjustment
                    );

                    // Optional: Broadcast LowStock event if threshold reached
                    var stock = await _stockRepository.GetByProductAndBranchAsync(item.ProductId, dto.BranchId);
                    if (stock != null && stock.Quantity <= stock.LowStockThreshold)
                    {
                        await _hubContext.Clients.Group($"branch-{dto.BranchId}")
                            .SendAsync("LowStock", item.ProductId, productNames[item.ProductId], stock.Quantity, dto.BranchId);
                    }
                }

                await transaction.CommitAsync();

                // 7. Broadcast "NewSale" and "NewOrder" events
                var todayOrdersCount = await _context.Orders
                    .Where(o => o.BranchId == dto.BranchId && o.CreatedAt.Date == DateTime.UtcNow.Date)
                    .CountAsync();

                await _hubContext.Clients.Group($"branch-{dto.BranchId}")
                    .SendAsync("NewSale", order.Id, order.TotalAmount, user.FullName);
                
                await _hubContext.Clients.Group($"branch-{dto.BranchId}")
                    .SendAsync("NewOrder", todayOrdersCount);

                // 8. Return mapped OrderDto
                return new OrderDto
                {
                    Id = order.Id,
                    BranchId = order.BranchId,
                    CustomerName = customer?.FullName,
                    CashierName = user.FullName,
                    TotalAmount = order.TotalAmount,
                    DiscountAmount = order.DiscountAmount,
                    PaymentMode = order.PaymentMode.ToString(),
                    PointsEarned = order.PointsEarned,
                    PointsRedeemed = order.PointsRedeemed,
                    Status = order.Status.ToString(),
                    CreatedAt = order.CreatedAt,
                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = productNames[i.ProductId],
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        LineTotal = (i.UnitPrice * i.Quantity) - i.DiscountAmount
                    }).ToList()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
