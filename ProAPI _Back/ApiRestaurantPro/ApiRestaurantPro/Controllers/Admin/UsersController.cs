// Controllers/Admin/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.UnitWork;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;

        public UsersController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetUsers()
        {
            try
            {
                var users = await _unitOfWork.AdminUsers.GetAllUsersAsync(_userManager);

                return Ok(new ApiResponse<List<UserResponseDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving users",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(string id)
        {
            try
            {
                var user = await _unitOfWork.AdminUsers.GetUserByIdAsync(id, _userManager);

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving user",
                    Error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromForm] AdminUserCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                // Check if email exists
                if (await _unitOfWork.AdminUsers.IsEmailExistsAsync(dto.Email))
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }

                // Check if username exists
                if (await _unitOfWork.AdminUsers.IsUsernameExistsAsync(dto.UserName))
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Username already exists"
                    });
                }

                // Check if role exists
                var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Role does not exist"
                    });
                }

                string imageUrl = await SaveImage(dto.ImageFile, "users");

                var user = new ApplicationUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = dto.Address,
                    ImageUrl = imageUrl,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, dto.Role);

                    // Create user cart and wishlist
                    await _unitOfWork.AdminUsers.CreateUserShoppingDataAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    var roles = await _userManager.GetRolesAsync(user);

                    var userResponse = new UserResponseDto
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

                    return Ok(new ApiResponse<UserResponseDto>
                    {
                        Success = true,
                        Message = "User created successfully",
                        Data = userResponse
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "User creation failed",
                        Error = string.Join("; ", result.Errors.Select(e => e.Description))
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Error creating user",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(string id, [FromForm] AdminUserUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return NotFound(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Check if email exists (excluding current user)
                if (await _unitOfWork.AdminUsers.IsEmailExistsAsync(dto.Email, id))
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }

                // Check if username exists (excluding current user)
                if (await _unitOfWork.AdminUsers.IsUsernameExistsAsync(dto.UserName, id))
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Username already exists"
                    });
                }

                // Check if role exists
                var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
                if (!roleExists)
                {
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = "Role does not exist"
                    });
                }

                user.UserName = dto.UserName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                user.Address = dto.Address;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "Admin";

                // Update password if provided
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                    if (!resetResult.Succeeded)
                    {
                        return BadRequest(new ApiResponse<UserResponseDto>
                        {
                            Success = false,
                            Message = "Password update failed",
                            Error = string.Join("; ", resetResult.Errors.Select(e => e.Description))
                        });
                    }
                }

                // Update role
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);

                // Update image if provided
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    DeleteOldImage(user.ImageUrl, "users");
                    user.ImageUrl = await SaveImage(dto.ImageFile, "users");
                }

                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);

                var userResponse = new UserResponseDto
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

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = userResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>
                {
                    Success = false,
                    Message = "Error updating user",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id)
        {
            try
            {
                await _unitOfWork.AdminUsers.SoftDeleteUserAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting user",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse<List<UserResponseDto>>>> GetCustomers()
        {
            try
            {
                var customers = await _unitOfWork.AdminUsers.GetCustomersAsync(_userManager, _roleManager);

                return Ok(new ApiResponse<List<UserResponseDto>>
                {
                    Success = true,
                    Message = "Customers retrieved successfully",
                    Data = customers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving customers",
                    Error = ex.Message
                });
            }
        }

        private async Task<string> SaveImage(IFormFile? imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
                return $"/images/{folderName}/default-avatar.jpg";

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

        private void DeleteOldImage(string? imageUrl, string folderName)
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
    }
}