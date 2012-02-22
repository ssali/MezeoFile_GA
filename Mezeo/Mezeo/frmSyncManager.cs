using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using MezeoFileSupport;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace Mezeo
{
    public partial class frmSyncManager : Form
    {
        NotificationManager cnotificationManager;

        #region Private Members

        private MezeoFileCloud cMezeoFileCloud;
        private LoginDetails cLoginDetails;

        private string[] statusMessages = new string[3];
        private int statusMessageCounter = 0;
        private bool isAnalysingStructure = false;
        private bool isAnalysisCompleted = false;
        public bool isSyncInProgress = false;
        private FileDownloader fileDownloder;
        private bool isDownloadingFile = false;
        private StructureDownloader stDownloader;
        private int fileDownloadCount = 0;
        private DateTime lastSync;
 
        Thread analyseThread;
        Thread downloadingThread;
        ThreadLockObject lockObject;

        #endregion

        #region Constructors
        
        public frmSyncManager()
        {
            InitializeComponent();
            LoadResources();
        }
        public frmSyncManager(MezeoFileCloud mezeoFileCloud, LoginDetails loginDetails, NotificationManager notificationManager)
        {
            InitializeComponent();
            LoadResources();

            cMezeoFileCloud = mezeoFileCloud;
            cLoginDetails = loginDetails;
            cnotificationManager = notificationManager;
        }
        
        #endregion

        #region Form Drawing Events
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //Rectangle paintRect=new Rectangle(0, 0,panel1.Width, panel1.Height);
            //LinearGradientBrush brush = new LinearGradientBrush(paintRect, Color.FromArgb(133, 213, 122), Color.FromArgb(38, 160, 31),90.0f);
            //e.Graphics.FillRectangle(brush, paintRect);
        }
        
        private void DrawRoundedRectangle(Graphics gfx, Rectangle Bounds, int CornerRadius, Pen DrawPen, Color FillColor)
        {
            int strokeOffset = Convert.ToInt32(Math.Ceiling(DrawPen.Width));
            Bounds = Rectangle.Inflate(Bounds, -strokeOffset, -strokeOffset);

            DrawPen.EndCap = DrawPen.StartCap = LineCap.Round;

            GraphicsPath gfxPath = new GraphicsPath();
            gfxPath.AddArc(Bounds.X, Bounds.Y, CornerRadius, CornerRadius, 180, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y, CornerRadius, CornerRadius, 270, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
            gfxPath.AddArc(Bounds.X, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
            gfxPath.CloseAllFigures();

            gfx.FillPath(new SolidBrush(FillColor), gfxPath);
            gfx.DrawPath(DrawPen, gfxPath);
        }
        
        private void panel_Paint(object sender, PaintEventArgs e)
        {
            Panel pnl = sender as Panel;
            Rectangle bounds = new Rectangle(0, 0, pnl.Width, pnl.Height);
            Pen pen = new Pen(Color.FromArgb(206,207,188));

            DrawRoundedRectangle(e.Graphics, bounds, 10, pen, Color.Transparent);
        }

        #endregion

        #region Form Events

        private void btnMoveFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            
            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int index = BasicInfo.SyncDirPath.LastIndexOf("\\");
                string folderName = BasicInfo.SyncDirPath.Substring(index+1);
                
                System.IO.Directory.Move(BasicInfo.SyncDirPath, browserDialog.SelectedPath + "\\" +  folderName);

                BasicInfo.SyncDirPath = browserDialog.SelectedPath + "\\" + folderName;

                lnkFolderPath.Text = BasicInfo.SyncDirPath;

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }


        private void btnSyncNow_Click(object sender, EventArgs e)
        {
            InitializeSync();
        }

        private void tmrNextSync_Tick(object sender, EventArgs e)
        {
            SyncNow();
            tmrNextSync.Enabled = false;
        }

        private void lnkFolderPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFolder();
        }

       

        private void frmSyncManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void tmrSwapStatusMessage_Tick(object sender, EventArgs e)
        {
            statusMessageCounter++;

            if (statusMessageCounter >= statusMessages.Length)
            {
                statusMessageCounter = 0;
            }

            lblStatusL1.Text = statusMessages[statusMessageCounter];
        }


        private void frmSyncManager_Load(object sender, EventArgs e)
        {
            Hide();
            UpdateUsageLabel();
        }

        private void rbSyncOn_Click(object sender, EventArgs e)
        {
            BasicInfo.AutoSync = true;
            
            if (!isSyncInProgress)
            {
                ShowSyncMessage();
            }
        }

        private void rbSyncOff_CheckedChanged(object sender, EventArgs e)
        {
            BasicInfo.AutoSync = false;
            
            if (!isSyncInProgress)
            {
                ShowSyncMessage();
            }
        }

        #endregion

        #region Downloader Events

        void fileDownloder_fileDownloadCompleted()
        {
            isSyncInProgress = false;

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                     LanguageTranslator.GetValue("TrayBalloonInitialSyncText"),
                                                                    ToolTipIcon.None);
            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");

            ShowSyncMessage();
        }

        void fileDownloder_downloadEvent(object sender, FileDownloaderEvents e)
        {
            if (!this.IsHandleCreated)
                return;

            this.Invoke((MethodInvoker)delegate
            {
                lblStatusL3.Text = e.FileName;
                if (isAnalysisCompleted)
                {
                    showProgress();

                }
                fileDownloadCount += 1;
            });
        }

        void stDownloader_downloadEvent(object sender, StructureDownloaderEvent e)
        {
            if (!this.IsHandleCreated)
                return;

            if (e.IsCompleted && !lockObject.StopThread)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    isAnalysingStructure = false;
                    tmrSwapStatusMessage.Enabled = false;
                    pbSyncProgress.Style = ProgressBarStyle.Blocks;
                    isAnalysisCompleted = true;
                    fileDownloder.IsAnalysisCompleted = true;
                    pbSyncProgress.Maximum = stDownloader.TotalFileCount - (fileDownloadCount - 1);
                    fileDownloadCount = 1;
                    lblPercentDone.Text = "";
                    lblPercentDone.Visible = true;

                    showProgress();


                    // MessageBox.Show(pbSyncProgress.Maximum.ToString());
                });
            }
        }

        void fileDownloder_cancelDownloadEvent()
        {
            isDownloadingFile = false;
            OnThreadCancel();
        }

        void stDownloader_cancelDownloadEvent()
        {
            isAnalysingStructure = false;
            tmrSwapStatusMessage.Enabled = false;
            OnThreadCancel();
        }



        #endregion

        #region Functions and Methods

        private void OnThreadCancel()
        {
            if (!isAnalysingStructure && !isDownloadingFile)
            {
                isSyncInProgress = false;
                ShowSyncMessage();
                btnSyncNow.Enabled = true;
            }
        }

        private void LoadResources()
        {
            this.Text = LanguageTranslator.GetValue("SyncManagerTitle");
            this.lblFileSync.Text = LanguageTranslator.GetValue("SyncManagerFileSyncLabel");
            this.rbSyncOff.Text = LanguageTranslator.GetValue("SyncManagerOffButtonText");
            this.rbSyncOn.Text = LanguageTranslator.GetValue("SyncManagerOnButtonText");
            this.lblFolder.Text = LanguageTranslator.GetValue("SyncManagerFolderLabel");
            this.lblStatus.Text = LanguageTranslator.GetValue("SyncManagerStatusLabel");
            this.lblUsage.Text = LanguageTranslator.GetValue("SyncManagerUsageLabel");

           // lblStatusL2.Visible = false;

            this.btnMoveFolder.Text = LanguageTranslator.GetValue("SyncManagerMoveFolderButtonText");
            this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");

            this.lnkAbout.Text = LanguageTranslator.GetValue("SyncManagerAboutLinkText");
            this.lnkHelp.Text = LanguageTranslator.GetValue("SyncManagerHelpLinkText");

            this.lblUserName.Text = BasicInfo.UserName;
            this.lnkServerUrl.Text = BasicInfo.ServiceUrl;
            this.lnkFolderPath.Text = BasicInfo.SyncDirPath;

            statusMessages[0] = LanguageTranslator.GetValue("SyncManagerAnalyseMessage1");
            statusMessages[1]= LanguageTranslator.GetValue("SyncManagerAnalyseMessage2");
            statusMessages[2] = LanguageTranslator.GetValue("SyncManagerAnalyseMessage3");
        }

        private void UpdateUsageLabel()
        {
            this.lblUsageDetails.Text = GetUsageString();
        }

        public void InitializeSync()
        {
            if (!isSyncInProgress)
            {
                isAnalysingStructure = true;
                isDownloadingFile = true;
                isSyncInProgress = true;
                label1.Visible = false;

                SyncNow();
            }
            else
            {
                btnSyncNow.Enabled = false;
                lockObject.StopThread = true;
                cMezeoFileCloud.StopSyncProcess();
            }
        }

        public void SyncNow()
        {

            //if (BasicInfo.IsInitialSync)
            {
                cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
                cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText"); ;
                cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedTitleText"),
                                                                        LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedText"),
                                                                        ToolTipIcon.None);

                Queue<LocalItemDetails> queue = new Queue<LocalItemDetails>();
                lockObject = new ThreadLockObject();
                lockObject.StopThread = false;
                stDownloader = new StructureDownloader(queue, lockObject, cLoginDetails.szContainerContentsUrl, cMezeoFileCloud);
                fileDownloder = new FileDownloader(queue, lockObject, cMezeoFileCloud, isAnalysisCompleted);
                stDownloader.downloadEvent += new StructureDownloader.StructureDownloadEvent(stDownloader_downloadEvent);
                fileDownloder.downloadEvent += new FileDownloader.FileDownloadEvent(fileDownloder_downloadEvent);
                fileDownloder.fileDownloadCompletedEvent += new FileDownloader.FileDownloadCompletedEvent(fileDownloder_fileDownloadCompleted);

                stDownloader.cancelDownloadEvent += new StructureDownloader.CancelDownLoadEvent(stDownloader_cancelDownloadEvent);
                fileDownloder.cancelDownloadEvent += new FileDownloader.CancelDownLoadEvent(fileDownloder_cancelDownloadEvent);
                analyseThread = new Thread(stDownloader.startAnalyseItemDetails);
                downloadingThread = new Thread(fileDownloder.consume);

                setUpControls();
                analyseThread.Start();
                downloadingThread.Start();
            }
        }

       
        private void showProgress()
        {
            pbSyncProgress.Value = fileDownloadCount;
            double progress = ((double)fileDownloadCount / pbSyncProgress.Maximum) * 100.0;
            lblPercentDone.Text = (int)progress + "%";
            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerDownloading") + " " + (fileDownloadCount) + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + pbSyncProgress.Maximum;
        }

        private void setUpControls()
        {
            btnSyncNow.Text = this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            
            lblStatusL1.Text = statusMessages[0];
            lblStatusL1.Visible = true;

            lblStatusL3.Text = "";
            lblStatusL3.Visible = true;

            tmrSwapStatusMessage.Enabled = true;
            
            pbSyncProgress.Style = ProgressBarStyle.Marquee;
            pbSyncProgress.Visible = true;
        }

        private string FormatSizeString(double dblSize)
        {
	        int n = 0;
	        double dblUsage = ((dblSize / 1000) / 1000)/ 1000 ;
	        if(dblUsage > 0 && dblUsage < 1)
	        {
		        n++;
		        dblUsage = (dblSize / 1000) / 1000;
		        if(dblUsage > 0 && dblUsage < 1)
		        {
			        n++;
			        dblUsage = (dblSize / 1000);
			        if(dblUsage > 0 && dblUsage < 1)
			        {
				        n++;
				        dblUsage = dblSize;
			        }
		        }
	        }

            dblUsage = Math.Truncate(dblUsage * 100) / 100;
            string strDesc = string.Format("{0:N2}", dblUsage);

	        switch(n)
	        {
	        case 0:
		        strDesc += " GB";
		        break;
	        case 1:
		        strDesc += " MB";
		        break;
	        case 2:
		        strDesc += " KB";
		        break;
	        case 3:
		        strDesc += " Bytes";
		        break;
	        default:
		        break;
	        }

            return strDesc;
        }

        private string GetUsageString()
        {
            string usedSize = FormatSizeString(cLoginDetails.dblStorage_Used);
            string allocatedSize = "";

            
            if (cLoginDetails.dblStorage_Allocated == -1)
            {
                allocatedSize = LanguageTranslator.GetValue("SyncManagerUsageUnlimited");
            }
            else
            {
                allocatedSize = FormatSizeString(cLoginDetails.dblStorage_Allocated);
            }

            allocatedSize += " " + LanguageTranslator.GetValue("SyncManagerUsageUsed");

            return usedSize + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + allocatedSize;

        }

        private void ShowSyncMessage()
        {
            lastSync = DateTime.Now;
            BasicInfo.LastSyncAt = lastSync;

            if (BasicInfo.AutoSync)
            {
                tmrNextSync.Enabled = true;
            }

            DisableProgress();
            this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");

            if (BasicInfo.AutoSync)
            {
                ShowAutoSyncMessage();
            }
            else
            {
                ShowSyncDisabledMessage();
            }
        }

        private void ShowAutoSyncMessage()
        {
           
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerStatusAllFilesInSyncLabel");
           
            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("d MMM, yyyy h:mm tt");
            label1.Visible = true;
            label1.Text = LanguageTranslator.GetValue("SyncManagerStatusNextSyncAtLabel") + " " + lastSync.AddMinutes(5).ToString("h:mm tt"); ;
            
        }

        private void ShowSyncDisabledMessage()
        {
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            label1.Text = LanguageTranslator.GetValue("SyncManagerResumeSync");
            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("d MMM, yyyy h:mm tt");
            label1.Visible = true;
        }

        private void DisableProgress()
        {
            lblPercentDone.Visible = false;
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            //lblStatusL2.Visible = true;
        }

        private void EnableProgress()
        {
            lblPercentDone.Visible = true;
            pbSyncProgress.Visible = true;
            //lblStatusL2.Visible = false;
        }

        private void OpenFolder()
        {
            string argument = BasicInfo.SyncDirPath;
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private bool ConnectedToInternet()
        {
            
            return false;
        }

        #endregion

       
    }
}
