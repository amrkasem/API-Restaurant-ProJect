// Controllers/Customer/WishlistController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using System.Security.Claims;

namespace ApiRestaurantPro.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/[controller]")]
    [Authorize(Roles = "Customer")]
    public class WishlistsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistsController(MyDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Helper Methods

        private async Task<string> GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not logged in");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return userId;
        }

        private async Task UpdateWishlistTotal(int wishlistId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.Id == wishlistId);

            if (wishlist != null)
            {
                wishlist.TotalEstimatedPrice = wishlist.WishlistItems
                    .Where(wi => !wi.IsDeleted)
                    .Sum(wi => wi.DesiredQuantity * wi.Price);
                wishlist.UpdatedAt = DateTime.UtcNow;
                wishlist.UpdatedBy = "Customer";
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        /// <summary>
        /// Get customer's wishlist
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CustomerWishlistDto>>> GetWishlist()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                            .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(w => w.UserId == currentUserId && !w.IsDeleted);

                if (wishlist == null)
                {
                    // Create new wishlist if doesn't exist
                    wishlist = new Wishlist
                    {
                        UserId = currentUserId,
                        TotalEstimatedPrice = 0,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.Wishlists.Add(wishlist);
                    await _context.SaveChangesAsync();
                }

                var wishlistDto = new CustomerWishlistDto
                {
                    Id = wishlist.Id,
                    TotalEstimatedPrice = wishlist.TotalEstimatedPrice,
                    ItemsCount = wishlist.WishlistItems?.Count(wi => !wi.IsDeleted) ?? 0,
                    Items = wishlist.WishlistItems?
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new CustomerWishlistItemDto
                        {
                            Id = wi.Id,
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            CategoryName = wi.MenuItem.Category?.Name,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price,
                            IsAvailable = wi.MenuItem.IsAvailable
                        }).ToList() ?? new List<CustomerWishlistItemDto>()
                };

                return Ok(new ApiResponse<CustomerWishlistDto>
                {
                    Success = true,
                    Message = "Wishlist retrieved successfully",
                    Data = wishlistDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = "Error retrieving wishlist",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get wishlist items count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetWishlistCount()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .FirstOrDefaultAsync(w => w.UserId == currentUserId && !w.IsDeleted);

                int count = wishlist?.WishlistItems?.Count(wi => !wi.IsDeleted) ?? 0;

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = "Wishlist count retrieved successfully",
                    Data = count
                });
            }
            catch (Exception)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = 0
                });
            }
        }

        /// <summary>
        /// Add product to wishlist
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CustomerWishlistDto>>> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var product = await _context.MenuItems
                    .Include(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == dto.ProductId && !m.IsDeleted && m.IsAvailable);

                if (product == null)
                {
                    return BadRequest(new ApiResponse<CustomerWishlistDto>
                    {
                        Success = false,
                        Message = "Product not found or not available"
                    });
                }

                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                            .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(w => w.UserId == currentUserId && !w.IsDeleted);

                if (wishlist == null)
                {
                    wishlist = new Wishlist
                    {
                        UserId = currentUserId,
                        TotalEstimatedPrice = 0,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.Wishlists.Add(wishlist);
                    await _context.SaveChangesAsync();
                }

                var existingItem = wishlist.WishlistItems?
                    .FirstOrDefault(wi => wi.MenuItemId == dto.ProductId && !wi.IsDeleted);

                if (existingItem != null)
                {
                    return BadRequest(new ApiResponse<CustomerWishlistDto>
                    {
                        Success = false,
                        Message = "Product already in wishlist"
                    });
                }

                var wishlistItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    MenuItemId = dto.ProductId,
                    DesiredQuantity = 1,
                    Price = product.Price,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Customer"
                };

                _context.WishlistItems.Add(wishlistItem);
                await _context.SaveChangesAsync();

                await UpdateWishlistTotal(wishlist.Id);

                // Reload wishlist
                wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                            .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(w => w.Id == wishlist.Id);

                var wishlistDto = new CustomerWishlistDto
                {
                    Id = wishlist.Id,
                    TotalEstimatedPrice = wishlist.TotalEstimatedPrice,
                    ItemsCount = wishlist.WishlistItems.Count(wi => !wi.IsDeleted),
                    Items = wishlist.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new CustomerWishlistItemDto
                        {
                            Id = wi.Id,
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            CategoryName = wi.MenuItem.Category?.Name,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price,
                            IsAvailable = wi.MenuItem.IsAvailable
                        }).ToList()
                };

                return Ok(new ApiResponse<CustomerWishlistDto>
                {
                    Success = true,
                    Message = "Product added to wishlist successfully",
                    Data = wishlistDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = "Error adding to wishlist",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update wishlist item quantity
        /// </summary>
        [HttpPut("update/{wishlistItemId}")]
        public async Task<ActionResult<ApiResponse<CustomerWishlistDto>>> UpdateQuantity(
            int wishlistItemId,
            [FromBody] UpdateWishlistQuantityDto dto)
        {
            try
            {
                if (dto.Quantity < 1 || dto.Quantity > 100)
                {
                    return BadRequest(new ApiResponse<CustomerWishlistDto>
                    {
                        Success = false,
                        Message = "Invalid quantity"
                    });
                }

                string currentUserId = await GetCurrentUserId();

                var wishlistItem = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                        .ThenInclude(w => w.WishlistItems)
                            .ThenInclude(wi => wi.MenuItem)
                                .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(wi => wi.Id == wishlistItemId &&
                                             wi.Wishlist.UserId == currentUserId &&
                                             !wi.IsDeleted);

                if (wishlistItem == null)
                {
                    return NotFound(new ApiResponse<CustomerWishlistDto>
                    {
                        Success = false,
                        Message = "Wishlist item not found"
                    });
                }

                wishlistItem.DesiredQuantity = dto.Quantity;
                wishlistItem.UpdatedAt = DateTime.UtcNow;
                wishlistItem.UpdatedBy = "Customer";

                await _context.SaveChangesAsync();
                await UpdateWishlistTotal(wishlistItem.WishlistId);

                // Reload wishlist
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                            .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(w => w.Id == wishlistItem.WishlistId);

                var wishlistDto = new CustomerWishlistDto
                {
                    Id = wishlist.Id,
                    TotalEstimatedPrice = wishlist.TotalEstimatedPrice,
                    ItemsCount = wishlist.WishlistItems.Count(wi => !wi.IsDeleted),
                    Items = wishlist.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new CustomerWishlistItemDto
                        {
                            Id = wi.Id,
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            CategoryName = wi.MenuItem.Category?.Name,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price,
                            IsAvailable = wi.MenuItem.IsAvailable
                        }).ToList()
                };

                return Ok(new ApiResponse<CustomerWishlistDto>
                {
                    Success = true,
                    Message = "Wishlist updated successfully",
                    Data = wishlistDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = "Error updating wishlist",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Remove item from wishlist
        /// </summary>
        [HttpDelete("remove/{wishlistItemId}")]
        public async Task<ActionResult<ApiResponse<CustomerWishlistDto>>> RemoveFromWishlist(int wishlistItemId)
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var wishlistItem = await _context.WishlistItems
                    .Include(wi => wi.Wishlist)
                        .ThenInclude(w => w.WishlistItems)
                            .ThenInclude(wi => wi.MenuItem)
                                .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(wi => wi.Id == wishlistItemId &&
                                             wi.Wishlist.UserId == currentUserId &&
                                             !wi.IsDeleted);

                if (wishlistItem == null)
                {
                    return NotFound(new ApiResponse<CustomerWishlistDto>
                    {
                        Success = false,
                        Message = "Wishlist item not found"
                    });
                }

                var wishlistId = wishlistItem.WishlistId;

                // Hard delete
                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();

                await UpdateWishlistTotal(wishlistId);

                // Reload wishlist
                var wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                        .ThenInclude(wi => wi.MenuItem)
                            .ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(w => w.Id == wishlistId);

                var wishlistDto = new CustomerWishlistDto
                {
                    Id = wishlist.Id,
                    TotalEstimatedPrice = wishlist.TotalEstimatedPrice,
                    ItemsCount = wishlist.WishlistItems.Count(wi => !wi.IsDeleted),
                    Items = wishlist.WishlistItems
                        .Where(wi => !wi.IsDeleted)
                        .Select(wi => new CustomerWishlistItemDto
                        {
                            Id = wi.Id,
                            MenuItemId = wi.MenuItemId,
                            MenuItemName = wi.MenuItem.Name,
                            MenuItemImageUrl = wi.MenuItem.ImageUrl,
                            CategoryName = wi.MenuItem.Category?.Name,
                            DesiredQuantity = wi.DesiredQuantity,
                            Price = wi.Price,
                            Subtotal = wi.DesiredQuantity * wi.Price,
                            IsAvailable = wi.MenuItem.IsAvailable
                        }).ToList()
                };

                return Ok(new ApiResponse<CustomerWishlistDto>
                {
                    Success = true,
                    Message = "Item removed from wishlist successfully",
                    Data = wishlistDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerWishlistDto>
                {
                    Success = false,
                    Message = "Error removing from wishlist",
                    Error = ex.Message
                });
            }
        }
    }
}