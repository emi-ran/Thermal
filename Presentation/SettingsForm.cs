using System;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using Thermal.Core;

namespace Thermal.Presentation
{
    public partial class SettingsForm : Form
    {
        private AppSettings currentSettings;
        private ToolTip? toolTip;

        public SettingsForm(AppSettings settings)
        {
            InitializeComponent();
            currentSettings = settings; // Dışarıdan gelen ayarları al
            LoadSettings();
            SetupToolTips();
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
            // Eşikleri kontrol et (Threshold1 < Threshold2 olmalı)
            if (numTempThreshold1.Value >= numTempThreshold2.Value)
            {
                MessageBox.Show("Sıcaklık Eşiği 1, Sıcaklık Eşiği 2'den küçük olmalıdır.", "Geçersiz Ayar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numTempThreshold1.Focus();
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