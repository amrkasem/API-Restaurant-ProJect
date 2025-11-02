// Controllers/AdminController.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.Models.ENUMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            MyDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        #region Dashboard
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var totalCustomers = await _context.UserRoles
                    .Join(_roleManager.Roles,
                          ur => ur.RoleId,
                          r => r.Id,
                          (ur, r) => new { ur.UserId, RoleName = r.Name })
                    .Where(x => x.RoleName == "Customer")
                    .Join(_userManager.Users,
                          ur => ur.UserId,
                          u => u.Id,
                          (ur, u) => u)
                    .CountAsync(u => !u.IsDeleted);

                var stats = new DashboardStatsDto
                {
                    TotalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted),
                    TotalProducts = await _context.MenuItems.CountAsync(m => !m.IsDeleted),
                    TotalOrders = await _context.Orders.CountAsync(o => !o.IsDeleted),
                    TotalUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted),
                    TotalCustomers = totalCustomers,
                    PendingOrders = await _context.Orders
                        .CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Pending),
                    TodayRevenue = await _context.Orders
                        .Where(o => !o.IsDeleted && o.CreatedAt.Date == today)
                        .SumAsync(o => o.Total),
                    ActiveProducts = await _context.MenuItems
                        .CountAsync(m => !m.IsDeleted && m.IsAvailable)
                };

                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Message = "Dashboard stats retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DashboardStatsDto>
                {
                    Success = false,
                    Message = "Error retrieving dashboard stats",
                    Error = ex.Message
                });
            }
        }
        #endregion

        #region Helper Methods
        private async Task<string> SaveImage(IFormFile imageFile, string folderName)
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

        private void DeleteOldImage(string imageUrl, string folderName)
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
        #endregion
    }

    // Generic API Response Wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }
}