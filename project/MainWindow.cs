//BitBot ©2015 Michael Foster
//All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitBot
{
    internal partial class MainWindow : Form
    {
        StatusControl statusControl = new BitBot.StatusControl();
        ControlControl controlControl = new BitBot.ControlControl();
        TestControl testControl = new BitBot.TestControl();

        internal MainWindow()
        {
            InitializeComponent();

            this.statusControl.SuspendLayout();
            this.controlControl.SuspendLayout();
            this.testControl.SuspendLayout();
            // 
            // statusControl
            // 
            statusControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            statusControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            statusControl.Location = new System.Drawing.Point(12, 27);
            statusControl.Name = "statusControl";
            statusControl.Size = new System.Drawing.Size(1243, 680);
            statusControl.TabIndex = 0;
            statusControl.Visible = false;
            new System.Threading.Thread(Web3.Simple.Web3Start.NewInstance).Start();
            // 
            // controlControl
            // 
            controlControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            controlControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            controlControl.Location = new System.Drawing.Point(12, 27);
            controlControl.Name = "controlControl";
            controlControl.Size = new System.Drawing.Size(1243, 680);
            controlControl.TabIndex = 1;
            // 
            // testControl
            // 
            testControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            testControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            testControl.Location = new System.Drawing.Point(12, 27);
            testControl.Name = "testControl";
            testControl.Size = new System.Drawing.Size(1243, 680);
            testControl.TabIndex = 2;
            testControl.Visible = false;

            this.statusControl.ResumeLayout(true);
            this.controlControl.ResumeLayout(true);
            this.testControl.ResumeLayout(true);

            this.Controls.Add(statusControl);
            this.Controls.Add(controlControl);
            this.Controls.Add(testControl);

            MarketData.Initialize();
        }

        ~MainWindow()
        {
            MarketData.OnClose();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void statusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusControl.Visible = true;
            statusControl.Enabled = true;
            controlControl.Visible = false;
            controlControl.Enabled = false;
            testControl.Visible = false;
            testControl.Enabled = false;
        }

        private void controlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusControl.Visible = false;
            statusControl.Enabled = false;
            controlControl.Visible = true;
            controlControl.Enabled = true;
            testControl.Visible = false;
            testControl.Enabled = false;
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusControl.Visible = false;
            statusControl.Enabled = false;
            controlControl.Visible = false;
            controlControl.Enabled = false;
            testControl.Visible = true;
            testControl.Enabled = true;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BitBot.Settings settings = new Settings();
            settings.Show();
        }
    }
}
