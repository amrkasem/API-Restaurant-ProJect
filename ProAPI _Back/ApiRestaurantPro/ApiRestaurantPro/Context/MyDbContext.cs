using ApiRestaurantPro.Models;
using ApiRestaurantPro.Models.ENUMS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ApiRestaurantPro.Context
{
    public class MyDbContext : IdentityDbContext<ApplicationUser>
    {
        // ========== DbSets ==========
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        // ========== Constructors ==========
        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options)
           : base(options)
        {
        }

        // ========== Configuration ==========
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Server=DESKTOP-UHG43DP\\SQLEXPRESS;Database=APro_API3010_Db1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            // ========== ENTITY CONFIGURATIONS ==========

            // CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.HasMany(c => c.MenuItems)
                      .WithOne(m => m.Category)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MENU ITEM
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasQueryFilter(m => !m.IsDeleted);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.HasOne(m => m.Category)
                      .WithMany(c => c.MenuItems)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasQueryFilter(o => !o.IsDeleted);
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);
                entity.Property(e => e.Tax).HasPrecision(10, 2);
                entity.Property(e => e.Discount).HasPrecision(10, 2);
                entity.Property(e => e.Total).HasPrecision(10, 2);
                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ORDER ITEM
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasQueryFilter(oi => !oi.IsDeleted);
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);
                entity.HasOne(oi => oi.MenuItem)
                      .WithMany(m => m.OrderItems)
                      .HasForeignKey(oi => oi.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CART
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasQueryFilter(c => !c.IsDeleted);
                entity.Property(e => e.Total).HasPrecision(10, 2);
            });

            // CART ITEM
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasQueryFilter(ci => !ci.IsDeleted);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.HasOne(ci => ci.MenuItem)
                      .WithMany()
                      .HasForeignKey(ci => ci.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // WISHLIST
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasQueryFilter(w => !w.IsDeleted);
                entity.Property(e => e.TotalEstimatedPrice).HasPrecision(10, 2);
            });

            // WISHLIST ITEM
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasQueryFilter(wi => !wi.IsDeleted);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.HasOne(wi => wi.MenuItem)
                      .WithMany(m => m.WishlistItems)
                      .HasForeignKey(wi => wi.MenuItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========== RELATIONSHIPS (using ApplicationUser) ==========
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Wishlist)
                .WithOne(w => w.User)
                .HasForeignKey<Wishlist>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wishlist>()
                .HasMany(w => w.WishlistItems)
                .WithOne(wi => wi.Wishlist)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== SEED DATA ==========
            SeedData(modelBuilder, seedDate);
        }

        // ========== SEED DATA METHOD ==========
        private void SeedData(ModelBuilder modelBuilder, DateTime seedDate)
        {
            // ========== SEED ROLES ==========
            var roles = new List<IdentityRole<string>>
    {
        new IdentityRole<string> { Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN" },
        new IdentityRole<string> { Id = "role-visitor", Name = "Visitor", NormalizedName = "VISITOR" },
        new IdentityRole<string> { Id = "role-subscriber", Name = "Subscriber", NormalizedName = "SUBSCRIBER" },
        new IdentityRole<string> { Id = "role-customer", Name = "Customer", NormalizedName = "CUSTOMER" }
    };
            modelBuilder.Entity<IdentityRole<string>>().HasData(roles);

            // ========== SEED USERS ==========
            var hasher = new PasswordHasher<ApplicationUser>();
            var users = new List<ApplicationUser>
    {
        // Admin User
        new ApplicationUser
        {
            Id = "1",
            UserName = "admin@restaurant.com",
            NormalizedUserName = "ADMIN@RESTAURANT.COM",
            Email = "admin@restaurant.com",
            NormalizedEmail = "ADMIN@RESTAURANT.COM",
            PhoneNumber = "01000000000",
            Address = "Head Office - Sohag",
            ImageUrl = "/images/users/admin.jpg",
            CreatedAt = seedDate,
            CreatedBy = "System",
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(null, "Admin@123")
        },
        // Visitor User
        new ApplicationUser
        {
            Id = "2",
            UserName = "visitor@restaurant.com",
            NormalizedUserName = "VISITOR@RESTAURANT.COM",
            Email = "visitor@restaurant.com",
            NormalizedEmail = "VISITOR@RESTAURANT.COM",
            PhoneNumber = "01011111111",
            Address = "Cairo, Egypt",
            ImageUrl = "/images/users/visitor.jpg",
            CreatedAt = seedDate,
            CreatedBy = "System",
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(null, "Visitor@123")
        },
        // Subscriber User
        new ApplicationUser
        {
            Id = "3",
            UserName = "subscriber@restaurant.com",
            NormalizedUserName = "SUBSCRIBER@RESTAURANT.COM",
            Email = "subscriber@restaurant.com",
            NormalizedEmail = "SUBSCRIBER@RESTAURANT.COM",
            PhoneNumber = "01022222222",
            Address = "Sohag, Egypt",
            ImageUrl = "/images/users/subscriber.jpg",
            CreatedAt = seedDate,
            CreatedBy = "System",
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(null, "Subscriber@123")
        },
        // Customer User
        new ApplicationUser
        {
            Id = "4",
            UserName = "customer@restaurant.com",
            NormalizedUserName = "CUSTOMER@RESTAURANT.COM",
            Email = "customer@restaurant.com",
            NormalizedEmail = "CUSTOMER@RESTAURANT.COM",
            PhoneNumber = "01033333333",
            Address = "Alexandria, Egypt",
            ImageUrl = "/images/users/customer.jpg",
            CreatedAt = seedDate,
            CreatedBy = "System",
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(null, "Customer@123")
        }
    };
            modelBuilder.Entity<ApplicationUser>().HasData(users);
            // ========== SEED ROLES ==========
            var userRoles = new List<IdentityRole> // ✅ بدون <string>
{
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Visitor", NormalizedName = "VISITOR" },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Subscriber", NormalizedName = "SUBSCRIBER" },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Customer", NormalizedName = "CUSTOMER" }
            };
            modelBuilder.Entity<IdentityRole>().HasData(userRoles); // ✅ بدون <string>

            // ========== SEED CATEGORIES ==========
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Appetizers",
                    Description = "Start your meal with our delicious appetizers and salads",
                    ImageUrl = "/images/categories/catFood.jpeg",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "System"
                },
                new Category
                {
                    Id = 2,
                    Name = "Main Courses",
                    Description = "Our signature main dishes prepared fresh daily",
                    ImageUrl = "/images/categories/catFood.jpeg",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "System"
                },
                new Category
                {
                    Id = 3,
                    Name = "Beverages",
                    Description = "Hot and cold drinks to complement your meal",
                    ImageUrl = "/images/categories/userImage.jpeg",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "System"
                },
                new Category
                {
                    Id = 4,
                    Name = "Desserts",
                    Description = "Sweet treats to end your meal perfectly",
                    ImageUrl = "/images/categories/userImage.jpeg",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "System"
                }
            );

            // ========== SEED MENU ITEMS ==========
            modelBuilder.Entity<MenuItem>().HasData(
                // Appetizers
                new MenuItem { Id = 1, Name = "Greek Salad", Description = "Traditional Greek salad with feta cheese, olives, and fresh vegetables", Price = 25.00m, CategoryId = 1, PreparationTime = 10, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 2, Name = "Tomato Soup", Description = "Creamy tomato soup served hot with garlic bread", Price = 18.00m, CategoryId = 1, PreparationTime = 15, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 3, Name = "Caesar Salad", Description = "Classic Caesar salad with crispy croutons and parmesan", Price = 22.00m, CategoryId = 1, PreparationTime = 10, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },

                // Main Courses
                new MenuItem { Id = 4, Name = "Beef Steak", Description = "Premium grilled beef steak with seasonal vegetables", Price = 85.00m, CategoryId = 2, PreparationTime = 30, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 5, Name = "Grilled Chicken", Description = "Marinated grilled chicken breast served with rice", Price = 55.00m, CategoryId = 2, PreparationTime = 25, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 6, Name = "Salmon Fillet", Description = "Fresh Atlantic salmon with lemon butter sauce", Price = 75.00m, CategoryId = 2, PreparationTime = 20, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 7, Name = "Pasta Carbonara", Description = "Creamy pasta with bacon and parmesan cheese", Price = 45.00m, CategoryId = 2, PreparationTime = 18, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },

                // Beverages
                new MenuItem { Id = 8, Name = "Fresh Orange Juice", Description = "100% natural fresh orange juice", Price = 12.00m, CategoryId = 3, PreparationTime = 5, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 9, Name = "Turkish Coffee", Description = "Traditional Turkish coffee", Price = 8.00m, CategoryId = 3, PreparationTime = 8, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 10, Name = "Iced Latte", Description = "Cold espresso with milk and ice", Price = 15.00m, CategoryId = 3, PreparationTime = 5, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },

                // Desserts
                new MenuItem { Id = 11, Name = "Cheesecake", Description = "Classic New York cheesecake with strawberry topping", Price = 28.00m, CategoryId = 4, PreparationTime = 10, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 12, Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with molten center", Price = 32.00m, CategoryId = 4, PreparationTime = 15, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" },
                new MenuItem { Id = 13, Name = "Tiramisu", Description = "Italian classic with coffee-soaked ladyfingers", Price = 30.00m, CategoryId = 4, PreparationTime = 10, ImageUrl = "/images/menu/MenuItem.jpeg", IsAvailable = true, CreatedAt = seedDate, CreatedBy = "System" }
            );

            // ========== SEED ORDERS (Using only User IDs 3 & 4) ==========
            modelBuilder.Entity<Order>().HasData(
                // Subscriber Orders (UserId = 3)
                new Order { Id = 1, UserId = "3", CustomerName = "Subscriber User", Subtotal = 150.00m, Tax = 0.00m, Discount = 0.00m, Total = 150.00m, Status = OrderStatus.Pending, DeliveryAddress = "Sohag, Egypt", PhoneNumber = "01022222222", PaymentMethod = PaymentMethod.Cash, CreatedAt = seedDate, CreatedBy = "Customer" },
                new Order { Id = 2, UserId = "3", CustomerName = "Subscriber User", Subtotal = 85.00m, Tax = 0.00m, Discount = 0.00m, Total = 85.00m, Status = OrderStatus.Delivered, DeliveryAddress = "Sohag, Egypt", PhoneNumber = "01022222222", PaymentMethod = PaymentMethod.CreditCard, CreatedAt = seedDate.AddDays(-5), CreatedBy = "Customer" },
                new Order { Id = 3, UserId = "3", CustomerName = "Subscriber User", Subtotal = 202.00m, Tax = 0.00m, Discount = 0.00m, Total = 202.00m, Status = OrderStatus.Preparing, DeliveryAddress = "Sohag, Egypt", PhoneNumber = "01022222222", PaymentMethod = PaymentMethod.Cash, CreatedAt = seedDate.AddHours(-2), CreatedBy = "Customer" },

                // Customer Orders (UserId = 4)
                new Order { Id = 4, UserId = "4", CustomerName = "Customer User", Subtotal = 110.00m, Tax = 0.00m, Discount = 0.00m, Total = 110.00m, Status = OrderStatus.Canceled, DeliveryAddress = "Alexandria, Egypt", PhoneNumber = "01033333333", PaymentMethod = PaymentMethod.Cash, CreatedAt = seedDate.AddDays(-3), CreatedBy = "Customer" },
                new Order { Id = 5, UserId = "4", CustomerName = "Customer User", Subtotal = 170.00m, Tax = 0.00m, Discount = 0.00m, Total = 170.00m, Status = OrderStatus.Ready, DeliveryAddress = "Alexandria, Egypt", PhoneNumber = "01033333333", PaymentMethod = PaymentMethod.CreditCard, CreatedAt = seedDate.AddHours(-1), CreatedBy = "Customer" }
            );

            // ========== SEED ORDER ITEMS ==========
            modelBuilder.Entity<OrderItem>().HasData(
                // Order 1 Items - Total: 150
                new OrderItem { Id = 1, OrderId = 1, MenuItemId = 4, Quantity = 1, Subtotal = 85.00m, CreatedAt = seedDate, CreatedBy = "Customer" },
                new OrderItem { Id = 2, OrderId = 1, MenuItemId = 1, Quantity = 2, Subtotal = 50.00m, CreatedAt = seedDate, CreatedBy = "Customer" },
                new OrderItem { Id = 3, OrderId = 1, MenuItemId = 10, Quantity = 1, Subtotal = 15.00m, CreatedAt = seedDate, CreatedBy = "Customer" },

                // Order 2 Items - Total: 85
                new OrderItem { Id = 4, OrderId = 2, MenuItemId = 4, Quantity = 1, Subtotal = 85.00m, CreatedAt = seedDate.AddDays(-5), CreatedBy = "Customer" },

                // Order 3 Items - Total: 202
                new OrderItem { Id = 5, OrderId = 3, MenuItemId = 6, Quantity = 2, Subtotal = 150.00m, CreatedAt = seedDate.AddHours(-2), CreatedBy = "Customer" },
                new OrderItem { Id = 6, OrderId = 3, MenuItemId = 11, Quantity = 1, Subtotal = 28.00m, CreatedAt = seedDate.AddHours(-2), CreatedBy = "Customer" },
                new OrderItem { Id = 7, OrderId = 3, MenuItemId = 8, Quantity = 2, Subtotal = 24.00m, CreatedAt = seedDate.AddHours(-2), CreatedBy = "Customer" },

                // Order 4 Items - Total: 110
                new OrderItem { Id = 8, OrderId = 4, MenuItemId = 5, Quantity = 2, Subtotal = 110.00m, CreatedAt = seedDate.AddDays(-3), CreatedBy = "Customer" },

                // Order 5 Items - Total: 170
                new OrderItem { Id = 9, OrderId = 5, MenuItemId = 4, Quantity = 2, Subtotal = 170.00m, CreatedAt = seedDate.AddHours(-1), CreatedBy = "Customer" }
            );
        }

        // ========== AUTO TRACKING (Timestamps) ==========
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = null;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
