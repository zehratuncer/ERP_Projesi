using System.Text;
using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Identity;
using ERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Veritabanı Bağlantısı (SQL Server veya In-Memory fallback)
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Geliştirme aşamasında SQL Server henüz kurulu değilse In-Memory / LocalDb olarak ayağa kalkabilmesi için
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ERP_InMemory_Db"));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // 2. Identity & Güvenlik Servisleri
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // 3. Excel & PDF Dışa Aktarım Servisleri
        services.AddScoped<IExcelExportService, Services.Export.ClosedXmlExcelExportService>();
        services.AddScoped<IPdfReportService, Services.Export.QuestPdfReportService>();


        // 3. JWT Bearer Kimlik Doğrulama Yapılandırması
        var secretKey = configuration["Jwt:SecretKey"] ?? "SuperSecretKeyForERPProjectThatIsAtLeast32CharactersLong!";
        var issuer = configuration["Jwt:Issuer"] ?? "ERP_Core_API";
        var audience = configuration["Jwt:Audience"] ?? "ERP_Clients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
