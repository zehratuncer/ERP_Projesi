using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StationeryDataAdaptation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-1111-1111-111111111111"),
                columns: new[] { "Description", "Quantity" },
                values: new object[] { "Okul açılış sezonu toptan A4 kağıt girişi", 100 });

            migrationBuilder.UpdateData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-2222-2222-2222-222222222222"),
                columns: new[] { "Description", "Quantity" },
                values: new object[] { "Atatürk Anadolu Lisesi kurumsal dönem başı sipariş sevkiyatı", 50 });

            migrationBuilder.InsertData(
                table: "InventoryTransactions",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "ProductId", "Quantity", "TransactionDate", "TransactionType", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { new Guid("cccccccc-3333-3333-3333-333333333333"), new DateTime(2026, 1, 4, 11, 15, 0, 0, DateTimeKind.Utc), "Kurumsal ofis sınav & test kalemi teslimatı", false, new Guid("bbbbbbbb-2222-2222-2222-222222222222"), 15, new DateTime(2026, 1, 4, 11, 15, 0, 0, DateTimeKind.Utc), 2, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
                    { new Guid("cccccccc-4444-4444-4444-444444444444"), new DateTime(2026, 1, 5, 16, 0, 0, 0, DateTimeKind.Utc), "Depoda ambalajı hasar gören defter paketi düzeltmesi", false, new Guid("bbbbbbbb-3333-3333-3333-333333333333"), 2, new DateTime(2026, 1, 5, 16, 0, 0, 0, DateTimeKind.Utc), 3, null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                columns: new[] { "Code", "Description", "Name", "SupplierId", "Unit", "UnitPrice" },
                values: new object[] { "KRT-001", "Yüksek beyazlıkta 80gr 2500 yaprak lazer/inkjet fotokopi kağıdı", "Copier Bond A4 80gr Fotokopi Kağıdı (5'li Koli)", new Guid("dddddddd-2222-2222-2222-222222222222"), "Koli", 780.00m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-2222-2222-2222-222222222222"),
                columns: new[] { "Code", "CurrentStock", "Description", "MinStockLevel", "Name", "SupplierId", "Unit", "UnitPrice" },
                values: new object[] { "KRT-042", 5, "Özel SV yapıştırma kırılmaya dirençli sınav ve çizim kalemi", 20, "Faber-Castell 2B Sınav Kurşun Kalem (72'li Kutu)", new Guid("dddddddd-1111-1111-1111-111111111111"), "Kutu", 360.00m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-3333-3333-3333-333333333333"),
                columns: new[] { "Code", "Description", "Name", "Unit", "UnitPrice" },
                values: new object[] { "KRT-089", "Sert kapak, mikroperforeli kaliteli 1. hamur kağıt okul ve ofis defteri", "Gıpta Spiralli A4 Çizgili Defter 96 Yaprak (10'lu Paket)", "Paket", 290.00m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-4444-4444-4444-444444444444"),
                columns: new[] { "Code", "CurrentStock", "Description", "Name", "Unit", "UnitPrice" },
                values: new object[] { "KRT-114", 18, "Solventsiz, yıkanabilir ve kokusuz güçlü kırtasiye yapıştırıcı standı", "Pritt Stick Kuru Yapıştırıcı 43gr (24'lü Stand)", "Stand", 950.00m });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Code", "CreatedDate", "CurrentStock", "Description", "IsActive", "IsDeleted", "MinStockLevel", "Name", "SupplierId", "Unit", "UnitPrice", "UpdatedDate" },
                values: new object[] { new Guid("bbbbbbbb-5555-5555-5555-555555555555"), "KRT-205", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "Maksimum 25 sayfa kapasiteli metal iç mekanizmalı masaüstü zımba", true, false, 10, "Maped Ağır Büro Zımba Makinesi No:24/6", new Guid("dddddddd-1111-1111-1111-111111111111"), "Adet", 175.00m, null });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-1111-1111-111111111111"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "Saray Mah. Site Yolu Cad. No:5, Ümraniye/İstanbul", "siparis@adel.com.tr", "Adel Kalemcilik & Kırtasiye A.Ş.", "+90 (216) 555 20 20", "0080012345" });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-2222-2222-2222-222222222222"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "İkitelli OSB, Kağıtçılar Sanayi Sitesi 3. Cadde No:14, Başakşehir/İstanbul", "satis@kopierkagit.com", "Kopier A4 Kağıt & Ambalaj Sanayi Ltd.", "+90 (212) 641 10 30", "5840987654" });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-3333-3333-3333-333333333333"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "1. Organize Sanayi Bölgesi Dağıstan Cad. No:7, Sincan/Ankara", "info@gipta.com.tr", "Gıpta Ofis & Okul Kırtasiye Ürünleri A.Ş.", "+90 (312) 888 40 50", "4110456789" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-5555-5555-5555-555555555555"));

            migrationBuilder.UpdateData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-1111-1111-111111111111"),
                columns: new[] { "Description", "Quantity" },
                values: new object[] { "Açılış stok girişi", 50 });

            migrationBuilder.UpdateData(
                table: "InventoryTransactions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-2222-2222-2222-222222222222"),
                columns: new[] { "Description", "Quantity" },
                values: new object[] { "Montaj hattına sevk", 42 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                columns: new[] { "Code", "Description", "Name", "SupplierId", "Unit", "UnitPrice" },
                values: new object[] { "PRD-001", "Yüksek dayanımlı paslanmaz çelik bağlantı elemanı", "M4 Çelik Civata (100 lük Paket)", new Guid("dddddddd-1111-1111-1111-111111111111"), "Paket", 120.50m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-2222-2222-2222-222222222222"),
                columns: new[] { "Code", "CurrentStock", "Description", "MinStockLevel", "Name", "SupplierId", "Unit", "UnitPrice" },
                values: new object[] { "PRD-042", 2, "Endüstriyel pres ve tezgahlar için ISO VG 46 yağ", 5, "Hidrolik Yağ (20L Varil)", new Guid("dddddddd-2222-2222-2222-222222222222"), "Varil", 2450.00m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-3333-3333-3333-333333333333"),
                columns: new[] { "Code", "Description", "Name", "Unit", "UnitPrice" },
                values: new object[] { "PRD-089", "Çift tarafı kauçuk kapaklı bilyalı sabit rulman", "Endüstriyel Rulman 6204-2RS", "Adet", 85.00m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-4444-4444-4444-444444444444"),
                columns: new[] { "Code", "CurrentStock", "Description", "Name", "Unit", "UnitPrice" },
                values: new object[] { "PRD-105", 45, "Üç fazlı sincap kafesli asenkron motor", "Elektrik Motoru 1.5kW 1400d/d", "Adet", 4200.00m });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-1111-1111-111111111111"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "İkitelli OSB, Metal İş Sanayi Sitesi 12. Blok No:45, Başakşehir/İstanbul", "siparis@alfacivata.com", "Alfa Civata & Bağlantı Elemanları San. Tic. Ltd. Şti.", "+90 (212) 555 10 20", "1234567890" });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-2222-2222-2222-222222222222"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "Gebze Organize Sanayi Bölgesi 1000. Sokak No:12, Gebze/Kocaeli", "info@petrokimya.com.tr", "PetroKimya Endüstriyel Yağlar A.Ş.", "+90 (262) 641 33 44", "9876543210" });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-3333-3333-3333-333333333333"),
                columns: new[] { "Address", "Email", "Name", "Phone", "TaxNumber" },
                values: new object[] { "Dudullu OSB DES Sanayi Sitesi 105. Sokak No:8, Ümraniye/İstanbul", "satis@rulmantek.com", "RulmanTek Makine ve Güç Aktarım Ltd.", "+90 (216) 444 88 99", "4567891230" });
        }
    }
}
