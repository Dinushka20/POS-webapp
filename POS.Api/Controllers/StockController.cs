using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Api.Data;
using POS.Api.DTOs;
using POS.Api.Enums;
using POS.Api.Repositories;
using POS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace POS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository _stockRepository;
        private readonly AppDbContext _context;

        public StockController(IStockRepository stockRepository, AppDbContext context)
        {
            _stockRepository = stockRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetByBranch([FromQuery] int branchId)
        {
            var stocks = await _stockRepository.GetByBranchAsync(branchId);
            var dtos = stocks.Select(s => new StockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                BranchId = s.BranchId,
                Quantity = s.Quantity,
                LowStockThreshold = s.LowStockThreshold,
                Status = s.Quantity <= 0 ? "OutOfStock" : s.Quantity <= s.LowStockThreshold ? "Low" : "InStock"
            });
            return Ok(dtos);
        }

        [HttpGet("low")]
        public async Task<IActionResult> GetLowStock([FromQuery] int branchId)
        {
            var stocks = await _stockRepository.GetLowStockAsync(branchId);
            var dtos = stocks.Select(s => new StockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                BranchId = s.BranchId,
                Quantity = s.Quantity,
                LowStockThreshold = s.LowStockThreshold,
                Status = s.Quantity <= 0 ? "OutOfStock" : "Low"
            });
            return Ok(dtos);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("audit/{productId}")]
        public async Task<IActionResult> GetAuditLog(int productId)
        {
            var logs = await _context.StockAuditLogs
                .Include(l => l.Product)
                .Include(l => l.Branch)
                .Where(l => l.ProductId == productId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return Ok(logs.Select(l => new 
            {
                l.Id,
                l.ProductId,
                ProductName = l.Product.Name,
                l.BranchId,
                BranchName = l.Branch.Name,
                l.UserId,
                l.Adjustment,
                Reason = l.Reason.ToString(),
                l.Timestamp
            }));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{productId}/adjust")]
        public async Task<IActionResult> AdjustStock(int productId, [FromBody] AdjustStockDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
            var reasonEnum = Enum.Parse<StockAdjustReason>(dto.Reason);

            await _stockRepository.AdjustAsync(productId, dto.BranchId, dto.Adjustment, userId, reasonEnum);
            return NoContent();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("transfer")]
        public async Task<IActionResult> TransferStock([FromBody] TransferStockDto dto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
                await _stockRepository.TransferAsync(dto.ProductId, dto.FromBranchId, dto.ToBranchId, dto.Quantity, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
