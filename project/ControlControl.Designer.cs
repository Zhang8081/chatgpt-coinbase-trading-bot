namespace BitBot
{
    partial class ControlControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblCoinBaseStatus = new System.Windows.Forms.Label();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.tbStatus = new System.Windows.Forms.RichTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSequence = new System.Windows.Forms.Label();
            this.btnTestBook = new System.Windows.Forms.Button();
            this.rbFile = new System.Windows.Forms.RadioButton();
            this.rbDisplay = new System.Windows.Forms.RadioButton();
            this.websocketSelectGroupBok = new System.Windows.Forms.GroupBox();
            this.btnTestMessage = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.websocketSelectGroupBok.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(133, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Status";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exchanges";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "coinbase";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(340, 42);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblCoinBaseStatus
            // 
            this.lblCoinBaseStatus.AutoSize = true;
            this.lblCoinBaseStatus.Location = new System.Drawing.Point(134, 47);
            this.lblCoinBaseStatus.Name = "lblCoinBaseStatus";
            this.lblCoinBaseStatus.Size = new System.Drawing.Size(76, 13);
            this.lblCoinBaseStatus.TabIndex = 5;
            this.lblCoinBaseStatus.Text = "not connected";
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(434, 42);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 6;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // tbStatus
            // 
            this.tbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStatus.Location = new System.Drawing.Point(978, 0);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.Size = new System.Drawing.Size(305, 758);
            this.tbStatus.TabIndex = 4;
            this.tbStatus.Text = "";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(220, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Sequence";
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.Location = new System.Drawing.Point(221, 47);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(27, 13);
            this.lblSequence.TabIndex = 8;
            this.lblSequence.Text = "N/A";
            // 
            // btnTestBook
            // 
            this.btnTestBook.Location = new System.Drawing.Point(340, 71);
            this.btnTestBook.Name = "btnTestBook";
            this.btnTestBook.Size = new System.Drawing.Size(134, 23);
            this.btnTestBook.TabIndex = 9;
            this.btnTestBook.Text = "Live Order Book Test";
            this.btnTestBook.UseVisualStyleBackColor = true;
            this.btnTestBook.Click += new System.EventHandler(this.btnTestBook_Click);
            // 
            // rbFile
            // 
            this.rbFile.AutoSize = true;
            this.rbFile.Checked = true;
            this.rbFile.Location = new System.Drawing.Point(6, 19);
            this.rbFile.Name = "rbFile";
            this.rbFile.Size = new System.Drawing.Size(41, 17);
            this.rbFile.TabIndex = 10;
            this.rbFile.TabStop = true;
            this.rbFile.Text = "File";
            this.rbFile.UseVisualStyleBackColor = true;
            // 
            // rbDisplay
            // 
            this.rbDisplay.AutoSize = true;
            this.rbDisplay.Location = new System.Drawing.Point(6, 42);
            this.rbDisplay.Name = "rbDisplay";
            this.rbDisplay.Size = new System.Drawing.Size(92, 17);
            this.rbDisplay.TabIndex = 11;
            this.rbDisplay.Text = "Status Display";
            this.rbDisplay.UseVisualStyleBackColor = true;
            // 
            // websocketSelectGroupBok
            // 
            this.websocketSelectGroupBok.Controls.Add(this.rbFile);
            this.websocketSelectGroupBok.Controls.Add(this.rbDisplay);
            this.websocketSelectGroupBok.Location = new System.Drawing.Point(340, 100);
            this.websocketSelectGroupBok.Name = "websocketSelectGroupBok";
            this.websocketSelectGroupBok.Size = new System.Drawing.Size(211, 67);
            this.websocketSelectGroupBok.TabIndex = 12;
            this.websocketSelectGroupBok.TabStop = false;
            this.websocketSelectGroupBok.Text = "Websocket Message Output";
            // 
            // btnTestMessage
            // 
            this.btnTestMessage.Location = new System.Drawing.Point(340, 184);
            this.btnTestMessage.Name = "btnTestMessage";
            this.btnTestMessage.Size = new System.Drawing.Size(134, 23);
            this.btnTestMessage.TabIndex = 13;
            this.btnTestMessage.Text = "Send Test Message";
            this.btnTestMessage.UseVisualStyleBackColor = true;
            this.btnTestMessage.Click += new System.EventHandler(this.btnTestMessage_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(49, 255);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(632, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "NOTE:  VALID API KEYS MUST BE ENTERED IN THE TOOLS-SETTINGS MENU FOR ANYTHING TO " +
    "WORK!";
            // 
            // ControlControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnTestMessage);
            this.Controls.Add(this.websocketSelectGroupBok);
            this.Controls.Add(this.btnTestBook);
            this.Controls.Add(this.lblSequence);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.lblCoinBaseStatus);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ControlControl";
            this.Size = new System.Drawing.Size(1283, 758);
            this.websocketSelectGroupBok.ResumeLayout(false);
            this.websocketSelectGroupBok.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblCoinBaseStatus;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.RichTextBox tbStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.Button btnTestBook;
        private System.Windows.Forms.RadioButton rbFile;
        private System.Windows.Forms.RadioButton rbDisplay;
        private System.Windows.Forms.GroupBox websocketSelectGroupBok;
        private System.Windows.Forms.Button btnTestMessage;
        private System.Windows.Forms.Label label5;
    }
}
