# 🚀 AI-Powered Next-Gen ERP System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0%2F9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![Python](https://img.shields.io/badge/Python-FastAPI-3776AB?style=for-the-badge&logo=python&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

<p align="center">
  <b>Clean Architecture</b> ve <b>CQRS</b> prensipleriyle tasarlanmış; aşamalı olarak geliştirilen, kurumsal iş akışlarını ve ileri aşamada proaktif <b>AI Copilot & ML</b> motorunu barındıran modern ERP platformu.
</p>

[📌 Proje Özeti](#-proje-özeti) •
[🗺️ Sürüm & Yol Haritası](#-sürüm-ve-yol-haritası-roadmap) •
[🏗️ Sistem Mimarisi](#️-sistem-mimarisi) •
[💻 Teknoloji Yığını](#-teknoloji-yığını) •
[🛠️ Kurulum](#️-kurulum-ve-başlatma) •
[📋 TODO & İlerleme](TODO.md)

---

</div>

## 📌 Proje Özeti

Bu proje, işletmelerin stok, tedarikçi, satın alma ve operasyonel süreçlerini güvenilir, modüler ve yüksek performanslı bir mimaride yönetmeyi hedefler. Geliştirme süreci 3 ana aşamada (MVP v1, v2 ve v3) planlanmıştır.

---

## 🗺️ Sürüm ve Yol Haritası (Roadmap)

Sistem geliştirme stratejisi adım adım değer üretecek şekilde fazlara ayrılmıştır:

```
┌───────────────────────────┐      ┌───────────────────────────┐      ┌───────────────────────────┐
│          MVP v1           │      │            v2             │      │            v3             │
│    Çekirdek ERP & Stok    │ ───► │  İş Süreçleri & Monetize  │ ───► │      AI & ML Entegre      │
└───────────────────────────┘      └───────────────────────────┘      └───────────────────────────┘
```

### 🔹 MVP (v1) — Çekirdek ERP & Stok Yönetimi *(Öncelikli Aşama)*
İlk aşamada sağlam bir temel oluşturularak uygulamanın canlıya alınabilir en yalın ve stabil hali inşa edilir:
* 👤 **Kullanıcı & Güvenlik:**
  * Güvenli Giriş (Login / JWT & Refresh Token)
  * Rol Yönetimi (Admin, Manager, User)
* 📦 **Stok & Envanter Yönetimi:**
  * Ürün Ekleme / Düzenleme / Listeleme
  * Stok Giriş & Çıkış Hareketleri (Inventory Transactions)
  * Kritik Stok Eşik Takibi ve Uyarıları
* 🚚 **Tedarikçi Yönetimi:**
  * Tedarikçi Tanımlama & Düzenleme
  * Ürüne Göre Tedarikçi Eşleştirme ve Fiyat Listesi
* 📊 **Yönetici Dashboard:**
  * Toplam Ürün Sayısı
  * Kritik Stoktaki Ürünler Sayacı ve Listesi
  * Son Stok Hareketleri Akışı

---

### 🔸 v2 — Kurumsal İş Süreçleri & Değer Katan Özellikler
İşletmelere doğrudan ticari değer sağlayan ve operasyonel verimliliği artıran kurumsal modüller:
* 📑 **Satın Alma Talepleri:** Departman bazlı satın alma ihtiyaçlarının girilmesi.
* ✅ **Çok Kademeli Onay Sistemi:** Yönetici onay akışları (`Taslak`, `Onay Bekliyor`, `Onaylandı`, `Reddedildi`).
* 📈 **Basit & Gelişmiş Raporlama:** Satış/stok devir hızı, tedarikçi performans özetleri.
* 📄 **Excel / PDF Export:** Tüm listeler ve raporlar için dışa aktarım desteği.
* 🔔 **Bildirim Sistemi:** Kritik stok uyarıları, talep onay bildirimleri (Web / E-posta).

---

### 🧠 v3 — AI Copilot & Makine Öğrenimi (ML) Sistemi
Projeye tam yapay zeka ve öngörü yeteneklerinin kazandırıldığı son aşama:
* 🤖 **AI ERP Copilot:** Doğal dil ile ERP verilerini sorgulama, özetleme ve kontrollü Tool Calling mekanizması.
* 📈 **Makine Öğrenimi Tabanlı Talep Tahminleme (Demand Forecasting):** Python FastAPI servisi ile geçmiş stok hareketlerinden tüketim tahmini.
* 🛡️ **Human-in-the-Loop Karar Desteği:** Yapay zekanın doğrudan sipariş açmak yerine yöneticiye onaylatarak işlem yapması.

---

## 🏗️ Sistem Mimarisi

Sistem, sorumlulukların net ayrıldığı çok katmanlı Clean Architecture prensiplerini benimser:

```
src/
├── 📁 Domain                     # Kurumsal İş Mantığı & Saf Entity Modelleri
│   ├── Entities                 # User, Role, Product, Inventory, Supplier vb.
│   ├── Enums                    # StockMovementType, UserRole vb.
│   └── Interfaces               # Temel Repository & UnitOfWork arayüzleri
│
├── 📁 Application                # CQRS Komutları, Query'ler ve İş Kuralları (MediatR)
│   ├── Features/                # Auth, Products, Inventory, Suppliers, Dashboard
│   ├── DTOs/                    # Data Transfer Objects
│   └── Validators/              # FluentValidation kuralları
│
├── 📁 Infrastructure             # EF Core, Veritabanı ve Dış Servisler
│   ├── Persistence/             # DbContext, Migrations, Entity Configurations
│   └── Identity/                # JWT Token üretimi ve şifreleme
│
└── 📁 API                        # Sunum Katmanı (RESTful Controller'lar & Middleware)
```

---

## 💻 Teknoloji Yığını

| Katman | Teknoloji / Kütüphane | Kullanım Amacı |
| :--- | :--- | :--- |
| **Backend Core** | C# .NET 8 / 9, ASP.NET Core Web API | Güçlü ve ölçeklenebilir RESTful API motoru |
| **Mimari Yaklaşım** | Clean Architecture, CQRS, MediatR | Katmanlı ve test edilebilir mimari |
| **ORM & Veritabanı** | Entity Framework Core, SQL Server / PostgreSQL | Veri modelleme ve migration yönetimi |
| **Doğrulama & Güvenlik**| FluentValidation, JWT, BCrypt, Rate Limiting | Güvenli kimlik denetimi ve veri validasyonu |
| **Frontend** | Angular 17+, TypeScript, SCSS / Modern CSS | Reaktif ve modern kullanıcı arayüzü |
| **DevOps & Dağıtım** | Docker, Docker Compose | Konteynerize geliştirme ve dağıtım |
| **AI Motoru (v3)** | Python, FastAPI, Scikit-Learn / LLM API | Talep tahmini ve doğal dil destekli Copilot |

---

## 🛠️ Kurulum ve Başlatma

### 📋 Ön Koşullar
* [.NET 8+ SDK](https://dotnet.microsoft.com/download)
* [Node.js (v18+)](https://nodejs.org/) & [Angular CLI](https://angular.io/cli)
* [Docker Desktop](https://www.docker.com/) *(Opsiyonel)*

```bash
# Projeyi klonlayın
git clone https://github.com/kullaniciadi/ERP_Projesi.git
cd ERP_Projesi

# Backend çalıştırma
cd src/API
dotnet run

# Frontend çalıştırma
cd frontend
npm install
ng serve
```

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) kapsamında lisanslanmıştır.