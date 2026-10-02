namespace Thermal.Presentation
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupTimers = new System.Windows.Forms.GroupBox();
            this.lblShortInterval = new System.Windows.Forms.Label();
            this.numShortInterval = new System.Windows.Forms.NumericUpDown();
            this.lblLongInterval = new System.Windows.Forms.Label();
            this.numLongInterval = new System.Windows.Forms.NumericUpDown();
            this.lblHideDelay = new System.Windows.Forms.Label();
            this.numHideDelay = new System.Windows.Forms.NumericUpDown();
            
            this.groupColors = new System.Windows.Forms.GroupBox();
            this.lblTempThreshold1 = new System.Windows.Forms.Label();
            this.numTempThreshold1 = new System.Windows.Forms.NumericUpDown();
            this.btnColorLow = new System.Windows.Forms.Button();
            this.lblTempThreshold2 = new System.Windows.Forms.Label();
            this.numTempThreshold2 = new System.Windows.Forms.NumericUpDown();
            this.btnColorMid = new System.Windows.Forms.Button();
            this.lblColorHigh = new System.Windows.Forms.Label();
            this.btnColorHigh = new System.Windows.Forms.Button();

            this.groupHardware = new System.Windows.Forms.GroupBox();
            this.lblCpuHardware = new System.Windows.Forms.Label();
            this.cmbCpuHardware = new System.Windows.Forms.ComboBox();
            this.lblCpuSensor = new System.Windows.Forms.Label();
            this.cmbCpuSensor = new System.Windows.Forms.ComboBox();
            this.lblGpuHardware = new System.Windows.Forms.Label();
            this.cmbGpuHardware = new System.Windows.Forms.ComboBox();
            this.lblGpuSensor = new System.Windows.Forms.Label();
            this.cmbGpuSensor = new System.Windows.Forms.ComboBox();

            this.groupGeneral = new System.Windows.Forms.GroupBox();
            this.chkEnableMouseHover = new System.Windows.Forms.CheckBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();

            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            this.groupTimers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numShortInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHideDelay)).BeginInit();
            this.groupColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold2)).BeginInit();
            this.groupHardware.SuspendLayout();
            this.groupGeneral.SuspendLayout();
            this.SuspendLayout();

            // 
            // groupTimers
            // 
            this.groupTimers.Controls.Add(this.lblShortInterval);
            this.groupTimers.Controls.Add(this.numShortInterval);
            this.groupTimers.Controls.Add(this.lblLongInterval);
            this.groupTimers.Controls.Add(this.numLongInterval);
            this.groupTimers.Controls.Add(this.lblHideDelay);
            this.groupTimers.Controls.Add(this.numHideDelay);
            this.groupTimers.Location = new System.Drawing.Point(12, 12);
            this.groupTimers.Name = "groupTimers";
            this.groupTimers.Size = new System.Drawing.Size(280, 110);
            this.groupTimers.TabIndex = 0;
            this.groupTimers.TabStop = false;
            this.groupTimers.Text = "Güncelleme Sıklığı (Saniye)";
            // 
            // lblShortInterval
            // 
            this.lblShortInterval.Location = new System.Drawing.Point(15, 25);
            this.lblShortInterval.Name = "lblShortInterval";
            this.lblShortInterval.Size = new System.Drawing.Size(140, 20);
            this.lblShortInterval.Text = "Normal Güncelleme Aralığı:";
            this.lblShortInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numShortInterval
            // 
            this.numShortInterval.Location = new System.Drawing.Point(170, 23);
            this.numShortInterval.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numShortInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numShortInterval.Name = "numShortInterval";
            this.numShortInterval.Size = new System.Drawing.Size(60, 20);
            this.numShortInterval.TabIndex = 1;
            this.numShortInterval.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblLongInterval
            // 
            this.lblLongInterval.Location = new System.Drawing.Point(15, 53);
            this.lblLongInterval.Name = "lblLongInterval";
            this.lblLongInterval.Size = new System.Drawing.Size(140, 20);
            this.lblLongInterval.Text = "Gizli Güncelleme Aralığı:";
            this.lblLongInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numLongInterval
            // 
            this.numLongInterval.Location = new System.Drawing.Point(170, 51);
            this.numLongInterval.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            this.numLongInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 }); // Minimum 1 saniye yapıldı arka plan pollemeyi hızlı yapabilmek için
            this.numLongInterval.Name = "numLongInterval";
            this.numLongInterval.Size = new System.Drawing.Size(60, 20);
            this.numLongInterval.TabIndex = 3;
            this.numLongInterval.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblHideDelay
            // 
            this.lblHideDelay.Location = new System.Drawing.Point(15, 81);
            this.lblHideDelay.Name = "lblHideDelay";
            this.lblHideDelay.Size = new System.Drawing.Size(140, 20);
            this.lblHideDelay.Text = "Gizleme Gecikmesi:";
            this.lblHideDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numHideDelay
            // 
            this.numHideDelay.Location = new System.Drawing.Point(170, 79);
            this.numHideDelay.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            this.numHideDelay.Name = "numHideDelay";
            this.numHideDelay.Size = new System.Drawing.Size(60, 20);
            this.numHideDelay.TabIndex = 5;
            this.numHideDelay.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // groupColors
            // 
            this.groupColors.Controls.Add(this.lblTempThreshold1);
            this.groupColors.Controls.Add(this.numTempThreshold1);
            this.groupColors.Controls.Add(this.btnColorLow);
            this.groupColors.Controls.Add(this.lblTempThreshold2);
            this.groupColors.Controls.Add(this.numTempThreshold2);
            this.groupColors.Controls.Add(this.btnColorMid);
            this.groupColors.Controls.Add(this.lblColorHigh);
            this.groupColors.Controls.Add(this.btnColorHigh);
            this.groupColors.Location = new System.Drawing.Point(12, 130);
            this.groupColors.Name = "groupColors";
            this.groupColors.Size = new System.Drawing.Size(280, 120);
            this.groupColors.TabIndex = 1;
            this.groupColors.TabStop = false;
            this.groupColors.Text = "Sıcaklık Eşikleri ve Renkler";
            // 
            // lblTempThreshold1
            // 
            this.lblTempThreshold1.Location = new System.Drawing.Point(15, 25);
            this.lblTempThreshold1.Name = "lblTempThreshold1";
            this.lblTempThreshold1.Size = new System.Drawing.Size(140, 20);
            this.lblTempThreshold1.Text = "Sıcaklık Eşiği 1 (Düşük):";
            this.lblTempThreshold1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numTempThreshold1
            // 
            this.numTempThreshold1.DecimalPlaces = 1;
            this.numTempThreshold1.Location = new System.Drawing.Point(170, 23);
            this.numTempThreshold1.Maximum = new decimal(new int[] { 119, 0, 0, 0 });
            this.numTempThreshold1.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numTempThreshold1.Name = "numTempThreshold1";
            this.numTempThreshold1.Size = new System.Drawing.Size(60, 20);
            this.numTempThreshold1.TabIndex = 1;
            this.numTempThreshold1.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // btnColorLow
            // 
            this.btnColorLow.Location = new System.Drawing.Point(236, 22);
            this.btnColorLow.Name = "btnColorLow";
            this.btnColorLow.Size = new System.Drawing.Size(30, 22);
            this.btnColorLow.TabIndex = 2;
            this.btnColorLow.UseVisualStyleBackColor = false;
            this.btnColorLow.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // lblTempThreshold2
            // 
            this.lblTempThreshold2.Location = new System.Drawing.Point(15, 55);
            this.lblTempThreshold2.Name = "lblTempThreshold2";
            this.lblTempThreshold2.Size = new System.Drawing.Size(140, 20);
            this.lblTempThreshold2.Text = "Sıcaklık Eşiği 2 (Orta):";
            this.lblTempThreshold2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numTempThreshold2
            // 
            this.numTempThreshold2.DecimalPlaces = 1;
            this.numTempThreshold2.Location = new System.Drawing.Point(170, 53);
            this.numTempThreshold2.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            this.numTempThreshold2.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numTempThreshold2.Name = "numTempThreshold2";
            this.numTempThreshold2.Size = new System.Drawing.Size(60, 20);
            this.numTempThreshold2.TabIndex = 4;
            this.numTempThreshold2.Value = new decimal(new int[] { 70, 0, 0, 0 });
            // 
            // btnColorMid
            // 
            this.btnColorMid.Location = new System.Drawing.Point(236, 52);
            this.btnColorMid.Name = "btnColorMid";
            this.btnColorMid.Size = new System.Drawing.Size(30, 22);
            this.btnColorMid.TabIndex = 5;
            this.btnColorMid.UseVisualStyleBackColor = false;
            this.btnColorMid.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // lblColorHigh
            // 
            this.lblColorHigh.Location = new System.Drawing.Point(15, 85);
            this.lblColorHigh.Name = "lblColorHigh";
            this.lblColorHigh.Size = new System.Drawing.Size(140, 20);
            this.lblColorHigh.Text = "Yüksek Sıcaklık Rengi:";
            this.lblColorHigh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnColorHigh
            // 
            this.btnColorHigh.Location = new System.Drawing.Point(170, 83);
            this.btnColorHigh.Name = "btnColorHigh";
            this.btnColorHigh.Size = new System.Drawing.Size(96, 24);
            this.btnColorHigh.TabIndex = 7;
            this.btnColorHigh.UseVisualStyleBackColor = false;
            this.btnColorHigh.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // groupHardware
            // 
            this.groupHardware.Controls.Add(this.lblCpuHardware);
            this.groupHardware.Controls.Add(this.cmbCpuHardware);
            this.groupHardware.Controls.Add(this.lblCpuSensor);
            this.groupHardware.Controls.Add(this.cmbCpuSensor);
            this.groupHardware.Controls.Add(this.lblGpuHardware);
            this.groupHardware.Controls.Add(this.cmbGpuHardware);
            this.groupHardware.Controls.Add(this.lblGpuSensor);
            this.groupHardware.Controls.Add(this.cmbGpuSensor);
            this.groupHardware.Location = new System.Drawing.Point(12, 260);
            this.groupHardware.Name = "groupHardware";
            this.groupHardware.Size = new System.Drawing.Size(280, 140);
            this.groupHardware.TabIndex = 2;
            this.groupHardware.TabStop = false;
            this.groupHardware.Text = "Donanım ve Sensör Seçimi";
            // 
            // lblCpuHardware
            // 
            this.lblCpuHardware.Location = new System.Drawing.Point(15, 23);
            this.lblCpuHardware.Name = "lblCpuHardware";
            this.lblCpuHardware.Size = new System.Drawing.Size(80, 20);
            this.lblCpuHardware.Text = "CPU Seçimi:";
            this.lblCpuHardware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbCpuHardware
            // 
            this.cmbCpuHardware.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCpuHardware.Location = new System.Drawing.Point(100, 20);
            this.cmbCpuHardware.Name = "cmbCpuHardware";
            this.cmbCpuHardware.Size = new System.Drawing.Size(165, 21);
            this.cmbCpuHardware.TabIndex = 1;
            // 
            // lblCpuSensor
            // 
            this.lblCpuSensor.Location = new System.Drawing.Point(15, 51);
            this.lblCpuSensor.Name = "lblCpuSensor";
            this.lblCpuSensor.Size = new System.Drawing.Size(80, 20);
            this.lblCpuSensor.Text = "CPU Sensörü:";
            this.lblCpuSensor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbCpuSensor
            // 
            this.cmbCpuSensor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCpuSensor.Location = new System.Drawing.Point(100, 48);
            this.cmbCpuSensor.Name = "cmbCpuSensor";
            this.cmbCpuSensor.Size = new System.Drawing.Size(165, 21);
            this.cmbCpuSensor.TabIndex = 3;
            // 
            // lblGpuHardware
            // 
            this.lblGpuHardware.Location = new System.Drawing.Point(15, 79);
            this.lblGpuHardware.Name = "lblGpuHardware";
            this.lblGpuHardware.Size = new System.Drawing.Size(80, 20);
            this.lblGpuHardware.Text = "GPU Seçimi:";
            this.lblGpuHardware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbGpuHardware
            // 
            this.cmbGpuHardware.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGpuHardware.Location = new System.Drawing.Point(100, 76);
            this.cmbGpuHardware.Name = "cmbGpuHardware";
            this.cmbGpuHardware.Size = new System.Drawing.Size(165, 21);
            this.cmbGpuHardware.TabIndex = 5;
            // 
            // lblGpuSensor
            // 
            this.lblGpuSensor.Location = new System.Drawing.Point(15, 107);
            this.lblGpuSensor.Name = "lblGpuSensor";
            this.lblGpuSensor.Size = new System.Drawing.Size(80, 20);
            this.lblGpuSensor.Text = "GPU Sensörü:";
            this.lblGpuSensor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbGpuSensor
            // 
            this.cmbGpuSensor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGpuSensor.Location = new System.Drawing.Point(100, 104);
            this.cmbGpuSensor.Name = "cmbGpuSensor";
            this.cmbGpuSensor.Size = new System.Drawing.Size(165, 21);
            this.cmbGpuSensor.TabIndex = 7;
            // 
            // groupGeneral
            // 
            this.groupGeneral.Controls.Add(this.chkEnableMouseHover);
            this.groupGeneral.Controls.Add(this.chkStartWithWindows);
            this.groupGeneral.Location = new System.Drawing.Point(12, 410);
            this.groupGeneral.Name = "groupGeneral";
            this.groupGeneral.Size = new System.Drawing.Size(280, 75);
            this.groupGeneral.TabIndex = 3;
            this.groupGeneral.TabStop = false;
            this.groupGeneral.Text = "Genel Ayarlar";
            // 
            // chkEnableMouseHover
            // 
            this.chkEnableMouseHover.Location = new System.Drawing.Point(18, 22);
            this.chkEnableMouseHover.Name = "chkEnableMouseHover";
            this.chkEnableMouseHover.Size = new System.Drawing.Size(240, 20);
            this.chkEnableMouseHover.TabIndex = 0;
            this.chkEnableMouseHover.Text = "Fare Üzerine Gelince Göster";
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.Location = new System.Drawing.Point(18, 46);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(240, 20);
            this.chkStartWithWindows.TabIndex = 1;
            this.chkStartWithWindows.Text = "Windows ile Başlat";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(116, 495);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(80, 26);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Kaydet";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(202, 495);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 26);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "İptal";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(304, 535);
            this.Controls.Add(this.groupTimers);
            this.Controls.Add(this.groupColors);
            this.Controls.Add(this.groupHardware);
            this.Controls.Add(this.groupGeneral);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thermal Ayarları";
            this.groupTimers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numShortInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHideDelay)).EndInit();
            this.groupColors.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold2)).EndInit();
            this.groupHardware.ResumeLayout(false);
            this.groupGeneral.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupTimers;
        private System.Windows.Forms.Label lblShortInterval;
        private System.Windows.Forms.NumericUpDown numShortInterval;
        private System.Windows.Forms.Label lblLongInterval;
        private System.Windows.Forms.NumericUpDown numLongInterval;
        private System.Windows.Forms.Label lblHideDelay;
        private System.Windows.Forms.NumericUpDown numHideDelay;

        private System.Windows.Forms.GroupBox groupColors;
        private System.Windows.Forms.Label lblTempThreshold1;
        private System.Windows.Forms.NumericUpDown numTempThreshold1;
        private System.Windows.Forms.Button btnColorLow;
        private System.Windows.Forms.Label lblTempThreshold2;
        private System.Windows.Forms.NumericUpDown numTempThreshold2;
        private System.Windows.Forms.Button btnColorMid;
        private System.Windows.Forms.Label lblColorHigh;
        private System.Windows.Forms.Button btnColorHigh;

        private System.Windows.Forms.GroupBox groupHardware;
        private System.Windows.Forms.Label lblCpuHardware;
        private System.Windows.Forms.ComboBox cmbCpuHardware;
        private System.Windows.Forms.Label lblCpuSensor;
        private System.Windows.Forms.ComboBox cmbCpuSensor;
        private System.Windows.Forms.Label lblGpuHardware;
        private System.Windows.Forms.ComboBox cmbGpuHardware;
        private System.Windows.Forms.Label lblGpuSensor;
        private System.Windows.Forms.ComboBox cmbGpuSensor;

        private System.Windows.Forms.GroupBox groupGeneral;
        private System.Windows.Forms.CheckBox chkEnableMouseHover;
        private System.Windows.Forms.CheckBox chkStartWithWindows;

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}