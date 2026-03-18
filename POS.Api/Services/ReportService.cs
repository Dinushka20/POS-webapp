using Microsoft.EntityFrameworkCore;
using POS.Api.Data;
using POS.Api.DTOs;
using POS.Api.Models;
using POS.Api.Repositories;

namespace POS.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context;
        private readonly IPdfService _pdfService;

        public ReportService(IOrderRepository orderRepository, AppDbContext context, IPdfService pdfService)
        {
            _orderRepository = orderRepository;
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<SummaryReportDto> GetSummaryAsync(int branchId, DateTime date)
        {
            var dateStart = date.Date;
            var dateEnd = dateStart.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.BranchId == branchId && o.CreatedAt >= dateStart && o.CreatedAt < dateEnd)
                .ToListAsync();

            var newCustomers = await _context.Customers
                .Where(c => c.CreatedAt >= dateStart && c.CreatedAt < dateEnd)
                .CountAsync();

            int totalOrders = orders.Count;
            decimal totalSales = orders.Sum(o => o.TotalAmount);
            decimal averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

            return new SummaryReportDto
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalCustomers = newCustomers,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<IEnumerable<DailyReportDto>> GetDailyAsync(int branchId, DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1);

            return await _context.Orders
                .Where(o => o.BranchId == branchId && o.CreatedAt >= fromDate && o.CreatedAt < toDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailyReportDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int branchId, int limit)
        {
            // Calculate top products across ALL time. Or specific? The requirement just says GetTopProductsAsync.
            // Let's do it across all time for the given branch.
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.BranchId == branchId)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQtySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => (oi.UnitPrice * oi.Quantity) - oi.DiscountAmount)
                })
                .OrderByDescending(t => t.TotalQtySold)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<HourlyReportDto>> GetHourlyAsync(int branchId, DateTime date)
        {
            var dateStart = date.Date;
            var dateEnd = dateStart.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.BranchId == branchId && o.CreatedAt >= dateStart && o.CreatedAt < dateEnd)
                .ToListAsync();

            var hourlyStats = Enumerable.Range(0, 24).Select(hour => new HourlyReportDto
            {
                Hour = hour,
                OrderCount = orders.Count(o => o.CreatedAt.Hour == hour),
                Revenue = orders.Where(o => o.CreatedAt.Hour == hour).Sum(o => o.TotalAmount)
            }).ToList();

            return hourlyStats;
        }

        public async Task<byte[]> GenerateDailyPdfAsync(int branchId, DateTime date)
        {
            var branch = await _context.Branches.FindAsync(branchId) 
                ?? throw new InvalidOperationException("Branch not found.");

            var dateStart = date.Date;
            var dateEnd = dateStart.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.BranchId == branchId && o.CreatedAt >= dateStart && o.CreatedAt < dateEnd)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            return _pdfService.GenerateDailySalesReport(orders, branch.Name, date);
        }
    }
}
