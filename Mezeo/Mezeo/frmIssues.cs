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
        DbHandler dbHandler;

        public frmIssues()
        {
            InitializeComponent();
            ClearInfoLabels();
            LoadResources();
        }

        public frmIssues(CloudService mezeoFileCloud)
        {
            InitializeComponent();
            dbHandler = new DbHandler();
            ClearInfoLabels();

            cMezeoFileCloud = mezeoFileCloud;
        }

        public void AddIssuesToList(List<IssueFound> issuesList)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (issuesList != null)
                    {
                        foreach (IssueFound issue in issuesList)
                        {
                            AddIssueToList(issue);
                        }
                    }
                });
            }
            else
            {
                if (issuesList != null)
                {
                    foreach (IssueFound issue in issuesList)
                    {
                        AddIssueToList(issue);
                    }
                }
            }
        }

        public void AddIssueToList(IssueFound issue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lvIssues.Items.Add(issue.LocalFilePath);
                    lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.IssueTitle);
                    lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.ConflictTimeStamp.ToString("M/d/yyyy h:mm tt"));
                    lvIssues.Items[lvIssues.Items.Count - 1].Tag = issue;
                });
            }
            else
            {
                lvIssues.Items.Add(issue.LocalFilePath);
                lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.IssueTitle);
                lvIssues.Items[lvIssues.Items.Count - 1].SubItems.Add(issue.ConflictTimeStamp.ToString("M/d/yyyy h:mm tt"));
                lvIssues.Items[lvIssues.Items.Count - 1].Tag = issue;
            }
        }

        public void ClearList()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lvIssues.Items.Clear();
                });
            }
            else
            {
                lvIssues.Items.Clear();
            }
        }

        private void DisableNameLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblLocalFileTitle.Visible = false;
                    lblLocalModifiedTitle.Visible = false;
                    lblLocalSizeTitle.Visible = false;
                    lblFileInfoTitle.Visible = false;
                    lblServerModifiedTitle.Visible = false;
                    lblServerSizeTitle.Visible = false;
                });
            }
            else
            {
                lblLocalFileTitle.Visible = false;
                lblLocalModifiedTitle.Visible = false;
                lblLocalSizeTitle.Visible = false;
                lblFileInfoTitle.Visible = false;
                lblServerModifiedTitle.Visible = false;
                lblServerSizeTitle.Visible = false;
            }
        }

        private void EnableNameLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblLocalFileTitle.Visible = true;
                    lblLocalModifiedTitle.Visible = true;
                    lblLocalSizeTitle.Visible = true;
                    lblFileInfoTitle.Visible = true;
                    lblServerModifiedTitle.Visible = true;
                    lblServerSizeTitle.Visible = true;
                });
            }
            else
            {
                lblLocalFileTitle.Visible = true;
                lblLocalModifiedTitle.Visible = true;
                lblLocalSizeTitle.Visible = true;
                lblFileInfoTitle.Visible = true;
                lblServerModifiedTitle.Visible = true;
                lblServerSizeTitle.Visible = true;
            }
        } 

        private void DeleteSelectedRow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    foreach (int index in lvIssues.SelectedIndices)
                    {
                        dbHandler.DeleteConflict(((IssueFound)lvIssues.SelectedItems[index].Tag).ConflictDbId);
                        lvIssues.Items.RemoveAt(index);
                    }

                    if (lvIssues.Items.Count == 0)
                    {
                        btnIgnoreConflict.Visible = false;
                        ClearInfoLabels();
                        DisableNameLabels();
                        lblDescription.Text = " " + LanguageTranslator.GetValue("ConflictResolveText");
                    }
                });
            }
            else
            {
                foreach (int index in lvIssues.SelectedIndices)
                {
                    dbHandler.DeleteConflict(((IssueFound)lvIssues.SelectedItems[index].Tag).ConflictDbId);
                    lvIssues.Items.RemoveAt(index);
                }

                if (lvIssues.Items.Count == 0)
                {
                    btnIgnoreConflict.Visible = false;
                    ClearInfoLabels();
                    DisableNameLabels();
                    lblDescription.Text = " " + LanguageTranslator.GetValue("ConflictResolveText");
                }
            }
        }

        private void lvIssues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (lvIssues.SelectedItems.Count > 0)
                    {
                        UpdateInfoLabels((IssueFound)lvIssues.SelectedItems[0].Tag);
                    }
                });
            }
            else
            {
                if (lvIssues.SelectedItems.Count > 0)
                {
                    UpdateInfoLabels((IssueFound)lvIssues.SelectedItems[0].Tag);
                }
            }
        }

        private void UpdateInfoLabels(IssueFound issue)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblUpdateStatus.Text = issue.IssueTitle;

                    lblDescription.Text = issue.IssueDescripation;
                    /*As file placed in deep directory structure contains backslashes, we need to replace them in to forward slashes to open the file
                    in browser.*/
                    lnkFileInfo.Text = BasicInfo.ServiceUrl + "/#info/" + issue.ServerFileInfo.Replace('\\', '/');

                    lblFileSize.Text = issue.ServerSize;
                    lblModified.Text = issue.ServerIssueDT.ToString("M/d/yyyy h:mm tt");

                    lblLocalFileSize.Text = issue.LocalSize;
                    lblLocalModifiedDate.Text = issue.LocalIssueDT.ToString("M/d/yyyy h:mm tt");
                    lnkLocalFile.Text = issue.LocalFilePath;

                    lnkLocalFile.Visible = true;
                    lnkFileInfo.Visible = true;

                    if (issue.cType == IssueFound.ConflictType.CONFLICT_MODIFIED)
                        btnIgnoreConflict.Visible = true;
                    else
                        btnIgnoreConflict.Visible = false;
                });
            }
            else
            {
                lblUpdateStatus.Text = issue.IssueTitle;

                lblDescription.Text = issue.IssueDescripation;
                /*As file placed in deep directory structure contains backslashes, we need to replace them in to forward slashes to open the file
                in browser.*/
                lnkFileInfo.Text = BasicInfo.ServiceUrl + "/#info/" + issue.ServerFileInfo.Replace('\\', '/');

                lblFileSize.Text = issue.ServerSize;
                lblModified.Text = issue.ServerIssueDT.ToString("M/d/yyyy h:mm tt");

                lblLocalFileSize.Text = issue.LocalSize;
                lblLocalModifiedDate.Text = issue.LocalIssueDT.ToString("M/d/yyyy h:mm tt");
                lnkLocalFile.Text = issue.LocalFilePath;

                lnkLocalFile.Visible = true;
                lnkFileInfo.Visible = true;
                
                if (issue.cType == IssueFound.ConflictType.CONFLICT_MODIFIED)
                    btnIgnoreConflict.Visible = true;
                else
                    btnIgnoreConflict.Visible = false;
            }
        }

        private void ClearInfoLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblUpdateStatus.Text = "";
                    lblDescription.Text = "";

                   // lnkFileInfo.Text = "";
                    lnkFileInfo.Visible = false;
                    lblFileSize.Text = "";
                    lblModified.Text = "";

                    lblLocalFileSize.Text = "";
                    lblLocalModifiedDate.Text = "";
                   // lnkLocalFile.Text = "";
                    lnkLocalFile.Visible = false;
                });
            }
            else
            {
                lblUpdateStatus.Text = "";
                lblDescription.Text = "";

              //  lnkFileInfo.Text = "";
                lnkFileInfo.Visible = false;
                lblFileSize.Text = "";
                lblModified.Text = "";

                lblLocalFileSize.Text = "";
                lblLocalModifiedDate.Text = "";
              //  lnkLocalFile.Text = "";
                lnkLocalFile.Visible = false;
            }
        }

        private void frmIssues_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearInfoLabels();
            EnableNameLabels();
            e.Cancel = true;
           
            for(int i=0; i<lvIssues.Items.Count; i++)
            {
                lvIssues.Items[i].Selected = false;
            }
            //if(lvIssues.Items.Count > 0)
            //    lvIssues.Items[0].Selected = true;
            
            //ClearInfoLabels();
            this.Hide();
            return;
        }

        private void frmIssues_Shown(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (lvIssues.Items.Count > 0)
                    {
                        lvIssues.Items[0].Selected = false;
                      //  UpdateInfoLabels((IssueFound)lvIssues.Items[0].Tag);
                    }
                });
            }
            else
            {
                if (lvIssues.Items.Count > 0)
                {
                    lvIssues.Items[0].Selected = false;
                 //   UpdateInfoLabels((IssueFound)lvIssues.Items[0].Tag);
                }
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
          List<IssueFound> issuesList =  dbHandler.GetConflicts();
          int count = 0;
          if (issuesList != null)
              count = issuesList.Count();

          return count;
        }

        private void lnkLocalFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
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
                });
            }
            else
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

                    /*No need to add content, so removing that part*/
                    string argument = BasicInfo.ServiceUrl + "/#info" + szSunUri.Substring(szSunUri.LastIndexOf("/")); ;

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
