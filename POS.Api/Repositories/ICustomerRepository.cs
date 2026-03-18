using POS.Api.Models;

namespace POS.Api.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
    }
}
