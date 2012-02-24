using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mezeo
{
    public partial class frmConsole : Form
    {
        public frmConsole()
        {
            InitializeComponent();
        }

        private void frmConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void LogMessage(string tag, string message)
        {
            //lvLogList.Items.Add(DateTime.Now.ToString());
            //lvLogList.Items[lvLogList.Items.Count - 1].SubItems.Add(tag);
            //lvLogList.Items[lvLogList.Items.Count - 1].SubItems.Add(message);
            //lvLogList.Items[lvLogList.Items.Count - 1].EnsureVisible();
        }

        private void lvLogList_MouseClick(object sender, MouseEventArgs e)
        {
            
        }
    }
}
