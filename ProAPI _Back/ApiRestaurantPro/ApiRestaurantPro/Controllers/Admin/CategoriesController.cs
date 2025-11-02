// Controllers/Admin/CategoriesController.cs
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
    public class CategoriesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(MyDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.MenuItems)
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CategoryResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        IsActive = c.IsActive,
                        MenuItemsCount = c.MenuItems.Count(m => !m.IsDeleted),
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CategoryResponseDto>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CategoryResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving categories",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.MenuItems)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                var categoryDto = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    MenuItemsCount = category.MenuItems.Count(m => !m.IsDeleted),
                    CreatedAt = category.CreatedAt
                };

                return Ok(new ApiResponse<CategoryResponseDto>
                {
                    Success = true,
                    Message = "Category retrieved successfully",
                    Data = categoryDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving category",
                    Error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategory([FromForm] CategoryCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                // Check for duplicate name
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower() && !c.IsDeleted);

                if (existingCategory != null)
                {
                    return BadRequest(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category name already exists"
                    });
                }

                string imageUrl = await SaveImage(dto.ImageFile, "categories");

                var category = new Category
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description,
                    ImageUrl = imageUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryResponse = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    MenuItemsCount = 0,
                    CreatedAt = category.CreatedAt
                };

                return Ok(new ApiResponse<CategoryResponseDto>
                {
                    Success = true,
                    Message = "Category created successfully",
                    Data = categoryResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryResponseDto>
                {
                    Success = false,
                    Message = "Error creating category",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> UpdateCategory(int id, [FromForm] CategoryUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var category = await _context.Categories.FindAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                // Check for duplicate name (excluding current category)
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower()
                                           && c.Id != id
                                           && !c.IsDeleted);

                if (existingCategory != null)
                {
                    return BadRequest(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category name already exists"
                    });
                }

                category.Name = dto.Name.Trim();
                category.Description = dto.Description;
                category.IsActive = dto.IsActive;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = "Admin";

                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    DeleteOldImage(category.ImageUrl, "categories");
                    category.ImageUrl = await SaveImage(dto.ImageFile, "categories");
                }

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                var categoryResponse = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    MenuItemsCount = await _context.MenuItems.CountAsync(m => m.CategoryId == id && !m.IsDeleted),
                    CreatedAt = category.CreatedAt
                };

                return Ok(new ApiResponse<CategoryResponseDto>
                {
                    Success = true,
                    Message = "Category updated successfully",
                    Data = categoryResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryResponseDto>
                {
                    Success = false,
                    Message = "Error updating category",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.MenuItems)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (category == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                // Soft delete category
                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = "Admin";

                // Soft delete related products
                foreach (var menuItem in category.MenuItems.Where(m => !m.IsDeleted))
                {
                    menuItem.IsDeleted = true;
                    menuItem.UpdatedAt = DateTime.UtcNow;
                    menuItem.UpdatedBy = "Admin";
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Category deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting category",
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