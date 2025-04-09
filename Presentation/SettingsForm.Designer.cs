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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.numShortInterval = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numLongInterval = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnColorHigh = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btnColorMid = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.numTempThreshold2 = new System.Windows.Forms.NumericUpDown();
            this.btnColorLow = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numTempThreshold1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.chkEnableMouseHover = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTipSettings = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numHideDelay = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numShortInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongInterval)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold1)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHideDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = false;
            this.label1.Location = new System.Drawing.Point(15, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Normal Güncelleme Aralığı:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numShortInterval
            // 
            this.numShortInterval.Location = new System.Drawing.Point(151, 23);
            this.numShortInterval.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numShortInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numShortInterval.Name = "numShortInterval";
            this.numShortInterval.Size = new System.Drawing.Size(50, 20);
            this.numShortInterval.TabIndex = 1;
            this.numShortInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Gizli Güncelleme Aralığı:";
            // 
            // numLongInterval
            // 
            this.numLongInterval.Location = new System.Drawing.Point(151, 49);
            this.numLongInterval.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.numLongInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numLongInterval.Name = "numLongInterval";
            this.numLongInterval.Size = new System.Drawing.Size(50, 20);
            this.numLongInterval.TabIndex = 3;
            this.numLongInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numLongInterval);
            this.groupBox1.Controls.Add(this.numShortInterval);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 85);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Güncelleme Sıklığı (Saniye)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnColorHigh);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnColorMid);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.numTempThreshold2);
            this.groupBox2.Controls.Add(this.btnColorLow);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numTempThreshold1);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 103);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 115);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sıcaklık Renk Eşikleri (°C)";
            // 
            // btnColorHigh
            // 
            this.btnColorHigh.BackColor = System.Drawing.Color.Red;
            this.btnColorHigh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColorHigh.Location = new System.Drawing.Point(151, 79);
            this.btnColorHigh.Name = "btnColorHigh";
            this.btnColorHigh.Size = new System.Drawing.Size(50, 23);
            this.btnColorHigh.TabIndex = 8;
            this.btnColorHigh.UseVisualStyleBackColor = false;
            this.btnColorHigh.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Yüksek Sıcaklık Renk:";
            // 
            // btnColorMid
            // 
            this.btnColorMid.BackColor = System.Drawing.Color.Yellow;
            this.btnColorMid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColorMid.Location = new System.Drawing.Point(207, 51);
            this.btnColorMid.Name = "btnColorMid";
            this.btnColorMid.Size = new System.Drawing.Size(30, 23);
            this.btnColorMid.TabIndex = 6;
            this.btnColorMid.UseVisualStyleBackColor = false;
            this.btnColorMid.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Sıcaklık Eşiği 2 (<= Sarı):";
            // 
            // numTempThreshold2
            // 
            this.numTempThreshold2.DecimalPlaces = 1;
            this.numTempThreshold2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numTempThreshold2.Location = new System.Drawing.Point(151, 54);
            this.numTempThreshold2.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.numTempThreshold2.Name = "numTempThreshold2";
            this.numTempThreshold2.Size = new System.Drawing.Size(50, 20);
            this.numTempThreshold2.TabIndex = 5;
            this.numTempThreshold2.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // btnColorLow
            // 
            this.btnColorLow.BackColor = System.Drawing.Color.LimeGreen;
            this.btnColorLow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnColorLow.Location = new System.Drawing.Point(207, 23);
            this.btnColorLow.Name = "btnColorLow";
            this.btnColorLow.Size = new System.Drawing.Size(30, 23);
            this.btnColorLow.TabIndex = 3;
            this.btnColorLow.UseVisualStyleBackColor = false;
            this.btnColorLow.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Sıcaklık Eşiği 1 (<= Yeşil):";
            // 
            // numTempThreshold1
            // 
            this.numTempThreshold1.DecimalPlaces = 1;
            this.numTempThreshold1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numTempThreshold1.Location = new System.Drawing.Point(151, 26);
            this.numTempThreshold1.Maximum = new decimal(new int[] {
            119,
            0,
            0,
            0});
            this.numTempThreshold1.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numTempThreshold1.Name = "numTempThreshold1";
            this.numTempThreshold1.Size = new System.Drawing.Size(50, 20);
            this.numTempThreshold1.TabIndex = 2;
            this.numTempThreshold1.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(207, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 0;
            // 
            // chkEnableMouseHover
            // 
            this.chkEnableMouseHover.AutoSize = true;
            this.chkEnableMouseHover.Checked = true;
            this.chkEnableMouseHover.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableMouseHover.Location = new System.Drawing.Point(18, 51);
            this.chkEnableMouseHover.Name = "chkEnableMouseHover";
            this.chkEnableMouseHover.Size = new System.Drawing.Size(176, 17);
            this.chkEnableMouseHover.TabIndex = 2;
            this.chkEnableMouseHover.Text = "Fare Üzerine Gelince Göster/Gizle";
            this.toolTipSettings.SetToolTip(this.chkEnableMouseHover, "'Otomatik Gizle' aktifken, fare göstergenin üzerine geldiğinde otomatik olarak göst"
        + "erilmesini sağlar.");
            this.chkEnableMouseHover.UseVisualStyleBackColor = true;
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(18, 295);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(117, 17);
            this.chkStartWithWindows.TabIndex = 3;
            this.chkStartWithWindows.Text = "Windows ile Başlat";
            this.toolTipSettings.SetToolTip(this.chkStartWithWindows, "İşaretlendiğinde, uygulama Windows başladığında otomatik olarak çalışır.");
            this.chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(116, 320);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Kaydet";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(197, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "İptal";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numHideDelay);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.chkEnableMouseHover);
            this.groupBox3.Location = new System.Drawing.Point(12, 204);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(260, 83);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Otomatik Gizleme Davranışları";
            // 
            // numHideDelay
            // 
            this.numHideDelay.Location = new System.Drawing.Point(151, 24);
            this.numHideDelay.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numHideDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numHideDelay.Name = "numHideDelay";
            this.numHideDelay.Size = new System.Drawing.Size(50, 20);
            this.numHideDelay.TabIndex = 1;
            this.numHideDelay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 26);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(118, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Gizleme Gecikmesi (sn):";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(284, 360);
            this.Controls.Add(this.chkStartWithWindows);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thermal Ayarları";
            ((System.ComponentModel.ISupportInitialize)(this.numShortInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLongInterval)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTempThreshold1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHideDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
            this.groupBox3.Controls.Remove(this.chkStartWithWindows);
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numShortInterval;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numLongInterval;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnColorLow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numTempThreshold1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnColorHigh;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnColorMid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numTempThreshold2;
        private System.Windows.Forms.CheckBox chkEnableMouseHover;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTipSettings;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown numHideDelay;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkStartWithWindows;
    }
}