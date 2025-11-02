// Controllers/Customer/CartController.cs
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
    public class CartController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(MyDbContext context, UserManager<ApplicationUser> userManager)
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

        #endregion

        /// <summary>
        /// Get customer's cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetCart()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                if (cart == null)
                {
                    // Create new cart if doesn't exist
                    cart = new Cart
                    {
                        UserId = currentUserId,
                        Total = 0,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartDto = new CartResponseDto
                {
                    Id = cart.Id,
                    Total = cart.Total,
                    ItemsCount = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                    Items = cart.CartItems?.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemImageUrl = ci.MenuItem.ImageUrl,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        Subtotal = ci.Quantity * ci.Price
                    }).ToList() ?? new List<CartItemDto>()
                };

                return Ok(new ApiResponse<CartResponseDto>
                {
                    Success = true,
                    Message = "Cart retrieved successfully",
                    Data = cartDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving cart",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get cart items count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartCount()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                int count = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = "Cart count retrieved successfully",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Data = 0
                });
            }
        }

        /// <summary>
        /// Add product to cart
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartResponseDto>>> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var product = await _context.MenuItems.FindAsync(dto.ProductId);

                if (product == null || !product.IsAvailable)
                {
                    return BadRequest(new ApiResponse<CartResponseDto>
                    {
                        Success = false,
                        Message = "Product not available"
                    });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = currentUserId,
                        Total = 0,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.MenuItemId == dto.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                    existingItem.Price = product.Price;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.UpdatedBy = "Customer";
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        MenuItemId = dto.ProductId,
                        Quantity = dto.Quantity,
                        Price = product.Price,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // Reload cart with items
                await _context.Entry(cart)
                    .Collection(c => c.CartItems)
                    .Query()
                    .Include(ci => ci.MenuItem)
                    .LoadAsync();

                cart.Total = cart.CartItems?.Sum(ci => ci.Quantity * ci.Price) ?? 0;
                cart.UpdatedAt = DateTime.UtcNow;
                cart.UpdatedBy = "Customer";

                await _context.SaveChangesAsync();

                var cartDto = new CartResponseDto
                {
                    Id = cart.Id,
                    Total = cart.Total,
                    ItemsCount = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                    Items = cart.CartItems?.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemImageUrl = ci.MenuItem.ImageUrl,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        Subtotal = ci.Quantity * ci.Price
                    }).ToList() ?? new List<CartItemDto>()
                };

                return Ok(new ApiResponse<CartResponseDto>
                {
                    Success = true,
                    Message = "Product added to cart successfully",
                    Data = cartDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = "Error adding to cart",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update/{cartItemId}")]
        public async Task<ActionResult<ApiResponse<CartResponseDto>>> UpdateQuantity(
            int cartItemId,
            [FromBody] UpdateCartQuantityDto dto)
        {
            try
            {
                if (dto.Quantity < 1 || dto.Quantity > 100)
                {
                    return BadRequest(new ApiResponse<CartResponseDto>
                    {
                        Success = false,
                        Message = "Invalid quantity"
                    });
                }

                string currentUserId = await GetCurrentUserId();

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                        .ThenInclude(c => c.CartItems)
                            .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == currentUserId);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponse<CartResponseDto>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                cartItem.Quantity = dto.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                cartItem.UpdatedBy = "Customer";

                await _context.SaveChangesAsync();

                var cart = cartItem.Cart;
                cart.Total = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var cartDto = new CartResponseDto
                {
                    Id = cart.Id,
                    Total = cart.Total,
                    ItemsCount = cart.CartItems.Sum(ci => ci.Quantity),
                    Items = cart.CartItems.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemImageUrl = ci.MenuItem.ImageUrl,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        Subtotal = ci.Quantity * ci.Price
                    }).ToList()
                };

                return Ok(new ApiResponse<CartResponseDto>
                {
                    Success = true,
                    Message = "Cart updated successfully",
                    Data = cartDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = "Error updating cart",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("remove/{cartItemId}")]
        public async Task<ActionResult<ApiResponse<CartResponseDto>>> RemoveFromCart(int cartItemId)
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                        .ThenInclude(c => c.CartItems)
                            .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == currentUserId);

                if (cartItem == null)
                {
                    return NotFound(new ApiResponse<CartResponseDto>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                var cart = cartItem.Cart;
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Reload cart
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);

                cart.Total = cart.CartItems?.Sum(ci => ci.Quantity * ci.Price) ?? 0;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var cartDto = new CartResponseDto
                {
                    Id = cart.Id,
                    Total = cart.Total,
                    ItemsCount = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                    Items = cart.CartItems?.Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        MenuItemId = ci.MenuItemId,
                        MenuItemName = ci.MenuItem.Name,
                        MenuItemImageUrl = ci.MenuItem.ImageUrl,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        Subtotal = ci.Quantity * ci.Price
                    }).ToList() ?? new List<CartItemDto>()
                };

                return Ok(new ApiResponse<CartResponseDto>
                {
                    Success = true,
                    Message = "Item removed from cart successfully",
                    Data = cartDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartResponseDto>
                {
                    Success = false,
                    Message = "Error removing from cart",
                    Error = ex.Message
                });
            }
        }
    }
}