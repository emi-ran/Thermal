\
<!-- Language Switcher -->
**Türkçe** | [English](README.md)

# Thermal Watcher

CPU ve GPU sıcaklıklarınızı gerçek zamanlı olarak izlemek ve bunları minimalist, özelleştirilebilir bir arayüz (overlay) ile görüntülemek için tasarlanmış hafif bir Windows uygulamasıdır.


## Özellikler

*   **Gerçek Zamanlı İzleme:** [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) kütüphanesini kullanarak CPU ve GPU sıcaklıklarını takip eder.
*   **Minimalist Arayüz:** Sıcaklıkları genellikle ekranın sağ üst köşesinde yer alan küçük, göze batmayan bir panelde gösterir.
*   **Sistem Tepsisi Entegrasyonu:** Hızlı erişim için bir içerik menüsüyle birlikte sistem tepsisinde rahatça çalışır.
*   **Özelleştirilebilir Ayarlar:**
    *   **Güncelleme Aralıkları:** Arayüz görünürken ve gizliyken (Otomatik Gizleme modu) farklı yenileme hızları ayarlayın.
    *   **Sıcaklık Eşikleri:** Sıcaklık aralıkları tanımlayın (örn. <50°C, 50-70°C, >70°C).
    *   **Renk Kodlaması:** Kolay görsel geri bildirim için sıcaklık aralıklarına farklı renkler (Düşük, Orta, Yüksek) atayın.
    *   **Otomatik Gizleme:** Fare yakınında olmadığında arayüzü otomatik olarak gizleyin.
    *   **Fareyle Gösterme:** İsteğe bağlı olarak, fare arayüzün "sıcak bölgesine" girdiğinde otomatik olarak gösterin (sadece Otomatik Gizleme etkinken çalışır).
    *   **Gizleme Gecikmesi:** Fare sıcak bölgeden ayrıldıktan sonra arayüzün gizlenmesi için ne kadar bekleneceğini yapılandırın.
*   **Yüksek Sıcaklık Önceliği:** Sıcaklıklar 'Yüksek' eşiğini aşarsa, Otomatik Gizleme ayarlarından bağımsız olarak arayüzü otomatik olarak görünür tutar ve sık sık günceller.
*   **Ayarların Kaydedilmesi:** Tercihlerinizi Windows Kayıt Defteri'ne (`HKEY_CURRENT_USER\\Software\\ThermalApp`) kaydeder, böylece uygulama yeniden başlatıldığında ayarlarınız korunur.

## Kurulum ve Kullanım

1.  **İndirme:** En son `Thermal.exe` dosyasını **[Releases (Sürümler)](https://github.com/emi-ran/Thermal-Watcher/releases/)** sayfasından indirin.
2.  **Çalıştırma:** `Thermal.exe` dosyasını çalıştırın. Donanım sensör verilerine doğru şekilde erişebildiğinden emin olmak için **Yönetici olarak çalıştırılması** önerilir.
3.  **Sistem Tepsisi:** Uygulama simgesi sistem tepsinizde görünecektir. Sağ tıklayarak şunları yapabilirsiniz:
    *   **Ayarlar...:** Davranışı ve görünümü özelleştirmek için ayarlar penceresini açın.
    *   **Otomatik Gizle:** Otomatik gizleme özelliğini açıp kapatın.
    *   **Çıkış:** Uygulamayı kapatın.

## Bağımlılıklar

*   **.NET 9 Masaüstü Çalışma Zamanı:** Eğer *framework-dependent* sürümü indirirseniz, .NET 9 Masaüstü Çalışma Zamanı'nın kurulu olması gerekir. *Self-contained* sürüm çalışma zamanını içerir ancak daha büyüktür. Çalışma zamanını [Microsoft'un .NET web sitesinden](https://dotnet.microsoft.com/tr-tr/download/dotnet/9.0) indirebilirsiniz.
*   **LibreHardwareMonitorLib:** Bu kütüphane sensör verilerine erişmek için kullanılır. Projeye NuGet aracılığıyla dahil edilmiştir.

## Kaynaktan Derleme

1.  Depoyu klonlayın: `git clone https://github.com/emi-ran/Thermal-Watcher.git`
2.  `Thermal.sln` dosyasını Visual Studio'da açın (2022 veya üzeri, .NET 9 SDK kurulu olması önerilir).
3.  Çözümü derleyin (Build > Build Solution).

Alternatif olarak .NET CLI kullanın:

```bash
git clone https://github.com/emi-ran/Thermal-Watcher.git
cd Thermal-Watcher
dotnet build -c Release
```

## Katkıda Bulunma

Katkılarınız memnuniyetle karşılanır! Hatalar, özellik istekleri veya öneriler için lütfen bir pull request gönderin veya bir issue açın.

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.