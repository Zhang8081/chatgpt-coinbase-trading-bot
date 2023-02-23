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
    internal partial class Settings : Form
    {
        internal Settings()
        {
            InitializeComponent();
        }

        private void Settings_Shown(object sender, EventArgs e)
        {
            nbCbMinTimeReload.Value = (decimal)coinbase_bot.Properties.Settings.Default.cbMinTimeReload;
            nbCbAutoReloadTime.Value = (decimal)coinbase_bot.Properties.Settings.Default.cbAutoReloadTime;
            nbCbWebsocketMessages.Value = (decimal)coinbase_bot.Properties.Settings.Default.cbAutoReloadMessages;

            cbCbSecondsElapsed.Checked = coinbase_bot.Properties.Settings.Default.cbTimeTrigger;
            labelCbAutoReloadTime.Enabled = coinbase_bot.Properties.Settings.Default.cbTimeTrigger;
            nbCbAutoReloadTime.Enabled = coinbase_bot.Properties.Settings.Default.cbTimeTrigger;

            cbCbWsMessages.Checked = coinbase_bot.Properties.Settings.Default.cbMessageTrigger;
            labelCbWebsocketMessages.Enabled = coinbase_bot.Properties.Settings.Default.cbMessageTrigger;
            nbCbWebsocketMessages.Enabled = coinbase_bot.Properties.Settings.Default.cbMessageTrigger;

            tbAPIID.Text = coinbase_bot.Properties.Settings.Default.tbAPIID;
            tbAPISecret.Text = coinbase_bot.Properties.Settings.Default.tbAPISecret;
            tbAPIPassphrase.Text = coinbase_bot.Properties.Settings.Default.tbAPIPassphrase;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Save();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbCbWsMessages_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbCbWsMessages.Checked && !cbCbSecondsElapsed.Checked)
            {
                cbCbSecondsElapsed.Checked = true;
            }

            labelCbWebsocketMessages.Enabled = cbCbWsMessages.Checked;
            nbCbWebsocketMessages.Enabled = cbCbWsMessages.Checked;
        }

        private void cbCbSecondsElapsed_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbCbWsMessages.Checked && !cbCbSecondsElapsed.Checked)
            {
                cbCbWsMessages.Checked = true;
            }

            labelCbAutoReloadTime.Enabled = cbCbSecondsElapsed.Checked;
            nbCbAutoReloadTime.Enabled = cbCbSecondsElapsed.Checked;
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasChanges())
            {
                DialogResult result = MessageBox.Show("Would you like to save your changes?", "Save Changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Stop);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    Save();
                    this.Close();
                }
            }
        }

        private bool HasChanges()
        {
            if (
                nbCbMinTimeReload.Value == (decimal)coinbase_bot.Properties.Settings.Default.cbMinTimeReload
                && nbCbAutoReloadTime.Value == (decimal)coinbase_bot.Properties.Settings.Default.cbAutoReloadTime
                && nbCbWebsocketMessages.Value == (decimal)coinbase_bot.Properties.Settings.Default.cbAutoReloadMessages
                && cbCbSecondsElapsed.Checked == coinbase_bot.Properties.Settings.Default.cbTimeTrigger
                && cbCbWsMessages.Checked == coinbase_bot.Properties.Settings.Default.cbMessageTrigger
                )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Save()
        {
            coinbase_bot.Properties.Settings.Default.cbMinTimeReload = decimal.ToInt32(nbCbMinTimeReload.Value);
            coinbase_bot.Properties.Settings.Default.cbAutoReloadTime = decimal.ToInt32(nbCbAutoReloadTime.Value);
            coinbase_bot.Properties.Settings.Default.cbAutoReloadMessages = decimal.ToInt32(nbCbWebsocketMessages.Value);

            coinbase_bot.Properties.Settings.Default.cbTimeTrigger = cbCbSecondsElapsed.Checked;
            coinbase_bot.Properties.Settings.Default.cbMessageTrigger = cbCbWsMessages.Checked;

            coinbase_bot.Properties.Settings.Default.tbAPIID = tbAPIID.Text;
            coinbase_bot.Properties.Settings.Default.tbAPISecret = tbAPISecret.Text;
            coinbase_bot.Properties.Settings.Default.tbAPIPassphrase = tbAPIPassphrase.Text;

            coinbase_bot.Properties.Settings.Default.Save();
        }
    }
}
