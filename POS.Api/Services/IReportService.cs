using POS.Api.DTOs;

namespace POS.Api.Services
{
    public interface IReportService
    {
        Task<SummaryReportDto> GetSummaryAsync(int branchId, DateTime date);
        Task<IEnumerable<DailyReportDto>> GetDailyAsync(int branchId, DateTime from, DateTime to);
        Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int branchId, int limit);
        Task<IEnumerable<HourlyReportDto>> GetHourlyAsync(int branchId, DateTime date);
        Task<byte[]> GenerateDailyPdfAsync(int branchId, DateTime date);
    }
}
