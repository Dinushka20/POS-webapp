using POS.Api.Models;

namespace POS.Api.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync(int? branchId, DateTime? from, DateTime? to);
        Task<Order?> GetByIdAsync(int id);
        Task<Order> CreateAsync(Order order);
        Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
    }
}
