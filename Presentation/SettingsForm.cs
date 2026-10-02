using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Thermal.Core;
using Thermal.Monitoring;

namespace Thermal.Presentation
{
    public partial class SettingsForm : Form
    {
        private AppSettings currentSettings;
        private HardwareMonitor? hardwareMonitor;
        private ToolTip? toolTip;

        public SettingsForm(AppSettings settings, HardwareMonitor? monitor = null)
        {
            InitializeComponent();
            currentSettings = settings;
            hardwareMonitor = monitor;
            
            ApplyDarkTheme();
            LoadSettings();
            SetupToolTips();
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(28, 28, 30);
            this.ForeColor = Color.FromArgb(240, 240, 240);

            // Style all controls recursively
            StyleControlsRecursive(this.Controls);
        }

        private void StyleControlsRecursive(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                if (ctrl is GroupBox gb)
                {
                    gb.ForeColor = Color.FromArgb(240, 240, 240);
                    StyleControlsRecursive(gb.Controls);
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = Color.FromArgb(200, 200, 200);
                }
                else if (ctrl is CheckBox chk)
                {
                    chk.ForeColor = Color.FromArgb(240, 240, 240);
                    chk.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is NumericUpDown num)
                {
                    num.BackColor = Color.FromArgb(44, 44, 46);
                    num.ForeColor = Color.White;
                    num.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is ComboBox cmb)
                {
                    cmb.BackColor = Color.FromArgb(44, 44, 46);
                    cmb.ForeColor = Color.White;
                    cmb.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is Button btn)
                {
                    // If it is color picker button, we don't overwrite its BackColor
                    if (btn.Name == "btnColorLow" || btn.Name == "btnColorMid" || btn.Name == "btnColorHigh")
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 1;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                    }
                    else
                    {
                        btn.BackColor = Color.FromArgb(50, 50, 52);
                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Cursor = Cursors.Hand;
                    }
                }
            }
        }

        private void LoadSettings()
        {
            numShortInterval.Value = currentSettings.ShortUpdateIntervalMs / 1000;
            numLongInterval.Value = currentSettings.LongUpdateIntervalMs / 1000;
            numHideDelay.Value = currentSettings.HideDelayMs / 1000;

            numTempThreshold1.Value = (decimal)currentSettings.TempThreshold1;
            btnColorLow.BackColor = currentSettings.ColorLowTemp;

            numTempThreshold2.Value = (decimal)currentSettings.TempThreshold2;
            btnColorMid.BackColor = currentSettings.ColorMidTemp;
            btnColorHigh.BackColor = currentSettings.ColorHighTemp;

            chkEnableMouseHover.Checked = currentSettings.EnableMouseHoverShow;
            chkStartWithWindows.Checked = currentSettings.StartWithWindows;

            // Load CPU Hardware list
            cmbCpuHardware.Items.Clear();
            cmbCpuHardware.Items.Add("Otomatik Algıla (Varsayılan)");
            cmbCpuHardware.SelectedIndex = 0;
            if (hardwareMonitor != null)
            {
                var cpus = hardwareMonitor.GetCpuNames();
                foreach (var cpu in cpus)
                {
                    cmbCpuHardware.Items.Add(cpu);
                    if (cpu.Equals(currentSettings.SelectedCpuName, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbCpuHardware.SelectedItem = cpu;
                    }
                }
            }

            // Load CPU Sensor Prefs
            cmbCpuSensor.Items.Clear();
            cmbCpuSensor.Items.Add("CPU Package (Önerilen)");
            cmbCpuSensor.Items.Add("Core Max");
            cmbCpuSensor.Items.Add("En Sıcak Çekirdek");
            if (currentSettings.CpuSensorPreference >= 0 && currentSettings.CpuSensorPreference < cmbCpuSensor.Items.Count)
            {
                cmbCpuSensor.SelectedIndex = currentSettings.CpuSensorPreference;
            }
            else
            {
                cmbCpuSensor.SelectedIndex = 0;
            }

            // Load GPU Hardware list
            cmbGpuHardware.Items.Clear();
            cmbGpuHardware.Items.Add("Otomatik Algıla (Varsayılan)");
            cmbGpuHardware.SelectedIndex = 0;
            if (hardwareMonitor != null)
            {
                var gpus = hardwareMonitor.GetGpuNames();
                foreach (var gpu in gpus)
                {
                    cmbGpuHardware.Items.Add(gpu);
                    if (gpu.Equals(currentSettings.SelectedGpuName, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbGpuHardware.SelectedItem = gpu;
                    }
                }
            }

            // Load GPU Sensor Prefs
            cmbGpuSensor.Items.Clear();
            cmbGpuSensor.Items.Add("GPU Core (Önerilen)");
            cmbGpuSensor.Items.Add("GPU Hot Spot");
            if (currentSettings.GpuSensorPreference >= 0 && currentSettings.GpuSensorPreference < cmbGpuSensor.Items.Count)
            {
                cmbGpuSensor.SelectedIndex = currentSettings.GpuSensorPreference;
            }
            else
            {
                cmbGpuSensor.SelectedIndex = 0;
            }
        }

        private void SetupToolTips()
        {
            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 10000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            toolTip.SetToolTip(numShortInterval, "Normal durumda sıcaklık değerlerinin güncellenme sıklığı (saniye).");
            toolTip.SetToolTip(numLongInterval, "'Otomatik Gizle' aktifken ve gösterge gizliyken güncelleme sıklığı (saniye).");
            toolTip.SetToolTip(numHideDelay, "Fare gösterge alanından ayrıldıktan sonra gizlenmesi için beklenecek süre (saniye).");
            toolTip.SetToolTip(numTempThreshold1, "Bu sıcaklığın altındaki değerler için kullanılacak renk.");
            toolTip.SetToolTip(btnColorLow, "Düşük sıcaklıklar için kullanılacak rengi seçin.");
            toolTip.SetToolTip(numTempThreshold2, "Bu sıcaklığın altındaki (ve Eşik 1 üzerindeki) değerler için kullanılacak renk.");
            toolTip.SetToolTip(btnColorMid, "Orta sıcaklıklar için kullanılacak rengi seçin.");
            toolTip.SetToolTip(btnColorHigh, "Eşik 2 üzerindeki sıcaklıklar için kullanılacak rengi seçin.");
            toolTip.SetToolTip(chkEnableMouseHover, "'Otomatik Gizle' aktifken, fare göstergenin üzerine geldiğinde otomatik olarak gösterilmesini sağlar.");
            toolTip.SetToolTip(chkStartWithWindows, "İşaretlendiğinde, uygulama Windows başladığında otomatik olarak çalışır.");
            toolTip.SetToolTip(cmbCpuHardware, "İzlemek istediğiniz spesifik işlemciyi seçin. Birden fazla işlemci yoksa varsayılanda bırakın.");
            toolTip.SetToolTip(cmbCpuSensor, "Hangi işlemci sıcaklık sensörünün okunacağını belirler.");
            toolTip.SetToolTip(cmbGpuHardware, "İzlemek istediğiniz ekran kartını seçin. Özellikle çift ekran kartlı laptoplarda faydalıdır.");
            toolTip.SetToolTip(cmbGpuSensor, "GPU çekirdek sıcaklığı mı (genel standart), yoksa yerel en sıcak nokta mı (Hot Spot) izlenecek.");
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;

            using (ColorDialog colorDialog = new ColorDialog())
            {
                colorDialog.Color = btn.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    btn.BackColor = colorDialog.Color;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Eşikleri kontrol et
            if (numTempThreshold1.Value >= numTempThreshold2.Value)
            {
                MessageBox.Show("Sıcaklık Eşiği 1, Sıcaklık Eşiği 2'den küçük olmalıdır.", "Geçersiz Ayar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numTempThreshold1.Focus();
                return;
            }

            // Güncelleme aralıklarını kontrol et
            if (numShortInterval.Value > numLongInterval.Value)
            {
                MessageBox.Show("Normal güncelleme aralığı, gizli güncelleme aralığından büyük olamaz.", "Geçersiz Ayar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numShortInterval.Focus();
                return;
            }

            // Ayarları güncelle
            currentSettings.ShortUpdateIntervalMs = Math.Max(1000, (int)numShortInterval.Value * 1000);
            currentSettings.LongUpdateIntervalMs = Math.Max(1000, (int)numLongInterval.Value * 1000);
            currentSettings.HideDelayMs = Math.Max(0, (int)numHideDelay.Value * 1000);

            currentSettings.TempThreshold1 = (float)numTempThreshold1.Value;
            currentSettings.ColorLowTemp = btnColorLow.BackColor;

            currentSettings.TempThreshold2 = (float)numTempThreshold2.Value;
            currentSettings.ColorMidTemp = btnColorMid.BackColor;
            currentSettings.ColorHighTemp = btnColorHigh.BackColor;

            currentSettings.EnableMouseHoverShow = chkEnableMouseHover.Checked;
            currentSettings.StartWithWindows = chkStartWithWindows.Checked;

            // CPU Seçimi
            if (cmbCpuHardware.SelectedIndex == 0)
            {
                currentSettings.SelectedCpuName = "";
            }
            else
            {
                currentSettings.SelectedCpuName = cmbCpuHardware.SelectedItem?.ToString() ?? "";
            }
            currentSettings.CpuSensorPreference = cmbCpuSensor.SelectedIndex;

            // GPU Seçimi
            if (cmbGpuHardware.SelectedIndex == 0)
            {
                currentSettings.SelectedGpuName = "";
            }
            else
            {
                currentSettings.SelectedGpuName = cmbGpuHardware.SelectedItem?.ToString() ?? "";
            }
            currentSettings.GpuSensorPreference = cmbGpuSensor.SelectedIndex;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}