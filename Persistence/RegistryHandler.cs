using Microsoft.Win32; // Registry işlemleri için
using System;
using System.Drawing;
using System.Globalization; // Sayı formatları için
using Thermal.Core; // AppSettings için using eklendi
using System.Windows.Forms; // Application.ExecutablePath için

namespace Thermal.Persistence // Namespace güncellendi
{
    /// <summary>
    /// Uygulama ayarlarını Windows Kayıt Defteri'ne kaydeder ve yükler.
    /// </summary>
    internal static class RegistryHandler
    {
        // Ayarların kaydedileceği anahtar yolu (CurrentUser altında)
        private const string RegistryPath = @"Software\ThermalApp";
        // Başlangıç anahtar yolu
        private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        // Kayıt defterindeki uygulama adı (Başlangıç için)
        private const string AppName = "ThermalWatcher";

        /// <summary>
        /// Verilen AppSettings nesnesini Kayıt Defteri'ne kaydeder.
        /// </summary>
        /// <param name="settings">Kaydedilecek ayarlar.</param>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                // Anahtarı aç (yoksa oluştur) ve yazma izni iste
                using (RegistryKey? key = Registry.CurrentUser.CreateSubKey(RegistryPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine("RegistryHandler Hata: Kayıt defteri anahtarı oluşturulamadı/açılamadı.");
                        return;
                    }

                    // Değerleri kaydet
                    key.SetValue("ShortUpdateIntervalMs", settings.ShortUpdateIntervalMs, RegistryValueKind.DWord);
                    key.SetValue("LongUpdateIntervalMs", settings.LongUpdateIntervalMs, RegistryValueKind.DWord);
                    key.SetValue("HideDelayMs", settings.HideDelayMs, RegistryValueKind.DWord);

                    // Float değerleri InvariantCulture ile string olarak kaydetmek daha güvenli olabilir
                    key.SetValue("TempThreshold1", settings.TempThreshold1.ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);
                    key.SetValue("TempThreshold2", settings.TempThreshold2.ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);

                    // Renkleri ARGB integer olarak kaydet
                    key.SetValue("ColorLowTemp", settings.ColorLowTemp.ToArgb(), RegistryValueKind.DWord);
                    key.SetValue("ColorMidTemp", settings.ColorMidTemp.ToArgb(), RegistryValueKind.DWord);
                    key.SetValue("ColorHighTemp", settings.ColorHighTemp.ToArgb(), RegistryValueKind.DWord);

                    // Boolean değeri integer olarak kaydet (1=true, 0=false)
                    key.SetValue("EnableMouseHoverShow", settings.EnableMouseHoverShow ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("StartWithWindows", settings.StartWithWindows ? 1 : 0, RegistryValueKind.DWord);

                    Console.WriteLine("RegistryHandler: Ayarlar başarıyla kaydedildi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegistryHandler Hata: Ayarlar kaydedilirken hata oluştu: {ex.Message}");
                // Hata durumunda kullanıcıya bilgi verilebilir (opsiyonel)
                // MessageBox.Show($"Ayarlar kaydedilirken bir hata oluştu:\n{ex.Message}", "Kayıt Defteri Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Ayarları Kayıt Defteri'nden yükler. Kayıt bulunamazsa varsayılan ayarlarla döner.
        /// </summary>
        /// <returns>Yüklenen veya varsayılan AppSettings nesnesi.</returns>
        public static AppSettings LoadSettings()
        {
            AppSettings settings = new AppSettings(); // Varsayılan değerlerle başla

            try
            {
                // Anahtarı oku (yoksa null döner)
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath, false)) // Sadece okuma
                {
                    if (key == null)
                    {
                        Console.WriteLine("RegistryHandler: Kayıt defteri anahtarı bulunamadı. Varsayılan ayarlar kullanılacak.");
                        // Anahtar yoksa ilk çalıştırmadır, varsayılanları kaydedebiliriz.
                        SaveSettings(settings);
                        return settings;
                    }

                    Console.WriteLine("RegistryHandler: Ayarlar kayıt defterinden yükleniyor...");

                    // Değerleri oku (varsayılan değerlerle birlikte GetValue kullanarak)
                    settings.ShortUpdateIntervalMs = Convert.ToInt32(key.GetValue("ShortUpdateIntervalMs", settings.ShortUpdateIntervalMs));
                    settings.LongUpdateIntervalMs = Convert.ToInt32(key.GetValue("LongUpdateIntervalMs", settings.LongUpdateIntervalMs));
                    settings.HideDelayMs = Convert.ToInt32(key.GetValue("HideDelayMs", settings.HideDelayMs));

                    // Float değerleri string'den parse et
                    if (float.TryParse(key.GetValue("TempThreshold1", settings.TempThreshold1.ToString(CultureInfo.InvariantCulture))?.ToString(),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out float temp1))
                    {
                        settings.TempThreshold1 = temp1;
                    }
                    if (float.TryParse(key.GetValue("TempThreshold2", settings.TempThreshold2.ToString(CultureInfo.InvariantCulture))?.ToString(),
                        NumberStyles.Float, CultureInfo.InvariantCulture, out float temp2))
                    {
                        settings.TempThreshold2 = temp2;
                    }

                    // Renkleri ARGB integer'dan çevir
                    settings.ColorLowTemp = Color.FromArgb(Convert.ToInt32(key.GetValue("ColorLowTemp", settings.ColorLowTemp.ToArgb())));
                    settings.ColorMidTemp = Color.FromArgb(Convert.ToInt32(key.GetValue("ColorMidTemp", settings.ColorMidTemp.ToArgb())));
                    settings.ColorHighTemp = Color.FromArgb(Convert.ToInt32(key.GetValue("ColorHighTemp", settings.ColorHighTemp.ToArgb())));

                    // Boolean değerleri integer'dan çevir
                    settings.EnableMouseHoverShow = Convert.ToInt32(key.GetValue("EnableMouseHoverShow", settings.EnableMouseHoverShow ? 1 : 0)) == 1;
                    settings.StartWithWindows = Convert.ToInt32(key.GetValue("StartWithWindows", settings.StartWithWindows ? 1 : 0)) == 1;

                    Console.WriteLine("RegistryHandler: Ayarlar başarıyla yüklendi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegistryHandler Hata: Ayarlar yüklenirken hata oluştu: {ex.Message}");
                // Hata durumunda varsayılan ayarlarla devam et (settings zaten varsayılanlarla oluşturuldu)
                // MessageBox.Show($"Ayarlar yüklenirken bir hata oluştu, varsayılanlar kullanılacak:\n{ex.Message}", "Kayıt Defteri Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return settings;
        }

        /// <summary>
        /// Uygulamanın Windows başlangıcında otomatik olarak çalışmasını ayarlar.
        /// </summary>
        /// <param name="enable">True ise başlangıca ekle, False ise kaldır.</param>
        public static void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine($"RegistryHandler Hata: Başlangıç anahtarı açılamadı: {StartupRegistryPath}");
                        return;
                    }

                    string executablePath = Application.ExecutablePath;

                    if (enable)
                    {
                        // Uygulama yolunu tırnak içine alarak kaydet (boşluklu yollar için önemli)
                        key.SetValue(AppName, $"\"{executablePath}\"", RegistryValueKind.String);
                        Console.WriteLine($"RegistryHandler: Uygulama başlangıca eklendi: {AppName}");
                    }
                    else
                    {
                        // Anahtar varsa kaldır
                        if (key.GetValue(AppName) != null)
                        {
                            key.DeleteValue(AppName, false); // false: Değer yoksa hata verme
                            Console.WriteLine($"RegistryHandler: Uygulama başlangıçtan kaldırıldı: {AppName}");
                        }
                        else
                        {
                            Console.WriteLine($"RegistryHandler: Uygulama zaten başlangıçta değildi: {AppName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegistryHandler Hata: Başlangıç ayarı değiştirilirken hata oluştu: {ex.Message}");
                MessageBox.Show($"Windows başlangıç ayarı değiştirilirken bir hata oluştu:\n{ex.Message}\n\nLütfen uygulamayı yönetici olarak çalıştırmayı deneyin.",
                                "Başlangıç Ayarı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}