# 📚 Kırtasiye & Ofis ERP Sistemi (Stationery & Office ERP)

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-22.0-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Realtime-512BD4?style=for-the-badge&logoColor=white)
![Tests](https://img.shields.io/badge/Tests-59%2F59%20Passing-success?style=for-the-badge&logo=vitest&logoColor=white)

<p align="center">
  <b>Kitap, Kırtasiye, Okul ve Ofis Malzemeleri</b> sektörüne özel olarak geliştirilmiş; <b>Clean Architecture</b> ve <b>CQRS</b> prensiplerini benimseyen, Hızlı Kasa (POS), limit kurallı onay akışları, mal kabul otomasyonu, stok alarm motoru ve kurumsal raporlama yeteneklerine sahip modern web tabanlı ERP platformu.
</p>

[📌 Modüller](#-temel-modüller) •
[🏗️ Mimari Yapı](#️-mimari-yapı) •
[🔐 Giriş Hesapları](#-varsayılan-test-hesapları) •
[🚀 Kurulum](#-kurulum-ve-başlatma) •
[🧪 Testler](#-test-kapsamı-ve-doğrulama) •
[📖 Kullanım Kılavuzu](KULLANIM_KILAVUZU.md) •
[🗺️ TODO & Yol Haritası](TODO.md)

---

</div>

## 📌 Proje Vizyonu ve Sektörel Uyarlama

Bu platform; okul açılış sezonları, sınav dönemleri ve kurumsal ofis tedarik süreçlerini yöneten kırtasiye işletmelerinin operasyonel verimliliğini maksimuma çıkarmak üzere optimize edilmiştir:
- **Koli / Paket / Kutu / Adet** bazlı dinamik kırtasiye birim hiyerarşisi.
- Kritik stok seviyesi altına düşen sarf malzemelerinde (A4 fotokopi kağıdı, 2B sınav kalemleri, defterler) **gerçek zamanlı alarm motoru**.
- **Hızlı Kasa (POS)** ile saniyeler içinde barkodlu perakende satış, sepet iskonto yönetimi ve anlık stok düşümü.
- Limit bazlı **Çok Kademeli Onay Akışı** (10.000 TL altı Şube Müdürü, üzeri Genel Satın Alma Direktörü onayı).
- Onaylanan satın alma talebini tek tıkla doğrudan **Mal Kabul & Depo Girişi** fişine dönüştürme.
- 90+ gündür hareketi olmayan ürünler için **Ölü Stok (Dead Stock)** ve bağlanan sermaye analizi.
- **QuestPDF** ve **ClosedXML** ile kurumsal formatta PDF ve Excel dışa aktarım motoru.

---

## 🌟 Temel Modüller

| Modül | Açıklama & Yetenekler |
| :--- | :--- |
| 📊 **Yönetici Dashboard** | Gerçek zamanlı KPI sayaçları (Toplam Ürün, Tedarikçi, Envanter Değeri, Kritik Stok Uyarıları) ve son depo hareketleri akışı. |
| 🛒 **Hızlı Kasa (POS)** | Barkod okuma & arama, sepet yönetimi, iskonto hesaplama, Nakit/Kredi Kartı ödeme ve anında stoktan düşüş. |
| 📦 **Stok & Envanter** | Ürün tanımlama, kritik stok eşik takibi, Giriş/Çıkış/Düzeltme hareketleri ve filtrelenebilir hareket geçmişi. |
| 🚚 **Tedarikçi Yönetimi** | Firma kartları, yetkili iletişim bilgileri, tedarikçiye ürün atama ve tedarikçi ürün matrisi. |
| 📑 **Satın Alma Talepleri** | Çok kalemli talep girişi, tahmini maliyet hesaplaması ve departman bazlı ihtiyaç yönetimi. |
| ✅ **Çok Kademeli Onay** | Limit kurallı onay mekanizması, onay/red gerekçeleri ve görsel denetim zaman çizelgesi (Audit Timeline). |
| 📥 **Mal Kabul Otomasyonu** | Onaylanan talebi tek tıkla depoya kabul ederek stok miktarını otomatik artırma (`ConvertPurchaseRequestToInventory`). |
| 📈 **Raporlar & Analitik** | Satış cirosu, kâr marjı, en çok satan ürünler ve 90+ gün hareketsiz kalan ölü stok analiz raporları. |
| 📄 **Dışa Aktarma Motoru** | Tüm listeler ve raporlar için tek tıkla profesyonel PDF ve Excel çıktısı üretimi. |
| 🔔 **Canlı Bildirim Merkezi** | SignalR WebSocket altyapısı ile anlık masaüstü bildirimleri ve sesli kritik stok uyarıları. |

---

## 🏗️ Mimari Yapı

Proje, kurumsal standartlarda **Clean Architecture** ve **CQRS** (Command Query Responsibility Segregation) desenleriyle katmanlandırılmıştır:

```
ERP_Projesi/
├── src/
│   ├── Core/
│   │   ├── ERP.Domain/             # Entity'ler, Enum'lar, Sabitler ve Domain Kuralları
│   │   └── ERP.Application/        # CQRS Komut & Sorguları (MediatR), DTO'lar, FluentValidation
│   │
│   ├── Infrastructure/
│   │   └── ERP.Infrastructure/     # EF Core DbContext, Global Soft-Delete Filtreleri, JWT, SignalR Hub
│   │
│   └── Presentation/
│       └── ERP.API/                # ASP.NET Core REST API Controller'ları, Swagger Dokümantasyonu
│
├── frontend/                       # Angular 22 Standalone Components, Reactive Signals, Modern Dark UI
├── tests/                          # xUnit & FluentAssertions Backend Unit/Integration/E2E Test Paketi
└── docker-compose.yml              # SQL Server 2022, API ve Frontend Konteyner Orkestrasyonu
```

---

## 🔐 Varsayılan Test Hesapları

Sistemde tüm iş akışlarını hemen deneyebilmeniz için zengin seed verileri ve yetkilendirilmiş kullanıcılar hazırdır:

> **🔑 Tüm hesapların parolası:** `Admin123!`

* 👑 **Sistem Yöneticisi (Admin):** `admin@erp.com` — *Tam yetki, 10.000 TL üstü direktör onayları.*
* 👔 **Şube Müdürü (Manager):** `manager@erp.com` — *Stok, tedarikçi, 10.000 TL altı onaylar ve analitik.*
* 🧑‍💼 **Kasa Personeli (Employee):** `kasiyer@erp.com` — *Hızlı Kasa (POS) satışı ve satın alma talep girişi.*

---

## 🚀 Kurulum ve Başlatma

### 🐳 1. Docker ile Hızlı Başlatma (Önerilen)

Tüm sistemi (SQL Server 2022, .NET Web API ve Angular Frontend) tek komutla ayağa kaldırabilirsiniz:

```powershell
# Depoyu klonlayın
git clone https://github.com/zehratuncer/ERP_Projesi.git
cd ERP_Projesi

# Docker konteynırlarını derleyin ve başlatın
docker compose up -d --build
```

* **Frontend Uygulaması:** [http://localhost:4200](http://localhost:4200)
* **Backend API & Swagger:** [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

### 💻 2. Manuel Geliştirici Modu

```powershell
# 1. Backend API Başlatma
cd src/Presentation/ERP.API
dotnet run

# 2. Frontend Uygulamasını Başlatma
cd ../../../frontend
npm install
npm start
```

---

## 🧪 Test Kapsamı ve Doğrulama

Projede uçtan uca iş akışları, veri tabanı bütünlüğü, OWASP güvenlik standartları ve arayüz hesaplamaları test otomasyonu ile güvence altına alınmıştır:

```powershell
# Backend Testlerini Çalıştırma (39 xUnit Testi)
dotnet test

# Frontend Testlerini Çalıştırma (20 Vitest Testi)
cd frontend
npm test -- --watch=false
```

- **Backend:** 39 testin tamamı başarılı (`Passed: 39, Failed: 0`).
- **Frontend:** 20 testin tamamı başarılı (`Passed: 20, Failed: 0`).

---

## 🗺️ Sürüm & Yol Haritası

- [x] **v1 (MVP):** Çekirdek ERP, JWT Auth, Rol Yetkilendirme, Stok & Kritik Eşik Takibi, Tedarikçi Eşleştirme, Yönetici Dashboard.
- [x] **v2 (Kurumsal Süreçler):** Hızlı Kasa (POS), Çok Kademeli Onay Akışı, Mal Kabul Otomasyonu, Analitik & Ölü Stok Raporları, PDF/Excel Dışa Aktarma, SignalR Bildirimler.
- [ ] **v3 (İleri Kurumsal & E-Dönüşüm):** GİB E-Fatura / E-İrsaliye Entegrasyonu, Çoklu Şube & Depo Transferleri, B2B Bayi Portalı, Raf/Adresli Stok Sayımı, Full Audit Trail & Redis Caching.

Ayrıntılı adımlar ve detaylı test senaryoları için lütfen [Kullanım Kılavuzu](KULLANIM_KILAVUZU.md) ve [TODO.md](TODO.md) belgelerini inceleyiniz.

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) altında sunulmaktadır.