using POS.Api.Models;

namespace POS.Api.Services
{
    public interface IPdfService
    {
        byte[] GenerateDailySalesReport(List<Order> orders, string branchName, DateTime date);
        byte[] GenerateReceipt(Order order, string branchName);
    }
}
