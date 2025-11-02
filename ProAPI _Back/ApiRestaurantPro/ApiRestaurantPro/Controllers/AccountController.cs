using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

//2/11/2025 - 11:59 AM sunday

namespace ApiRestaurantPro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly MyDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            MyDbContext context) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _environment = environment;
            _context = context;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            try
            {
                // Check if email exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "Email already exists" });

                // Check if username exists
                var existingUsername = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUsername != null)
                    return BadRequest(new { success = false, message = "Username already exists" });

                string imageUrl = "/images/users/default.jpg";
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    try
                    {
                        using var memoryStream = new MemoryStream();
                        await dto.ImageFile.CopyToAsync(memoryStream);
                        var imageBytes = memoryStream.ToArray();
                        var base64String = Convert.ToBase64String(imageBytes);

                        imageUrl = $"data:{dto.ImageFile.ContentType};base64,{base64String}";

                        Console.WriteLine($" Image saved as base64, size: {imageBytes.Length} bytes");
                    }
                    catch (Exception uploadEx)
                    {
                        Console.WriteLine($"❌ Image processing failed: {uploadEx.Message}");
                        imageUrl = "/images/users/default.jpg";
                    }
                }
                else
                {
                    Console.WriteLine("ℹ No image uploaded, using default image");
                }

                //  Create new user   
                var user = new ApplicationUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber, 
                    Address = dto.Address, 
                    ImageUrl = imageUrl,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Self"
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Registration failed",
                        errors = result.Errors.Select(e => e.Description)
                    });

                // Assign Customer role
                await _userManager.AddToRoleAsync(user, "Customer");

                // Create user cart and wishlist automatically
                await CreateUserShoppingData(user);

                return Ok(new
                {
                    success = true,
                    message = "Registration successful",
                    data = new
                    {
                        userId = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        phoneNumber = user.PhoneNumber,
                        address = user.Address,
                        imageUrl = user.ImageUrl,
                        roles = new List<string> { "Customer" }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during registration",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Login user and generate JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized(new { success = false, message = "Invalid email or password" });

                if (user.IsDeleted)
                    return Unauthorized(new { success = false, message = "Account is deactivated" });

                var result = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);

                if (!result.Succeeded)
                    return Unauthorized(new { success = false, message = "Invalid email or password" });

                //  Generate JWT token
                var token = await GenerateJwtToken(user);

                var roles = await _userManager.GetRolesAsync(user);

                Console.WriteLine($"Login - User: {user.Email}, Roles: {string.Join(", ", roles)}");

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = new LoginResponseDto
                    {
                        Token = token,
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = roles.ToList(),     
                        ImageUrl = user.ImageUrl,
                        ExpiresAt = DateTime.UtcNow.AddHours(int.Parse(_configuration["JWT:ExpiryInHours"]))
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during login",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);

                var profile = new UserResponseDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    ImageUrl = user.ImageUrl,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                };

                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                // Update user properties
                user.PhoneNumber = dto.PhoneNumber;
                user.Address = dto.Address;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = user.UserName;

                // Handle image upload
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "users");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }

                    user.ImageUrl = $"/images/users/{fileName}";
                }

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Profile update failed",
                        errors = result.Errors.Select(e => e.Description)
                    });

                return Ok(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating profile",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { success = true, message = "Logout successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during logout",
                    error = ex.Message
                });
            }
        }

        #region Helper Methods

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userId", user.Id),
                new Claim("imageUrl", user.ImageUrl ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryHours = int.Parse(_configuration["JWT:ExpiryInHours"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task CreateUserShoppingData(ApplicationUser user)
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
            await _context.SaveChangesAsync();
        }

        #endregion
    }

    // DTO for profile update
    public class UpdateProfileDto
    {
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number is too long")]
        public string PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address is too long")]
        public string Address { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}