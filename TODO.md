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

## 🧪 Faz 6: Test, Kalite Güvence & Kapsamlı Sistem Doğrulaması (Testing & QA)

Bu faz, ERP sisteminin tüm modüllerinin (Kimlik Doğrulama, Stok, POS Kasa, Satın Alma, Çok Kademeli Onay, Analitik Raporlama, Export ve Bildirimler) iş kurallarına, güvenlik standartlarına ve uçtan uca akışlara uygunluğunu doğrulamak için tasarlanmıştır.

### 6.1. Backend Birim & Entegrasyon Testleri (Unit & Integration Tests)
- [x] **Kimlik Doğrulama & Yetkilendirme (Auth & IAM):**
  - [x] `LoginCommand` BCrypt şifre doğrulama, hatalı şifrede `401 Unauthorized` ve kilitli hesap kontrolü.
  - [x] JWT Token Claims doğrulaması (`UserId`, `Email`, `Role`, `Department`, `exp`).
  - [x] `[Authorize(Roles = "Admin,Manager")]` attribute testleri ve rol bazlı erişim kısıtlarının doğrulanması.
- [x] **Stok & Envanter İş Kuralları:**
  - [x] Yetersiz stok durumunda stok çıkışı (`CurrentStock < Quantity`) denemesinde `BusinessRuleException` fırlatılması ve işlemin iptal edilmesi.
  - [x] Başarılı stok girişi ve çıkışında `Product.CurrentStock` alanının atomik ve doğru güncellenmesi.
  - [x] Stok adedi `MinStockLevel` kritik eşiğin altına indiğinde `IsLowStock = true` bayrağı ve `Notification` tetiklenmesi.
  - [x] Eşzamanlı (concurrency) stok hareketlerinde veri bütünlüğünün korunması.
- [x] **Barkodlu POS Satış Motoru Testleri:**
  - [x] Çoklu ürün sepeti toplam tutar, KDV dökümü, satır bazlı indirim ve genel indirim matematiksel hesaplama doğrulaması.
  - [x] Ödeme türleri (Nakit, Kredi Kartı, Parçalı Ödeme, Açık Hesap / Veresiye) ve doğru para üstü hesaplama testi.
  - [x] Satış onaylandığında (`CompleteSaleCommand`):
    - [x] `Sale` ve `SaleItem` kayıtlarının veritabanına eksiksiz yazılması.
    - [x] Sepetteki tüm ürünler için otomatik `InventoryTransaction` (Çıkış) kaydı düşülmesi.
    - [x] Ürünlerin mevcut stok adetlerinin eksilmesi.
    - [x] Sepetteki herhangi bir üründe hata oluşursa Transaction Rollback yapılması ve hiçbir kaydın bozulmaması.
- [x] **Satın Alma & Onay Motoru Testleri:**
  - [x] `CreatePurchaseRequestCommand` boş kalem, negatif miktar veya geçersiz departman gönderildiğinde `FluentValidation` hatası üretilmesi.
  - [x] Talep durum yaşam döngüsü kuralları (`Draft` ➔ `PendingApproval` ➔ `Approved` / `Rejected` ➔ `Completed`).
  - [x] Reddetme (`RejectPurchaseRequestCommand`) işleminde zorunlu gerekçe/açıklama kontrolü.
  - [x] **Mal Kabul Dönüştürme Testi (`ConvertPurchaseRequestToInventoryCommand`):**
    - [x] Sadece `Approved` durumundaki taleplerin depoya kabul edilebilmesi.
    - [x] Mal kabul işlemiyle birlikte talep edilen ürünlerin stok adetlerinin depoya otomatik eklenmesi ve durumun `Completed` olması.
- [x] **Analitik & Raporlama Hesaplama Testleri:**
  - [x] Kategori Brüt Kâr Marjı hesaplama algoritması: `((Toplam Satış Cirosu - Toplam Maliyet) / Toplam Satış Cirosu) * 100`.
  - [x] Stok Devir Hızı (Turnover Rate) ve Tüketim İndeksi formüllerinin doğrulanması.
  - [x] Atıl / Hareketsiz Stok (Dead Stock) filtrelerinin (60, 90, 180, 365 gün) hareketsiz ürünleri doğru filtrelemesi.
- [x] **Dışa Aktarma (Export Engine) Testleri:**
  - [x] QuestPDF ile kurumsal antetli Satın Alma Talep Formu ve İrsaliye PDF dosyalarının bozulmadan üretilmesi.
  - [x] ClosedXML ile ürün, stok ve rapor listelerinin doğru başlıklar, para birimi formatı ve sayısal tiplerle `.xlsx` formatında üretilmesi.

---

### 6.2. Frontend UI/UX & Fonksiyonel Doğrulama Testleri
- [x] **Sayfa Korumaları (Route Guards) & Oturum Yönetimi:**
  - [x] Giriş yapmamış kullanıcının doğrudan `/inventory`, `/pos`, `/reports` veya `/purchase-requests` adreslerine girmesinin `AuthGuard` ile engellenmesi ve `/login`'e yönlendirilmesi.
  - [x] `Employee` rolündeki kullanıcının yönetici onay ve analitik sayfalarına girmesinin `RoleGuard` ile engellenmesi.
  - [x] JWT süresi bittiğinde `ErrorInterceptor`'ın `401 Unauthorized` yakalayarak kullanıcıyı bilgilendirip oturumu sonlandırması.
- [x] **Hızlı Kasa / POS Arayüz Testleri:**
  - [x] Barkod giriş kutusunun daima odakta (autofocus) kalması ve barkod okutulduğunda ürünün sepete anında `+1` eklenmesi.
  - [x] Aynı barkod tekrar okutulduğunda sepet satırındaki adedin otomatik `x2`, `x3` artması.
  - [x] Klavye kısayollarının (`F2` Ödeme, `F4` Temizle, `+ / -` Adet Değiştir, `Delete` Satır Sil) sorunsuz çalışması.
  - [x] Tahsilat modalında girilen nakit tutara göre para üstünün canlı hesaplanması ve termal fiş yazdırma penceresinin tetiklenmesi.
- [x] **Satın Alma Talepleri & Onay Paneli Arayüz Testleri:**
  - [x] Filtreleme çubuğundaki (Durum, Departman, Öncelik, Arama) inputlar ile `🔍 Filtrele` butonunun hizasının ve filtreleme sonuçlarının doğrulanması.
  - [x] Yeni Talep Modalı: Dinamik `➕ Ürün / Satır Ekle` butonu, satır silme (`❌`), ürün seçimi, birim fiyat girişi ve genel toplamın canlı güncellenmesi.
  - [x] Yönetici Onay Modalları (`Hızlı Onayla`, `Gerekçeli Reddet`, `Depoya Mal Kabul Et`) ve Audit Timeline zaman çizelgesi görsel doğrulaması.
- [x] **Tedarikçi & Stok Yönetimi UI Testleri:**
  - [x] Tedarikçi ürün ilişkilendirme modalının (`1200px`) geniş ekran uyumu, koyu tema dropdown okunabilirliği ve `+ Ürünü Bağla` aksiyonunun çalışması.
  - [x] Kritik stok seviyesindeki ürünlerin kırmızı renkli rozet ve uyarılarla öne çıkması.
- [x] **Raporlama & Analitik UI Testleri:**
  - [x] `🔄 Raporları Güncelle` butonunun zaman aralığı ve tarih filtreleri ile tam hizalı çalışması.
  - [x] Kategori kârlılık grafikleri, sezonluk trend grafikleri ve atıl stok listelerinin filtrelere göre anlık güncellenmesi.
  - [x] `🖨️ Yazdır / PDF` ve `📥 Excel İndir (.xlsx)` butonlarının loading durumları ve dosya indirme aksiyonlarının testi.
- [x] **Canlı Bildirimler & SignalR Testleri:**
  - [x] Yeni bir talep oluşturulduğunda veya stok kritik eşiğe düştüğünde navbar bildirim çanında kırmızı bildirim rozetinin (Badge) anında belirmesi ve Toast bildiriminin açılması.
  - [x] Bildirime tıklandığında ilgili detay modalına veya sayfaya otomatik yönlendirme yapılması.

---

### 6.3. Uçtan Uca Senaryo & İş Akışı Testleri (E2E Workflow Scenarios)
- [x] **Senaryo 1: Tam Kasa Satış & Otomatik Stok Düşümü Akışı:**
  - `Kasiyer (Employee)` ile giriş yap ➔ `/pos` sayfasına git ➔ Barkod okutarak 3 farklı kırtasiye ürününü sepete ekle ➔ `F2` ile tahsilat panelini aç ➔ Nakit ödeme tutarını gir ➔ Satışı onayla ➔ Fiş çıktısını al ➔ `/inventory` sayfasına git ve satılan ürünlerin stok miktarlarının tam olarak satılan adet kadar düştüğünü teyit et.
- [x] **Senaryo 2: Kritik Stok Alarmı & Satın Alma Talep Oluşturma Akışı:**
  - Depodaki bir ürünün stoğunu kritik eşiğin (`MinStockLevel`) altına düşür ➔ Dashboard'da ve Bildirim Çanında kırmızı kritik stok uyarısının belirdiğini doğrula ➔ `/purchase-requests` sayfasına git ➔ `+ Yeni Talep Oluştur` modalını aç ➔ Kritik ürünü seçerek satın alma talebini onaya gönder (`PendingApproval`).
- [x] **Senaryo 3: Yönetici Onayı & Depoya Mal Kabul / Stok Artış Akışı:**
  - `Yönetici (Manager/Admin)` ile giriş yap ➔ Satın Alma Talepleri `Onayımı Bekleyenler` sekmesine git ➔ Talebi incele ve onay notu girerek `Onayla` ➔ Talebin durumu `Approved` olsun ➔ `Depoya Mal Kabul Et` butonuna bas ➔ Mal kabul fişini onayla ➔ Ürünün mevcut stoğunun depoda otomatik olarak arttığını ve talebin `Completed` durumuna geçtiğini doğrula.
- [x] **Senaryo 4: Excel & PDF Kurumsal Belge Dışa Aktarım Akışı:**
  - Satın alma talebinin detayına git ➔ `Antetli PDF İndir` butonuna bas ➔ PDF belgesinin tarayıcıda önizlendiğini ve Türkçe karakterlerin düzgün basıldığını doğrula ➔ Raporlar sayfasına git ➔ `Excel İndir` butonuna bas ➔ İndirilen `.xlsx` dosyasında sayıların ve tarihlerin doğru hücre formatında açıldığını teyit et.

---

### 6.4. Veritabanı Bütünlüğü, Güvenlik & Performans Doğrulaması
- [x] **Veritabanı Bütünlüğü & Migration Doğrulaması:**
  - [x] Sıfır veritabanında `dotnet ef database update` komutunun hatasız çalışması.
  - [x] Seed Data (Başlangıç rolleri, admin kullanıcısı, temel kırtasiye kategorileri ve mock ürünler) verilerinin eksiksiz yüklenmesi.
  - [x] Foreign Key ilişkileri ve Soft-Delete (`IsDeleted = true`) filtrelerinin EF Core seviyesinde veri sızdırmazlığı.
- [x] **Güvenlik & OWASP Uyumluluk Testleri:**
  - [x] SQL Injection koruması (Tüm sorguların EF Core LINQ / Parametrik çalışması).
  - [x] XSS (Cross-Site Scripting) koruması (HTML input sanitization).
  - [x] Güvenli Şifreleme (Şifrelerin düz metin yerine BCrypt hash ile saklanması).
  - [x] CORS Politikası (Sadece frontend origin'ine izin verilmesi).
- [x] **Docker Konteynır & Dağıtım Doğrulaması:**
  - [x] `docker compose up -d --build` komutuyla `erp-sqlserver`, `erp-api` ve `erp-frontend` konteynırlarının sağlıklı (healthy) kalkması.
  - [x] Nginx Reverse-Proxy yönlendirmesinin (`/api` ve `/hubs` trafiğinin backend'e sorunsuz aktarılması) doğrulanması.

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
- [x] **"Onayımı Bekleyenler" (Pending Approvals) Gelen Kutusu:**
  - Yöneticinin tek ekranda bekleyen kırtasiye taleplerini inceleyebileceği özet kartlar ve sayaçlar.
- [x] **Hızlı Onay / Red Aksiyon Modalı:**
  - Tek tıkla onaylama veya red gerekçesi girerek geri gönderme arayüzü (`isApproveModalOpen`, `isRejectModalOpen`, `isConvertModalOpen`).
- [x] **Görsel İş Akışı Zaman Çizelgesi (Audit Timeline):**
  - Talebin hangi aşamada kim tarafından incelendiğini gösteren görsel durum akışı ve geçmiş denetim kaydı.


---

## 📈 Faz 10: Kırtasiye Raporlama & Stok Analitiği (Reporting & Analytics)

### 10.1. Backend (Analitik & İstatistik Servisleri)
- [x] **Application & Raporlama CQRS Query'leri:**
  - `GetStockTurnoverRateQuery` (Kırtasiye ürünlerinin devir hızı, en hızlı eriyen A4 kağıt, fotokopi malzemeleri ve kalem grupları).
  - `GetSeasonalDemandTrendsQuery` (Okul açılış sezonu [Ağustos-Ekim], sınav dönemleri ve ofis sezonu stok hareket trendleri).
  - `GetDeadStockQuery` (Son 90/180 günde hiç hareketi olmayan hareketsiz/ölü kırtasiye stokları).
  - `GetSupplierPerformanceQuery` (Tedarikçilerin ortalama teslimat süresi, fiyat değişim oranları ve sipariş tamamlama başarısı).
  - `GetCategoryProfitabilityQuery` (Kategori bazında kâr marjı, toplam ciro ve stok maliyeti).
- [x] **API Endpoint'leri:**
  - `GET /api/reports/stock-turnover`
  - `GET /api/reports/seasonal-trends`
  - `GET /api/reports/dead-stock`
  - `GET /api/reports/supplier-performance`
  - `GET /api/reports/category-analytics`

### 10.2. Frontend (İnteraktif Raporlama Dashboard'u)
- [x] **Görsel Veri Grafikleri (Charts):**
  - Chart.js / ApexCharts entegrasyonu (Kategori dağılım pasta grafiği, aylık tüketim çizgi grafiği, tedarikçi karşılaştırma sütun grafiği).
- [x] **Özelleştirilebilir Filtreleme Çubuğu:**
  - Tarih aralığı (Son 30 Gün, Sezonluk, Yıllık), Kırtasiye Kategorisi (Kağıt, Yazı Gereçleri, Ofis, Sanatsal), Tedarikçi seçimi.
- [x] **Özet KPI Analiz Kartları:**
  - En Hızlı Tükenen Ürün, En Maliyetli Kategori, Hareketsiz Stok Maliyeti vb.

---

## 📄 Faz 11: Excel & PDF Dışa Aktarım Motoru (Export Engine)

### 11.1. Backend (Belge Üretim Servisleri)
- [x] **Kütüphane Kurulumu & Altyapı:**
  - `ClosedXML` / `MiniExcel` (Excel üretimi) ve `QuestPDF` (Vektörel & modern PDF tasarımı).
- [x] **Application Servisleri:**
  - `IExcelExportService` & `IPdfReportService` arayüzleri ve implementasyonları.
  - Kurumsal Kırtasiye Antetli **Satın Alma Talep Formu PDF** şablonu.
  - **Mal Kabul & Stok Giriş / Çıkış Fişi PDF** çıktısı.
  - Filtrelenmiş Ürün Listesi, Stok Hareketleri ve Raporların Biçimlendirilmiş **Excel (.xlsx)** çıktısı (Otomatik kolon genişlikleri, başlık stilleri, para birimi formatı).
- [x] **API Endpoint'leri:**
  - `GET /api/purchase-requests/{id}/export-pdf`
  - `GET /api/inventory/export-excel`
  - `GET /api/products/export-excel`
  - `GET /api/reports/{reportType}/export-excel`

### 11.2. Frontend (Export Butonları & Önizleme)
- [x] **Dışa Aktarma Butonları & Durum Yönetimi:**
  - Tablolarda ve raporlarda "Excel İndir" ve "PDF Yazdır" butonları, indirme esnasında yükleme animasyonu (Loading state).
- [x] **PDF Önizleme & Yazdırma Modalı:**
  - Talep formu ve irsaliye çıktısını tarayıcıda önizleme ve doğrudan yazdırma desteği.


---

## 🔔 Faz 12: Bildirim & Anlık Uyarı Sistemi (Notification System)

### 12.1. Backend (SignalR & E-Posta Altyapısı)
- [x] **Domain & Entity Tasarımı:**
  - `Notification` entity (UserId, Title, Message, Type [Info, Warning, StockAlert, ApprovalNeeded], IsRead, ActionUrl, CreatedDate).
- [x] **SignalR Real-Time Hub:**
  - `NotificationHub` oluşturulması ve kullanıcı/rol gruplarına anlık socket mesajı gönderimi.
- [x] **E-Posta Bildirim Servisi (SMTP / MailKit):**
  - Şablonlu HTML E-postalar:
    - *"Yeni Satın Alma Talebi Onayınızı Bekliyor"* (Yöneticiye).
    - *"Talebiniz Onaylandı / Reddedildi"* (Talep sahibine).
    - *"Kritik Stok Alarmı: [Ürün Adı] tükenmek üzere!"* (Depo sorumlusuna).
- [x] **Arka Plan Görevi (Background Worker / Quartz.NET):**
  - Belirli periyotlarla (örneğin her sabah 08:30) kritik eşiğe düşen kırtasiye ürünlerini tarayıp yöneticilere toplu bildirim/e-posta özeti geçmesi.
- [x] **API Endpoint'leri:**
  - `GET /api/notifications`
  - `PUT /api/notifications/{id}/read`
  - `PUT /api/notifications/read-all`
  - `GET /api/notifications/unread-count`

### 12.2. Frontend (Bildirim Çanı & Canlı Uyarılar)
- [x] **Navbar Bildirim Çanı & Rozeti (`NotificationBellComponent`):**
  - Okunmamış bildirim sayısı rozeti (Badge) ve son 10 bildirimin yer aldığı şık açılır panel (Dropdown).
- [x] **SignalR İstemci Entegrasyonu (`NotificationService`):**
  - Canlı web socket bağlantısı, yeni bildirim geldiğinde masaüstü sesi / Toastr animasyonu.
- [x] **Bildirim Merkezi Sayfası:**
  - Tüm geçmiş bildirimlerin filtrelenebileceği, tıklandığında ilgili talebe veya ürüne yönlendiren detay ekranı.

---

## 💎 v3: Gelişmiş Kurumsal ERP, B2B, Finans & E-Dönüşüm (Advanced Enterprise & Finance)

Bu sürüm; kırtasiye işletmesinin kurumsal B2B müşteri ve bayi ağını yönetmesini, cari hesap ve finansal hareketlerini takip etmesini, resmi GİB E-Fatura / E-İrsaliye e-dönüşüm süreçlerini gerçekleştirmesini, çok şubeli/depolu operasyonlarını koordine etmesini ve kurumsal güvenlik/denetim loglama standartlarını sağlamayı amaçlar.

---

## 💰 Faz 13: Cari Hesaplar, Müşteri & Finans Yönetimi (Customer & Financial Management)

### 13.1. Backend (Cari & Finans CQRS)
- [ ] **Domain & Entity Tasarımı:**
  - `Customer` (Cari Hesap) entity:
    - Code, Title, TaxNumber, TaxOffice, IdentityNumber, CustomerType [Individual, Corporate, School, Government], RiskLimit, PaymentTermDays (Vade), CurrentBalance, CreditBalance, DebitBalance, Address, City, Phone, Email, IsB2BEnabled, IsActive.
  - `AccountTransaction` (Cari Hesap Hareketi) entity:
    - CustomerId, TransactionType [SaleInvoice, PurchaseInvoice, CashCollection, BankCollection, PaymentOut, DebitNote, CreditNote], Amount, BalanceAfterTransaction, DocumentNumber, DueDate (Vade Tarihi), Description, TransactionDate, UserId.
  - `CashDesk` & `BankAccount` entity'leri:
    - Kasa ve Banka hesap tanımları, döviz cinsi (TRY, USD, EUR), anlık bakiye takibi ve hesaplar arası virman/transfer kayıtları.
- [ ] **Application & CQRS (Cari & Kasa/Banka İşlemleri):**
  - `CreateCustomerCommand` & `UpdateCustomerCommand` (Vergi No / TC Kimlik No benzersizlik ve biçim doğrulaması).
  - `CreateAccountTransactionCommand` (Tahsilat / Tediye makbuzu girişi, anlık cari bakiye atomik güncellemesi).
  - `GetCustomerStatementQuery` (Tarih aralıklı, borç/alacak ve kümülatif bakiye dökümlü detaylı Cari Hesap Ekstresi).
  - `GetCustomerAgingReportQuery` (Vadesi geçmiş ve yaklaşan alacaklar için 30-60-90+ gün Yaşlandırma Raporu).
  - `GetCashAndBankBalancesQuery` (Güncel kasa ve banka likidite durumu).
- [ ] **API Endpoint'leri:**
  - `GET /api/customers`, `POST /api/customers`, `GET /api/customers/{id}`, `PUT /api/customers/{id}`
  - `GET /api/customers/{id}/statement` (Cari Ekstre)
  - `GET /api/customers/aging-report` (Cari Yaşlandırma Raporu)
  - `POST /api/finance/transactions` (Tahsilat/Ödeme girişi)
  - `GET /api/finance/cash-banks` (Kasa ve Banka hesapları)

### 13.2. Frontend (Cari & Finansal Yönetim Arayüzü)
- [ ] **Cari Hesap Kartları & Yönetim Ekranı:**
  - Kurumsal/Bireysel cari listesi, arama, risk limiti doluluk çubuğu, borç/alacak renkli rozetleri.
- [ ] **Cari Hesap Ekstresi (Statement View):**
  - Tarih aralığına göre işlem geçmişi tablosu, devreden bakiye, satır bazlı bakiye yürütme ve tek tıkla `Ekstre PDF / Excel İndir` aksiyonları.
- [ ] **Hızlı Tahsilat / Tediye Modalı:**
  - Nakit, Kredi Kartı, Havale/EFT ve Çek/Senet tahsilat formu, anında bakiye düşümü.
- [ ] **Finansal Durum & Yaşlandırma Dashboard'u:**
  - Vadesi geçen alacaklar tablosu, haftalık nakit akış projeksiyonu grafiği.

---

## 📑 Faz 14: E-Dönüşüm: E-Fatura, E-Arşiv & E-İrsaliye Entegrasyonu (E-Transformation & Invoicing)

### 14.1. Backend (GİB Standartları & Entegratör Servisi)
- [ ] **Domain & Entity Tasarımı:**
  - `Invoice` entity:
    - InvoiceNumber (GİB formatında Örn: GIB2026000000001), GibUuid (Guid), CustomerId, SaleId/PurchaseRequestId, InvoiceType [Sales, Return, Withholding (Tevkifat), Exception], InvoiceScenario [Basic, Commercial, Export, Public], IssueDate, DueDate, TotalGrossAmount, TotalDiscountAmount, TotalVatAmount, TotalWithholdingAmount, PayableAmount, Currency, Status [Draft, Queued, SentToGib, Approved, Rejected, Cancelled], XmlData, SignedXmlData.
  - `InvoiceItem` entity:
    - ProductId, Quantity, Unit, UnitPrice, VatRate (0, 1, 10, 20), VatAmount, DiscountRate, DiscountAmount, WithholdingRate (Örn: 5/10), TotalAmount.
  - `Waybill` (E-İrsaliye) entity ve sevk irsaliyesi kalemleri.
- [ ] **Application & E-Fatura Servisleri:**
  - `IEInvoiceService` & `IEWaybillService` Entegratör Abstraction katmanı (GİB UBL-TR 1.2 standartlarında XML şablon üretimi, Schematron validasyonu, dijital imzalama simülasyonu / Entegratör API köprüsü).
  - `CreateInvoiceFromSaleCommand` (Tamamlanan POS satışından veya kurumsal siparişten tek tıkla E-Arşiv / E-Fatura oluşturma).
  - `SendInvoiceToGibCommand` & `CancelInvoiceCommand` (GİB durum sorgulama ve iptal süreçleri).
  - `GetInvoiceHtmlPreviewQuery` (XSLT şablonu ile resmi mühürlü HTML fatura render motoru).
- [ ] **API Endpoint'leri:**
  - `POST /api/invoices/from-sale/{saleId}`
  - `GET /api/invoices`, `GET /api/invoices/{id}`, `GET /api/invoices/{id}/html-preview`, `GET /api/invoices/{id}/xml`
  - `POST /api/invoices/{id}/send-gib`
  - `POST /api/invoices/{id}/cancel`

### 14.2. Frontend (E-Fatura & E-İrsaliye Portalı)
- [ ] **Fatura Listesi & GİB Durum Takip Paneli:**
  - Gelen/Giden faturalar, GİB onay durumu renkli rozetleri (Yeşil: Başarılı, Sarı: İşleniyor, Kırmızı: Hatalı/Reddedildi).
- [ ] **Resmi E-Fatura Önizleme Modalı:**
  - GİB uyumlu antetli, karekodlu (QR Code), mali mühürlü ve tevkifat dökümlü HTML/PDF önizleme penceresi.
- [ ] **Toplu Fatura İşlemleri:**
  - Seçilen satışları toplu faturalandırma ve tek tıkla e-posta ile müşteriye gönderme.

---

## 🏢 Faz 15: Çoklu Şube, Depo & Şubeler Arası Transfer (Multi-Branch & Warehouse Transfers)

### 15.1. Backend (Şube & Transfer Yönetimi)
- [ ] **Domain & Entity Tasarımı:**
  - `Branch` entity (Code, Name, City, Address, Phone, IsHeadquarters).
  - `Warehouse` entity (BranchId, Code, Name, LocationType [MainStore, RetailShop, Outlet, DamagedGoodsWarehouse]).
  - `WarehouseStock` entity (WarehouseId, ProductId, CurrentStock, MinStockLevel, MaxStockLevel).
  - `StockTransfer` entity (TransferNumber, SourceWarehouseId, TargetWarehouseId, Status [Draft, InTransit, Received, Cancelled], ShippedDate, ReceivedDate, ShippedByUserId, ReceivedByUserId, Description).
  - `StockTransferItem` entity (TransferId, ProductId, Quantity, Unit).
- [ ] **Application & CQRS (Transfer İş Akışı):**
  - `CreateStockTransferCommand` (Şubeler arası talep oluşturma).
  - `ShipStockTransferCommand` (Kaynak depodan stok düşümü, transferin `InTransit / Yolda` durumuna geçmesi).
  - `ReceiveStockTransferCommand` (Hedef depoda mal kabul onayı, hedef stoğa otomatik ekleme, transferin `Received` olması).
  - `GetWarehouseStockMatrixQuery` (Ürünlerin şube ve depolara göre anlık stok matrisi ve karşılaştırması).
- [ ] **API Endpoint'leri:**
  - `GET /api/branches`, `POST /api/branches`
  - `GET /api/warehouses`, `POST /api/warehouses`
  - `GET /api/warehouses/stock-matrix`
  - `POST /api/stock-transfers`, `GET /api/stock-transfers/{id}`
  - `POST /api/stock-transfers/{id}/ship`
  - `POST /api/stock-transfers/{id}/receive`

### 15.2. Frontend (Şube/Depo & Transfer Paneli)
- [ ] **Depolar Arası Stok Karşılaştırma Matrisi:**
  - Tüm şube ve mağazalardaki stok seviyelerini yan yana gösteren interaktif tablo.
- [ ] **Şubeler Arası Sevk Formu & İrsaliye:**
  - Çıkış/Varış deposu seçimi, barkodla transfer edilecek ürünleri sepete ekleme, sevk irsaliyesi yazdırma.
- [ ] **"Yoldaki Transferler" (In-Transit) Takip & Kabul Paneli:**
  - Şubeye ulaşan sevkiyatların incelenmesi, eksik/hasarlı kontrolü ve tek tıkla depoya kabul butonu.

---

## 🤝 Faz 16: B2B Bayi Portalı & Kurumsal Toplu Sipariş (B2B Partner Portal & Bulk Ordering)

### 16.1. Backend (B2B Sipariş & Fiyatlandırma Motoru)
- [ ] **Domain & Entity Tasarımı:**
  - `PriceList` & `ProductPrice` entity'leri (Fiyat Listesi Tanımları: Perakende, Toptan Bayi, Okul Özel, Sezonluk Kampanya Fiyatları).
  - `CustomerUser` entity (Kurumsal müşterinin B2B portal yetkilileri).
  - `B2BOrder` & `B2BOrderItem` entity'leri (OrderNumber, CustomerId, Status [Submitted, UnderReview, Confirmed, Preparing, Shipped, Invoiced, Cancelled], PaymentStatus, ShippingAddress, TotalAmount).
- [ ] **Application & CQRS:**
  - `CreateB2BOrderCommand` & `ApproveB2BOrderCommand`:
    - Cari risk limiti kontrolü (`Customer.RiskLimit` aşımı engelleme).
    - Müşteriye özel tanımlı iskonto/fiyat listesinin otomatik uygulanması.
    - Sipariş onaylandığında otomatik rezerve stok oluşturulması.
  - `ImportBulkOrderFromExcelCommand` (Kurumların Excel sipariş listesini tek tıkla sepete aktarması).
- [ ] **API Endpoint'leri:**
  - `GET /api/b2b/catalog` (Bayiye özel fiyatlı ürün kataloğu)
  - `POST /api/b2b/orders`, `GET /api/b2b/orders`, `GET /api/b2b/orders/{id}`
  - `POST /api/b2b/orders/import-excel`
  - `POST /api/b2b/orders/{id}/confirm`

### 16.2. Frontend (B2B Bayi Arayüzü)
- [ ] **B2B Katalog & Hızlı Sipariş Matrisi:**
  - Kurumsal müşteriye özel dinamik fiyatlar, stok durumu ("Stokta Var", "Siparişle Temin"), toplu adet girişi.
- [ ] **Excel ile Toplu Sipariş Yükleme:**
  - Ürün kodu ve adet içeren Excel dosyasını sürükle-bırak ile anında siparişe dönüştürme.
- [ ] **Bayi Sipariş & Cari Hesap Portali:**
  - Sipariş kargo/hazırlık durumu, açık faturalar ve online bakiye özet ekranı.

---

## 🏷️ Faz 17: Gelişmiş Stok Sayımı, Barkod Basımı & Raf/Adres Takibi (Advanced Inventory & Shelving)

### 17.1. Backend (Sayım & Raf Lokasyonu)
- [ ] **Domain & Entity Tasarımı:**
  - `ShelfLocation` entity (WarehouseId, Aisle [Koridor], Rack [Raf], Shelf [Kat], Bin [Göz/Bölme], FullAddressCode [Örn: A-03-2-B]).
  - `ProductShelfLocation` (Ürünün depodaki birincil ve ikincil raf adresleri).
  - `StockCount` & `StockCountItem` entity'leri (CountNumber, WarehouseId, Status [InProgress, Reconciled, Cancelled], SystemQuantity, CountedQuantity, DifferenceQuantity, CountDate, Notes).
- [ ] **Application & Servisler:**
  - `StartStockCountCommand` & `SubmitCountedItemCommand` (Kör sayım veya sistem adetli sayım).
  - `ReconcileStockCountCommand` (Sayım mutabakatı: Sayım farkları için otomatik `TransactionType: Adjustment` stok düzeltme fişleri oluşturma).
  - `IBarcodeLabelService` (Zebra / Termal yazıcılar için EPL/ZPL ve PDF formatında EAN-13, Code-128 ve QR kodlu raf/ürün etiket şablonları üretimi).
- [ ] **API Endpoint'leri:**
  - `GET /api/inventory/shelves`, `POST /api/inventory/shelves`
  - `POST /api/inventory/counts/start`, `POST /api/inventory/counts/{id}/submit-item`
  - `POST /api/inventory/counts/{id}/reconcile` (Mutabakat ve eşitleme)
  - `POST /api/inventory/labels/generate-pdf` (Toplu etiket basımı)

### 17.2. Frontend (Mobil Uyumlu Sayım & Etiket Basım Arayüzü)
- [ ] **Mobil El Terminali / Barkodlu Sayım Modu:**
  - Mobil cihazlarda tam ekran hızlı barkod okutma, anlık sesli geri bildirim (Bip/Hata sesi) ve sayım listesi.
- [ ] **Sayım Farkı & Mutabakat Ekranı:**
  - Fazla ve eksik çıkan ürünlerin maliyet etkisini gösteren karşılaştırma tablosu ve tek tıkla `Stokları Eşitle` aksiyonu.
- [ ] **Dinamik Fiyat & Raf Etiketi Yazdırma Sihirbazı:**
  - Etiket boyutu seçimi (40x20mm, 60x30mm, A4 çoklu etiket), ürün seçimi, indirimli fiyat / kampanya etiketi basımı.

---

## 🛡️ Faz 18: Sistem Loglama, Güvenlik Denetimi & Performans İzleme (Audit Logging & Observability)

### 18.1. Backend (Full Audit Trail & Caching)
- [ ] **Domain & Entity Tasarımı:**
  - `AuditLog` entity:
    - UserId, UserEmail, Action [Create, Update, Delete, Login, Approval, Export], EntityName, EntityId, OldValues (JSON), NewValues (JSON), IpAddress, UserAgent, Timestamp.
- [ ] **Application & Altyapı:**
  - `ApplicationDbContext` içinde otomatik Entity Change Tracking ile tüm değişikliklerin anlık `AuditLog` tablosuna kaydedilmesi.
  - **Redis Distributed Caching:** Sık sorgulanan ürün katalogları, yetkiler ve kategori istatistikleri için Redis önbellekleme (`IDistributedCache`).
  - **Rate Limiting Middleware:** API brute-force ve aşırı istek kısıtlaması (`AspNetCoreRateLimit` / .NET 8 built-in RateLimiter).
  - **Sağlık Denetimi (Health Checks):** `/health`, `/health/ready`, `/health/live` endpoint'leri (SQL Server, Redis ve Disk durumu).
- [ ] **API Endpoint'leri:**
  - `GET /api/admin/audit-logs` (Gelişmiş filtreleme ve JSON diff sorgulama)
  - `GET /api/admin/system-health`

### 18.2. Frontend (Sistem Denetimi & Yönetim Konsolu)
- [ ] **Audit Log & İşlem Geçmişi İzleme Ekranı:**
  - Kim, hangi ürünü veya fiyatı ne zaman değiştirdi arayüzü; eski ve yeni değerlerin renkli JSON diff karşılaştırması.
- [ ] **Sistem Sağlık & Performans Paneli:**
  - Veritabanı bağlantı gecikmesi, bellek kullanımı ve API yanıt sürelerini gösteren canlı sistem monitörü.

---

## 📌 Geliştirme Standartları & Notlar
* **Git Commit Kuralı:** `feat(pos): add complete sale command with stock deduction`, `feat(purchase-request): add create request command`
* **Clean Code:** Controller'lar zayıf (thin), handler'lar bağımsız olmalıdır.
* **Hata Yönetimi:** Tüm hatalar standart `ApiResponse` nesnesi ile dönmelidir.
