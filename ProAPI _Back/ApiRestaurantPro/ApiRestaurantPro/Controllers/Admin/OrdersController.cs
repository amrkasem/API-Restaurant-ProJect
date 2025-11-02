// Controllers/Admin/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : ControllerBase
    {
        private readonly MyDbContext _context;

        public OrdersController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderResponseDto
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
                        UserId = o.UserId,
                        UserName = o.User.UserName ?? "Unknown",
                        OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                        {
                            MenuItemId = oi.MenuItemId,
                            MenuItemName = oi.MenuItem.Name,
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Subtotal = oi.Subtotal,
                            SpecialInstructions = oi.SpecialInstructions
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<OrderResponseDto>>
                {
                    Success = true,
                    Message = "Orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                var orderDto = new OrderResponseDto
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    PhoneNumber = order.PhoneNumber,
                    OrderType = order.OrderType,
                    DeliveryAddress = order.DeliveryAddress,
                    Subtotal = order.Subtotal,
                    Tax = order.Tax,
                    Discount = order.Discount,
                    Total = order.Total,
                    Status = order.Status,
                    PaymentMethod = order.PaymentMethod,
                    EstimatedDeliveryTime = order.EstimatedDeliveryTime,
                    Notes = order.Notes,
                    CreatedAt = order.CreatedAt,
                    UserId = order.UserId,
                    UserName = order.User.UserName ?? "Unknown",
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        Subtotal = oi.Subtotal,
                        SpecialInstructions = oi.SpecialInstructions
                    }).ToList()
                };

                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order retrieved successfully",
                    Data = orderDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Error retrieving order",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Invalid data",
                        Error = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var order = await _context.Orders.FindAsync(id);
                if (order == null || order.IsDeleted)
                {
                    return NotFound(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                var oldStatus = order.Status;
                order.Status = dto.NewStatus;
                order.UpdatedAt = DateTime.UtcNow;

                // Add status change note
                var statusNote = $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Status changed from {oldStatus} to {dto.NewStatus}";
                if (!string.IsNullOrEmpty(dto.Notes))
                {
                    statusNote += $": {dto.Notes}";
                }

                order.Notes += statusNote;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Get updated order with related data
                var updatedOrder = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == id);

                var orderDto = new OrderResponseDto
                {
                    Id = updatedOrder!.Id,
                    CustomerName = updatedOrder.CustomerName,
                    PhoneNumber = updatedOrder.PhoneNumber,
                    OrderType = updatedOrder.OrderType,
                    DeliveryAddress = updatedOrder.DeliveryAddress,
                    Subtotal = updatedOrder.Subtotal,
                    Tax = updatedOrder.Tax,
                    Discount = updatedOrder.Discount,
                    Total = updatedOrder.Total,
                    Status = updatedOrder.Status,
                    PaymentMethod = updatedOrder.PaymentMethod,
                    EstimatedDeliveryTime = updatedOrder.EstimatedDeliveryTime,
                    Notes = updatedOrder.Notes,
                    CreatedAt = updatedOrder.CreatedAt,
                    UserId = updatedOrder.UserId,
                    UserName = updatedOrder.User?.UserName ?? "Unknown",
                    OrderItems = updatedOrder.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        MenuItemId = oi.MenuItemId,
                        MenuItemName = oi.MenuItem.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        Subtotal = oi.Subtotal,
                        SpecialInstructions = oi.SpecialInstructions
                    }).ToList()
                };

                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    Data = orderDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Error updating order status",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (order == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                // Soft delete order
                order.IsDeleted = true;
                order.UpdatedAt = DateTime.UtcNow;
                order.UpdatedBy = "Admin";

                // Soft delete order items
                foreach (var orderItem in order.OrderItems.Where(oi => !oi.IsDeleted))
                {
                    orderItem.IsDeleted = true;
                    orderItem.UpdatedAt = DateTime.UtcNow;
                    orderItem.UpdatedBy = "Admin";
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Order deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting order",
                    Error = ex.Message
                });
            }
        }
    }
}