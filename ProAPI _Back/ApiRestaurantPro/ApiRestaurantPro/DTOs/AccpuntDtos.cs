// DTOs/AccountDtos.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ApiRestaurantPro.DTOs
{
    /// <summary>
    /// Login Data Transfer Object
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Register Data Transfer Object 
    /// </summary>
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number is too long")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Address is too long")]
        public string? Address { get; set; } 

        public IFormFile? ImageFile { get; set; } 
    }

    /// <summary>
    /// User Response DTO
    /// </summary>
    public class UserResponseDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Login Response DTO
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}