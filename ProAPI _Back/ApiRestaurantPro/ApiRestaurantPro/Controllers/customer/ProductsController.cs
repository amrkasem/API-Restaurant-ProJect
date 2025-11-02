// Controllers/Customer/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;

namespace ApiRestaurantPro.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ProductsController(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all available products with optional category filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerProductDto>>>> GetProducts(
            [FromQuery] int? categoryId = null)
        {
            try
            {
                var productsQuery = _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => !m.IsDeleted && m.IsAvailable);

                if (categoryId.HasValue && categoryId > 0)
                {
                    productsQuery = productsQuery.Where(m => m.CategoryId == categoryId);
                }

                var products = await productsQuery
                    .OrderBy(m => m.Name)
                    .Select(m => new CustomerProductDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        ImageUrl = m.ImageUrl,
                        CategoryId = m.CategoryId,
                        CategoryName = m.Category.Name,
                        PreparationTime = m.PreparationTime,
                        IsAvailable = m.IsAvailable
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CustomerProductDto>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CustomerProductDto>>
                {
                    Success = false,
                    Message = "Error retrieving products",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get product details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerProductDto>>> GetProductDetails(int id)
        {
            try
            {
                var product = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.Id == id && !m.IsDeleted)
                    .Select(m => new CustomerProductDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        ImageUrl = m.ImageUrl,
                        CategoryId = m.CategoryId,
                        CategoryName = m.Category.Name,
                        PreparationTime = m.PreparationTime,
                        IsAvailable = m.IsAvailable
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new ApiResponse<CustomerProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<CustomerProductDto>
                {
                    Success = true,
                    Message = "Product details retrieved successfully",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerProductDto>
                {
                    Success = false,
                    Message = "Error retrieving product details",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search products by name or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<CustomerProductDto>>>> SearchProducts(
            [FromQuery] string searchTerm,
            [FromQuery] int? categoryId = null)
        {
            try
            {
                var productsQuery = _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => !m.IsDeleted && m.IsAvailable);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    productsQuery = productsQuery.Where(m =>
                        m.Name.Contains(searchTerm) ||
                        m.Description.Contains(searchTerm));
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    productsQuery = productsQuery.Where(m => m.CategoryId == categoryId);
                }

                var products = await productsQuery
                    .OrderBy(m => m.Name)
                    .Select(m => new CustomerProductDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        ImageUrl = m.ImageUrl,
                        CategoryId = m.CategoryId,
                        CategoryName = m.Category.Name,
                        PreparationTime = m.PreparationTime,
                        IsAvailable = m.IsAvailable
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CustomerProductDto>>
                {
                    Success = true,
                    Message = "Search completed successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CustomerProductDto>>
                {
                    Success = false,
                    Message = "Error searching products",
                    Error = ex.Message
                });
            }
        }
    }
}