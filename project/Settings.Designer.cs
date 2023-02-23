namespace BitBot
{
    partial class Settings
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nbCbMinTimeReload = new System.Windows.Forms.NumericUpDown();
            this.labelCbAutoReloadTime = new System.Windows.Forms.Label();
            this.nbCbAutoReloadTime = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.cbCbWsMessages = new System.Windows.Forms.CheckBox();
            this.cbCbSecondsElapsed = new System.Windows.Forms.CheckBox();
            this.labelCbWebsocketMessages = new System.Windows.Forms.Label();
            this.nbCbWebsocketMessages = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbAPIID = new System.Windows.Forms.TextBox();
            this.tbAPISecret = new System.Windows.Forms.TextBox();
            this.tbAPIPassphrase = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nbCbMinTimeReload)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbCbAutoReloadTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbCbWebsocketMessages)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Location = new System.Drawing.Point(12, 287);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(578, 287);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 100;
            this.label1.Text = "Coinbase";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(304, 13);
            this.label2.TabIndex = 101;
            this.label2.Text = "Absolute minimum time between full order book loads (seconds)";
            // 
            // nbCbMinTimeReload
            // 
            this.nbCbMinTimeReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nbCbMinTimeReload.Location = new System.Drawing.Point(564, 28);
            this.nbCbMinTimeReload.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nbCbMinTimeReload.Minimum = new decimal(new int[] {
            61,
            0,
            0,
            0});
            this.nbCbMinTimeReload.Name = "nbCbMinTimeReload";
            this.nbCbMinTimeReload.Size = new System.Drawing.Size(89, 20);
            this.nbCbMinTimeReload.TabIndex = 1;
            this.nbCbMinTimeReload.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nbCbMinTimeReload.ThousandsSeparator = true;
            this.nbCbMinTimeReload.Value = new decimal(new int[] {
            61,
            0,
            0,
            0});
            // 
            // labelCbAutoReloadTime
            // 
            this.labelCbAutoReloadTime.AutoSize = true;
            this.labelCbAutoReloadTime.Location = new System.Drawing.Point(12, 120);
            this.labelCbAutoReloadTime.Name = "labelCbAutoReloadTime";
            this.labelCbAutoReloadTime.Size = new System.Drawing.Size(539, 13);
            this.labelCbAutoReloadTime.TabIndex = 102;
            this.labelCbAutoReloadTime.Text = "Time between automatic order book reloads and integrity tests if no message error" +
    " reloads are triggered (seconds)";
            // 
            // nbCbAutoReloadTime
            // 
            this.nbCbAutoReloadTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nbCbAutoReloadTime.Location = new System.Drawing.Point(564, 118);
            this.nbCbAutoReloadTime.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nbCbAutoReloadTime.Minimum = new decimal(new int[] {
            420,
            0,
            0,
            0});
            this.nbCbAutoReloadTime.Name = "nbCbAutoReloadTime";
            this.nbCbAutoReloadTime.Size = new System.Drawing.Size(89, 20);
            this.nbCbAutoReloadTime.TabIndex = 5;
            this.nbCbAutoReloadTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nbCbAutoReloadTime.ThousandsSeparator = true;
            this.nbCbAutoReloadTime.Value = new decimal(new int[] {
            450,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(173, 13);
            this.label4.TabIndex = 103;
            this.label4.Text = "Full order book reload triggered by: ";
            // 
            // cbCbWsMessages
            // 
            this.cbCbWsMessages.AutoSize = true;
            this.cbCbWsMessages.Checked = true;
            this.cbCbWsMessages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCbWsMessages.Location = new System.Drawing.Point(191, 57);
            this.cbCbWsMessages.Name = "cbCbWsMessages";
            this.cbCbWsMessages.Size = new System.Drawing.Size(184, 17);
            this.cbCbWsMessages.TabIndex = 2;
            this.cbCbWsMessages.Text = "Number of Websocket Messages";
            this.cbCbWsMessages.UseVisualStyleBackColor = true;
            this.cbCbWsMessages.CheckedChanged += new System.EventHandler(this.cbCbWsMessages_CheckedChanged);
            // 
            // cbCbSecondsElapsed
            // 
            this.cbCbSecondsElapsed.AutoSize = true;
            this.cbCbSecondsElapsed.Location = new System.Drawing.Point(381, 57);
            this.cbCbSecondsElapsed.Name = "cbCbSecondsElapsed";
            this.cbCbSecondsElapsed.Size = new System.Drawing.Size(90, 17);
            this.cbCbSecondsElapsed.TabIndex = 3;
            this.cbCbSecondsElapsed.Text = "Elapsed Time";
            this.cbCbSecondsElapsed.UseVisualStyleBackColor = true;
            this.cbCbSecondsElapsed.CheckedChanged += new System.EventHandler(this.cbCbSecondsElapsed_CheckedChanged);
            // 
            // labelCbWebsocketMessages
            // 
            this.labelCbWebsocketMessages.AutoSize = true;
            this.labelCbWebsocketMessages.Location = new System.Drawing.Point(12, 84);
            this.labelCbWebsocketMessages.Name = "labelCbWebsocketMessages";
            this.labelCbWebsocketMessages.Size = new System.Drawing.Size(381, 13);
            this.labelCbWebsocketMessages.TabIndex = 106;
            this.labelCbWebsocketMessages.Text = "Number of Websocket messages received before order book reload is triggered";
            // 
            // nbCbWebsocketMessages
            // 
            this.nbCbWebsocketMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nbCbWebsocketMessages.Location = new System.Drawing.Point(564, 82);
            this.nbCbWebsocketMessages.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nbCbWebsocketMessages.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nbCbWebsocketMessages.Name = "nbCbWebsocketMessages";
            this.nbCbWebsocketMessages.Size = new System.Drawing.Size(89, 20);
            this.nbCbWebsocketMessages.TabIndex = 4;
            this.nbCbWebsocketMessages.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nbCbWebsocketMessages.ThousandsSeparator = true;
            this.nbCbWebsocketMessages.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(378, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 107;
            this.label3.Text = "API ID:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(358, 177);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 108;
            this.label5.Text = "API Secret:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(334, 201);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 109;
            this.label6.Text = "API Passphrase:";
            // 
            // tbAPIID
            // 
            this.tbAPIID.Location = new System.Drawing.Point(425, 148);
            this.tbAPIID.Name = "tbAPIID";
            this.tbAPIID.Size = new System.Drawing.Size(228, 20);
            this.tbAPIID.TabIndex = 110;
            // 
            // tbAPISecret
            // 
            this.tbAPISecret.Location = new System.Drawing.Point(425, 174);
            this.tbAPISecret.Name = "tbAPISecret";
            this.tbAPISecret.Size = new System.Drawing.Size(228, 20);
            this.tbAPISecret.TabIndex = 111;
            // 
            // tbAPIPassphrase
            // 
            this.tbAPIPassphrase.Location = new System.Drawing.Point(425, 198);
            this.tbAPIPassphrase.Name = "tbAPIPassphrase";
            this.tbAPIPassphrase.Size = new System.Drawing.Size(228, 20);
            this.tbAPIPassphrase.TabIndex = 112;
            // 
            // Settings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(665, 322);
            this.Controls.Add(this.tbAPIPassphrase);
            this.Controls.Add(this.tbAPISecret);
            this.Controls.Add(this.tbAPIID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nbCbWebsocketMessages);
            this.Controls.Add(this.labelCbWebsocketMessages);
            this.Controls.Add(this.cbCbSecondsElapsed);
            this.Controls.Add(this.cbCbWsMessages);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nbCbAutoReloadTime);
            this.Controls.Add(this.labelCbAutoReloadTime);
            this.Controls.Add(this.nbCbMinTimeReload);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Settings";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Settings_FormClosing);
            this.Shown += new System.EventHandler(this.Settings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.nbCbMinTimeReload)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbCbAutoReloadTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nbCbWebsocketMessages)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nbCbMinTimeReload;
        private System.Windows.Forms.Label labelCbAutoReloadTime;
        private System.Windows.Forms.NumericUpDown nbCbAutoReloadTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbCbWsMessages;
        private System.Windows.Forms.CheckBox cbCbSecondsElapsed;
        private System.Windows.Forms.Label labelCbWebsocketMessages;
        private System.Windows.Forms.NumericUpDown nbCbWebsocketMessages;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbAPIID;
        private System.Windows.Forms.TextBox tbAPISecret;
        private System.Windows.Forms.TextBox tbAPIPassphrase;
    }
}