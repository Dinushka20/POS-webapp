using Microsoft.EntityFrameworkCore;
using POS.Api.Data;
using POS.Api.Enums;
using POS.Api.Models;

namespace POS.Api.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _context;

        public StockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stock>> GetByBranchAsync(int branchId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Where(s => s.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<Stock?> GetByProductAndBranchAsync(int productId, int branchId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);
        }

        public async Task<IEnumerable<Stock>> GetLowStockAsync(int branchId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Branch)
                .Where(s => s.BranchId == branchId && s.Quantity <= s.LowStockThreshold)
                .ToListAsync();
        }

        public async Task AdjustAsync(int productId, int branchId, int adjustment, string userId, StockAdjustReason reason)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = productId,
                    BranchId = branchId,
                    Quantity = adjustment,
                    LowStockThreshold = 10
                };
                _context.Stocks.Add(stock);
            }
            else
            {
                stock.Quantity += adjustment;
            }

            // Create audit log
            var auditLog = new StockAuditLog
            {
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Adjustment = adjustment,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };
            _context.StockAuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
        }

        public async Task TransferAsync(int productId, int fromBranchId, int toBranchId, int quantity, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var fromStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == fromBranchId);

                if (fromStock == null || fromStock.Quantity < quantity)
                {
                    throw new InvalidOperationException("Insufficient stock for transfer.");
                }

                fromStock.Quantity -= quantity;

                var toStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == toBranchId);

                if (toStock == null)
                {
                    toStock = new Stock
                    {
                        ProductId = productId,
                        BranchId = toBranchId,
                        Quantity = quantity,
                        LowStockThreshold = 10
                    };
                    _context.Stocks.Add(toStock);
                }
                else
                {
                    toStock.Quantity += quantity;
                }

                // Audit logs for both branches
                _context.StockAuditLogs.Add(new StockAuditLog
                {
                    ProductId = productId,
                    BranchId = fromBranchId,
                    UserId = userId,
                    Adjustment = -quantity,
                    Reason = StockAdjustReason.Adjustment,
                    Timestamp = DateTime.UtcNow
                });

                _context.StockAuditLogs.Add(new StockAuditLog
                {
                    ProductId = productId,
                    BranchId = toBranchId,
                    UserId = userId,
                    Adjustment = quantity,
                    Reason = StockAdjustReason.Adjustment,
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
