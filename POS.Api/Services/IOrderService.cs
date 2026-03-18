using POS.Api.DTOs;

namespace POS.Api.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
    }
}
