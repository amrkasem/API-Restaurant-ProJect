// Controllers/Admin/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.UnitWork;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.AdminCategories.GetAllCategoriesWithDetailsAsync();

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
                var category = await _unitOfWork.AdminCategories.GetCategoryWithDetailsAsync(id);

                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponse<CategoryResponseDto>
                {
                    Success = true,
                    Message = "Category retrieved successfully",
                    Data = category
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
                if (await _unitOfWork.AdminCategories.IsCategoryNameExistsAsync(dto.Name))
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

                await _unitOfWork.AdminCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

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

                var category = await _unitOfWork.AdminCategories.GetByIdAsync(id);
                if (category == null || category.IsDeleted)
                {
                    return NotFound(new ApiResponse<CategoryResponseDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                // Check for duplicate name (excluding current category)
                if (await _unitOfWork.AdminCategories.IsCategoryNameExistsAsync(dto.Name, id))
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

                _unitOfWork.AdminCategories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                var categoryResponse = await _unitOfWork.AdminCategories.GetCategoryWithDetailsAsync(id);

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
                await _unitOfWork.AdminCategories.SoftDeleteCategoryWithMenuItemsAsync(id, "Admin");
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Category deleted successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Category not found"
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