using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thermal.Presentation
{
    internal class SystemTrayHandler : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private ToolStripMenuItem? autoHideMenuItem; // Nullable yapıldı
        private ToolStripMenuItem? settingsMenuItem;

        // Event'ler
        public event EventHandler? ExitRequested;
        public event EventHandler<bool>? AutoHideChanged;
        public event EventHandler? SettingsRequested;

        public SystemTrayHandler()
        {
            notifyIcon = new NotifyIcon();
            SetupContextMenu();
            SetupIcon();
        }

        private void SetupContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            var statusLabel = new ToolStripMenuItem("Thermal Çalışıyor") { Enabled = false };
            autoHideMenuItem = new ToolStripMenuItem("Otomatik Gizle") { CheckOnClick = true, Checked = false }; // Başlangıç durumu false
            autoHideMenuItem.CheckedChanged += OnAutoHideCheckedChanged;
            settingsMenuItem = new ToolStripMenuItem("Ayarlar...");
            settingsMenuItem.Click += OnSettingsClicked;
            var exitMenuItem = new ToolStripMenuItem("Çıkış");
            exitMenuItem.Click += OnExitClicked;

            contextMenu.Items.AddRange(new ToolStripItem[] { statusLabel, autoHideMenuItem, settingsMenuItem, new ToolStripSeparator(), exitMenuItem });
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void SetupIcon()
        {
            notifyIcon.Text = "Thermal Monitor";
            try { notifyIcon.Icon = SystemIcons.Application; } catch { }
            notifyIcon.Visible = true;
            Console.WriteLine("SystemTrayHandler: İkon gösterildi.");
        }

        private void OnAutoHideCheckedChanged(object? sender, EventArgs e)
        {
            if (autoHideMenuItem != null)
            {
                bool isChecked = autoHideMenuItem.Checked;
                Console.WriteLine($"SystemTrayHandler: Otomatik Gizle değiştirildi: {isChecked}");
                AutoHideChanged?.Invoke(this, isChecked);
            }
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("SystemTrayHandler: Ayarlar istendi.");
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            Console.WriteLine("SystemTrayHandler: Çıkış istendi.");
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        // Otomatik Gizle durumunu dışarıdan ayarlamak için
        public void SetAutoHideState(bool enabled)
        {
            if (autoHideMenuItem != null && autoHideMenuItem.Checked != enabled)
            {
                // Olayı geçici olarak kaldır
                autoHideMenuItem.CheckedChanged -= OnAutoHideCheckedChanged;
                autoHideMenuItem.Checked = enabled;
                // Olayı tekrar ekle
                autoHideMenuItem.CheckedChanged += OnAutoHideCheckedChanged;
                Console.WriteLine($"SystemTrayHandler: Otomatik Gizle durumu programatik olarak ayarlandı: {enabled} (Olay tetiklenmedi)");
            }
        }

        public void Dispose()
        {
            notifyIcon?.Dispose();
            Console.WriteLine("SystemTrayHandler: Kapatıldı.");
        }
    }
}