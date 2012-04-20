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
        private CloudService cMezeoFileCloud;

        public frmIssues()
        {
            InitializeComponent();
            ClearInfoLabels();
            LoadResources();
        }

        public frmIssues(CloudService mezeoFileCloud)
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

        private void DisableNameLabels()
        {
            lblLocalFileTitle.Visible = false;
            lblLocalModifiedTitle.Visible = false;
            lblLocalSizeTitle.Visible = false;
            lblFileInfoTitle.Visible = false;
            lblServerModifiedTitle.Visible = false;
            lblServerSizeTitle.Visible = false;
        }

        private void EnableNameLabels()
        {
            lblLocalFileTitle.Visible = true;
            lblLocalModifiedTitle.Visible = true;
            lblLocalSizeTitle.Visible = true;
            lblFileInfoTitle.Visible = true;
            lblServerModifiedTitle.Visible = true;
            lblServerSizeTitle.Visible = true;
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
            lnkFileInfo.Text = BasicInfo.ServiceUrl + "/#info/" + issue.ServerFileInfo;
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
            if (lvIssues.SelectedItems.Count > 0)
            {
                IssueFound iFound = (IssueFound)lvIssues.SelectedItems[0].Tag;
                if (iFound.LocalFilePath.Length != 0)
                {
                    int nLen = iFound.ServerFileUri.LastIndexOf("/");
                    string szSunUri = iFound.ServerFileUri.Substring(0, nLen);

                    string argument = BasicInfo.ServiceUrl + "/#info" + iFound.ServerFileUri.Substring(szSunUri.LastIndexOf("/")); ;
                    System.Diagnostics.Process.Start(argument);
                }
            }

        }

        private void LoadResources()
        {
            this.Text = AboutBox.AssemblyTitle + " " + LanguageTranslator.GetValue("IssuesTitle");
            lblHeader.Text = LanguageTranslator.GetValue("IssuesHeader");
            btnEventViewer.Text = LanguageTranslator.GetValue("IssuesEventViewerButtonText");
            btnIgnoreConflict.Text = LanguageTranslator.GetValue("IssuesIgnoreConflictButtonText");
            lblLocalFileTitle.Text = LanguageTranslator.GetValue("IssuesLocalFileLabel");
            lblLocalModifiedTitle.Text = LanguageTranslator.GetValue("IssuesModifiedLabel");
            lblServerModifiedTitle.Text = LanguageTranslator.GetValue("IssuesModifiedLabel");
            lblLocalSizeTitle.Text = LanguageTranslator.GetValue("IssuesSizeLabel");
            lblServerSizeTitle.Text = LanguageTranslator.GetValue("IssuesSizeLabel");
            lblFileInfoTitle.Text = LanguageTranslator.GetValue("IssuesFileInfoLabel");

            lvIssues.Columns[0].Text = LanguageTranslator.GetValue("IssuesNameColumnText");
            lvIssues.Columns[1].Text = LanguageTranslator.GetValue("IssuesIssuesColumnText");
            lvIssues.Columns[2].Text = LanguageTranslator.GetValue("IssuesDateAndTimeColumnText");

            lnkAbout.Text = LanguageTranslator.GetValue("SyncManagerAboutLinkText");
            lnkHelp.Text = LanguageTranslator.GetValue("SyncManagerHelpLinkText");

        }

        private void frmIssues_Load(object sender, EventArgs e)
        {

        }
    }
}
