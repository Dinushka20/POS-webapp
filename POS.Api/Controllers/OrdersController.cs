using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Api.DTOs;
using POS.Api.Repositories;
using POS.Api.Services;

namespace POS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrdersController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var orders = await _orderRepository.GetAllAsync(branchId, from, to);
            var dtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                BranchId = o.BranchId,
                CustomerName = o.Customer?.FullName,
                CashierName = o.User?.FullName ?? "Unknown",
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                PaymentMode = o.PaymentMode.ToString(),
                PointsEarned = o.PointsEarned,
                PointsRedeemed = o.PointsRedeemed,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();

            var dto = new OrderDto
            {
                Id = order.Id,
                BranchId = order.BranchId,
                CustomerName = order.Customer?.FullName,
                CashierName = order.User?.FullName ?? "Unknown",
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                PaymentMode = order.PaymentMode.ToString(),
                PointsEarned = order.PointsEarned,
                PointsRedeemed = order.PointsRedeemed,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount,
                    LineTotal = (i.UnitPrice * i.Quantity) - i.DiscountAmount
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var order = await _orderService.CreateOrderAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
