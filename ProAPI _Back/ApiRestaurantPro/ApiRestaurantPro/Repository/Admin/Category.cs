// Repository/Admin/AdminCategoryRepository.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Repository.Admin
{
    public interface IAdminCategoryRepository : IGenericRepository<Category>
    {
        Task<List<CategoryResponseDto>> GetAllCategoriesWithDetailsAsync();
        Task<CategoryResponseDto?> GetCategoryWithDetailsAsync(int id);
        Task<bool> IsCategoryNameExistsAsync(string name, int? excludeId = null);
        Task SoftDeleteCategoryWithMenuItemsAsync(int categoryId, string deletedBy);
    }

    public class AdminCategoryRepository : GenericRepository<Category>, IAdminCategoryRepository
    {
        public AdminCategoryRepository(MyDbContext context) : base(context)
        {
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesWithDetailsAsync()
        {
            return await _context.Categories
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
        }

        public async Task<CategoryResponseDto?> GetCategoryWithDetailsAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
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
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories
                .Where(c => !c.IsDeleted && c.Name.ToLower() == name.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task SoftDeleteCategoryWithMenuItemsAsync(int categoryId, string deletedBy)
        {
            var category = await _context.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = deletedBy;

            foreach (var menuItem in category.MenuItems.Where(m => !m.IsDeleted))
            {
                menuItem.IsDeleted = true;
                menuItem.UpdatedAt = DateTime.UtcNow;
                menuItem.UpdatedBy = deletedBy;
            }

            _context.Categories.Update(category);
        }
    }
}