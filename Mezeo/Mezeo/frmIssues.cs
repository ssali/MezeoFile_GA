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
            lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.ConflictTimeStamp.ToString("d/M/yyyy h:mm tt"));
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
                ClearInfoLabels();
            }
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
            lblModified.Text = issue.ServerIssueDT.ToString();

            lblLocalFileSize.Text = issue.LocalSize;
            lblLocalModifiedDate.Text = issue.LocalIssueDT.ToString();
            lnkLocalFile.Text = issue.LocalFilePath;

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
    }
}
