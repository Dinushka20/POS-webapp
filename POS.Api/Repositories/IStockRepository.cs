using POS.Api.Enums;
using POS.Api.Models;

namespace POS.Api.Repositories
{
    public interface IStockRepository
    {
        Task<IEnumerable<Stock>> GetByBranchAsync(int branchId);
        Task<Stock?> GetByProductAndBranchAsync(int productId, int branchId);
        Task<IEnumerable<Stock>> GetLowStockAsync(int branchId);
        Task AdjustAsync(int productId, int branchId, int adjustment, string userId, StockAdjustReason reason);
        Task TransferAsync(int productId, int fromBranchId, int toBranchId, int quantity, string userId);
    }
}
