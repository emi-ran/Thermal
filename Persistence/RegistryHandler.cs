using Microsoft.Win32; // Registry işlemleri için
using System;
using System.Drawing;
using System.Globalization; // Sayı formatları için
using Thermal.Core; // AppSettings için using eklendi
using System.Windows.Forms; // Application.ExecutablePath için
using System.Diagnostics; // Process başlatma ve schtasks kontrolü için

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

                    // Boolean değerleri integer olarak kaydet (1=true, 0=false)
                    key.SetValue("EnableMouseHoverShow", settings.EnableMouseHoverShow ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("StartWithWindows", settings.StartWithWindows ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("AutoHideEnabledPreference", settings.AutoHideEnabledPreference ? 1 : 0, RegistryValueKind.DWord);

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

                    // Değerleri oku ve sınırlandır (en az 1 saniye olması garanti edilir)
                    settings.ShortUpdateIntervalMs = Math.Max(1000, Convert.ToInt32(key.GetValue("ShortUpdateIntervalMs", settings.ShortUpdateIntervalMs)));
                    settings.LongUpdateIntervalMs = Math.Max(1000, Convert.ToInt32(key.GetValue("LongUpdateIntervalMs", settings.LongUpdateIntervalMs)));
                    settings.HideDelayMs = Math.Max(0, Convert.ToInt32(key.GetValue("HideDelayMs", settings.HideDelayMs)));

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
                    settings.AutoHideEnabledPreference = Convert.ToInt32(key.GetValue("AutoHideEnabledPreference", settings.AutoHideEnabledPreference ? 1 : 0)) == 1;

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
        /// Yönetici izinleri gerektiren uygulamalar için en yüksek yetkilerle Görev Zamanlayıcı görevi oluşturur.
        /// </summary>
        /// <param name="enable">True ise başlangıca ekle, False ise kaldır.</param>
        public static void SetStartup(bool enable)
        {
            try
            {
                // Eski kayıt defteri Run girdisi kalmışsa temizle
                CleanLegacyRegistryRun();

                string executablePath = Application.ExecutablePath;

                if (enable)
                {
                    // Görev Zamanlayıcısı'nda oturum açıldığında en yüksek yetkilerle çalışacak görev oluştur
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/create /tn \"{AppName}\" /tr \"\\\"{executablePath}\\\"\" /sc onlogon /rl highest /f",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    using (Process? process = Process.Start(startInfo))
                    {
                        process?.WaitForExit();
                        if (process?.ExitCode != 0)
                        {
                            throw new Exception($"schtasks oluşturma işlemi hata kodu ({process?.ExitCode}) ile sonuçlandı.");
                        }
                    }
                    Console.WriteLine($"RegistryHandler: Uygulama Görev Zamanlayıcısı'na eklendi: {AppName}");
                }
                else
                {
                    // Zamanlanmış görevi sil
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/delete /tn \"{AppName}\" /f",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    using (Process? process = Process.Start(startInfo))
                    {
                        process?.WaitForExit();
                        // ExitCode 0: Başarılı, 1: Görev bulunamadı (onu da başarı sayabiliriz)
                    }
                    Console.WriteLine($"RegistryHandler: Uygulama Görev Zamanlayıcısı'nden kaldırıldı: {AppName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegistryHandler Hata: Başlangıç ayarı değiştirilirken hata oluştu: {ex.Message}");
                MessageBox.Show($"Windows başlangıç ayarı Görev Zamanlayıcı ile değiştirilirken bir hata oluştu:\n{ex.Message}",
                                "Görev Zamanlayıcı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Eski Registry Run girdisini silerek temizler.
        /// </summary>
        private static void CleanLegacyRegistryRun()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (key != null && key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName, false);
                        Console.WriteLine("RegistryHandler: Eski kayıt defteri başlangıç girdisi temizlendi.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegistryHandler: Eski kayıt defteri girdisi temizlenirken hata: {ex.Message}");
            }
        }
    }
}