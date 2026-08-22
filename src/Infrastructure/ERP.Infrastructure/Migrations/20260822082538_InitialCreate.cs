using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Code", "CreatedDate", "CurrentStock", "Description", "IsActive", "IsDeleted", "MinStockLevel", "Name", "SupplierId", "Unit", "UnitPrice", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-1111-1111-1111-111111111111"), "PRD-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Yüksek dayanımlı paslanmaz çelik bağlantı elemanı", true, false, 25, "M4 Çelik Civata (100 lük Paket)", null, "Paket", 120.50m, null },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), "PRD-042", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Endüstriyel pres ve tezgahlar için ISO VG 46 yağ", true, false, 5, "Hidrolik Yağ (20L Varil)", null, "Varil", 2450.00m, null },
                    { new Guid("bbbbbbbb-3333-3333-3333-333333333333"), "PRD-089", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "Çift tarafı kauçuk kapaklı bilyalı sabit rulman", true, false, 30, "Endüstriyel Rulman 6204-2RS", null, "Adet", 85.00m, null },
                    { new Guid("bbbbbbbb-4444-4444-4444-444444444444"), "PRD-105", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "Üç fazlı sincap kafesli asenkron motor", true, false, 10, "Elektrik Motoru 1.5kW 1400d/d", null, "Adet", 4200.00m, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tam yetkili sistem yöneticisi", false, "Admin", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Departman ve süreç yöneticisi", false, "Manager", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standart sistem personeli", false, "Employee", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "RoleId", "UpdatedDate" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@erp.com", "Zehra Tuncer (Sistem Yöneticisi)", true, false, "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W", new Guid("11111111-1111-1111-1111-111111111111"), null });

            migrationBuilder.InsertData(
                table: "InventoryTransactions",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "ProductId", "Quantity", "TransactionDate", "TransactionType", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("cccccccc-1111-1111-1111-111111111111"), new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), "Açılış stok girişi", false, new Guid("bbbbbbbb-1111-1111-1111-111111111111"), 50, new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("cccccccc-2222-2222-2222-222222222222"), new DateTime(2026, 1, 3, 14, 30, 0, 0, DateTimeKind.Utc), "Montaj hattına sevk", false, new Guid("bbbbbbbb-1111-1111-1111-111111111111"), 42, new DateTime(2026, 1, 3, 14, 30, 0, 0, DateTimeKind.Utc), 2, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_UserId",
                table: "InventoryTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
