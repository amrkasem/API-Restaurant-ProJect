// Controllers/Admin/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(MyDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDto>>>> GetProducts()
        {
            try
            {
                var products = await _context.MenuItems
                    .Include(m => m.Category)
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new ProductResponseDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        IsAvailable = m.IsAvailable,
                        PreparationTime = m.PreparationTime,
                        ImageUrl = m.ImageUrl,
                        CategoryId = m.CategoryId,
                        CategoryName = m.Category.Name,
                        CreatedAt = m.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ProductResponseDto>>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ProductResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving products",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                var productDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    PreparationTime = product.PreparationTime,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    CreatedAt = product.CreatedAt
                };

                return Ok(new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Product retrieved successfully",
                    Data = productDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving product",
                    Error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> CreateProduct([FromForm] ProductCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                // Check if category exists
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

                if (category == null)
                {
                    return BadRequest(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Selected category does not exist"
                    });
                }

                // Activate category if inactive
                if (!category.IsActive)
                {
                    category.IsActive = true;
                    category.UpdatedAt = DateTime.UtcNow;
                    category.UpdatedBy = "Admin";
                    _context.Categories.Update(category);
                }

                string imageUrl = await SaveImage(dto.ImageFile, "products");

                var product = new MenuItem
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description,
                    Price = dto.Price,
                    CategoryId = dto.CategoryId,
                    PreparationTime = dto.PreparationTime,
                    IsAvailable = dto.IsAvailable,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.MenuItems.Add(product);
                await _context.SaveChangesAsync();

                var productResponse = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    PreparationTime = product.PreparationTime,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = category.Name,
                    CreatedAt = product.CreatedAt
                };

                return Ok(new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = productResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponseDto>
                {
                    Success = false,
                    Message = "Error creating product",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> UpdateProduct(int id, [FromForm] ProductUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var product = await _context.MenuItems.FindAsync(id);
                if (product == null || product.IsDeleted)
                {
                    return NotFound(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Check if category exists
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

                if (category == null)
                {
                    return BadRequest(new ApiResponse<ProductResponseDto>
                    {
                        Success = false,
                        Message = "Selected category does not exist"
                    });
                }

                product.Name = dto.Name.Trim();
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.CategoryId = dto.CategoryId;
                product.PreparationTime = dto.PreparationTime;
                product.IsAvailable = dto.IsAvailable;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = "Admin";

                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    DeleteOldImage(product.ImageUrl, "products");
                    product.ImageUrl = await SaveImage(dto.ImageFile, "products");
                }

                _context.MenuItems.Update(product);
                await _context.SaveChangesAsync();

                var productResponse = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    PreparationTime = product.PreparationTime,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = category.Name,
                    CreatedAt = product.CreatedAt
                };

                return Ok(new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Product updated successfully",
                    Data = productResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductResponseDto>
                {
                    Success = false,
                    Message = "Error updating product",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                // Check if this is the last product in the category
                var category = product.Category;
                if (category != null)
                {
                    var productCount = await _context.MenuItems
                        .CountAsync(m => m.CategoryId == category.Id && !m.IsDeleted);

                    if (productCount == 1) // This is the last product
                    {
                        category.IsActive = false;
                        category.UpdatedAt = DateTime.UtcNow;
                        category.UpdatedBy = "Admin";
                        _context.Categories.Update(category);
                    }
                }

                // Soft delete product
                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = "Admin";

                _context.MenuItems.Update(product);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting product",
                    Error = ex.Message
                });
            }
        }

        private async Task<string> SaveImage(IFormFile? imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
                return $"/images/{folderName}/default.jpg";

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folderName}/{uniqueFileName}";
        }

        private void DeleteOldImage(string? imageUrl, string folderName)
        {
            if (!string.IsNullOrEmpty(imageUrl) && !imageUrl.Contains("default"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
        }
    }
}