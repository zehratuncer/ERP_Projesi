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
                name: "ApprovalWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
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
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
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
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalWorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ApprovalWorkflows_ApprovalWorkflowId",
                        column: x => x.ApprovalWorkflowId,
                        principalTable: "ApprovalWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalEstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CurrentApprovalStep = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Users_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CashierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Users_CashierUserId",
                        column: x => x.CashierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateTable(
                name: "ApprovalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_Users_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstimatedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedTotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ApprovalWorkflows",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "IsDeleted", "MaxAmount", "MinAmount", "Name", "UpdatedDate" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999991"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Limit bazlı kademeli kırtasiye satın alma onay iş akışı (10.000 TL altı Şube Müdürü, üzeri Genel Müdür/Direktör).", true, false, null, 0m, "Standart Kırtasiye Onay Akışı", null });

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
                table: "Suppliers",
                columns: new[] { "Id", "Address", "ContactPerson", "CreatedDate", "Email", "IsActive", "IsDeleted", "Name", "Phone", "TaxNumber", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("dddddddd-1111-1111-1111-111111111111"), "Saray Mah. Site Yolu Cad. No:5, Ümraniye/İstanbul", "Mehmet Yılmaz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "siparis@adel.com.tr", true, false, "Adel Kalemcilik & Kırtasiye A.Ş.", "+90 (216) 555 20 20", "0080012345", null },
                    { new Guid("dddddddd-2222-2222-2222-222222222222"), "İkitelli OSB, Kağıtçılar Sanayi Sitesi 3. Cadde No:14, Başakşehir/İstanbul", "Ayşe Demir", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "satis@kopierkagit.com", true, false, "Kopier A4 Kağıt & Ambalaj Sanayi Ltd.", "+90 (212) 641 10 30", "5840987654", null },
                    { new Guid("dddddddd-3333-3333-3333-333333333333"), "1. Organize Sanayi Bölgesi Dağıstan Cad. No:7, Sincan/Ankara", "Kemal Kaya", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@gipta.com.tr", true, false, "Gıpta Ofis & Okul Kırtasiye Ürünleri A.Ş.", "+90 (312) 888 40 50", "4110456789", null },
                    { new Guid("dddddddd-4444-4444-4444-444444444444"), "Dudullu OSB Baraj Yolu No:28, Ümraniye/İstanbul", "Canan Öztürk", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tedarik@fabercastell.com.tr", true, false, "Faber-Castell & Daksil Dağıtım A.Ş.", "+90 (216) 420 80 90", "3201847192", null },
                    { new Guid("dddddddd-5555-5555-5555-555555555555"), "Kemankeş Karamustafapaşa Mah. Rıhtım Cad. No:19, Beyoğlu/İstanbul", "Serdar Aksoy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "iletisim@mapedturkiye.com", true, false, "Maped & Büro Araçları Tic. Ltd. Şti.", "+90 (212) 243 70 80", "1892049182", null }
                });

            migrationBuilder.InsertData(
                table: "ApprovalSteps",
                columns: new[] { "Id", "ApprovalWorkflowId", "CreatedDate", "IsDeleted", "IsRequired", "MaxAmount", "MinAmount", "RoleId", "StepName", "StepNumber", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-999999999992"), new Guid("99999999-9999-9999-9999-999999999991"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, 10000m, 0m, new Guid("22222222-2222-2222-2222-222222222222"), "Birim / Şube Müdürü Onayı", 1, null, null },
                    { new Guid("99999999-9999-9999-9999-999999999993"), new Guid("99999999-9999-9999-9999-999999999991"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, null, 10000.01m, new Guid("11111111-1111-1111-1111-111111111111"), "Genel Satın Alma Direktörü Onayı", 2, null, null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Code", "CreatedDate", "CurrentStock", "Description", "IsActive", "IsDeleted", "MinStockLevel", "Name", "SupplierId", "Unit", "UnitPrice", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-1111-1111-1111-111111111111"), "KRT-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Yüksek beyazlıkta 80gr 2500 yaprak lazer/inkjet fotokopi kağıdı", true, false, 25, "Copier Bond A4 80gr Fotokopi Kağıdı (5'li Koli)", new Guid("dddddddd-2222-2222-2222-222222222222"), "Koli", 780.00m, null },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), "KRT-042", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Özel SV yapıştırma kırılmaya dirençli sınav ve çizim kalemi", true, false, 20, "Faber-Castell 2B Sınav Kurşun Kalem (72'li Kutu)", new Guid("dddddddd-1111-1111-1111-111111111111"), "Kutu", 360.00m, null },
                    { new Guid("bbbbbbbb-3333-3333-3333-333333333333"), "KRT-089", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "Sert kapak, mikroperforeli kaliteli 1. hamur kağıt okul ve ofis defteri", true, false, 30, "Gıpta Spiralli A4 Çizgili Defter 96 Yaprak (10'lu Paket)", new Guid("dddddddd-3333-3333-3333-333333333333"), "Paket", 290.00m, null },
                    { new Guid("bbbbbbbb-4444-4444-4444-444444444444"), "KRT-114", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "Solventsiz, yıkanabilir ve kokusuz güçlü kırtasiye yapıştırıcı standı", true, false, 10, "Pritt Stick Kuru Yapıştırıcı 43gr (24'lü Stand)", new Guid("dddddddd-4444-4444-4444-444444444444"), "Stand", 950.00m, null },
                    { new Guid("bbbbbbbb-5555-5555-5555-555555555555"), "KRT-205", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "Maksimum 25 sayfa kapasiteli metal iç mekanizmalı masaüstü zımba", true, false, 10, "Maped Ağır Büro Zımba Makinesi No:24/6", new Guid("dddddddd-5555-5555-5555-555555555555"), "Adet", 175.00m, null },
                    { new Guid("bbbbbbbb-6666-6666-6666-666666666666"), "KRT-301", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "Canlı ve parlak renkler, yüksek örtücülük, çocuklara özel toksik olmayan formül", true, false, 15, "Faber-Castell 24'lü Suluboya & Fırça Seti", new Guid("dddddddd-1111-1111-1111-111111111111"), "Set", 210.00m, null },
                    { new Guid("bbbbbbbb-7777-7777-7777-777777777777"), "KRT-401", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "Ortopedik sırt desteği, su geçirmez kumaş ve çok bölmeli geniş hacim", true, false, 8, "Yaygan Lisanslı Ergonomik Okul Sırt Çantası", new Guid("dddddddd-3333-3333-3333-333333333333"), "Adet", 850.00m, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Email", "FullName", "IsActive", "IsDeleted", "PasswordHash", "RoleId", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@erp.com", "Zehra Tuncer (Sistem Yöneticisi)", true, false, "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W", new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "manager@erp.com", "Ahmet Yılmaz (Kırtasiye & Şube Müdürü)", true, false, "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W", new Guid("22222222-2222-2222-2222-222222222222"), null },
                    { new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "kasiyer@erp.com", "Elif Kaya (Kasa & Satış Personeli)", true, false, "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W", new Guid("33333333-3333-3333-3333-333333333333"), null }
                });

            migrationBuilder.InsertData(
                table: "InventoryTransactions",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "ProductId", "Quantity", "TransactionDate", "TransactionType", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("cccccccc-1111-1111-1111-111111111111"), new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), "Okul açılış sezonu toptan A4 fotokopi kağıdı mal kabul girişi", false, new Guid("bbbbbbbb-1111-1111-1111-111111111111"), 100, new DateTime(2026, 1, 2, 10, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("cccccccc-2222-2222-2222-222222222222"), new DateTime(2026, 1, 3, 14, 30, 0, 0, DateTimeKind.Utc), "Atatürk Anadolu Lisesi kurumsal dönem başı sipariş sevkiyatı", false, new Guid("bbbbbbbb-1111-1111-1111-111111111111"), 50, new DateTime(2026, 1, 3, 14, 30, 0, 0, DateTimeKind.Utc), 2, null, new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
                    { new Guid("cccccccc-3333-3333-3333-333333333333"), new DateTime(2026, 1, 4, 11, 15, 0, 0, DateTimeKind.Utc), "Kurumsal ofis sınav & test kalemi teslimatı", false, new Guid("bbbbbbbb-2222-2222-2222-222222222222"), 15, new DateTime(2026, 1, 4, 11, 15, 0, 0, DateTimeKind.Utc), 2, null, new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc") },
                    { new Guid("cccccccc-4444-4444-4444-444444444444"), new DateTime(2026, 1, 5, 16, 0, 0, 0, DateTimeKind.Utc), "Depoda ambalajı hasar gören defter paketi düzeltmesi", false, new Guid("bbbbbbbb-3333-3333-3333-333333333333"), 2, new DateTime(2026, 1, 5, 16, 0, 0, 0, DateTimeKind.Utc), 3, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "ActionUrl", "CreatedDate", "IsDeleted", "Message", "RoleName", "Title", "Type", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-1111-1111-1111-111111111111"), "/inventory", new DateTime(2026, 8, 27, 8, 0, 0, 0, DateTimeKind.Utc), false, "[KRT-001] Copier Bond A4 Kağıt stok miktarı (8 Koli) kritik eşik seviyesinin (25 Koli) altına düştü!", "Admin", "Kritik Stok Uyarısı", 3, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("44444444-2222-2222-2222-222222222222"), "/purchasing", new DateTime(2026, 8, 27, 8, 30, 0, 0, DateTimeKind.Utc), false, "TALEP-20260827-001 numaralı ve ₺14.500,00 tutarındaki satın alma talebi Genel Satın Alma Direktörü onayınızı beklemektedir.", "Admin", "Onayınızı Bekleyen Satın Alma Talebi", 4, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("44444444-3333-3333-3333-333333333333"), "/inventory", new DateTime(2026, 8, 27, 8, 15, 0, 0, DateTimeKind.Utc), false, "[KRT-042] Faber-Castell 2B Sınav Kurşun Kalem stok miktarı (5 Kutu) kritik eşik seviyesinin (20 Kutu) altına düştü!", "Manager", "Kritik Stok Uyarısı", 3, null, new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "ActionUrl", "CreatedDate", "IsDeleted", "IsRead", "Message", "RoleName", "Title", "Type", "UpdatedDate", "UserId" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), "/purchasing", new DateTime(2026, 8, 26, 15, 30, 0, 0, DateTimeKind.Utc), false, true, "TALEP-20260826-002 numaralı satın alma talebiniz Şube Müdürü Ahmet Yılmaz tarafından onaylanmıştır.", "Employee", "Talep Onaylandı", 1, null, new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc") });

            migrationBuilder.InsertData(
                table: "PurchaseRequests",
                columns: new[] { "Id", "CreatedDate", "CurrentApprovalStep", "Department", "IsDeleted", "Note", "Priority", "RequestNumber", "RequesterUserId", "RequiredDate", "Status", "TotalEstimatedAmount", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("88888888-1111-1111-1111-111111111111"), new DateTime(2026, 8, 27, 8, 30, 0, 0, DateTimeKind.Utc), 2, "Merkez Mağaza Satış & Depo", false, "Kritik stok seviyesine düşen A4 fotokopi kağıdı ve kurşun kalemler için acil tedarik talebi.", 2, "TALEP-20260827-001", new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), null, 2, 14500.00m, null },
                    { new Guid("88888888-2222-2222-2222-222222222222"), new DateTime(2026, 8, 26, 14, 0, 0, 0, DateTimeKind.Utc), 1, "Okul & Kurumsal Satış", false, "Gıpta spiralli defter stok takviyesi (Onaylandı - Mal kabul yapılabilir).", 2, "TALEP-20260826-002", new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), null, 3, 5800.00m, null },
                    { new Guid("88888888-3333-3333-3333-333333333333"), new DateTime(2026, 8, 25, 11, 20, 0, 0, DateTimeKind.Utc), 2, "Yönetim & İdari İşler", false, "Lüks dolmakalem ve özel masaüstü deri setleri talebi.", 2, "TALEP-20260825-003", new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), null, 4, 25500.00m, null },
                    { new Guid("88888888-4444-4444-4444-444444444444"), new DateTime(2026, 8, 24, 9, 15, 0, 0, DateTimeKind.Utc), 1, "Merkez Mağaza Satış & Depo", false, "Pritt yapıştırıcı ve zımba makinesi dönem başı siparişi (Depoya teslim alındı).", 2, "TALEP-20260824-004", new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), null, 5, 8750.00m, null }
                });

            migrationBuilder.InsertData(
                table: "Sales",
                columns: new[] { "Id", "CashierUserId", "CreatedDate", "CustomerName", "DiscountAmount", "FinalAmount", "IsDeleted", "PaymentMethod", "ReceiptNumber", "SaleDate", "TotalAmount", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("77777777-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), new DateTime(2026, 8, 27, 9, 15, 0, 0, DateTimeKind.Utc), "Mehmet Demir (Perakende Müşteri)", 0.00m, 940.00m, false, 1, "FIS-20260827-001", new DateTime(2026, 8, 27, 9, 15, 0, 0, DateTimeKind.Utc), 940.00m, null },
                    { new Guid("77777777-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-cccc-cccc-cccc-cccccccccccc"), new DateTime(2026, 8, 27, 10, 30, 0, 0, DateTimeKind.Utc), "Ayşe Yılmaz (Öğrenci Velisi)", 0.00m, 1060.00m, false, 2, "FIS-20260827-002", new DateTime(2026, 8, 27, 10, 30, 0, 0, DateTimeKind.Utc), 1060.00m, null },
                    { new Guid("77777777-3333-3333-3333-333333333333"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 8, 26, 16, 45, 0, 0, DateTimeKind.Utc), "Özel Bilim Koleji (Kurumsal)", 0.00m, 4250.00m, false, 1, "FIS-20260826-003", new DateTime(2026, 8, 26, 16, 45, 0, 0, DateTimeKind.Utc), 4250.00m, null }
                });

            migrationBuilder.InsertData(
                table: "ApprovalHistories",
                columns: new[] { "Id", "Action", "ActionDate", "ApproverUserId", "Comment", "CreatedDate", "IsDeleted", "PurchaseRequestId", "StepName", "StepNumber", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("55555555-1111-1111-1111-111111111111"), 1, new DateTime(2026, 8, 26, 15, 30, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Okul açılışı öncesi defter stok gereksinimi onaylanmıştır.", new DateTime(2026, 8, 26, 15, 30, 0, 0, DateTimeKind.Utc), false, new Guid("88888888-2222-2222-2222-222222222222"), "Birim / Şube Müdürü Onayı", 1, null },
                    { new Guid("55555555-2222-2222-2222-222222222222"), 2, new DateTime(2026, 8, 25, 14, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Dönemlik bütçe aşımı sebebiyle lüks kalem talebi reddedilmiştir. Gelecek çeyrekte tekrar değerlendirilecektir.", new DateTime(2026, 8, 25, 14, 0, 0, 0, DateTimeKind.Utc), false, new Guid("88888888-3333-3333-3333-333333333333"), "Genel Satın Alma Direktörü Onayı", 2, null },
                    { new Guid("55555555-3333-3333-3333-333333333333"), 1, new DateTime(2026, 8, 24, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Standart büro malzeme ihtiyacı onaylandı.", new DateTime(2026, 8, 24, 10, 0, 0, 0, DateTimeKind.Utc), false, new Guid("88888888-4444-4444-4444-444444444444"), "Birim / Şube Müdürü Onayı", 1, null }
                });

            migrationBuilder.InsertData(
                table: "PurchaseRequestItems",
                columns: new[] { "Id", "CreatedDate", "EstimatedTotalPrice", "EstimatedUnitPrice", "IsDeleted", "Notes", "ProductId", "PurchaseRequestId", "RequestedQuantity", "Unit", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("66666666-1111-1111-1111-111111111111"), new DateTime(2026, 8, 27, 8, 30, 0, 0, DateTimeKind.Utc), 11700.00m, 780.00m, false, "Fotokopi kağıdı tükenmek üzere, acil sevk gerekli", new Guid("bbbbbbbb-1111-1111-1111-111111111111"), new Guid("88888888-1111-1111-1111-111111111111"), 15, "Koli", null },
                    { new Guid("66666666-2222-2222-2222-222222222222"), new DateTime(2026, 8, 27, 8, 30, 0, 0, DateTimeKind.Utc), 2800.00m, 280.00m, false, "Sınav haftası için 2B kurşun kalem desteği", new Guid("bbbbbbbb-2222-2222-2222-222222222222"), new Guid("88888888-1111-1111-1111-111111111111"), 10, "Kutu", null },
                    { new Guid("66666666-3333-3333-3333-333333333333"), new DateTime(2026, 8, 26, 14, 0, 0, 0, DateTimeKind.Utc), 5800.00m, 290.00m, false, "Okul açılış sezonu defter takviyesi", new Guid("bbbbbbbb-3333-3333-3333-333333333333"), new Guid("88888888-2222-2222-2222-222222222222"), 20, "Paket", null },
                    { new Guid("66666666-4444-4444-4444-444444444444"), new DateTime(2026, 8, 24, 9, 15, 0, 0, DateTimeKind.Utc), 8750.00m, 175.00m, false, "Kurumsal büro zımba teslimatı yapıldı", new Guid("bbbbbbbb-5555-5555-5555-555555555555"), new Guid("88888888-4444-4444-4444-444444444444"), 50, "Adet", null }
                });

            migrationBuilder.InsertData(
                table: "SaleItems",
                columns: new[] { "Id", "CreatedDate", "DiscountRate", "IsDeleted", "ProductId", "Quantity", "SaleId", "TotalPrice", "UnitPrice", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("33333333-1111-1111-1111-111111111111"), new DateTime(2026, 8, 27, 9, 15, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-2222-2222-2222-222222222222"), 1, new Guid("77777777-1111-1111-1111-111111111111"), 360.00m, 360.00m, null },
                    { new Guid("33333333-2222-2222-2222-222222222222"), new DateTime(2026, 8, 27, 9, 15, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-3333-3333-3333-333333333333"), 2, new Guid("77777777-1111-1111-1111-111111111111"), 580.00m, 290.00m, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 8, 27, 10, 30, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-7777-7777-7777-777777777777"), 1, new Guid("77777777-2222-2222-2222-222222222222"), 850.00m, 850.00m, null },
                    { new Guid("33333333-4444-4444-4444-444444444444"), new DateTime(2026, 8, 27, 10, 30, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-6666-6666-6666-666666666666"), 1, new Guid("77777777-2222-2222-2222-222222222222"), 210.00m, 210.00m, null },
                    { new Guid("33333333-5555-5555-5555-555555555555"), new DateTime(2026, 8, 26, 16, 45, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-1111-1111-1111-111111111111"), 5, new Guid("77777777-3333-3333-3333-333333333333"), 3900.00m, 780.00m, null },
                    { new Guid("33333333-6666-6666-6666-666666666666"), new DateTime(2026, 8, 26, 16, 45, 0, 0, DateTimeKind.Utc), 0.00m, false, new Guid("bbbbbbbb-5555-5555-5555-555555555555"), 2, new Guid("77777777-3333-3333-3333-333333333333"), 350.00m, 175.00m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_ApproverUserId",
                table: "ApprovalHistories",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_PurchaseRequestId",
                table: "ApprovalHistories",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApprovalWorkflowId",
                table: "ApprovalSteps",
                column: "ApprovalWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_RoleId",
                table: "ApprovalSteps",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_UserId",
                table: "ApprovalSteps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_UserId",
                table: "InventoryTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedDate",
                table: "Notifications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_ProductId",
                table: "PurchaseRequestItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_PurchaseRequestId",
                table: "PurchaseRequestItems",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_RequesterUserId",
                table: "PurchaseRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_RequestNumber",
                table: "PurchaseRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CashierUserId",
                table: "Sales",
                column: "CashierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ReceiptNumber",
                table: "Sales",
                column: "ReceiptNumber",
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
                name: "ApprovalHistories");

            migrationBuilder.DropTable(
                name: "ApprovalSteps");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItems");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "ApprovalWorkflows");

            migrationBuilder.DropTable(
                name: "PurchaseRequests");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
