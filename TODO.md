# 📋 ERP Projesi — Geliştirme Yol Haritası & TODO Listesi

Bu belge, projenin **MVP (v1)** sürümünü adım adım, modüler ve profesyonel bir yazılım geliştirme disiplini ile hayata geçirmek için hazırlanmıştır.

---

## 🎯 MVP (v1) Hedefi
Kullanıcı girişi, rol bazlı yetkilendirme, ürün/stok hareketleri, tedarikçi yönetimi ve özet dashboard modüllerini içeren stabil, test edilebilir ve çalışan bir temel ERP sürümü oluşturmak.

---

## 📅 Faz 1: Proje Kurulumu & Temel Altyapı (Setup & Foundation)

### 1.1. Çözüm (Solution) & Proje Yapısı
- [x] **.NET Solution Başlatma:** Clean Architecture yapısına uygun katmanlı .NET Solution oluşturulması.
  - `ERP.Domain` (Class Library - Sıfır dış bağımlılık)
  - `ERP.Application` (Class Library - MediatR, FluentValidation)
  - `ERP.Infrastructure` (Class Library - EF Core, SQL Server, Identity)
  - `ERP.API` (Web API - ASP.NET Core Controllers & Swagger)
- [x] **Temel Kütüphanelerin Yüklenmesi:**
  - MediatR, FluentValidation
  - Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools
  - Microsoft.AspNetCore.Authentication.JwtBearer, BCrypt.Net-Next
  - Microsoft.EntityFrameworkCore.Design, Swashbuckle.AspNetCore
- [x] **Ortak Tipler ve Base Modeller:**
  - `BaseEntity` (Id, CreatedDate, UpdatedDate, IsDeleted)
  - `ApiResponse<T>` (Standart API yanıt şablonu: Success, Data, Message, Errors)
  - Global Exception Handling Middleware & Custom Exceptions

### 1.2. Frontend (Angular) Başlatma
- [x] **Angular Projesinin Kurulması:**
  - Standalone component yapısı ve modern dizin mimarisi (`core/`, `shared/`, `features/`, `layouts/`).
  - Routing yapısı ve Route Guard'ların iskeleti (`auth.guard`, `role.guard`).
- [x] **Tasarım Sistemi & UI Kütüphanesi:**
  - Ortak renk paleti, tipografi, buton ve form stilleri (CSS/SCSS).
  - Toastr / Bildirim & Modal servislerinin kurulması (`ToastService`, `ToastContainerComponent`).
  - Responsive ana Layout (Sidebar, Navbar, Content Area, User Profile).

---

## 🔐 Faz 2: Kullanıcı & Kimlik Doğrulama (Auth & IAM)

### 2.1. Backend (Identity & Auth)
- [x] **Domain & Entity Tasarımı:**
  - `User` entity (Id, Email, PasswordHash, FullName, RoleId, Role, IsActive)
  - `Role` entity (`Admin`, `Manager`, `Employee`)
- [x] **Veritabanı Konfigürasyonu:**
  - EF Core `ApplicationDbContext` & `IdentityConfigurations` (Unique Email Index, Fluent API).
  - Seed Data: Başlangıç Admin kullanıcısı (`admin@erp.com` / `Admin123!`) ve temel roller (`Admin`, `Manager`, `Employee`).
- [x] **Application & CQRS:**
  - `LoginCommand` & `LoginCommandHandler` (BCrypt şifre doğrulaması + JWT Token üretimi).
  - `GetCurrentUserQuery` & Handler (`/api/auth/me`).
  - `LoginCommandValidator` (FluentValidation kuralları).
- [x] **API Endpoint'leri:**
  - `POST /api/auth/login`
  - `GET /api/auth/me` (`[Authorize]` ile korunan profil sorgusu).

### 2.2. Frontend (Auth Modülü)
- [x] **Login Sayfası:**
  - Modern, şık ve responsive login formu (Email, Şifre, Beni Hatırla, Hızlı Doldur).
  - Form validasyonları, hata uyarı kutusu ve yükleniyor animasyonu (Spinner).
- [x] **Auth State & Interceptor:**
  - `AuthService` (Gerçek backend `http://localhost:5000/api/auth/login` ve `/api/auth/me` entegrasyonu, `currentUser` sinyali).
  - `JwtInterceptor` (Tüm giden HTTP isteklerine `Authorization: Bearer <token>` ekleme).
  - `ErrorInterceptor` (401/403 ve 500 durumlarında otomatik Toast uyarısı ve Login'e yönlendirme).

---

## 📦 Faz 3: Stok & Ürün Yönetimi (Inventory Module)

### 3.1. Backend (Ürün & Stok)
- [x] **Domain & Entity Tasarımı:**
  - `Product` entity (Code, Name, Description, Unit, MinStockLevel, CurrentStock, UnitPrice, SupplierId, IsActive)
  - `InventoryTransaction` entity (ProductId, Quantity, TransactionType [In/Out/Adjustment], Description, TransactionDate, UserId)
  - `TransactionType` enum (In, Out, Adjustment).
- [x] **Application & CQRS (Ürün İşlemleri):**
  - `CreateProductCommand` & Validator (Benzersiz ürün kodu kontrolü).
  - `UpdateProductCommand` & `DeleteProductCommand` (Soft-delete).
  - `GetProductsQuery` (Arama, filtreleme, listeleme).
  - `GetProductByIdQuery`.
  - `GetLowStockProductsQuery` (Mevcut stok <= MinStockLevel olan kritik ürünler).
- [x] **Application & CQRS (Stok Hareketleri):**
  - `CreateStockMovementCommand` (Giriş / Çıkış):
    - Stok çıkışında yeterli bakiye kontrolü (`product.CurrentStock < request.Quantity` durumunda hata).
    - `Product.CurrentStock` alanının atomik olarak güncellenmesi.
  - `GetStockMovementsQuery` (Ürün bazlı veya genel son hareketler).
- [x] **API Endpoint'leri:**
  - `GET /api/products`, `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`
  - `GET /api/products/low-stock`
  - `POST /api/inventory/movement` (Stok Giriş/Çıkış)
  - `GET /api/inventory/movements`

### 3.2. Frontend (Ürün & Stok Arayüzü)
- [x] **Ürün Listesi Sayfası:**
  - Dinamik tablo (Ürün Kodu, Ürün Adı, Açıklama, Birim, Birim Fiyat, Mevcut Stok, Kritik Eşik, Durum).
  - Canlı arama kutusu ve "Tümü / Kritik Stok" sekmeleri.
  - Kritik stok seviyesinin altındaki ürünler için görsel kırmızı badge ve satır vurgusu.
- [x] **Ürün Ekleme & Düzenleme Modalı/Formu:**
  - Reaktif modal formu, birim seçimi, fiyat/stok doğrulamaları ve soft-delete silme aksiyonu.
- [x] **Stok Giriş/Çıkış Hızlı Aksiyon Modalı:**
  - Giriş / Çıkış / Sayım Düzeltmesi radyo butonları.
  - Canlı yeni stok önizlemesi ve yetersiz bakiye hata uyarısı.
  - Tüm stok hareket geçmişi listeleme penceresi (Audit drawer/modal).

---

## 🚚 Faz 4: Tedarikçi Yönetimi (Supplier Module)

### 4.1. Backend (Tedarikçi)
- [x] **Domain & Entity Tasarımı:**
  - `Supplier` entity (Name, ContactPerson, Email, Phone, Address, TaxNumber, IsActive)
- [x] **Application & CQRS:**
  - `CreateSupplierCommand` & Validator.
  - `UpdateSupplierCommand` & `DeleteSupplierCommand`.
  - `GetSuppliersQuery` (Arama & filtreleme).
  - `GetSupplierByIdQuery` & `GetSupplierProductsQuery` (Seçili tedarikçinin sağladığı ürünler).
- [x] **API Endpoint'leri:**
  - `GET /api/suppliers`, `POST /api/suppliers`, `PUT /api/suppliers/{id}`, `DELETE /api/suppliers/{id}`
  - `GET /api/suppliers/{id}/products`

### 4.2. Frontend (Tedarikçi Arayüzü)
- [x] **Tedarikçi Listesi:**
  - Firma adı, iletişim kişisi, telefon, e-posta ve aktif ürün sayısı.
- [x] **Tedarikçi Ekle / Güncelle Formu:**
  - Form validasyonları ve telefon/vergi no maskelemesi.
- [x] **Tedarikçi Detay & Ürün Listesi Görünümü:**
  - Tedarikçiye ait kayıtlı ürünlerin listelenmesi ve yeni ürün ilişkilendirme.

---

## 📊 Faz 5: Yönetici Özeti (Dashboard Module)

### 5.1. Backend (Dashboard KPI API)
- [x] **Application & CQRS:**
  - `GetDashboardSummaryQuery`:
    - Toplam Ürün Sayısı (`TotalProductsCount`)
    - Kritik Stoktaki Ürün Sayısı (`CriticalStockCount`)
    - Toplam Tedarikçi Sayısı (`TotalSuppliersCount`)
    - Toplam Envanter Adedi & Değeri (`TotalInventoryQuantity`, `TotalInventoryValue`)
    - Son 10 Stok Hareketi (`RecentStockMovements`)
    - Kritik Stok Uyarı Listesi (`CriticalStockAlerts`)
- [x] **API Endpoint:**
  - `GET /api/dashboard/summary`

### 5.2. Frontend (Dashboard Görünümü)
- [x] **KPI Sayaç Kartları (Stat Cards):**
  - Toplam Ürün, Kritik Stok Uyarısı, Tedarikçi Sayısı, Toplam Stok Adedi ve Parasal Değer.
- [x] **Kritik Stok Uyarı Tablosu:**
  - Acil sipariş verilmesi gereken ürünlerin hızlı görünümü ve tedarikçi bağlantıları.
- [x] **Son Hareketler Zaman Çizelgesi (Recent Activity Stream):**
  - Kim, hangi üründen ne kadar girdi/çıkardı akışı (Giriş / Çıkış / Düzeltme renk kodlarıyla).

---

## 🧪 Faz 6: Test, İnce Ayar & MVP Sürüm Doğrulaması

- [ ] **Birim Testleri (Unit Tests):**
  - Stok çıkışında yetersiz bakiye iş kuralı testi.
  - Kritik stok hesaplama mantığı testi.
  - JWT üretim ve rol eşleme doğrulaması.
- [ ] **Uçtan Uca Doğrulama (E2E Workflow Test):**
  - Admin ile giriş yap ➔ Tedarikçi oluştur ➔ Ürün tanımla ➔ Stok girişi yap ➔ Stok çıkışı yaparak kritik eşiğe düşür ➔ Dashboard'da uyarının belirdiğini teyit et.
- [ ] **Veritabanı Migration & Seed:**
  - Temiz kurulum için `dotnet ef database update` ve test mock verilerinin doğrulanması.

---

## 📌 Geliştirme Standartları & Notlar
* **Git Commit Kuralı:** `feat(auth): add jwt login command`, `fix(inventory): prevent negative stock on exit`
* **Clean Code:** Controller'lar zayıf (thin), handler'lar bağımsız olmalıdır.
* **Hata Yönetimi:** Tüm hatalar standart `ApiResponse` nesnesi ile dönmelidir.
