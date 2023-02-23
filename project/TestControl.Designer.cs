namespace BitBot
{
    partial class TestControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "resxSet")]
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
            this.tbOldBook = new System.Windows.Forms.RichTextBox();
            this.tbNewBook = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbStatusMessages = new System.Windows.Forms.RichTextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.cbStepThrough = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSequence = new System.Windows.Forms.Label();
            this.tbCurrentBook = new System.Windows.Forms.RichTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbExcludeSame = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // tbOldBook
            // 
            this.tbOldBook.Location = new System.Drawing.Point(103, 26);
            this.tbOldBook.Name = "tbOldBook";
            this.tbOldBook.Size = new System.Drawing.Size(288, 485);
            this.tbOldBook.TabIndex = 0;
            this.tbOldBook.Text = "";
            // 
            // tbNewBook
            // 
            this.tbNewBook.Location = new System.Drawing.Point(691, 26);
            this.tbNewBook.Name = "tbNewBook";
            this.tbNewBook.Size = new System.Drawing.Size(288, 485);
            this.tbNewBook.TabIndex = 1;
            this.tbNewBook.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(467, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Current Order Book";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(708, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(254, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "What it should end up looking like";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1067, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Status Messages";
            // 
            // tbStatusMessages
            // 
            this.tbStatusMessages.Location = new System.Drawing.Point(985, 26);
            this.tbStatusMessages.Name = "tbStatusMessages";
            this.tbStatusMessages.Size = new System.Drawing.Size(288, 485);
            this.tbStatusMessages.TabIndex = 5;
            this.tbStatusMessages.Text = "";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(6, 26);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // cbStepThrough
            // 
            this.cbStepThrough.AutoSize = true;
            this.cbStepThrough.Checked = true;
            this.cbStepThrough.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStepThrough.Location = new System.Drawing.Point(6, 55);
            this.cbStepThrough.Name = "cbStepThrough";
            this.cbStepThrough.Size = new System.Drawing.Size(91, 17);
            this.cbStepThrough.TabIndex = 7;
            this.cbStepThrough.Text = "Step Through";
            this.cbStepThrough.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sequence";
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.Location = new System.Drawing.Point(4, 114);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(0, 13);
            this.lblSequence.TabIndex = 9;
            // 
            // tbCurrentBook
            // 
            this.tbCurrentBook.Location = new System.Drawing.Point(397, 26);
            this.tbCurrentBook.Name = "tbCurrentBook";
            this.tbCurrentBook.Size = new System.Drawing.Size(288, 485);
            this.tbCurrentBook.TabIndex = 10;
            this.tbCurrentBook.Text = "";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(173, 6);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "Old Order Book";
            // 
            // cbExcludeSame
            // 
            this.cbExcludeSame.AutoSize = true;
            this.cbExcludeSame.Checked = true;
            this.cbExcludeSame.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbExcludeSame.Location = new System.Drawing.Point(7, 79);
            this.cbExcludeSame.Name = "cbExcludeSame";
            this.cbExcludeSame.Size = new System.Drawing.Size(92, 17);
            this.cbExcludeSame.TabIndex = 12;
            this.cbExcludeSame.Text = "Hide Matches";
            this.cbExcludeSame.UseVisualStyleBackColor = true;
            // 
            // TestControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbExcludeSame);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbCurrentBook);
            this.Controls.Add(this.lblSequence);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbStepThrough);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.tbStatusMessages);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbNewBook);
            this.Controls.Add(this.tbOldBook);
            this.Name = "TestControl";
            this.Size = new System.Drawing.Size(1283, 758);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox tbOldBook;
        private System.Windows.Forms.RichTextBox tbNewBook;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox tbStatusMessages;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.CheckBox cbStepThrough;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.RichTextBox tbCurrentBook;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cbExcludeSame;
    }
}
