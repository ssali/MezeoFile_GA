using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mezeo;
using MezeoFileSupport;

namespace Mezeo
{
    public partial class frmIssues : Form
    {
        private MezeoFileCloud cMezeoFileCloud;

        public frmIssues()
        {
            InitializeComponent();
            ClearInfoLabels();      
        }

        public frmIssues(MezeoFileCloud mezeoFileCloud)
        {
            InitializeComponent();
            ClearInfoLabels();
   
            cMezeoFileCloud = mezeoFileCloud;
        }

        public void AddIssuesToList(List<IssueFound> issuesList)
        {
            foreach (IssueFound issue in issuesList)
            {
                AddIssueToList(issue);
            }
        }

        public void AddIssueToList(IssueFound issue)
        {
            lvIssues.Items.Add(issue.LocalFilePath);
            lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.IssueTitle);
            lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.ConflictTimeStamp.ToString("M/d/yyyy h:mm tt"));
            lvIssues.Items[lvIssues.Items.Count - 1].Tag = issue;

            

        }

        public void ClearList()
        {
            lvIssues.Items.Clear();
        }

        private void DeleteSelectedRow()
        {
            foreach (int index in lvIssues.SelectedIndices)
            {
                lvIssues.Items.RemoveAt(index);
            }

            if (lvIssues.Items.Count == 0)
            {
                btnIgnoreConflict.Visible = false;
                ClearInfoLabels();
                DisableNameLabels();
                lblDescription.Text = "  Everything is great!    All your files are in sync and there are no conflicts or errors to report at this time.";
            }
        }

        private void DisableNameLabels()
        {
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
        }

        private void EnableNameLabels()
        {
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
        }

        private void lvIssues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvIssues.SelectedItems.Count > 0)
            {
                UpdateInfoLabels((IssueFound)lvIssues.SelectedItems[0].Tag);
            }
        }

        private void UpdateInfoLabels(IssueFound issue)
        {
            lblUpdateStatus.Text = issue.IssueTitle;
  
            lblDescription.Text = issue.IssueDescripation;
            lnkFileInfo.Text = issue.ServerFileInfo;
            lblFileSize.Text = issue.ServerSize;
            lblModified.Text = issue.ServerIssueDT.ToString("M/d/yyyy h:mm tt");

            lblLocalFileSize.Text = issue.LocalSize;
            lblLocalModifiedDate.Text = issue.LocalIssueDT.ToString("M/d/yyyy h:mm tt");
            lnkLocalFile.Text = issue.LocalFilePath;

            if (issue.cType == IssueFound.ConflictType.CONFLICT_MODIFIED)
                btnIgnoreConflict.Visible = true;
            else
                btnIgnoreConflict.Visible = false;

        }

        private void ClearInfoLabels()
        {
            lblUpdateStatus.Text = "";
            lblDescription.Text = "";

            lnkFileInfo.Text = "";
            lblFileSize.Text = "";
            lblModified.Text = "";

            lblLocalFileSize.Text ="";
            lblLocalModifiedDate.Text ="";
            lnkLocalFile.Text = "";
        }

        private void frmIssues_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearInfoLabels();
            EnableNameLabels();
            e.Cancel = true;
            this.Hide();
            return;
        }

        private void frmIssues_Shown(object sender, EventArgs e)
        {
            if (lvIssues.Items.Count > 0)
            {
                lvIssues.Items[0].Selected = true;
                UpdateInfoLabels((IssueFound)lvIssues.Items[0].Tag);
            }
        }

        private void btnIgnoreConflict_Click(object sender, EventArgs e)
        {
            DeleteSelectedRow();
        }

        private void btnEventViewer_Click(object sender, EventArgs e)
        {
            bool bRet = cMezeoFileCloud.ExceuteEventViewer(AboutBox.AssemblyTitle);
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(BasicInfo.ServiceUrl + "/help/sync");
        }

        private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        public int GetItemsInList()
        {
            return lvIssues.Items.Count;
        }

        private void lnkLocalFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lvIssues.SelectedItems.Count > 0)
            {
                IssueFound iFound = (IssueFound)lvIssues.SelectedItems[0].Tag;
                if (iFound.LocalFilePath.Length != 0)
                {
                    string argument = iFound.LocalFilePath;
                    System.Diagnostics.Process.Start(argument);
                }
            }
        }

        private void lnkFileInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
