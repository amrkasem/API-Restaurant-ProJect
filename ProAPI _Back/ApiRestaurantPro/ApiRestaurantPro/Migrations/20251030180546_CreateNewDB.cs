using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiRestaurantPro.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityRole<string>",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole<string>", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalEstimatedPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    PreparationTime = table.Column<int>(type: "int", nullable: false),
                    DailyOrderCount = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WishlistId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    DesiredQuantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Wishlists_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Wishlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "389a19b5-c8d6-4ca5-975e-5a91880aa60a", null, "Subscriber", "SUBSCRIBER" },
                    { "9b8820f0-06db-453e-b239-fb957b1bced8", null, "Visitor", "VISITOR" },
                    { "a0f09c4e-9f85-478b-afd4-24a16858a18c", null, "Customer", "CUSTOMER" },
                    { "d5ea1f20-90eb-4410-ba24-bc6dcbfe435f", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "ConcurrencyStamp", "CreatedAt", "CreatedBy", "Email", "EmailConfirmed", "ImageUrl", "IsDeleted", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[,]
                {
                    { "1", 0, "Head Office - Sohag", "78c5bd1d-5648-4c72-9da3-0ebdd0cc75d2", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "admin@restaurant.com", true, "/images/users/admin.jpg", false, false, null, "ADMIN@RESTAURANT.COM", "ADMIN@RESTAURANT.COM", "AQAAAAIAAYagAAAAEDEgzaXD0X+n03zYh/xTi2NTHBsU4YsS6FRYvIKRhxE9mlpgdT5MkDY1rfPOhoxm7g==", "01000000000", false, "9dbb0cde-0411-404f-8f8e-ffae57e810b1", false, null, null, "admin@restaurant.com" },
                    { "2", 0, "Cairo, Egypt", "2d85ac18-c822-433c-9321-ca25a87cabbb", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "visitor@restaurant.com", true, "/images/users/visitor.jpg", false, false, null, "VISITOR@RESTAURANT.COM", "VISITOR@RESTAURANT.COM", "AQAAAAIAAYagAAAAEL7FXYl+3nVTHB1LtN4z5+sa695nQvtN+uLgGmGxTd+V98laRKR8U7SsJeOl3nCOpQ==", "01011111111", false, "5459fa2c-bddc-4f92-8dea-6fbb02dc0b26", false, null, null, "visitor@restaurant.com" },
                    { "3", 0, "Sohag, Egypt", "e1c698f2-5c59-4d6d-889e-5aa600ca52a0", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "subscriber@restaurant.com", true, "/images/users/subscriber.jpg", false, false, null, "SUBSCRIBER@RESTAURANT.COM", "SUBSCRIBER@RESTAURANT.COM", "AQAAAAIAAYagAAAAEJlcbwV42ADBheVX+Wawu9FHDX+ZvSY/j+Yhxuor65OPb7wFyRBni1UwsjqhsskzUQ==", "01022222222", false, "a977dfd3-83a4-49eb-a83f-54b68d648476", false, null, null, "subscriber@restaurant.com" },
                    { "4", 0, "Alexandria, Egypt", "7700d0b4-6668-4240-8d40-18a74e6bb238", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "customer@restaurant.com", true, "/images/users/customer.jpg", false, false, null, "CUSTOMER@RESTAURANT.COM", "CUSTOMER@RESTAURANT.COM", "AQAAAAIAAYagAAAAEOQlxQA5cOGDsaC36sAQtf2rpttgYSRlJ1YYBI6QEIW+R8nbxPCP5ODzJ31GgZzdfw==", "01033333333", false, "5e37977c-5c10-4651-b0df-a01a6def6143", false, null, null, "customer@restaurant.com" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Start your meal with our delicious appetizers and salads", "/images/categories/catFood.jpeg", true, false, "Appetizers", null, null },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Our signature main dishes prepared fresh daily", "/images/categories/catFood.jpeg", true, false, "Main Courses", null, null },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Hot and cold drinks to complement your meal", "/images/categories/userImage.jpeg", true, false, "Beverages", null, null },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Sweet treats to end your meal perfectly", "/images/categories/userImage.jpeg", true, false, "Desserts", null, null }
                });

            migrationBuilder.InsertData(
                table: "IdentityRole<string>",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "role-admin", null, "Admin", "ADMIN" },
                    { "role-customer", null, "Customer", "CUSTOMER" },
                    { "role-subscriber", null, "Subscriber", "SUBSCRIBER" },
                    { "role-visitor", null, "Visitor", "VISITOR" }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "CreatedBy", "DailyOrderCount", "Description", "ImageUrl", "IsAvailable", "IsDeleted", "Name", "PreparationTime", "Price", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Traditional Greek salad with feta cheese, olives, and fresh vegetables", "/images/menu/MenuItem.jpeg", true, false, "Greek Salad", 10, 25.00m, null, null },
                    { 2, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Creamy tomato soup served hot with garlic bread", "/images/menu/MenuItem.jpeg", true, false, "Tomato Soup", 15, 18.00m, null, null },
                    { 3, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Classic Caesar salad with crispy croutons and parmesan", "/images/menu/MenuItem.jpeg", true, false, "Caesar Salad", 10, 22.00m, null, null },
                    { 4, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Premium grilled beef steak with seasonal vegetables", "/images/menu/MenuItem.jpeg", true, false, "Beef Steak", 30, 85.00m, null, null },
                    { 5, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Marinated grilled chicken breast served with rice", "/images/menu/MenuItem.jpeg", true, false, "Grilled Chicken", 25, 55.00m, null, null },
                    { 6, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Fresh Atlantic salmon with lemon butter sauce", "/images/menu/MenuItem.jpeg", true, false, "Salmon Fillet", 20, 75.00m, null, null },
                    { 7, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Creamy pasta with bacon and parmesan cheese", "/images/menu/MenuItem.jpeg", true, false, "Pasta Carbonara", 18, 45.00m, null, null },
                    { 8, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "100% natural fresh orange juice", "/images/menu/MenuItem.jpeg", true, false, "Fresh Orange Juice", 5, 12.00m, null, null },
                    { 9, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Traditional Turkish coffee", "/images/menu/MenuItem.jpeg", true, false, "Turkish Coffee", 8, 8.00m, null, null },
                    { 10, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Cold espresso with milk and ice", "/images/menu/MenuItem.jpeg", true, false, "Iced Latte", 5, 15.00m, null, null },
                    { 11, 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Classic New York cheesecake with strawberry topping", "/images/menu/MenuItem.jpeg", true, false, "Cheesecake", 10, 28.00m, null, null },
                    { 12, 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Warm chocolate cake with molten center", "/images/menu/MenuItem.jpeg", true, false, "Chocolate Lava Cake", 15, 32.00m, null, null },
                    { 13, 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "System", 0, "Italian classic with coffee-soaked ladyfingers", "/images/menu/MenuItem.jpeg", true, false, "Tiramisu", 10, 30.00m, null, null }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CustomerName", "DeliveryAddress", "Discount", "EstimatedDeliveryTime", "IsDeleted", "Notes", "OrderType", "PaymentMethod", "PhoneNumber", "Status", "Subtotal", "Tax", "Total", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", "Subscriber User", "Sohag, Egypt", 0.00m, null, false, null, 1, 1, "01022222222", 1, 150.00m, 0.00m, 150.00m, null, null, "3" },
                    { 2, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", "Subscriber User", "Sohag, Egypt", 0.00m, null, false, null, 1, 2, "01022222222", 4, 85.00m, 0.00m, 85.00m, null, null, "3" },
                    { 3, new DateTime(2025, 1, 14, 22, 0, 0, 0, DateTimeKind.Utc), "Customer", "Subscriber User", "Sohag, Egypt", 0.00m, null, false, null, 1, 1, "01022222222", 2, 202.00m, 0.00m, 202.00m, null, null, "3" },
                    { 4, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", "Customer User", "Alexandria, Egypt", 0.00m, null, false, null, 1, 1, "01033333333", 5, 110.00m, 0.00m, 110.00m, null, null, "4" },
                    { 5, new DateTime(2025, 1, 14, 23, 0, 0, 0, DateTimeKind.Utc), "Customer", "Customer User", "Alexandria, Egypt", 0.00m, null, false, null, 1, 2, "01033333333", 3, 170.00m, 0.00m, 170.00m, null, null, "4" }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "MenuItemId", "OrderId", "Price", "Quantity", "SpecialInstructions", "Subtotal", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 4, 1, 0m, 1, null, 85.00m, null, null },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 1, 1, 0m, 2, null, 50.00m, null, null },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 10, 1, 0m, 1, null, 15.00m, null, null },
                    { 4, new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 4, 2, 0m, 1, null, 85.00m, null, null },
                    { 5, new DateTime(2025, 1, 14, 22, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 6, 3, 0m, 2, null, 150.00m, null, null },
                    { 6, new DateTime(2025, 1, 14, 22, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 11, 3, 0m, 1, null, 28.00m, null, null },
                    { 7, new DateTime(2025, 1, 14, 22, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 8, 3, 0m, 2, null, 24.00m, null, null },
                    { 8, new DateTime(2025, 1, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 5, 4, 0m, 2, null, 110.00m, null, null },
                    { 9, new DateTime(2025, 1, 14, 23, 0, 0, 0, DateTimeKind.Utc), "Customer", false, 4, 5, 0m, 2, null, 170.00m, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_MenuItemId",
                table: "CartItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_MenuItemId",
                table: "WishlistItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId",
                table: "WishlistItems",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId",
                table: "Wishlists",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "IdentityRole<string>");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Wishlists");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
