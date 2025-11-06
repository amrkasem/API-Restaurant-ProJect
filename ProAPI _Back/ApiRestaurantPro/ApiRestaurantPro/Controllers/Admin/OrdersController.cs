// Controllers/Admin/OrdersController.cs
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.Models.ENUMS;
using ApiRestaurantPro.UnitWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiRestaurantPro.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetOrders()
        {
            try
            {
                var orders = await _unitOfWork.AdminOrders.GetAllOrdersWithDetailsAsync();

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
                var order = await _unitOfWork.AdminOrders.GetOrderWithDetailsAsync(id);

                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderResponseDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order retrieved successfully",
                    Data = order
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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetOrdersByUser(string userId)
        {
            try
            {
                var orders = await _unitOfWork.AdminOrders.GetOrdersByUserIdAsync(userId);

                return Ok(new ApiResponse<List<OrderResponseDto>>
                {
                    Success = true,
                    Message = "User orders retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving user orders",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDto>>>> GetOrdersByStatus(OrderStatus status)
        {
            try
            {
                var orders = await _unitOfWork.AdminOrders.GetOrdersByStatusAsync(status);

                return Ok(new ApiResponse<List<OrderResponseDto>>
                {
                    Success = true,
                    Message = $"Orders with status '{status}' retrieved successfully",
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<OrderResponseDto>>
                {
                    Success = false,
                    Message = "Error retrieving orders by status",
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
                
                await _unitOfWork.AdminOrders.UpdateOrderStatusAsync(id, dto.NewStatus, dto.Notes);
                await _unitOfWork.SaveChangesAsync();

                var updatedOrder = await _unitOfWork.AdminOrders.GetOrderWithDetailsAsync(id);

                return Ok(new ApiResponse<OrderResponseDto>
                {
                    Success = true,
                    Message = "Order status updated successfully",
                    Data = updatedOrder
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<OrderResponseDto>
                {
                    Success = false,
                    Message = "Order not found"
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
                await _unitOfWork.AdminOrders.SoftDeleteOrderWithItemsAsync(id, "Admin");
                await _unitOfWork.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Order deleted successfully"
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
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

        [HttpGet("statistics/revenue")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue()
        {
            try
            {
                var revenue = await _unitOfWork.AdminOrders.GetTotalRevenueAsync();

                return Ok(new ApiResponse<decimal>
                {
                    Success = true,
                    Message = "Total revenue retrieved successfully",
                    Data = revenue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<decimal>
                {
                    Success = false,
                    Message = "Error retrieving revenue",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("statistics/count/{status}")]
        public async Task<ActionResult<ApiResponse<int>>> GetOrdersCountByStatus(OrderStatus status)
        {
            try
            {
                var count = await _unitOfWork.AdminOrders.GetOrdersCountByStatusAsync(status);

                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Count of '{status}' orders retrieved successfully",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Error retrieving orders count",
                    Error = ex.Message
                });
            }
        }
    }
}