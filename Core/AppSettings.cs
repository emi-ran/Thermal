using System.Drawing;

namespace Thermal.Core
{
    /// <summary>
    /// Uygulama ayarlarını tutan sınıf.
    /// </summary>
    public class AppSettings
    {
        // Zamanlayıcı Ayarları
        public int ShortUpdateIntervalMs { get; set; } = 10000; // Normal güncelleme aralığı (ms)
        public int LongUpdateIntervalMs { get; set; } = 30000; // Gizliyken güncelleme aralığı (ms)

        // Renk Eşikleri ve Renkler
        public float TempThreshold1 { get; set; } = 50.0f; // Altında Yeşil olacak eşik (°C)
        public Color ColorLowTemp { get; set; } = Color.LimeGreen;

        public float TempThreshold2 { get; set; } = 70.0f; // Altında Sarı olacak eşik (°C)
        public Color ColorMidTemp { get; set; } = Color.Yellow;

        public Color ColorHighTemp { get; set; } = Color.Red; // Threshold2 üstü Kırmızı

        // Otomatik Gizleme Davranışları
        public bool EnableMouseHoverShow { get; set; } = true; // Fare üzerine gelince göster AÇIK (varsayılan)
        public int HideDelayMs { get; set; } = 5000; // Gizleme gecikmesi (ms)
        public bool StartWithWindows { get; set; } = false; // Windows ile başlama ayarı
        public bool AutoHideEnabledPreference { get; set; } = false; // Otomatik Gizle tercihi

        // TODO: Ayarları dosyaya kaydetme/yükleme eklenebilir.
        // public void Save() { ... }
        // public static AppSettings Load() { ... }
    }
}