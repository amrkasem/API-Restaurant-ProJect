// Repository/Admin/AdminWishlistRepository.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Repository.Admin
{
    public interface IAdminWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<List<WishlistResponseDto>> GetAllWishlistsWithDetailsAsync();
        Task<WishlistResponseDto?> GetWishlistWithDetailsAsync(int id);
        Task<Wishlist?> GetWishlistWithItemsAsync(int id);
        Task SoftDeleteWishlistWithItemsAsync(int wishlistId, string deletedBy);
        Task<WishlistItem?> GetWishlistItemAsync(int itemId);
        Task SoftDeleteWishlistItemAsync(int itemId, string deletedBy);
        Task<List<WishlistResponseDto>> GetWishlistsByUserIdAsync(string userId);
    }

    public class AdminWishlistRepository : GenericRepository<Wishlist>, IAdminWishlistRepository
    {
        public AdminWishlistRepository(MyDbContext context) : base(context)
        {
        }

        public async Task<List<WishlistResponseDto>> GetAllWishlistsWithDetailsAsync()
        {
            return await _context.Wishlists
                .Include(w => w.User)
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.MenuItem)
                .Where(w => !w.IsDeleted)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WishlistResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    UserName = w.User.UserName ?? "Unknown",
                    UserEmail = w.User.Email ?? "Unknown",
                    TotalEstimatedPrice = w.TotalEstimatedPrice,
                    ItemsCount = w.WishlistItems.Count(wi => !wi.IsDeleted),
                    CreatedAt = w.CreatedAt,
                    WishlistItems = w.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new WishlistItemResponseDto
                        {
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<WishlistResponseDto?> GetWishlistWithDetailsAsync(int id)
        {
            return await _context.Wishlists
                .Include(w => w.User)
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.MenuItem)
                .Where(w => w.Id == id && !w.IsDeleted)
                .Select(w => new WishlistResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    UserName = w.User.UserName ?? "Unknown",
                    UserEmail = w.User.Email ?? "Unknown",
                    TotalEstimatedPrice = w.TotalEstimatedPrice,
                    ItemsCount = w.WishlistItems.Count(wi => !wi.IsDeleted),
                    CreatedAt = w.CreatedAt,
                    WishlistItems = w.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new WishlistItemResponseDto
                        {
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price
                        }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Wishlist?> GetWishlistWithItemsAsync(int id)
        {
            return await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        }

        public async Task SoftDeleteWishlistWithItemsAsync(int wishlistId, string deletedBy)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.Id == wishlistId && !w.IsDeleted);

            if (wishlist == null)
            {
                throw new KeyNotFoundException($"Wishlist with ID {wishlistId} not found");
            }

            // Soft delete wishlist
            wishlist.IsDeleted = true;
            wishlist.UpdatedAt = DateTime.UtcNow;
            wishlist.UpdatedBy = deletedBy;

            // Soft delete wishlist items
            foreach (var wishlistItem in wishlist.WishlistItems.Where(wi => !wi.IsDeleted))
            {
                wishlistItem.IsDeleted = true;
                wishlistItem.UpdatedAt = DateTime.UtcNow;
                wishlistItem.UpdatedBy = deletedBy;
            }

            _context.Wishlists.Update(wishlist);
        }

        public async Task<WishlistItem?> GetWishlistItemAsync(int itemId)
        {
            return await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi => wi.Id == itemId && !wi.IsDeleted);
        }

        public async Task SoftDeleteWishlistItemAsync(int itemId, string deletedBy)
        {
            var wishlistItem = await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi => wi.Id == itemId && !wi.IsDeleted);

            if (wishlistItem == null)
            {
                throw new KeyNotFoundException($"Wishlist item with ID {itemId} not found");
            }

            // Soft delete wishlist item
            wishlistItem.IsDeleted = true;
            wishlistItem.UpdatedAt = DateTime.UtcNow;
            wishlistItem.UpdatedBy = deletedBy;

            // Update wishlist total
            var wishlist = wishlistItem.Wishlist;
            wishlist.TotalEstimatedPrice = await _context.WishlistItems
                .Where(wi => wi.WishlistId == wishlist.Id && !wi.IsDeleted && wi.Id != itemId)
                .SumAsync(wi => wi.DesiredQuantity * wi.Price);
            wishlist.UpdatedAt = DateTime.UtcNow;
            wishlist.UpdatedBy = deletedBy;

            _context.WishlistItems.Update(wishlistItem);
            _context.Wishlists.Update(wishlist);
        }

        public async Task<List<WishlistResponseDto>> GetWishlistsByUserIdAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.User)
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.MenuItem)
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WishlistResponseDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    UserName = w.User.UserName ?? "Unknown",
                    UserEmail = w.User.Email ?? "Unknown",
                    TotalEstimatedPrice = w.TotalEstimatedPrice,
                    ItemsCount = w.WishlistItems.Count(wi => !wi.IsDeleted),
                    CreatedAt = w.CreatedAt,
                    WishlistItems = w.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new WishlistItemResponseDto
                        {
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price
                        }).ToList()
                })
                .ToListAsync();
        }
    }
}