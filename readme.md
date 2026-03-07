# University Club System

Bu proje iki ana bölüme ayrılmıştır:

## 📁 Klasör Yapısı

- **Backend/**: ASP.NET Core API projesi. Veritabanı işlemleri, modeller ve endpoint'ler burada bulunur.
- **Frontend/**: HTML, CSS ve JavaScript kullanarak geliştirilen kullanıcı arayüzü.

## 🚀 Çalıştırma Talimatları

### 1. Backend'i Başlatın
```bash
cd Backend
dotnet run
```
API varsayılan olarak `http://localhost:5149` adresinde çalışacaktır. Swagger arayüzüne `/swagger` üzerinden erişebilirsiniz.

### 2. Frontend'i Açın
- `Frontend/index.html` dosyasını tarayıcınızda açın (veya VS Code Live Server kullanın).
- Frontend, Backend API'sine `Frontend/js/services/apiService.js` üzerinden bağlanır.

## 🛠 Kullanılan Teknolojiler
- **Backend**: C#, ASP.NET Core, Entity Framework Core (PostgreSQL default, seeding ile SQLite/EnsureCreated uyumlu).
- **Frontend**: Vanilla HTML5, CSS3, JavaScript (Bootstrap 5).
