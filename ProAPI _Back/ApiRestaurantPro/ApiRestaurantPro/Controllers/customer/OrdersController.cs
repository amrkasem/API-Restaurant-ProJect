// Controllers/Customer/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.Models.ENUMS;
using ApiRestaurantPro.DTOs;
using System.Security.Claims;

namespace ApiRestaurantPro.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/[controller]")]
    [Authorize(Roles = "Customer")]
    public class OrdersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(MyDbContext context, UserManager<ApplicationUser> userManager)
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
        /// Get all customer orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CustomerOrderDto>>>> GetAllOrders()
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.UserId == currentUserId && !o.IsDeleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new CustomerOrderDto
                    {
                        Id = o.Id,
                        CustomerName = o.CustomerName,
                        PhoneNumber = o.PhoneNumber,
                        OrderType = o.OrderType,
                        DeliveryAddress = o.DeliveryAddress,
                        Subtotal = o.Subtotal,
                        Tax = o.Tax,
                        Discount = o.Discount,
                        Total = o.Total,
                        Status = o.Status,
                        PaymentMethod = o.PaymentMethod,
                        EstimatedDeliveryTime = o.EstimatedDeliveryTime,
                        Notes = o.Notes,
                        CreatedAt = o.CreatedAt,
                        OrderItems = o.OrderItems.Select(oi => new CustomerOrderItemDto
                        {
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItem.Name,
                            MenuItemImageUrl = oi.MenuItem.ImageUrl,
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Subtotal = oi.Subtotal,
                            SpecialInstructions = oi.SpecialInstructions
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CustomerOrderDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<List<CustomerOrderDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CustomerOrderDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerOrderDto>>> GetOrderDetails(int id)
        {
            try
            {
                string currentUserId = await GetCurrentUserId();

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .Where(o => o.Id == id && o.UserId == currentUserId && !o.IsDeleted)
                    .Select(o => new CustomerOrderDto
                    {
                        Id = o.Id,
                        CustomerName = o.CustomerName,
                        PhoneNumber = o.PhoneNumber,
                        OrderType = o.OrderType,
                        DeliveryAddress = o.DeliveryAddress,
                        Subtotal = o.Subtotal,
                        Tax = o.Tax,
                        Discount = o.Discount,
                        Total = o.Total,
                        Status = o.Status,
                        PaymentMethod = o.PaymentMethod,
                        EstimatedDeliveryTime = o.EstimatedDeliveryTime,
                        Notes = o.Notes,
                        CreatedAt = o.CreatedAt,
                        OrderItems = o.OrderItems.Select(oi => new CustomerOrderItemDto
                        {
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItem.Name,
                            MenuItemImageUrl = oi.MenuItem.ImageUrl,
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Subtotal = oi.Subtotal,
                            SpecialInstructions = oi.SpecialInstructions
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new ApiResponse<CustomerOrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponse<CustomerOrderDto>
                {
                    Success = true,
                    Message = "Order details retrieved successfully",
                    Data = order
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = "Error retrieving order details",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Place new order from cart
        /// </summary>
        [HttpPost("place-order")]
        public async Task<ActionResult<ApiResponse<CustomerOrderDto>>> PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CustomerOrderDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                string currentUserId = await GetCurrentUserId();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.MenuItem)
                    .FirstOrDefaultAsync(c => c.UserId == currentUserId);

                if (cart?.CartItems == null || !cart.CartItems.Any())
                {
                    return BadRequest(new ApiResponse<CustomerOrderDto>
                    {
                        Success = false,
                        Message = "Cart is empty. Please add items first."
                    });
                }

                if (dto.OrderType == OrderType.Delivery && string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                {
                    return BadRequest(new ApiResponse<CustomerOrderDto>
                    {
                        Success = false,
                        Message = "Delivery address is required for delivery orders"
                    });
                }

                // Calculate totals
                decimal subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
                decimal tax = subtotal * 0.14m;
                decimal discount = 0;

                // Apply discounts
                var hour = DateTime.Now.Hour;
                if (hour >= 15 && hour < 17)
                    discount = subtotal * 0.20m; // 20% Happy Hour discount
                else if (subtotal > 100)
                    discount = subtotal * 0.10m; // 10% bulk discount

                decimal total = subtotal + tax - discount;

                // Create order
                var order = new Order
                {
                    UserId = currentUserId,
                    CustomerName = dto.CustomerName,
                    PhoneNumber = dto.PhoneNumber,
                    OrderType = dto.OrderType,
                    DeliveryAddress = dto.DeliveryAddress,
                    PaymentMethod = dto.PaymentMethod,
                    Notes = dto.Notes,
                    Subtotal = subtotal,
                    Tax = tax,
                    Discount = discount,
                    Total = total,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Customer"
                };

                // Calculate estimated delivery time
                int maxPrepTime = cart.CartItems.Max(ci => ci.MenuItem?.PreparationTime ?? 30);
                order.EstimatedDeliveryTime = DateTime.Now.AddMinutes(maxPrepTime);
                if (dto.OrderType == OrderType.Delivery)
                    order.EstimatedDeliveryTime = order.EstimatedDeliveryTime?.AddMinutes(30);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        MenuItemId = cartItem.MenuItemId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Price,
                        Subtotal = cartItem.Quantity * cartItem.Price,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Customer"
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Clear cart
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.Total = 0;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Load order with items
                var createdOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                var orderDto = new CustomerOrderDto
                {
                    Id = createdOrder.Id,
                    CustomerName = createdOrder.CustomerName,
                    PhoneNumber = createdOrder.PhoneNumber,
                    OrderType = createdOrder.OrderType,
                    DeliveryAddress = createdOrder.DeliveryAddress,
                    Subtotal = createdOrder.Subtotal,
                    Tax = createdOrder.Tax,
                    Discount = createdOrder.Discount,
                    Total = createdOrder.Total,
                    Status = createdOrder.Status,
                    PaymentMethod = createdOrder.PaymentMethod,
                    EstimatedDeliveryTime = createdOrder.EstimatedDeliveryTime,
                    Notes = createdOrder.Notes,
                    CreatedAt = createdOrder.CreatedAt,
                    OrderItems = createdOrder.OrderItems.Select(oi => new CustomerOrderItemDto
                    {
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        MenuItemImageUrl = oi.MenuItem.ImageUrl,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        Subtotal = oi.Subtotal,
                        SpecialInstructions = oi.SpecialInstructions
                    }).ToList()
                };

                return Ok(new ApiResponse<CustomerOrderDto>
                {
                    Success = true,
                    Message = "Order placed successfully!",
                    Data = orderDto
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CustomerOrderDto>
                {
                    Success = false,
                    Message = "Error placing order",
                    Error = ex.Message
                });
            }
        }

    }
}