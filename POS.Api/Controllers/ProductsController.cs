using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Api.DTOs;
using POS.Api.Models;
using POS.Api.Repositories;

namespace POS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? barcode)
        {
            var products = await _productRepository.GetAllAsync(search, barcode);
            var dtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                CostPrice = p.CostPrice,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Price = product.Price,
                CostPrice = product.CostPrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                IsActive = product.IsActive
            };
            return Ok(dto);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var existing = await _productRepository.GetByBarcodeAsync(dto.Barcode);
            if (existing != null)
                return BadRequest(new { Message = "Product with this barcode already exists." });

            var product = new Product
            {
                Name = dto.Name,
                Barcode = dto.Barcode,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                CategoryId = dto.CategoryId,
                IsActive = true
            };

            await _productRepository.CreateAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
            
            if (!string.IsNullOrEmpty(dto.Barcode) && dto.Barcode != product.Barcode)
            {
                var existing = await _productRepository.GetByBarcodeAsync(dto.Barcode);
                if (existing != null) return BadRequest(new { Message = "Barcode already in use." });
                product.Barcode = dto.Barcode;
            }

            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.CostPrice.HasValue) product.CostPrice = dto.CostPrice.Value;
            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

            await _productRepository.UpdateAsync(product);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            await _productRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
