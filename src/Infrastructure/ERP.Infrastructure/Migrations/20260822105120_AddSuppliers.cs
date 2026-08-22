using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuppliers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                column: "SupplierId",
                value: new Guid("dddddddd-1111-1111-1111-111111111111"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-2222-2222-2222-222222222222"),
                column: "SupplierId",
                value: new Guid("dddddddd-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-3333-3333-3333-333333333333"),
                column: "SupplierId",
                value: new Guid("dddddddd-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-4444-4444-4444-444444444444"),
                column: "SupplierId",
                value: new Guid("dddddddd-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "Address", "ContactPerson", "CreatedDate", "Email", "IsActive", "IsDeleted", "Name", "Phone", "TaxNumber", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("dddddddd-1111-1111-1111-111111111111"), "İkitelli OSB, Metal İş Sanayi Sitesi 12. Blok No:45, Başakşehir/İstanbul", "Mehmet Yılmaz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "siparis@alfacivata.com", true, false, "Alfa Civata & Bağlantı Elemanları San. Tic. Ltd. Şti.", "+90 (212) 555 10 20", "1234567890", null },
                    { new Guid("dddddddd-2222-2222-2222-222222222222"), "Gebze Organize Sanayi Bölgesi 1000. Sokak No:12, Gebze/Kocaeli", "Ayşe Demir", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@petrokimya.com.tr", true, false, "PetroKimya Endüstriyel Yağlar A.Ş.", "+90 (262) 641 33 44", "9876543210", null },
                    { new Guid("dddddddd-3333-3333-3333-333333333333"), "Dudullu OSB DES Sanayi Sitesi 105. Sokak No:8, Ümraniye/İstanbul", "Kemal Kaya", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "satis@rulmantek.com", true, false, "RulmanTek Makine ve Güç Aktarım Ltd.", "+90 (216) 444 88 99", "4567891230", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_SupplierId",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                column: "SupplierId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-2222-2222-2222-222222222222"),
                column: "SupplierId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-3333-3333-3333-333333333333"),
                column: "SupplierId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-4444-4444-4444-444444444444"),
                column: "SupplierId",
                value: null);
        }
    }
}
