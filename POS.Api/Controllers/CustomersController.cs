using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Api.DTOs;
using POS.Api.Enums;
using POS.Api.Models;
using POS.Api.Repositories;

namespace POS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;

        public CustomersController(ICustomerRepository customerRepository, IOrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? phone)
        {
            IEnumerable<Customer> customers;

            if (!string.IsNullOrEmpty(phone))
            {
                var customer = await _customerRepository.GetByPhoneAsync(phone);
                customers = customer != null ? new List<Customer> { customer } : new List<Customer>();
            }
            else
            {
                customers = await _customerRepository.GetAllAsync();
            }

            var dtos = customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                LoyaltyPoints = c.LoyaltyPoints,
                Tier = c.Tier.ToString(),
                CreatedAt = c.CreatedAt
            });

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin,Manager,Cashier")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            var dto = new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                LoyaltyPoints = customer.LoyaltyPoints,
                Tier = customer.Tier.ToString(),
                CreatedAt = customer.CreatedAt
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetOrderHistory(int id)
        {
            var orders = await _orderRepository.GetByCustomerAsync(id);
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var existing = await _customerRepository.GetByPhoneAsync(dto.Phone);
            if (existing != null)
                return BadRequest(new { Message = "Customer with this phone number already exists." });

            var customer = new Customer
            {
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                LoyaltyPoints = 0,
                Tier = CustomerTier.Silver
            };

            await _customerRepository.CreateAsync(customer);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FullName)) customer.FullName = dto.FullName;
            
            if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != customer.Phone)
            {
                var existing = await _customerRepository.GetByPhoneAsync(dto.Phone);
                if (existing != null) return BadRequest(new { Message = "Phone number already in use." });
                customer.Phone = dto.Phone;
            }

            if (dto.Email != null) customer.Email = dto.Email;

            await _customerRepository.UpdateAsync(customer);
            return NoContent();
        }
    }
}
