// DTOs/AdminDtos.cs
using ApiRestaurantPro.Models.ENUMS;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiRestaurantPro.DTOs
{
    #region Category DTOs
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CategoryUpdateDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int MenuItemsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    #endregion

    #region Product DTOs
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, 180)]
        public int PreparationTime { get; set; } = 15;

        public bool IsAvailable { get; set; } = true;

        public IFormFile? ImageFile { get; set; }
    }

    public class ProductUpdateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(1, 180)]
        public int PreparationTime { get; set; } = 15;

        public bool IsAvailable { get; set; } = true;

        public IFormFile? ImageFile { get; set; }
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int PreparationTime { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    #endregion

    #region User Management DTOs
    public class AdminUserCreateDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Customer";

        public IFormFile? ImageFile { get; set; }
    }

    public class AdminUserUpdateDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? Password { get; set; }
    }

    
    #endregion

    #region Order DTOs
    public class OrderStatusUpdateDto
    {
        [Required]

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus NewStatus { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string? PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderType OrderType { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public string? SpecialInstructions { get; set; }
    }
    #endregion

    #region Wishlist DTOs
    public class WishlistResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public decimal TotalEstimatedPrice { get; set; }
        public int ItemsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<WishlistItemResponseDto> WishlistItems { get; set; } = new();
    }

    public class WishlistItemResponseDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? MenuItemImageUrl { get; set; }
        public int DesiredQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }
    #endregion

    #region Dashboard DTOs
    public class DashboardStatsDto
    {
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int ActiveProducts { get; set; }
    }
    #endregion
}