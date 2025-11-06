// Repository/Admin/AdminOrderRepository.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.DTOs;
using ApiRestaurantPro.Models;
using ApiRestaurantPro.Models.ENUMS;
using Microsoft.EntityFrameworkCore;

namespace ApiRestaurantPro.Repository.Admin
{
    public interface IAdminOrderRepository : IGenericRepository<Order>
    {
        Task<List<OrderResponseDto>> GetAllOrdersWithDetailsAsync();
        Task<OrderResponseDto?> GetOrderWithDetailsAsync(int id);
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes);
        Task SoftDeleteOrderWithItemsAsync(int orderId, string deletedBy);
        Task<List<OrderResponseDto>> GetOrdersByUserIdAsync(string userId);
        Task<List<OrderResponseDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetOrdersCountByStatusAsync(OrderStatus status);
    }

    public class AdminOrderRepository : GenericRepository<Order>, IAdminOrderRepository
    {
        public AdminOrderRepository(MyDbContext context) : base(context)
        {
        }

        public async Task<List<OrderResponseDto>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.Orders
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
        }

        public async Task<OrderResponseDto?> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.Id == id && !o.IsDeleted)
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
                .FirstOrDefaultAsync();
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // Add status change note
            var statusNote = $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Status changed from {oldStatus} to {newStatus}";
            if (!string.IsNullOrEmpty(notes))
            {
                statusNote += $": {notes}";
            }

            order.Notes += statusNote;

            _context.Orders.Update(order);
        }

        public async Task SoftDeleteOrderWithItemsAsync(int orderId, string deletedBy)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found");
            }

            // Soft delete order
            order.IsDeleted = true;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = deletedBy;

            // Soft delete order items
            foreach (var orderItem in order.OrderItems.Where(oi => !oi.IsDeleted))
            {
                orderItem.IsDeleted = true;
                orderItem.UpdatedAt = DateTime.UtcNow;
                orderItem.UpdatedBy = deletedBy;
            }

            _context.Orders.Update(order);
        }

        public async Task<List<OrderResponseDto>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId && !o.IsDeleted)
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
        }

        public async Task<List<OrderResponseDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.Status == status && !o.IsDeleted)
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
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.Total);
        }

        public async Task<int> GetOrdersCountByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .CountAsync(o => o.Status == status && !o.IsDeleted);
        }
    }
}