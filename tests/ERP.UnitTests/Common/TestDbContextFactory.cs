using System;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.UnitTests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext(string? dbName = null, bool seedMockData = true)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        if (seedMockData)
        {
            SeedTestMockData(context);
        }

        return context;
    }

    private static void SeedTestMockData(ApplicationDbContext context)
    {
        var adminRole = new Role
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = Roles.Admin,
            Description = "Tam yetkili sistem yöneticisi"
        };
        var managerRole = new Role
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = Roles.Manager,
            Description = "Departman yöneticisi"
        };
        var employeeRole = new Role
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = Roles.Employee,
            Description = "Standart personel"
        };

        context.Roles.AddRange(adminRole, managerRole, employeeRole);

        var adminUser = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = "admin@erp.com",
            FullName = "Zehra Tuncer (Sistem Yöneticisi)",
            PasswordHash = "$2a$11$q9o94O6k3Jb9vG6M2dYVn.6F1Z5x6i0q3pQ8nF5g8y8J6m5g8rK2W",
            RoleId = adminRole.Id,
            IsActive = true
        };
        context.Users.Add(adminUser);

        var supplier1 = new Supplier
        {
            Id = Guid.Parse("dddddddd-1111-1111-1111-111111111111"),
            Name = "Adel Kalemcilik & Kırtasiye A.Ş.",
            ContactPerson = "Mehmet Yılmaz",
            Email = "siparis@adel.com.tr",
            Phone = "+90 (216) 555 20 20",
            IsActive = true
        };
        var supplier2 = new Supplier
        {
            Id = Guid.Parse("dddddddd-2222-2222-2222-222222222222"),
            Name = "Kopier A4 Kağıt & Ambalaj Ltd.",
            ContactPerson = "Ayşe Demir",
            Email = "satis@kopierkagit.com",
            Phone = "+90 (212) 641 10 30",
            IsActive = true
        };
        context.Suppliers.AddRange(supplier1, supplier2);

        var p1 = new Product
        {
            Id = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"),
            Code = "KRT-001",
            Name = "Copier Bond A4 80gr Fotokopi Kağıdı",
            Unit = "Koli",
            CurrentStock = 50,
            MinStockLevel = 10,
            UnitPrice = 780.00m,
            SupplierId = supplier2.Id,
            IsActive = true
        };
        var p2 = new Product
        {
            Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
            Code = "KRT-042",
            Name = "Faber-Castell 2B Sınav Kurşun Kalem",
            Unit = "Kutu",
            CurrentStock = 40,
            MinStockLevel = 10,
            UnitPrice = 360.00m,
            SupplierId = supplier1.Id,
            IsActive = true
        };
        var p3 = new Product
        {
            Id = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333"),
            Code = "KRT-089",
            Name = "Gıpta Spiralli A4 Defter",
            Unit = "Paket",
            CurrentStock = 100,
            MinStockLevel = 20,
            UnitPrice = 290.00m,
            SupplierId = supplier1.Id,
            IsActive = true
        };
        context.Products.AddRange(p1, p2, p3);

        context.SaveChanges();
    }
}
