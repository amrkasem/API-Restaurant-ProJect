// DTOs/CustomerDtos.cs
using ApiRestaurantPro.Models.ENUMS;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiRestaurantPro.DTOs
{
    #region Dashboard DTOs
    public class CustomerDashboardDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public int CartItemsCount { get; set; }
        public int WishlistItemsCount { get; set; }
    }
    #endregion

    #region Product DTOs
    public class CustomerProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int PreparationTime { get; set; }
        public bool IsAvailable { get; set; }
    }
    #endregion

    #region Category DTOs
    public class CustomerCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductsCount { get; set; }
    }

    public class CustomerCategoryWithProductsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<CustomerProductDto> Products { get; set; } = new();
    }
    #endregion

    #region Cart DTOs
    public class CartResponseDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public int ItemsCount { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? MenuItemImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartQuantityDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
    #endregion

    #region Order DTOs
    public class CustomerOrderDto
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
        public List<CustomerOrderItemDto> OrderItems { get; set; } = new();
    }

    public class CustomerOrderItemDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? MenuItemImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public string? SpecialInstructions { get; set; }
    }

    public class PlaceOrderDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Order type is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderType OrderType { get; set; }

        [StringLength(500, ErrorMessage = "Delivery address cannot exceed 500 characters")]
        public string? DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
    #endregion

    #region Wishlist DTOs
    public class CustomerWishlistDto
    {
        public int Id { get; set; }
        public decimal TotalEstimatedPrice { get; set; }
        public int ItemsCount { get; set; }
        public List<CustomerWishlistItemDto> Items { get; set; } = new();
    }

    public class CustomerWishlistItemDto
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public string? MenuItemImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public int DesiredQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AddToWishlistDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }
    }

    public class UpdateWishlistQuantityDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }
    #endregion
}