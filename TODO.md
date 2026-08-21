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
- [ ] **Domain & Entity Tasarımı:**
  - `User` entity (Id, Email, PasswordHash, Salt, FullName, RoleId, IsActive)
  - `Role` entity (`Admin`, `Manager`, `Employee`)
- [ ] **Veritabanı Konfigürasyonu:**
  - EF Core EntityTypeConfiguration (Index, Unique Email).
  - Seed Data: Başlangıç Admin kullanıcısı ve temel roller.
- [ ] **Application & CQRS:**
  - `LoginCommand` & `LoginCommandHandler` (BCrypt şifre doğrulaması + JWT Token üretimi).
  - `RefreshTokenCommand` (Opsiyonel / Session sürekliliği için).
  - `LoginCommandValidator` (Email formatı, zorunlu alanlar).
- [ ] **API Endpoint'leri:**
  - `POST /api/auth/login`
  - `GET /api/auth/me` (Giriş yapan kullanıcının profil ve rol bilgisi).
  - `[Authorize(Roles = "Admin")]` özniteliklerinin test edilmesi.

### 2.2. Frontend (Auth Modülü)
- [ ] **Login Sayfası:**
  - Modern, şık ve responsive login formu (Email, Şifre, Beni Hatırla).
  - Form validasyonları ve hata mesajları.
- [ ] **Auth State & Interceptor:**
  - `AuthService` (Token saklama, `currentUser$` sinyali/observable).
  - `JwtInterceptor` (Tüm giden isteklere `Authorization: Bearer <token>` ekleme).
  - `ErrorInterceptor` (401/403 durumunda login'e yönlendirme).

---

## 📦 Faz 3: Stok & Ürün Yönetimi (Inventory Module)

### 3.1. Backend (Ürün & Stok)
- [ ] **Domain & Entity Tasarımı:**
  - `Product` entity (Code, Name, Description, Unit, MinStockLevel, CurrentStock, UnitPrice, SupplierId, IsActive)
  - `InventoryTransaction` entity (ProductId, Quantity, TransactionType [In/Out/Adjustment], Description, TransactionDate, UserId)
- [ ] **Application & CQRS (Ürün İşlemleri):**
  - `CreateProductCommand` & Validator (Benzersiz ürün kodu kontrolü).
  - `UpdateProductCommand` & `DeleteProductCommand` (Soft-delete).
  - `GetProductsQuery` (Sayfalama, arama, filtreleme).
  - `GetProductByIdQuery`.
  - `GetLowStockProductsQuery` (Mevcut stok <= MinStockLevel olan ürünler).
- [ ] **Application & CQRS (Stok Hareketleri):**
  - `CreateStockMovementCommand` (Giriş / Çıkış):
    - Stok çıkışında yeterli miktar kontrolü (Yetersiz stok hatası fırlatma).
    - `Product.CurrentStock` alanının atomik olarak güncellenmesi.
  - `GetStockMovementsQuery` (Ürün bazlı veya genel son hareketler).
- [ ] **API Endpoint'leri:**
  - `GET /api/products`, `POST /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`
  - `GET /api/products/low-stock`
  - `POST /api/inventory/movement` (Stok Giriş/Çıkış)
  - `GET /api/inventory/movements`

### 3.2. Frontend (Ürün & Stok Arayüzü)
- [ ] **Ürün Listesi Sayfası:**
  - Dinamik tablo (Ürün Adı, Kodu, Mevcut Stok, Kritik Eşik, Birim Fiyat, Tedarikçi, Durum).
  - Arama, filtreleme ve sayfalama bileşenleri.
  - Kritik stok seviyesinin altındaki ürünler için görsel badge/uyarı (`danger/warning`).
- [ ] **Ürün Ekleme & Düzenleme Modalı/Formu:**
  - Reaktif form kontrolleri ve doğrulamalar.
- [ ] **Stok Giriş/Çıkış Hızlı Aksiyon Modalı:**
  - Miktar, hareket tipi (Giriş/Çıkış), açıklama girişi.
  - Başarılı işlem sonrası anında tablo yenileme.

---

## 🚚 Faz 4: Tedarikçi Yönetimi (Supplier Module)

### 4.1. Backend (Tedarikçi)
- [ ] **Domain & Entity Tasarımı:**
  - `Supplier` entity (Name, ContactPerson, Email, Phone, Address, TaxNumber, IsActive)
- [ ] **Application & CQRS:**
  - `CreateSupplierCommand` & Validator.
  - `UpdateSupplierCommand` & `DeleteSupplierCommand`.
  - `GetSuppliersQuery` (Arama & filtreleme).
  - `GetSupplierProductsQuery` (Seçili tedarikçinin sağladığı ürünler).
- [ ] **API Endpoint'leri:**
  - `GET /api/suppliers`, `POST /api/suppliers`, `PUT /api/suppliers/{id}`, `DELETE /api/suppliers/{id}`
  - `GET /api/suppliers/{id}/products`

### 4.2. Frontend (Tedarikçi Arayüzü)
- [ ] **Tedarikçi Listesi:**
  - Firma adı, iletişim kişisi, telefon, e-posta ve aktif ürün sayısı.
- [ ] **Tedarikçi Ekle / Güncelle Formu:**
  - Form validasyonları ve telefon/vergi no maskelemesi.
- [ ] **Tedarikçi Detay & Ürün Listesi Görünümü:**
  - Tedarikçiye ait kayıtlı ürünlerin listelenmesi ve yeni ürün ilişkilendirme.

---

## 📊 Faz 5: Yönetici Özeti (Dashboard Module)

### 5.1. Backend (Dashboard KPI API)
- [ ] **Application & CQRS:**
  - `GetDashboardSummaryQuery`:
    - Toplam Ürün Sayısı (`TotalProductsCount`)
    - Kritik Stoktaki Ürün Sayısı (`CriticalStockCount`)
    - Toplam Tedarikçi Sayısı (`TotalSuppliersCount`)
    - Son 10 Stok Hareketi (`RecentStockMovements`)
    - Kritik Stok Uyarı Listesi (`CriticalStockAlerts`)
- [ ] **API Endpoint:**
  - `GET /api/dashboard/summary`

### 5.2. Frontend (Dashboard Görünümü)
- [ ] **KPI Sayaç Kartları (Stat Cards):**
  - Toplam Ürün, Kritik Stok Uyarısı, Tedarikçi Sayısı.
- [ ] **Kritik Stok Uyarı Tablosu:**
  - Acil sipariş verilmesi gereken ürünlerin hızlı görünümü.
- [ ] **Son Hareketler Zaman Çizelgesi (Recent Activity Stream):**
  - Kim, hangi üründen ne kadar girdi/çıkardı akışı.

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
