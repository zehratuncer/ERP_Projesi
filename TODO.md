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

## 🚀 v2: Kurumsal İş Süreçleri & Kırtasiye ERP Geliştirmeleri

Bu faz, kırtasiye işletmesinin kurumsal satın alma süreçlerini, sezonluk stok devir raporlarını, onay mekanizmalarını ve anlık bildirim altyapısını kurmayı amaçlar.

---

## 🛒 Faz 7: Barkodlu Hızlı Satış & Kasa (POS) Modülü

### 7.1. Backend (Satış İşlemleri & Stok Düşümü)
- [x] **Domain & Entity Tasarımı:**
  - `Sale` entity (ReceiptNumber, CashierUserId, TotalAmount, DiscountAmount, FinalAmount, PaymentMethod [Cash, CreditCard, Split, OnAccount], SaleDate, CustomerName).
  - `SaleItem` entity (SaleId, ProductId, Quantity, UnitPrice, TotalPrice, DiscountRate).
  - `PaymentMethod` enum (Cash, CreditCard, Split, OnAccount).
- [x] **Application & CQRS (Satış & Otomatik Stok Düşümü):**
  - `GetProductByBarcodeQuery` (Barkod tabancası okuttuğunda ürün koduna/barkoduna göre anında ürün detayını ve stok durumunu getirme).
  - `CompleteSaleCommand` & `CompleteSaleCommandValidator`:
    - Sepetteki tüm ürünler için atomik Transaction başlatılması.
    - Her bir kalem için yeterli stok kontrolü (`product.CurrentStock < item.Quantity` kontrolü).
    - Her ürünün `CurrentStock` adedinin satılan miktar kadar otomatik düşürülmesi.
    - Otomatik `InventoryTransaction` kaydı oluşturulması (`TransactionType: Out`, Açıklama: `Fiş No: {ReceiptNumber} Satışı`).
    - Satış sonrası ürün stoğu `MinStockLevel` kritik eşiğin altına inerse sistemde uyarı oluşturulması.
  - `GetSalesHistoryQuery` (Günlük kasa satış raporu, fiş sorgulama ve filtreleme).
- [x] **API Endpoint'leri:**
  - `GET /api/pos/product/{barcodeOrCode}` (Barkod ile anlık ürün getirme)
  - `POST /api/pos/complete-sale` (Sepeti onayla, tahsilatı yap, stoktan düş)
  - `GET /api/pos/receipt/{receiptNumber}` (Fiş/Satış detayı)
  - `GET /api/pos/daily-summary` (Günlük ciro ve kasa raporu)

### 7.2. Frontend (Kırtasiye Hızlı Kasa / POS Arayüzü)
- [x] **Barkod Odaklı Hızlı Kasa Ekranı (`PosComponent`):**
  - **Sürekli Aktif Barkod Girişi (Autofocus):** Barkod tabancası her okutmada (`Enter` ile) ürünü sepet listesine `+1` adet olarak anında ekler.
  - Aynı barkod peş peşe okutulduğunda sepet satırındaki adedi otomatik artırma (`x2`, `x3`).
  - **Klavye Kısayolları:** `F2` (Satışı Tamamla / Ödeme), `F4` (Sepeti Temizle), `+ / -` (Adet Değiştir), `Delete` (Satır Sil).
- [x] **Dinamik Sepet & Tahsilat Paneli:**
  - Anlık toplam tutar, KDV dökümü, alınan para ve para üstü hesaplama (Örn: 200 TL verildi ➔ 45 TL para üstü).
  - Ödeme yöntemi seçimi (Nakit / Kredi Kartı / Veresiye-Cari).
- [x] **Hızlı Fiş Çıktısı (Thermal Receipt):**
  - Satış bittiğinde otomatik fiş yazdırma penceresi (58mm/80mm termal fiş şablonu).

---

## 📑 Faz 8: Satın Alma Talepleri Modülü (Purchase Request Module)

### 8.1. Backend (Talep Yönetimi & CQRS)
- [x] **Domain & Entity Tasarımı:**
  - `PurchaseRequest` entity (RequestNumber, Department, RequesterUserId, Priority [Low, Medium, High, Urgent], Status [Draft, PendingApproval, Approved, Rejected, Completed], TotalEstimatedAmount, RequiredDate, Note).
  - `PurchaseRequestItem` entity (PurchaseRequestId, ProductId, RequestedQuantity, Unit, EstimatedUnitPrice, Notes).
  - `RequestPriority` ve `RequestStatus` enum tanımlamaları.
- [x] **Application & CQRS:**
  - `CreatePurchaseRequestCommand` & `CreatePurchaseRequestValidator` (En az bir kalem, pozitif miktar, departman doğrulama).
  - `UpdatePurchaseRequestCommand` (Sadece Taslak/Beklemede durumundaki talepler için).
  - `CancelPurchaseRequestCommand` (Talep sahibi veya yönetici tarafından iptal).
  - `GetPurchaseRequestsQuery` (Durum, departman, tarih aralığı, aciliyet ve talep eden bazlı gelişmiş filtreleme ve sayfalama).
  - `GetPurchaseRequestByIdQuery` (Kalemler, ürün detayları ve onay geçmişi ile birlikte).
- [x] **API Endpoint'leri:**
  - `POST /api/purchase-requests`
  - `GET /api/purchase-requests`
  - `GET /api/purchase-requests/{id}`
  - `PUT /api/purchase-requests/{id}`
  - `DELETE /api/purchase-requests/{id}/cancel`

### 8.2. Frontend (Kırtasiye Satın Alma Talep Arayüzü)
- [x] **Talep Listesi & Filtreleme:**
  - Talep numarası, departman, ürün çeşit sayısı, tahmini tutar, aciliyet (Renkli badge: Okul Sezonu Acil, Rutin Ofis İhtiyacı) ve onay durumu tablosu.
- [x] **Dinamik Çok Satırlı Talep Oluşturma Formu:**
  - Kırtasiye ürün arama (Defter, Kağıt, Kalem Grubu vb.), miktar/paket seçimi, birim fiyat ve canlı genel toplam hesaplama.
  - Hedef teslim tarihi ve departman bütçe kodu seçimi.
- [x] **Talep Detay & Süreç Takip Ekranı:**
  - Talep kalemleri, birim/miktar bilgisi ve anlık onay durum ilerleme çubuğu (Stepper).

---

## ✅ Faz 9: Çok Kademeli Onay Sistemi & İş Akışı (Multi-Level Approval Workflow)

### 9.1. Backend (Onay Motoru & Kuralları)
- [x] **Domain & Entity Tasarımı:**
  - `ApprovalWorkflow` & `ApprovalStep` entity'leri (StepNumber, RoleId/UserId, IsRequired, Status).
  - `ApprovalHistory` entity (PurchaseRequestId, ApproverUserId, Action [Approved, Rejected, Revised], Comment, ActionDate).
- [x] **Application & CQRS (Onay İşlemleri):**
  - `ApprovePurchaseRequestCommand` & `RejectPurchaseRequestCommand`:
    - Limit bazlı onay kuralı (Örn: 10.000 TL altı Şube/Kırtasiye Müdürü onayı, üzeri Genel Satın Alma Direktörü onayı).
    - Reddetme durumunda zorunlu açıklama/gerekçe kontrolü.
  - **Otomasyon / Stok Entegrasyonu:**
    - Onaylanan satın alma talebinin tek tıkla otomatik olarak Tedarikçi Satın Alma Siparişine (Purchase Order) veya doğrudan Mal Kabul / Stok Giriş Fişine dönüştürülmesi (`ConvertPurchaseRequestToInventoryCommand`).
- [x] **API Endpoint'leri:**
  - `POST /api/purchase-requests/{id}/approve`
  - `POST /api/purchase-requests/{id}/reject`
  - `GET /api/purchase-requests/{id}/approval-history`
  - `POST /api/purchase-requests/{id}/convert-to-inventory`

### 9.2. Frontend (Yönetici Onay Paneli)
- [ ] **"Onayımı Bekleyenler" (Pending Approvals) Gelen Kutusu:**
  - Yöneticinin tek ekranda bekleyen kırtasiye taleplerini inceleyebileceği özet kartlar ve sayaçlar.
- [ ] **Hızlı Onay / Red Aksiyon Modalı:**
  - Tek tıkla onaylama veya red gerekçesi girerek geri gönderme arayüzü.
- [ ] **Görsel İş Akışı Zaman Çizelgesi (Audit Timeline):**
  - Talebin hangi aşamada kim tarafından incelendiğini gösteren görsel durum akışı.

---

## 📈 Faz 10: Kırtasiye Raporlama & Stok Analitiği (Reporting & Analytics)

### 10.1. Backend (Analitik & İstatistik Servisleri)
- [ ] **Application & Raporlama CQRS Query'leri:**
  - `GetStockTurnoverRateQuery` (Kırtasiye ürünlerinin devir hızı, en hızlı eriyen A4 kağıt, fotokopi malzemeleri ve kalem grupları).
  - `GetSeasonalDemandTrendsQuery` (Okul açılış sezonu [Ağustos-Ekim], sınav dönemleri ve ofis sezonu stok hareket trendleri).
  - `GetDeadStockQuery` (Son 90/180 günde hiç hareketi olmayan hareketsiz/ölü kırtasiye stokları).
  - `GetSupplierPerformanceQuery` (Tedarikçilerin ortalama teslimat süresi, fiyat değişim oranları ve sipariş tamamlama başarısı).
  - `GetCategoryProfitabilityQuery` (Kategori bazında kâr marjı, toplam ciro ve stok maliyeti).
- [ ] **API Endpoint'leri:**
  - `GET /api/reports/stock-turnover`
  - `GET /api/reports/seasonal-trends`
  - `GET /api/reports/dead-stock`
  - `GET /api/reports/supplier-performance`
  - `GET /api/reports/category-analytics`

### 10.2. Frontend (İnteraktif Raporlama Dashboard'u)
- [ ] **Görsel Veri Grafikleri (Charts):**
  - Chart.js / ApexCharts entegrasyonu (Kategori dağılım pasta grafiği, aylık tüketim çizgi grafiği, tedarikçi karşılaştırma sütun grafiği).
- [ ] **Özelleştirilebilir Filtreleme Çubuğu:**
  - Tarih aralığı (Son 30 Gün, Sezonluk, Yıllık), Kırtasiye Kategorisi (Kağıt, Yazı Gereçleri, Ofis, Sanatsal), Tedarikçi seçimi.
- [ ] **Özet KPI Analiz Kartları:**
  - En Hızlı Tükenen Ürün, En Maliyetli Kategori, Hareketsiz Stok Maliyeti vb.

---

## 📄 Faz 11: Excel & PDF Dışa Aktarım Motoru (Export Engine)

### 11.1. Backend (Belge Üretim Servisleri)
- [ ] **Kütüphane Kurulumu & Altyapı:**
  - `ClosedXML` / `MiniExcel` (Excel üretimi) ve `QuestPDF` (Vektörel & modern PDF tasarımı).
- [ ] **Application Servisleri:**
  - `IExcelExportService` & `IPdfReportService` arayüzleri ve implementasyonları.
  - Kurumsal Kırtasiye Antetli **Satın Alma Talep Formu PDF** şablonu.
  - **Mal Kabul & Stok Giriş / Çıkış Fişi PDF** çıktısı.
  - Filtrelenmiş Ürün Listesi, Stok Hareketleri ve Raporların Biçimlendirilmiş **Excel (.xlsx)** çıktısı (Otomatik kolon genişlikleri, başlık stilleri, para birimi formatı).
- [ ] **API Endpoint'leri:**
  - `GET /api/purchase-requests/{id}/export-pdf`
  - `GET /api/inventory/export-excel`
  - `GET /api/products/export-excel`
  - `GET /api/reports/{reportType}/export-excel`

### 11.2. Frontend (Export Butonları & Önizleme)
- [ ] **Dışa Aktarma Butonları & Durum Yönetimi:**
  - Tablolarda ve raporlarda "Excel İndir" ve "PDF Yazdır" butonları, indirme esnasında yükleme animasyonu (Loading state).
- [ ] **PDF Önizleme & Yazdırma Modalı:**
  - Talep formu ve irsaliye çıktısını tarayıcıda önizleme ve doğrudan yazdırma desteği.

---

## 🔔 Faz 12: Bildirim & Anlık Uyarı Sistemi (Notification System)

### 12.1. Backend (SignalR & E-Posta Altyapısı)
- [ ] **Domain & Entity Tasarımı:**
  - `Notification` entity (UserId, Title, Message, Type [Info, Warning, StockAlert, ApprovalNeeded], IsRead, ActionUrl, CreatedDate).
- [ ] **SignalR Real-Time Hub:**
  - `NotificationHub` oluşturulması ve kullanıcı/rol gruplarına anlık socket mesajı gönderimi.
- [ ] **E-Posta Bildirim Servisi (SMTP / MailKit):**
  - Şablonlu HTML E-postalar:
    - *"Yeni Satın Alma Talebi Onayınızı Bekliyor"* (Yöneticiye).
    - *"Talebiniz Onaylandı / Reddedildi"* (Talep sahibine).
    - *"Kritik Stok Alarmı: [Ürün Adı] tükenmek üzere!"* (Depo sorumlusuna).
- [ ] **Arka Plan Görevi (Background Worker / Quartz.NET):**
  - Belirli periyotlarla (örneğin her sabah 08:30) kritik eşiğe düşen kırtasiye ürünlerini tarayıp yöneticilere toplu bildirim/e-posta özeti geçmesi.
- [ ] **API Endpoint'leri:**
  - `GET /api/notifications`
  - `PUT /api/notifications/{id}/read`
  - `PUT /api/notifications/read-all`

### 12.2. Frontend (Bildirim Çanı & Canlı Uyarılar)
- [ ] **Navbar Bildirim Çanı & Rozeti (`NotificationBellComponent`):**
  - Okunmamış bildirim sayısı rozeti (Badge) ve son 10 bildirimin yer aldığı şık açılır panel (Dropdown).
- [ ] **SignalR İstemci Entegrasyonu (`NotificationService`):**
  - Canlı web socket bağlantısı, yeni bildirim geldiğinde masaüstü sesi / Toastr animasyonu.
- [ ] **Bildirim Merkezi Sayfası:**
  - Tüm geçmiş bildirimlerin filtrelenebileceği, tıklandığında ilgili talebe veya ürüne yönlendiren detay ekranı.

---

## 📌 Geliştirme Standartları & Notlar
* **Git Commit Kuralı:** `feat(pos): add complete sale command with stock deduction`, `feat(purchase-request): add create request command`
* **Clean Code:** Controller'lar zayıf (thin), handler'lar bağımsız olmalıdır.
* **Hata Yönetimi:** Tüm hatalar standart `ApiResponse` nesnesi ile dönmelidir.
