// Repository/Admin/AdminProductRepository.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Repository.Admin
{
    public interface IAdminProductRepository : IGenericRepository<MenuItem>
    {
        Task<List<ProductResponseDto>> GetAllProductsWithDetailsAsync();
        Task<ProductResponseDto?> GetProductWithDetailsAsync(int id);
        Task<bool> IsProductNameExistsAsync(string name, int? excludeId = null);
        Task<MenuItem?> GetProductWithCategoryAsync(int id);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
        Task SoftDeleteProductAsync(int productId, string deletedBy);
    }

    public class AdminProductRepository : GenericRepository<MenuItem>, IAdminProductRepository
    {
        public AdminProductRepository(MyDbContext context) : base(context)
        {
        }

        public async Task<List<ProductResponseDto>> GetAllProductsWithDetailsAsync()
        {
            return await _context.MenuItems
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
        }

        public async Task<ProductResponseDto?> GetProductWithDetailsAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.Id == id && !m.IsDeleted)
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
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsProductNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.MenuItems
                .Where(m => !m.IsDeleted && m.Name.ToLower() == name.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<MenuItem?> GetProductWithCategoryAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<int> GetProductCountByCategoryAsync(int categoryId)
        {
            return await _context.MenuItems
                .CountAsync(m => m.CategoryId == categoryId && !m.IsDeleted);
        }

        public async Task SoftDeleteProductAsync(int productId, string deletedBy)
        {
            var product = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == productId && !m.IsDeleted);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found");
            }

            // Check if this is the last product in the category
            var productCount = await GetProductCountByCategoryAsync(product.CategoryId);

            if (productCount == 1 && product.Category != null) // This is the last product
            {
                product.Category.IsActive = false;
                product.Category.UpdatedAt = DateTime.UtcNow;
                product.Category.UpdatedBy = deletedBy;
                _context.Categories.Update(product.Category);
            }

            // Soft delete product
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = deletedBy;

            _context.MenuItems.Update(product);
        }
    }
}