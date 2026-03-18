using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Api.Repositories;
using POS.Api.Services;

namespace POS.Api.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IPdfService _pdfService;
        private readonly IOrderRepository _orderRepository;
        private readonly Data.AppDbContext _context;

        public ReportsController(IReportService reportService, IPdfService pdfService, IOrderRepository orderRepository, Data.AppDbContext context)
        {
            _reportService = reportService;
            _pdfService = pdfService;
            _orderRepository = orderRepository;
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int branchId, [FromQuery] DateTime date)
        {
            var report = await _reportService.GetSummaryAsync(branchId, date);
            return Ok(report);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily([FromQuery] int branchId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var report = await _reportService.GetDailyAsync(branchId, from, to);
            return Ok(report);
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int branchId, [FromQuery] int limit = 10)
        {
            var report = await _reportService.GetTopProductsAsync(branchId, limit);
            return Ok(report);
        }

        [HttpGet("hourly")]
        public async Task<IActionResult> GetHourly([FromQuery] int branchId, [FromQuery] DateTime date)
        {
            var report = await _reportService.GetHourlyAsync(branchId, date);
            return Ok(report);
        }

        [HttpGet("daily-pdf")]
        public async Task<IActionResult> GetDailyPdf([FromQuery] int branchId, [FromQuery] DateTime date)
        {
            try
            {
                var bytes = await _reportService.GenerateDailyPdfAsync(branchId, date);
                return File(bytes, "application/pdf", $"report_{date:yyyyMMdd}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("receipt/{orderId}")]
        public async Task<IActionResult> GetReceipt(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            var branch = await _context.Branches.FindAsync(order.BranchId);
            if (branch == null) return NotFound();

            var bytes = _pdfService.GenerateReceipt(order, branch.Name);
            return File(bytes, "application/pdf", $"receipt_{order.Id}.pdf");
        }
    }
}
