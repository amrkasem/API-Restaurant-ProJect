// Repository/Admin/AdminUserRepository.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Repository.Admin
{
    public interface IAdminUserRepository
    {
        Task<List<UserResponseDto>> GetAllUsersAsync(UserManager<ApplicationUser> userManager);
        Task<UserResponseDto?> GetUserByIdAsync(string userId, UserManager<ApplicationUser> userManager);
        Task<ApplicationUser?> GetUserEntityByIdAsync(string userId);
        Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null);
        Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null);
        Task<List<UserResponseDto>> GetCustomersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager);
        Task SoftDeleteUserAsync(string userId);
        Task CreateUserShoppingDataAsync(ApplicationUser user);
    }

    public class AdminUserRepository : IAdminUserRepository
    {
        private readonly MyDbContext _context;

        public AdminUserRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var users = await userManager.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponseDto
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    ImageUrl = u.ImageUrl,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // Add roles for each user
            foreach (var userDto in users)
            {
                var user = await userManager.FindByIdAsync(userDto.UserId);
                if (user != null)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    userDto.Roles = roles.ToList();
                }
            }

            return users;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(string userId, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.Users
                .Include(u => u.Orders.Where(o => !o.IsDeleted))
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return null;
            }

            var roles = await userManager.GetRolesAsync(user);

            return new UserResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ImageUrl = user.ImageUrl,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<ApplicationUser?> GetUserEntityByIdAsync(string userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<bool> IsEmailExistsAsync(string email, string? excludeUserId = null)
        {
            var query = _context.Users.Where(u => !u.IsDeleted && u.Email!.ToLower() == email.ToLower());

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username, string? excludeUserId = null)
        {
            var query = _context.Users.Where(u => !u.IsDeleted && u.UserName!.ToLower() == username.ToLower());

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(u => u.Id != excludeUserId);
            }

            return await query.AnyAsync();
        }

        public async Task<List<UserResponseDto>> GetCustomersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var customerRole = await roleManager.FindByNameAsync("Customer");
            if (customerRole == null)
            {
                return new List<UserResponseDto>();
            }

            var customerUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == customerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var customers = await userManager.Users
                .Where(u => customerUserIds.Contains(u.Id) && !u.IsDeleted)
                .Include(u => u.Orders.Where(o => !o.IsDeleted))
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponseDto
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    ImageUrl = u.ImageUrl,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // Add roles for each customer
            foreach (var customer in customers)
            {
                customer.Roles = new List<string> { "Customer" };
            }

            return customers;
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = "Admin";

            _context.Users.Update(user);
        }

        public async Task CreateUserShoppingDataAsync(ApplicationUser user)
        {
            // Create cart for user
            var cart = new Cart
            {
                UserId = user.Id,
                Total = 0,
                CreatedBy = "System"
            };

            // Create wishlist for user
            var wishlist = new Wishlist
            {
                UserId = user.Id,
                TotalEstimatedPrice = 0,
                CreatedBy = "System"
            };

            await _context.Carts.AddAsync(cart);
            await _context.Wishlists.AddAsync(wishlist);
        }
    }
}