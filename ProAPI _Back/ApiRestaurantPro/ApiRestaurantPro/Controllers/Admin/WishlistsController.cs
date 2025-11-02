// Controllers/Admin/WishlistsController.cs
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
    public class WishlistsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WishlistsController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<WishlistResponseDto>>>> GetWishlists()
        {
            try
            {
                var wishlists = await _context.Wishlists
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

                return Ok(new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = true,
                    Message = "Wishlists retrieved successfully",
                    Data = wishlists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<WishlistResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving wishlists",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WishlistResponseDto>>> GetWishlist(int id)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.User)
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                    .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

                if (wishlist == null)
                {
                    return NotFound(new ApiResponse<WishlistResponseDto>
                    {
                        Success = false,
                        Message = "Wishlist not found"
                    });
                }

                var wishlistDto = new WishlistResponseDto
                {
                    Id = wishlist.Id,
                    UserId = wishlist.UserId,
                    UserName = wishlist.User.UserName ?? "Unknown",
                    UserEmail = wishlist.User.Email ?? "Unknown",
                    TotalEstimatedPrice = wishlist.TotalEstimatedPrice,
                    ItemsCount = wishlist.WishlistItems.Count(wi => !wi.IsDeleted),
                    CreatedAt = wishlist.CreatedAt,
                    WishlistItems = wishlist.WishlistItems
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
                };

                return Ok(new ApiResponse<WishlistResponseDto>
                {
                    Success = true,
                    Message = "Wishlist retrieved successfully",
                    Data = wishlistDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<WishlistResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving wishlist",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWishlist(int id)
        {
            try
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

                if (wishlist == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Wishlist not found"
                    });
                }

                // Soft delete wishlist
                wishlist.IsDeleted = true;
                wishlist.UpdatedAt = DateTime.UtcNow;
                wishlist.UpdatedBy = "Admin";

                // Soft delete wishlist items
                foreach (var wishlistItem in wishlist.WishlistItems.Where(wi => !wi.IsDeleted))
                {
                    wishlistItem.IsDeleted = true;
                    wishlistItem.UpdatedAt = DateTime.UtcNow;
                    wishlistItem.UpdatedBy = "Admin";
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Wishlist deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting wishlist",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWishlistItem(int itemId)
        {
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                    .FirstOrDefaultAsync(wi => wi.Id == itemId && !wi.IsDeleted);

                if (wishlistItem == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Wishlist item not found"
                    });
                }

                // Soft delete wishlist item
                wishlistItem.IsDeleted = true;
                wishlistItem.UpdatedAt = DateTime.UtcNow;
                wishlistItem.UpdatedBy = "Admin";

                // Update wishlist total
                var wishlist = wishlistItem.Wishlist;
                wishlist.TotalEstimatedPrice = await _context.WishlistItems
                    .Where(wi => wi.WishlistId == wishlist.Id && !wi.IsDeleted)
                    .SumAsync(wi => wi.DesiredQuantity * wi.Price);
                wishlist.UpdatedAt = DateTime.UtcNow;
                wishlist.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Wishlist item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting wishlist item",
                    Error = ex.Message
                });
            }
        }
    }
}