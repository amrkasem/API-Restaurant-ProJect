// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Context;
using System.Security.Claims;

namespace ApiRestaurantPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(
            MyDbContext context,
            UserManager<ApplicationUser> userManager)
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

        #region Dashboard

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<CustomerDashboardDto>>> GetDashboard()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);

                var ordersCount = await _context.Orders
                    .CountAsync(o => o.UserId == currentUserId && !o.IsDeleted);

                var cartItemsCount = await _context.Carts
                    .Where(c => c.UserId == currentUserId)
                    .SelectMany(c => c.CartItems)
                    .SumAsync(ci => ci.Quantity);

                var wishlistItemsCount = await _context.Wishlists
                    .Where(w => w.UserId == currentUserId && !w.IsDeleted)
                    .SelectMany(w => w.WishlistItems.Where(wi => !wi.IsDeleted))
                    .CountAsync();

                var dashboardData = new CustomerDashboardDto
                {
                    UserName = user?.UserName ?? "Customer",
                    Email = user?.Email ?? "",
                    TotalOrders = ordersCount,
                    CartItemsCount = cartItemsCount,
                    WishlistItemsCount = wishlistItemsCount
                };

                return Ok(new ApiResponse<CustomerDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard data retrieved successfully",
                    Data = dashboardData
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerDashboardDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerDashboardDto>
                {
                    Success = false,
                    Message = "Error retrieving dashboard data",
                    Error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                UserName = User.Identity?.Name,
                Roles = User.Claims
                             .Where(c => c.Type == ClaimTypes.Role)
                             .Select(c => c.Value)
                             .ToList()
            });
        }


        #endregion
    }
}