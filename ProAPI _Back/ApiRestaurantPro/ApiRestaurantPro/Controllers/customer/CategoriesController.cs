// Controllers/Customer/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;

namespace ApiRestaurantPro.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public CategoriesController(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerCategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new CustomerCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        ProductsCount = c.MenuItems.Count(m => !m.IsDeleted && m.IsAvailable)
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CustomerCategoryDto>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CustomerCategoryDto>>
                {
                    Success = false,
                    Message = "Error retrieving categories",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get category with its products
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerCategoryWithProductsDto>>> GetCategoryWithProducts(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.MenuItems)
                    .Where(c => c.Id == id && !c.IsDeleted && c.IsActive)
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound(new ApiResponse<CustomerCategoryWithProductsDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                var categoryDto = new CustomerCategoryWithProductsDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Products = category.MenuItems
                        .Where(m => !m.IsDeleted && m.IsAvailable)
                        .Select(m => new CustomerProductDto
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Description = m.Description,
                            Price = m.Price,
                            ImageUrl = m.ImageUrl,
                            CategoryId = m.CategoryId,
                            CategoryName = category.Name,
                            PreparationTime = m.PreparationTime,
                            IsAvailable = m.IsAvailable
                        })
                        .ToList()
                };

                return Ok(new ApiResponse<CustomerCategoryWithProductsDto>
                {
                    Success = true,
                    Message = "Category retrieved successfully",
                    Data = categoryDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerCategoryWithProductsDto>
                {
                    Success = false,
                    Message = "Error retrieving category",
                    Error = ex.Message
                });
            }
        }
    }
}