// UnitWork/IUnitOfWork.cs
using ApiRestaurantPro.Repository.Admin;

namespace ApiRestaurantPro.UnitWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAdminCategoryRepository AdminCategories { get; }
        IAdminProductRepository AdminProducts { get; }
        IAdminOrderRepository AdminOrders { get; }
        IAdminWishlistRepository AdminWishlists { get; }
        IAdminUserRepository AdminUsers { get; }

        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}