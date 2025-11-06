// UnitWork/UnitOfWork.cs
using ApiRestaurantPro.Context;
using ApiRestaurantPro.Repository.Admin;

namespace ApiRestaurantPro.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _context;
        private IAdminCategoryRepository? _adminCategories;
        private IAdminProductRepository? _adminProducts;
        private IAdminOrderRepository? _adminOrders;
        private IAdminWishlistRepository? _adminWishlists;
        private IAdminUserRepository? _adminUsers;

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
        }

        public IAdminCategoryRepository AdminCategories
        {
            get
            {
                _adminCategories ??= new AdminCategoryRepository(_context);
                return _adminCategories;
            }
        }

        public IAdminProductRepository AdminProducts
        {
            get
            {
                _adminProducts ??= new AdminProductRepository(_context);
                return _adminProducts;
            }
        }

        public IAdminOrderRepository AdminOrders
        {
            get
            {
                _adminOrders ??= new AdminOrderRepository(_context);
                return _adminOrders;
            }
        }

        public IAdminWishlistRepository AdminWishlists
        {
            get
            {
                _adminWishlists ??= new AdminWishlistRepository(_context);
                return _adminWishlists;
            }
        }

        public IAdminUserRepository AdminUsers
        {
            get
            {
                _adminUsers ??= new AdminUserRepository(_context);
                return _adminUsers;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}