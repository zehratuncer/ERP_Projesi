# 📖 Kırtasiye & Ofis ERP Sistemi — Kullanım Kılavuzu ve Test Senaryoları

Bu belge, **Kırtasiye & Ofis ERP** sisteminin tüm modüllerini, iş akışlarını, hazır seed verilerini ve adım adım test senaryolarını içermektedir.

---

## 🔐 1. Sisteme Giriş ve Kullanıcı Rolleri

Sistemde rol bazlı yetkilendirme (Role-Based Access Control) ve limit kurallı onay iş akışları mevcuttur. Test edebilmeniz için 3 farklı rol seviyesinde kullanıcı hazır olarak tanımlanmıştır:

> **🔑 Tüm kullanıcılar için varsayılan şifre:** `Admin123!`

| Rol | E-Posta | Ad Soyad | Yetki ve Sorumluluk Alanı |
| :--- | :--- | :--- | :--- |
| 👑 **Admin** | `admin@erp.com` | Zehra Tuncer | **Tam Sistem Yöneticisi:** Tüm modüllere tam erişim, 10.000 TL üzeri satın alma taleplerini onaylama/reddetme, kullanıcı & sistem yönetimi. |
| 👔 **Manager** | `manager@erp.com` | Ahmet Yılmaz | **Kırtasiye & Şube Müdürü:** Stok yönetimi, tedarikçi ilişkileri, 10.000 TL altı talepleri onaylama, satış ve analitik raporları inceleme. |
| 🧑‍💼 **Employee** | `kasiyer@erp.com` | Elif Kaya | **Kasa & Satış Personeli:** Hızlı Kasa (POS) perakende satış, yeni satın alma talebi açma, stok hareketlerini izleme. |

---

## 🏢 2. Hazır Yüklenen Seed Veri Özeti

Sistem veritabanında tüm özellikleri doğrudan deneyimleyebilmeniz için kurumsal kırtasiye verileri hazır olarak yer almaktadır:

### 📦 Örnek Kırtasiye Ürünleri Kataloğu
1. **`KRT-001` Copier Bond A4 80gr Fotokopi Kağıdı (5'li Koli)**
   - *Stok:* 8 Koli | *Kritik Eşik:* 25 Koli | *Fiyat:* ₺780,00 | *Durum:* 🚨 **Kritik Stok Uyarısı** | *Tedarikçi:* Kopier Kağıt
2. **`KRT-042` Faber-Castell 2B Sınav Kurşun Kalem (72'li Kutu)**
   - *Stok:* 5 Kutu | *Kritik Eşik:* 20 Kutu | *Fiyat:* ₺360,00 | *Durum:* 🚨 **Kritik Stok Uyarısı** | *Tedarikçi:* Adel Kalemcilik
3. **`KRT-089` Gıpta Spiralli A4 Çizgili Defter (10'lu Paket)**
   - *Stok:* 12 Paket | *Kritik Eşik:* 30 Paket | *Fiyat:* ₺290,00 | *Durum:* 🚨 **Kritik Stok Uyarısı** | *Tedarikçi:* Gıpta Ofis
4. **`KRT-114` Pritt Stick Kuru Yapıştırıcı 43gr (24'lü Stand)**
   - *Stok:* 24 Stand | *Kritik Eşik:* 10 Stand | *Fiyat:* ₺950,00 | *Durum:* ✅ Yeterli | *Tedarikçi:* Faber-Castell & Daksil
5. **`KRT-205` Maped Ağır Büro Zımba Makinesi No:24/6**
   - *Stok:* 35 Adet | *Kritik Eşik:* 10 Adet | *Fiyat:* ₺175,00 | *Durum:* ✅ Yeterli | *Tedarikçi:* Maped Büro
6. **`KRT-301` Faber-Castell 24'lü Suluboya & Fırça Seti**
   - *Stok:* 45 Set | *Kritik Eşik:* 15 Set | *Fiyat:* ₺210,00 | *Durum:* ✅ Yeterli | *Tedarikçi:* Adel Kalemcilik
7. **`KRT-401` Yaygan Lisanslı Ergonomik Okul Sırt Çantası**
   - *Stok:* 18 Adet | *Kritik Eşik:* 8 Adet | *Fiyat:* ₺850,00 | *Durum:* ✅ Yeterli | *Tedarikçi:* Gıpta Ofis

---

## 🎯 3. Modül Modül Test Senaryoları

### 1️⃣ Yönetici Dashboard ([http://localhost:4200/dashboard](http://localhost:4200/dashboard))
* **Görüntüleme:**
  - 7 toplam ürün çeşidi, 3 kritik stok uyarısı, 5 tedarikçi, 147 toplam fiziksel stok adedi ve ₺68.140,00 toplam envanter değeri.
* **Kritik Stok Tablosu:**
  - Eşik altına düşen Copier Bond Kağıt, Faber-Castell Kalem ve Gıpta Defter'in kırmızı rozetlerle listelendiğini görün.
* **Son Depo Hareketleri Akışı:**
  - En son yapılan mal kabul, sevkiyat ve düzeltme fişlerini kullanıcı adları ve tarihleriyle inceleyin.

---

### 2️⃣ Hızlı Kasa / POS ([http://localhost:4200/pos](http://localhost:4200/pos))
* **Senaryo: Barkodlu / Hızlı Perakende Satışı:**
  1. Sağ taraftaki ürün ızgarasından **Faber-Castell Kurşun Kalem** ve **Suluboya Seti**'ne tıklayarak sepete ekleyin (veya arama kutusuna `KRT-042` yazın).
  2. Sepetteki adet butonlarıyla (`+` / `-`) miktarları artırın.
  3. İskonto alanına `%10` indirim uygulayın, ara toplam ve KDV dahil genel toplamın anlık hesaplandığını görün.
  4. **"Nakit"** veya **"Kredi Kartı"** ödeme türünü seçin ve **"Satışı Tamamla"** butonuna tıklayın.
  5. Başarılı toastr bildirimini ve ilgili ürünlerin stoklarından adedin anında düştüğünü teyit edin.

---

### 3️⃣ Stok & Ürün Yönetimi ([http://localhost:4200/inventory](http://localhost:4200/inventory))
* **Senaryo A: Yeni Kırtasiye Ürünü Ekleme:**
  - **"+ Yeni Ürün Ekle"** modalını açın. Ürün Kodu (`KRT-550`), Ürün Adı, Birim Fiyatı, Kritik Stok Eşiği ve Tedarikçi seçerek kaydedin.
* **Senaryo B: Manuel Stok Hareketi (Giriş / Çıkış / Düzeltme):**
  - **"Stok Hareketi Ekle"** butonuna basın. Bir ürün seçip `Giriş (In)` türünde 50 adet ekleyin. Anlık stok adedinin arttığını görün.
* **Senaryo C: Sadece Kritik Stoktakileri Filtreleme:**
  - Üst filtre alanından **"Kritik Stok Uyarısı Olanlar"** butonuna basarak acil sipariş gereken ürünleri tek tıkla izole edin.

---

### 4️⃣ Satın Alma Talepleri & Çok Kademeli Onay ([http://localhost:4200/purchasing](http://localhost:4200/purchasing))
Sistemde onay mekanizmasını ve iş akışını test edebilmeniz için **4 farklı durumda** talep bulunmaktadır:

#### 🔹 Test 1: Onay Bekleyen Talep (Yönetici Onayı)
* **Talep:** `TALEP-20260827-001` (Tutar: ₺14.500,00 — Durum: `Onay Bekliyor / PendingApproval`)
* **Test Adımı:** `admin@erp.com` ile giriş yapın. Talebin yanındaki **"Onayla"** butonuna basarak onaylayın veya **"Reddet"** diyerek gerekçe yazın. 10.000 TL üzeri kuralına göre direktör onayı tamamlanacaktır.

#### 🔹 Test 2: Onaylanmış Talebin Mal Kabulü (Stok Girişine Otomatik Dönüştürme)
* **Talep:** `TALEP-20260826-002` (Tutar: ₺5.800,00 — Durum: `Onaylandı / Approved`)
* **Test Adımı:** Talebin sağındaki yeşil **"Mal Kabul & Depoya Giriş Yap"** butonuna tıklayın. Açılan modalda onay verdiğinizde talep kalemlerindeki ürünler otomatik olarak depoya girecek ve `InventoryTransactions` tablosuna `Giriş (In)` olarak işlenecektir.

#### 🔹 Test 3: Reddedilen Talep & Denetim Zaman Çizelgesi (Audit Timeline)
* **Talep:** `TALEP-20260825-003` (Durum: `Reddedildi / Rejected`)
* **Test Adımı:** Talebe tıklayarak **Detay & Zaman Çizelgesi** panelini açın. Kimin hangi tarihte hangi red gerekçesini ("Bütçe aşımı...") yazdığını adım adım inceleyin.

---

### 5️⃣ Tedarikçi Yönetimi ([http://localhost:4200/suppliers](http://localhost:4200/suppliers))
* **Tedarikçi Ürün Eşleştirmeleri:**
  - Tedarikçi kartındaki **"Ürünleri Yönet"** butonuna basarak açılan geniş modalda tedarikçinin sağladığı kırtasiye ürünlerini ve fiyatlarını görüntüleyin veya yeni ürün atayın.

---

### 6️⃣ Raporlar & Analitik ([http://localhost:4200/reports](http://localhost:4200/reports))
* **Satış & Kâr Analizi:** POS satış fişlerinden hesaplanan toplam ciro, kâr marjı ve en çok satan ürünler grafiği.
* **Hareketsiz / Ölü Stok (Dead Stock) Raporu:** 90+ gündür çıkışı olmayan ürünleri ve depoda kilitli kalan sermaye tutarını tespit edin.
* **Dışa Aktarma (PDF & Excel Export):**
  - Sayfadaki **"PDF Raporu İndir"** ve **"Excel İndir"** butonlarına tıklayarak QuestPDF ve ClosedXML tarafından üretilen antetli resmi rapor çıktılarını bilgisayarınıza indirin.

---

### 7️⃣ Bildirim Çanı & Canlı Uyarılar ([http://localhost:4200/notifications](http://localhost:4200/notifications))
* **Canlı Socket Uyarıları:**
  - Navbar'ın sağ üst köşesinde yer alan **Bildirim Çanı** rozetinde okunmamış stok uyarıları ve onay bekleyen talepler listelenir.
  - Tıklandığında ilgili ekrana (Stok veya Satın Alma) anında yönlendirir.

---

## 🐳 4. Docker Komutları & Hızlı Başlatma

Veritabanını sıfırlamak veya konteynırları yeniden derlemek için:

```powershell
# Konteynırları durdurun ve SQL veritabanı disk alanını sıfırlayın
docker compose down -v

# Yeniden derleyip arka planda başlatın
docker compose up -d --build

# Çalışan konteynırların durumunu kontrol edin
docker compose ps
```

* **Frontend Arayüzü:** [http://localhost:4200](http://localhost:4200)
* **Backend API & Swagger Dokümantasyonu:** [http://localhost:5000/swagger](http://localhost:5000/swagger)
