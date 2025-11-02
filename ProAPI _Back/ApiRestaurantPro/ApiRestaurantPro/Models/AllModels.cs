using ApiRestaurantPro.Models.ENUMS;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiRestaurantPro.Models
{
    //11 All Models
    //1 ApplicationUser

    public class ApplicationUser : IdentityUser
    {
        [StringLength(200)]
        public string? Address { get; set; }

        public string? ImageUrl { get; set; }


        public List<Order>? Orders { get; set; }

        // User → Cart (1:1)
        public Cart? Cart { get; set; }

        // User → Wishlist (1:1)
        public Wishlist? Wishlist { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }


    //2 BaseEntity

    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = "System";

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    //3  Cart

    public class Cart : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        
    }

    //4  CartItem

    public class CartItem : BaseEntity
    {
        [Required]
        public int CartId { get; set; }

        [ForeignKey(nameof(CartId))]
        public Cart Cart { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(MenuItemId))]
        public MenuItem MenuItem { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Subtotal { get; set; }
        //public decimal Subtotal => Quantity * Price;
    }

    //5 Category 

    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        // Computed Property
        [NotMapped]
        public int ActiveItemsCount { get; set; }

      
    }

    //6  MenuItem

    public class MenuItem : BaseEntity
    {
        [Required(ErrorMessage = "Item name is required")]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 9999.99)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Range(1, 180)]
        public int PreparationTime { get; set; } = 15;

        public int DailyOrderCount { get; set; } = 0;

        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Navigation
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();


        
    }


    //7 Order

    public class Order : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(15)]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public OrderType OrderType { get; set; } = OrderType.DineIn;

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; }  //for admin 

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public DateTime? EstimatedDeliveryTime { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [Required]

        // Navigation
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }


       
    }

    //8  OrderItem

    public class OrderItem : BaseEntity
    {

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        [Required]
        public int OrderId { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; }



        //Business Logic
        /* 
         public void CalculateSubtotal()
         {
             if (MenuItem != null)
                 Subtotal = MenuItem.Price * Quantity;
         }
        */
    }

    //9  Wishlist

    public class Wishlist : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalEstimatedPrice { get; set; }
    }

    //10 WishlistItem

    public class WishlistItem : BaseEntity
    {
        [Required]
        public int WishlistId { get; set; }

        [ForeignKey(nameof(WishlistId))]
        public Wishlist Wishlist { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(MenuItemId))]
        public MenuItem MenuItem { get; set; }

        [Range(1, 100)]
        public int DesiredQuantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Subtotal => DesiredQuantity * Price;
    }
}
