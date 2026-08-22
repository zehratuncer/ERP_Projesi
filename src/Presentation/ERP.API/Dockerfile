# 1. Build Stage (.NET 8 SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala ve restore et
COPY ["src/Core/ERP.Domain/ERP.Domain.csproj", "src/Core/ERP.Domain/"]
COPY ["src/Core/ERP.Application/ERP.Application.csproj", "src/Core/ERP.Application/"]
COPY ["src/Infrastructure/ERP.Infrastructure/ERP.Infrastructure.csproj", "src/Infrastructure/ERP.Infrastructure/"]
COPY ["src/Presentation/ERP.API/ERP.API.csproj", "src/Presentation/ERP.API/"]

RUN dotnet restore "src/Presentation/ERP.API/ERP.API.csproj"

# Tüm kaynak kodları kopyala ve derle
COPY . .
WORKDIR "/app/src/Presentation/ERP.API"
RUN dotnet publish "ERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage (.NET 8 ASP.NET Core Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "ERP.API.dll"]
