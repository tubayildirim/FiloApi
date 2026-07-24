# FiloApi Proje Özeti ve Docker Rehberi

Bu dosya, projenizin mevcut teknolojik/mimari yapısını özetlemek ve geliştirme süreçlerinde kullanılan Docker konteyner teknolojisi hakkında temel rehberlik sağlamak amacıyla oluşturulmuştur.

---

## 📌 Proje Teknoloji ve Mimari Özeti

### 1. Katmanlı Mimari Yapı (Clean Architecture & CQRS)
Projeniz **Clean Architecture (Temiz Mimari)** standartlarına ve **CQRS (Command Query Responsibility Segregation)** desenine dayanır:

* **Domain**: Projenin çekirdeğidir. Bağımlılığı yoktur. Entity sınıflarını (araç, kişi vb.), Domain Event tanımlarını ve Repository arayüzlerini barındırır.
* **Application**: İş kurallarını yönetir. Sadece Domain katmanına bağımlıdır. MediatR kütüphanesi aracılığıyla Command ve Query Handler mekanizmasını yönetir. Mimari test kuralları doğrultusunda tüm CQRS istek sınıfları `sealed` (kalıtıma kapalı) olarak tasarlanmıştır.
* **Infrastructure**: Dış dünyaya açılan kapıdır. Veritabanı erişimi (Entity Framework Core), Caching servisleri (HybridCache) ve asenkron outbox arka plan işçisi burada yer alır.
* **Api**: Minimal API endpoints yapısını kullanarak HTTP isteklerini karşılar ve gerekli DTO nesnelerine dönüştürür.
* **Architecture Tests**: `NetArchTest` kütüphanesiyle yazılan 6 adet mimari kural testi, projenin katman sınırlarının ve adlandırma kurallarının bozulmasını engeller.

### 2. Modern Özellikler ve Teknolojiler
* **.NET 10 / C# 14**: En güncel .NET çalışma zamanı standartları kullanılmıştır.
* **.NET Aspire Orkestrasyonu**: Projelerin tek bir çatı altından çalıştırılmasını ve OTLP (OpenTelemetry Protocol) standartlarında entegre telemetri (logs, metrics, traces) paneline veri aktarımını sağlar.
* **Transactional Outbox Pattern**: Entity Framework veritabanı transaction bütünlüğüyle domain event'lerini `OutboxMessages` tablosuna yazar. `ProcessOutboxMessagesJob` adlı `BackgroundService` ise bu mesajları kuyruğa (`IEventBus`) asenkron olarak yansıtır. Herhangi bir hata durumunda 3 kez yeniden deneme (Retry) mekanizmasına sahiptir.
* **HybridCache (L1/L2 Önbellekleme)**: .NET'in yeni nesil önbellek kütüphanesidir. InMemory (L1) ile Distributed (L2) yapılarını koordine eder. **Cache Stampede** korumasıyla eşzamanlı veritabanı yüklenmesini engeller.
* **OpenTelemetry Custom Metrics**: İş kurallarımıza özel sayaçlar (oluşturulan araç sayısı, başarılı/başarısız outbox olay sayısı vb.) sisteme entegre edilmiştir.
* **Testcontainers Entegrasyonu**: Entegrasyon testlerinin bağımsız ve izole bir SQL Server konteyneri üzerinde çalışmasını sağlar.

---

## 🐳 Docker ve Konteyner (Container) Teknolojisi Nedir?

Docker hakkında temel bilgiler ve bunun yazılım geliştiricilere sağladığı avantajlar:

### 1. Docker Nedir?
Geleneksel olarak bir veritabanını veya servisi çalıştırmak için onu bilgisayarınıza kurmanız gerekir (Örn: Mac'e SQL Server kurmak, Redis yüklemek vb.). Docker ise uygulamaları ve onların bağımlı olduğu tüm kütüphaneleri, veritabanlarını **izole paketler** halinde çalıştırmanızı sağlayan bir platformdur.

Bu izole paketlere **Konteyner (Container)** adı verilir.

### 2. Temel Kavramlar
* **İmaj (Image)**: Bir konteynerin nasıl kurulacağını ve çalıştırılacağını tarif eden şablondur (İşletim sistemi ISO dosyası veya pasta tarifi gibidir). Örn: Microsoft SQL Server imajı veya PostgreSQL imajı.
* **Konteyner (Container)**: İmajın çalışır haldeki canlı örneğidir. (Yani tariften yapılan pastanın kendisidir). Kendi özel dosya sistemine, belleğine ve ağ bağlantılarına sahiptir. Bilgisayarınızın ana sistemini kirletmeden çalışır ve işi bittiğinde tamamen silinebilir.

### 3. Neden Kullanılır ve Avantajları Nelerdir?
* **"Benim makinemde çalışıyordu" Sorununun Çözümü**: Docker sayesinde yazdığınız kod, test ettiğiniz Docker konteynerinde nasıl çalışıyorsa, canlı sunucuda (prod ortamında) da birebir aynı şekilde çalışır.
* **Kolay Kurulum (Zero Installation)**: Bilgisayarınıza ağır SQL Server veritabanları kurup işletim sisteminizi yormak yerine; Docker sayesinde tek bir komutla saniyeler içinde bunları konteyner olarak ayağa kaldırır, işiniz bittiğinde tek tuşla kapatırsınız.
* **Testlerde Kolaylık (Testcontainers)**: Projedeki entegrasyon testlerini başlattığınızda, Testcontainers kütüphanesi arka planda Docker Desktop'a bağlanıp geçici bir SQL Server veritabanı konteyneri oluşturur. Testler bu temiz veritabanında koşar ve test bittiğinde bu konteyner otomatik olarak silinir. Bilgisayarınızda hiçbir kalıntı kalmaz.

### 4. Nasıl Çalıştırılır?
1. Bilgisayarınıza **Docker Desktop** uygulamasını indirip kurun ve çalıştırın.
2. Programın arka planda aktif olduğunu gördüğünüzde, projenizin ana dizininde şu komutla testlerinizi çalıştırabilirsiniz:
   ```bash
   dotnet test Filo.slnx
   ```
   Docker açıkken bu komut çalıştırıldığında, Testcontainers otomatik olarak Azure SQL Edge (MsSql) konteynerini indirecek, entegrasyon testlerini çalıştıracak ve konteyneri kapatacaktır.
