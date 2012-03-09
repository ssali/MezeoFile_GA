using System;
using System.IO;
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
using Mezeo;

namespace Mezeo
{
    public partial class frmSyncManager : Form
    {
        NotificationManager cnotificationManager;

        #region Private Members

        private MezeoFileCloud cMezeoFileCloud;
        private LoginDetails cLoginDetails;
        private frmLogin frmParent;
        private string[] statusMessages = new string[3];
        private int statusMessageCounter = 0;
        private bool isAnalysingStructure = false;
        private bool isAnalysisCompleted = false;
        
        public bool isSyncInProgress = false;
        public bool isLocalEventInProgress = false;
        public bool isEventCanceled = false;
        private FileDownloader fileDownloder;
        private bool isDownloadingFile = false;
        private StructureDownloader stDownloader;
        private int fileDownloadCount = 0;
        private DateTime lastSync;
        Queue<LocalItemDetails> queue;
        frmIssues frmIssuesFound;
        Watcher watcher;
        List<LocalEvents> LocalEventList;
        Object folderWatcherLockObject;
        //string folder = @"C:\Documents and Settings\Vinod Maurya\Mezeo File Sync (nikhil)";
        //LocalEvents[] events;
        List<LocalEvents> events;

        Thread analyseThread;
        Thread downloadingThread;
        ThreadLockObject lockObject;

        DbHandler dbHandler;

        public frmLogin ParentForm
        {
            get
            {
                return frmParent;
            }
            set
            {
                frmParent = value;
            }
        }

        #endregion

        #region Constructors and Properties
        
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

            LocalEventList = new List<LocalEvents>();
            folderWatcherLockObject = new Object();

            watcher = new Watcher(LocalEventList, lockObject, BasicInfo.SyncDirPath);
            watcher.WatchCompletedEvent += new Watcher.WatchCompleted(watcher_WatchCompletedEvent);
            CheckForIllegalCrossThreadCalls = false;

            watcher.StartMonitor();
            dbHandler = new DbHandler();
            dbHandler.OpenConnection();

            frmIssuesFound = new frmIssues(mezeoFileCloud);
        }


        public LoginDetails LoginDetail
        {
            set
            {
                cLoginDetails = value;
            }
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
                watcher.StopMonitor();
                
                int index = BasicInfo.SyncDirPath.LastIndexOf("\\");
                string folderName = BasicInfo.SyncDirPath.Substring(index+1);
                
                Directory.Move(BasicInfo.SyncDirPath, browserDialog.SelectedPath + "\\" +  folderName);

                BasicInfo.SyncDirPath = browserDialog.SelectedPath + "\\" + folderName;

                lnkFolderPath.Text = BasicInfo.SyncDirPath;
                
                watcher = new Watcher(LocalEventList, lockObject, BasicInfo.SyncDirPath);
                watcher.StartMonitor();

            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }


        private void btnSyncNow_Click(object sender, EventArgs e)
        {
            isEventCanceled = false;
            if (!isLocalEventInProgress)
            {
                InitializeSync();
            }
            else
            {
                btnSyncNow.Enabled = false;
                isLocalEventInProgress = false;
            }
        }

        private void tmrNextSync_Tick(object sender, EventArgs e)
        {           
            InitializeSync();
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

            tmrNextSync.Enabled = true;
            
            if (!isSyncInProgress)
            {
                ShowSyncMessage();
            }
        }

        private void rbSyncOff_CheckedChanged(object sender, EventArgs e)
        {
            BasicInfo.AutoSync = false;

            tmrNextSync.Enabled = false;

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
            if (BasicInfo.IsConnectedToInternet)
            {
                lblPercentDone.Text = "";
                pbSyncProgress.Visible = false;
                ShowNextSyncLabel(true);

                cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonInitialSyncText") + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText"),
                                                                        ToolTipIcon.None);
                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
                     
                //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
                //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonInitialSyncText") + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
                //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText"));

                BasicInfo.IsInitialSync = false;
                ShowSyncMessage();
            }
            else
            {
                DisableSyncManager();
            }
        }

        void ShowNextSyncLabel(bool bIsShow)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Visible = bIsShow;
                });

            }
            else
            {
                label1.Visible = bIsShow;
            }
        }

        void fileDownloder_downloadEvent(object sender, FileDownloaderEvents e)
        {
            //if (!this.IsHandleCreated)
            //    return;

            //this.Invoke((MethodInvoker)delegate
            //{
                lblStatusL3.Text = e.FileName;
                if (isAnalysisCompleted)
                {
                    showProgress();

                }
                fileDownloadCount++ ;
            //});
        }

        void stDownloader_downloadEvent(object sender, StructureDownloaderEvent e)
        {
            //if (!this.IsHandleCreated)
            //    return;

            if (e.IsCompleted && !lockObject.StopThread)
            {
                isAnalysingStructure = false;
                tmrSwapStatusMessage.Enabled = false;
                
                isAnalysisCompleted = true;
                fileDownloder.IsAnalysisCompleted = true;
                pbSyncProgress.Maximum = stDownloader.TotalFileCount - (fileDownloadCount);
                pbSyncProgress.Maximum++;
                fileDownloadCount = 1;
                lblPercentDone.Text = "";
                lblPercentDone.Visible = true;

                showProgress();

                if (!BasicInfo.IsConnectedToInternet)
                {
                    DisableSyncManager();
                }

                // MessageBox.Show(pbSyncProgress.Maximum.ToString());
                
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

        public void ApplicationExit()
        {
            if (lockObject != null)
                lockObject.ExitApplication = true;
            StopSync();
        }


        #endregion

        #region Functions and Methods

        private void OnThreadCancel()
        {
            if (!isAnalysingStructure && !isDownloadingFile)
            {
               // queue.Clear();
                if (lockObject.ExitApplication)
                    Application.Exit();
                else
                {
                    isSyncInProgress = false;
                    ShowSyncMessage(true);
                    btnSyncNow.Enabled = true;
                }
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
            this.btnIssuesFound.Text = LanguageTranslator.GetValue("SyncManagerIssueFoundButtonText");
            this.lnkAbout.Text = LanguageTranslator.GetValue("SyncManagerAboutLinkText");
            this.lnkHelp.Text = LanguageTranslator.GetValue("SyncManagerHelpLinkText");
            this.btnIssuesFound.Visible = false;

            this.lblUserName.Text = BasicInfo.UserName;
            this.lnkServerUrl.Text = BasicInfo.ServiceUrl;
            this.lnkFolderPath.Text = BasicInfo.SyncDirPath;

            statusMessages[0] = LanguageTranslator.GetValue("SyncManagerAnalyseMessage1");
            statusMessages[1]= LanguageTranslator.GetValue("SyncManagerAnalyseMessage2");
            statusMessages[2] = LanguageTranslator.GetValue("SyncManagerAnalyseMessage3");

            if (BasicInfo.AutoSync)
            {
                rbSyncOn.Checked = true;
            }
            else
            {
                rbSyncOff.Checked = true;
            }
          
        }

        private void UpdateUsageLabel()
        {
            if (cLoginDetails != null)
            {
                this.lblUsageDetails.Text = GetUsageString();
            }
            else
            {
                this.lblUsageDetails.Text = "Not Available";
            }
        }

        public void InitializeSync()
        {
            ShowNextSyncLabel(false);
            lblPercentDone.Text = "";
            pbSyncProgress.Value = 0;
            if (!isSyncInProgress)
            {
                
                btnSyncNow.Text = this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
                btnSyncNow.Refresh();
                isAnalysingStructure = true;
                isDownloadingFile = true;
                isSyncInProgress = true;
                ShowNextSyncLabel(false);
                isAnalysisCompleted = false;
                tmrNextSync.Enabled = false;
                pbSyncProgress.Visible = true;
                btnMoveFolder.Enabled = false;
                SyncNow();
            }
            else
            {
                isEventCanceled = true;
                StopSync();
            }
        }

        public void StopSync()
        {
            if (isSyncInProgress)
            {
               
                bwNQUpdate.CancelAsync();
                lblPercentDone.Text = "";
                pbSyncProgress.Visible = false;
                ShowNextSyncLabel(true);
                btnSyncNow.Enabled = false;
                if (lockObject != null)
                    lockObject.StopThread = true;

                cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                        ToolTipIcon.None);
                cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");

                //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
                //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncStopText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
                //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncStopText"));

                cMezeoFileCloud.StopSyncProcess();
            }
        }

        private void StopLocalSync()
        {
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            ShowNextSyncLabel(true);
            btnSyncNow.Enabled = true;

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                     LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                    ToolTipIcon.None);
            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncStopText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncStopText"));
        }

        public void SyncNow()
        {
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.mezeosyncstatus_syncing.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText"));
            
            //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

            if (BasicInfo.IsInitialSync)
            {
                cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedTitleText"),
                                                                        LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedText"),
                                                                        ToolTipIcon.None);

                //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedText"), LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedTitleText"));
                //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

                queue = new Queue<LocalItemDetails>();
                lockObject = new ThreadLockObject();
                lockObject.StopThread = false;
                stDownloader = new StructureDownloader(queue, lockObject, cLoginDetails.szContainerContentsUri, cMezeoFileCloud);
                fileDownloder = new FileDownloader(queue, lockObject, cMezeoFileCloud, isAnalysisCompleted);
                stDownloader.downloadEvent += new StructureDownloader.StructureDownloadEvent(stDownloader_downloadEvent);
                fileDownloder.downloadEvent += new FileDownloader.FileDownloadEvent(fileDownloder_downloadEvent);
                fileDownloder.fileDownloadCompletedEvent += new FileDownloader.FileDownloadCompletedEvent(fileDownloder_fileDownloadCompleted);

                stDownloader.cancelDownloadEvent += new StructureDownloader.CancelDownLoadEvent(stDownloader_cancelDownloadEvent);

                stDownloader.startDownloaderEvent += new StructureDownloader.StartDownLoaderEvent(stDownloader_startDownloaderEvent);

                fileDownloder.cancelDownloadEvent += new FileDownloader.CancelDownLoadEvent(fileDownloder_cancelDownloadEvent);
                analyseThread = new Thread(stDownloader.startAnalyseItemDetails);
                downloadingThread = new Thread(fileDownloder.consume);

                setUpControls();
                analyseThread.Start();
                
            }
            else
            {
                //frmParent.ShellNotifyIcon.StopNotifyIconBalloonText();
                lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerCheckingServer");
                lblStatusL3.Text = "";
                int nStatusCode = 0;
                string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;
                int nNQLength =  cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
                if (nNQLength > 0)
                {
                    pbSyncProgress.Maximum = nNQLength;
                    fileDownloadCount = 1;
                    showProgress();
                    bwNQUpdate.RunWorkerAsync();
                }
                else
                {
                    isSyncInProgress = false;
                    ShowSyncMessage();

                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                 LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                                ToolTipIcon.None);

                    cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");

                    //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
                    //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
                    //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"));
                    //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));

                    //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

                }
            }
        }

        private void UpdateFromNQ(NQDetails UpdateQ)
        {
           // int nCount = UpdateQ.Count();
            //for (int n = 0; n < nCount; n++)
            {
                NQDetails nqDetail = UpdateQ;

                int nStatusCode = 0;
                NSResult nsResult = null;
                nsResult = cMezeoFileCloud.GetNamespaceResult(cLoginDetails.szNamespaceUri + "/" +
                                                               nqDetail.StrMezeoExportedPath,
                                                               nqDetail.StrObjectType, ref nStatusCode);
                if (nsResult == null)
                    return;

                string strPath = nqDetail.StrParentUri.Substring(cLoginDetails.szNQParentUri.Length +1);
                string strKey = strPath.Replace("/" , "\\");
                strKey += nqDetail.StrObjectName;
                strPath = BasicInfo.SyncDirPath + "\\" + strKey;
                lblStatusL3.Text = strPath;

                if (nqDetail.StrEvent == "cdmi_create_complete")
                {
                    string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.CONTENT_URL + " = '" + nsResult.StrContentsUri + "'");
                    if (strDBKey.Trim().Length == 0)
                    {
                        FileFolderInfo fileFolderInfo = new FileFolderInfo();

                        fileFolderInfo.IsPublic = nsResult.bPublic;
                        fileFolderInfo.IsShared = nsResult.bShared;
                        fileFolderInfo.ContentUrl = nsResult.StrContentsUri;
                        fileFolderInfo.CreatedDate = nsResult.dtCreated;
                        fileFolderInfo.FileName = nsResult.StrName;
                        fileFolderInfo.FileSize = nsResult.dblSizeInBytes;
                        fileFolderInfo.MimeType = nsResult.StrMimeType;
                        fileFolderInfo.ModifiedDate = nsResult.dtModified;
                        fileFolderInfo.ParentUrl = nsResult.StrParentUri;
                        fileFolderInfo.Status = "SUCCESS";
                        fileFolderInfo.Type = nsResult.StrType;
                        fileFolderInfo.Key = strKey;

                        int lastSepIndex = strKey.LastIndexOf("\\");
                        string parentDirPath = "";

                        if (lastSepIndex != -1)
                        {
                            parentDirPath = strKey.Substring(0, strKey.LastIndexOf("\\"));
                            parentDirPath = parentDirPath.Substring(parentDirPath.LastIndexOf("\\") + 1);
                        }
                        
                        fileFolderInfo.ParentDir = parentDirPath;
                        bool bRet = false;
                        if (nqDetail.StrObjectType == "FILE")
                            bRet = cMezeoFileCloud.DownloadFile(nsResult.StrContentsUri + '/' + nsResult.StrName, strPath, ref nStatusCode);
                        else
                        {
                            Directory.CreateDirectory(strPath);
                            bRet = true;
                        }

                        if (bRet)
                        {
                            fileFolderInfo.ETag = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);
                            if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                            if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                            dbHandler.Write(fileFolderInfo);
                        }

                        if (nqDetail.StrObjectType == "DIRECTORY")
                        {
                            ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(nsResult.StrContentsUri, ref nStatusCode);
                            if (iDetails != null)
                            {
                                for (int num = 0; num < iDetails[0].nTotalItem; num++)
                                {
                                    DownloadFolderStructureForNQ(iDetails[num], strKey);
                                }
                            }
                        }
                    }
                }
                else if (nqDetail.StrEvent == "cdmi_modify_complete")
                {
                    string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.CONTENT_URL + " = '" + nsResult.StrContentsUri + "'");
                    string strDBEtag = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.E_TAG, DbHandler.KEY + " = '" + strDBKey + "'");
                    string strEtagCloud = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);
                    if (strDBEtag == strEtagCloud)
                    {
                        if (strKey != strDBKey)
                        {
                            File.Move(BasicInfo.SyncDirPath + "\\" + strDBKey, strPath);
                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + strKey + "'", DbHandler.KEY + "='" + strDBKey + "'");
                        }
                    }
                    else
                    {
                       bool bRet = cMezeoFileCloud.DownloadFile(nsResult.StrContentsUri + '/' + nsResult.StrName, strPath, ref nStatusCode);
                       if (bRet)
                       {
                           dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + "='" + strEtagCloud + "'", DbHandler.KEY + "='" + strKey + "'");
                           bool isFileExist = File.Exists(strPath);
                           if (isFileExist)
                           {
                               FileInfo fInfo = new FileInfo(strPath);
                               dbHandler.UpdateModifiedDate(fInfo.LastWriteTime, strKey);
                           }
                       }
                    }
                }
                else if (nqDetail.StrEvent == "cdmi_delete")
                {
                    bool isDirectory = false;
                    bool isFile = File.Exists(strPath);
                    if (!isFile)
                    {
                        isDirectory = Directory.Exists(strPath);
                        if (isDirectory)
                        {                            
                            DirectoryInfo rootDir = new DirectoryInfo(strPath);
                            WalkDirectoryTreeForDelete(rootDir);
                            Directory.Delete(strPath);
                            dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + strKey + "'");
                        }
                    }
                    else
                    {
                        File.Delete(strPath);
                        dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + strKey + "'");
                    }                      
                }
                else if (nqDetail.StrEvent == "cdmi_rename")
                {
                    string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.CONTENT_URL + " = '" + nsResult.StrContentsUri + "'");
                    if (strDBKey != strKey)
                    {
                        Directory.Move(BasicInfo.SyncDirPath + "\\" + strDBKey, strPath);
                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + strKey + "'", DbHandler.KEY + "='" + strDBKey + "'");

                        DirectoryInfo rootDir = new DirectoryInfo(strPath);
                        WalkDirectoryTree(rootDir, BasicInfo.SyncDirPath + "\\" + strDBKey);                         
                    }
                }
                else if (nqDetail.StrEvent == "cdmi_copy")
                {

                }
            }
        }

        private void DownloadFolderStructureForNQ(ItemDetails iDetail,string strParentKey)
        {
            FileFolderInfo fileFolderInfo = new FileFolderInfo();

            fileFolderInfo.IsPublic = iDetail.bPublic;
            fileFolderInfo.IsShared = iDetail.bShared;
            fileFolderInfo.ContentUrl = iDetail.szContentUrl;
            fileFolderInfo.CreatedDate = iDetail.dtCreated;
            fileFolderInfo.FileName = iDetail.strName;
            fileFolderInfo.FileSize = iDetail.dblSizeInBytes;
            fileFolderInfo.MimeType = iDetail.szMimeType;
            fileFolderInfo.ModifiedDate = iDetail.dtModified;
            fileFolderInfo.ParentUrl = iDetail.szParentUrl;
            fileFolderInfo.Status = "SUCCESS";
            fileFolderInfo.Type = iDetail.szItemType;
            fileFolderInfo.Key = strParentKey + "\\" + iDetail.strName;

            string strPath = BasicInfo.SyncDirPath + "\\" + fileFolderInfo.Key;

            int nStatusCode = 0;
            bool bRet = false;
            int lastSepIndex = fileFolderInfo.Key.LastIndexOf("\\");
            string parentDirPath = "";

            if (lastSepIndex != -1)
            {
                parentDirPath = fileFolderInfo.Key.Substring(0, fileFolderInfo.Key.LastIndexOf("\\"));
                parentDirPath = parentDirPath.Substring(parentDirPath.LastIndexOf("\\") + 1);
            }

            fileFolderInfo.ParentDir = parentDirPath;

            if (iDetail.szItemType == "FILE")
            {
                bRet = cMezeoFileCloud.DownloadFile(iDetail.szContentUrl + '/' + iDetail.strName, strPath, ref nStatusCode);
                if (bRet)
                {
                    fileFolderInfo.ETag = cMezeoFileCloud.GetETag(iDetail.szContentUrl, ref nStatusCode);
                    if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                    if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                    dbHandler.Write(fileFolderInfo);
                }
            }
            else
            {
                Directory.CreateDirectory(strPath);
                fileFolderInfo.ETag = cMezeoFileCloud.GetETag(iDetail.szContentUrl, ref nStatusCode);
                if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                dbHandler.Write(fileFolderInfo);

                ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(iDetail.szContentUrl, ref nStatusCode);
                if (iDetails != null)
                {
                    for (int num = 0; num < iDetails[0].nTotalItem; num++)
                    {
                        DownloadFolderStructureForNQ(iDetails[num], fileFolderInfo.Key);
                    }
                }
            }
        }

        void stDownloader_startDownloaderEvent(bool bStart)
        {
            if (bStart)
                downloadingThread.Start();
            else
                fileDownloder.ForceComplete();
        }

       
        private void showProgress()
        {
            double progress = ((double)fileDownloadCount / pbSyncProgress.Maximum) * 100.0;
            
            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText"));

            //if (!this.IsHandleCreated)
            //    return;

            //this.Invoke((MethodInvoker)delegate
            //{
                pbSyncProgress.Value = fileDownloadCount;
                //pbSyncProgress.Visible = true;
                //pbSyncProgress.Invalidate();
              //  pbSyncProgress.Style = ProgressBarStyle.Marquee;
                //lblPercentDone.Visible = true;
                //lblPercentDone.Invalidate();
                lblPercentDone.Text = (int)progress + "%";
                lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerDownloading") + " " + (fileDownloadCount) + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + pbSyncProgress.Maximum;

            //});
        }

        private void setUpControls()
        {
            
            
            lblStatusL1.Text = statusMessages[0];
            lblStatusL1.Visible = true;

            lblStatusL3.Text = "";
            lblStatusL3.Visible = true;

            tmrSwapStatusMessage.Enabled = true;
 
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
            int nStatusCode = 0;
            
            double usageSize = cMezeoFileCloud.GetStorageUsed(BasicInfo.ServiceUrl + "/v2", ref nStatusCode);
            if(usageSize == 0)
                usageSize = cLoginDetails.dblStorage_Used;

            string usedSize = FormatSizeString(usageSize);

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

        private void ShowSyncMessage(bool IsStopped = false)
        {
            lastSync = DateTime.Now;
            BasicInfo.LastSyncAt = lastSync;

            if (BasicInfo.AutoSync)
            {
                //tmrNextSync.Tick += new EventHandler(tmrNextSync_Tick);

                //if (InvokeRequired)
                //{

                //}

                tmrNextSync.Enabled = true;
                //tmrNextSync.Start();
                //tmrNextSync.Interval = 300000;
            }

            DisableProgress();
            this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");
            this.btnSyncNow.Enabled = true;

            UpdateUsageLabel();

            if (BasicInfo.AutoSync)
            {
                ShowAutoSyncMessage(IsStopped);
            }
            else
            {
                ShowSyncDisabledMessage();
            }
        }

        private void ShowAutoSyncMessage(bool IsStopped)
        {
           
            if(IsStopped)
                lblStatusL1.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            else
                lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerStatusAllFilesInSyncLabel");

            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
           // System.Threading.Thread.Sleep(200);
            //label1.Visible = true;
            label1.Text = LanguageTranslator.GetValue("SyncManagerStatusNextSyncAtLabel") + " " + lastSync.AddMinutes(5).ToString("h:mm tt"); ;
            
        }

        private void ShowSyncDisabledMessage()
        {
            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            label1.Text = LanguageTranslator.GetValue("SyncManagerResumeSync");
            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
            ShowNextSyncLabel(true);
        }

        private void DisableProgress()
        {
            lblPercentDone.Visible = false;
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            btnMoveFolder.Enabled = true;
            ShowNextSyncLabel(true);
        }

        private void EnableProgress()
        {
            lblPercentDone.Visible = true;
            pbSyncProgress.Visible = true;
            ShowNextSyncLabel(false);
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

        public void DisableSyncManager()
        {
            StopSync();

            pnlFileSyncOnOff.Enabled = false;
            rbSyncOff.Checked = true;

            btnMoveFolder.Enabled = false;
            btnSyncNow.Enabled = false;
            //lnkFolderPath.Enabled = false;

        }

        public void EnableSyncManager()
        {
            pnlFileSyncOnOff.Enabled = true;

            if (BasicInfo.AutoSync)
            {
                rbSyncOn.Checked = true;
            }
            else
            {
                rbSyncOff.Checked = true;
            }

            btnMoveFolder.Enabled = true;
            btnSyncNow.Enabled = true;
            if(lockObject != null)
                lockObject.StopThread = false;
            //lnkFolderPath.Enabled = false;

        }
        #endregion

        public string GetParentURI(string strPath)
        {
            if (strPath.IndexOf("\\") == -1)
                return cLoginDetails.szContainerContentsUri;

            strPath = strPath.Substring(0, (strPath.LastIndexOf("\\")));

            return GetContentURI(strPath);//dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, DbHandler.KEY + " = '" + strPath + "'");
        }

        public string GetContentURI(string strPath)
        {
            return dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + strPath + "'");
        }

        public string GetETag(string strPath)
        {
            return dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.E_TAG, DbHandler.KEY + " = '" + strPath + "'");
        }

        private int CheckForModifyEvent(LocalEvents lEvent)
        {
            string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + lEvent.FileName + "'");
            if (strCheck.Trim().Length == 0)
                return 2;
            else
            {
                DateTime DBModTime = dbHandler.GetDateTime(DbHandler.TABLE_NAME, DbHandler.MODIFIED_DATE, DbHandler.KEY + " = '" + lEvent.FileName + "'");

                DateTime ActualModTime = File.GetLastWriteTime(lEvent.FullPath);                

                if (ActualModTime > DBModTime)
                    return 1;
                else
                    return 0;
            }
        }

        private void AddInDBForAdded(LocalEvents lEvent)
        {
            FileFolderInfo fInfo = new FileFolderInfo();

            FileInfo fileInfo = new FileInfo(lEvent.FullPath);

            fInfo.Key = lEvent.FileName;
            fInfo.ContentUrl = "";
            fInfo.ParentUrl = GetParentURI(lEvent.FileName);
            fInfo.CreatedDate = fileInfo.CreationTime;
            fInfo.ModifiedDate = fileInfo.LastWriteTime;
            fInfo.MimeType = "";
            fInfo.IsPublic = false;
            fInfo.IsShared = false;
            fInfo.Status = "INPROGRESS";
           

            fInfo.ETag = "";

            if (lEvent.FileName.LastIndexOf("\\") == -1)
            {
                fInfo.FileName = lEvent.FileName;
                fInfo.ParentDir = "";
            }
            else
            {
                fInfo.FileName = lEvent.FileName.Substring(lEvent.FileName.LastIndexOf("\\") + 1);
                fInfo.ParentDir = lEvent.FileName.Substring(0, lEvent.FileName.LastIndexOf("\\"));
                fInfo.ParentDir = fInfo.ParentDir.Substring(fInfo.ParentDir.LastIndexOf("\\") +1);
            }

            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                fInfo.Type = "DIRECTORY";
                fInfo.FileSize = 0;
            }
            else
            {
                fInfo.Type = "FILE";
                fInfo.FileSize = fileInfo.Length;
            }

            dbHandler.Write(fInfo);
        }

        private void AddInDBForRename(LocalEvents lEvent)
        {
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + " = '" + lEvent.FileName + "'", DbHandler.KEY + " = '" + lEvent.OldFileName + "'");
            UpdateDBForStatus(lEvent, "INPROGRESS");
        }

        private void UpdateDBForStatus(LocalEvents lEvent, string strStatus)
        {
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS + " = '" + strStatus + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");
        }

        private void UpdateDBForAddedSuccess(string strContentUri, LocalEvents lEvent)
        {
            int nStatusCode = 0;
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL + " = '" + strContentUri + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

            string strEtag = cMezeoFileCloud.GetETag(strContentUri, ref nStatusCode);
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + " = '" + strEtag + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

            UpdateDBForStatus(lEvent, "SUCCESS");
        }

        private void UpdateDBForRemoveSuccess(LocalEvents lEvent)
        {
            dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY + " = '" + lEvent.FileName + "'");
        }

        private void UpdateDBForRenameSuccess(LocalEvents lEvent)
        {
            UpdateDBForStatus(lEvent, "SUCCESS");

            FileAttributes attr = File.GetAttributes(lEvent.FullPath);
            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo rootDir = new DirectoryInfo(lEvent.FullPath);
                WalkDirectoryTree(rootDir,lEvent.OldFullPath);
            }
        }

        private void WalkDirectoryTreeForDelete(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    string Key = fi.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);

                    File.Delete(fi.FullName);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + Key + "'");
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTreeForDelete(dirInfo);

                    string Key = dirInfo.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);

                    Directory.Delete(dirInfo.FullName);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + Key + "'");
                }
            }
        }

        private void WalkDirectoryTree(System.IO.DirectoryInfo root,string lEventOldPath)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                   string newKey = fi.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);
                   string oldKey = lEventOldPath.Substring(BasicInfo.SyncDirPath.Length + 1);
                   oldKey += "\\" + fi.Name;
                    
                   dbHandler.Update(DbHandler.TABLE_NAME,DbHandler.KEY + "='" + newKey + "'",DbHandler.KEY + "='" + oldKey + "'");

                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, lEventOldPath + "\\" + dirInfo.Name);

                    string newKey = dirInfo.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);
                    string oldKey = lEventOldPath.Substring(BasicInfo.SyncDirPath.Length + 1);
                    oldKey += "\\" + dirInfo.Name;

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + "='" + newKey + "'", DbHandler.KEY + "='" + oldKey + "'");
                }
            }
        }

        private void UpdateDBForModifiedSuccess(LocalEvents lEvent, string strContentURi)
        {
            int nStatusCode = 0;
            string strEtag = cMezeoFileCloud.GetETag(strContentURi, ref nStatusCode);
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + " = '" + strEtag + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

            FileInfo fileInfo = new FileInfo(lEvent.FullPath);

            dbHandler.UpdateModifiedDate(fileInfo.LastWriteTime, lEvent.FileName);

            UpdateDBForStatus(lEvent, "SUCCESS");
        }

        private void HandleEvents()
        {
            isLocalEventInProgress = true;

            List<int> RemoveIndexes = new List<int>();
            foreach (LocalEvents lEvent in events)
            {
                bool bRet = true;
               
                FileAttributes attr = FileAttributes.Normal ;

                bool isDirectory = false;
                bool isFile = File.Exists(lEvent.FullPath);
                if (!isFile)
                    isDirectory = Directory.Exists(lEvent.FullPath);
                if (isFile || isDirectory)
                    attr = File.GetAttributes(lEvent.FullPath);

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                {
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        bRet = false;
                    else
                    {
                        int nRet = CheckForModifyEvent(lEvent);
                        if (nRet == 0)
                            bRet = false;
                        else if (nRet == 1)
                            bRet = true;
                        else if (nRet == 2)
                        {
                            int nIndex = lEvent.FileName.LastIndexOf("\\");
                            if (nIndex == -1)
                                bRet = false;
                            else
                            {
                                string str = lEvent.FileName.Substring(0, nIndex);
                                foreach (LocalEvents id in events)
                                {
                                    if (id.FileName == str)
                                    {
                                        lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;
                                        bRet = true;
                                        break;
                                    }
                                    else
                                        bRet = false;
                                }
                            }
                        }

                    }
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED || lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + lEvent.FileName + "'");
                    if (strCheck.Trim().Length == 0)
                        bRet = true;
                    else
                        bRet = false;
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                {
                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + lEvent.FileName + "'");
                    if (strCheck.Trim().Length == 0)
                        bRet = false;
                    else
                        bRet = true;
                }

                if (lEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
                {
                    if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden || (attr & FileAttributes.Temporary) == FileAttributes.Temporary)
                        bRet = false;
                }

                if (bRet)
                {
                    if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                    {
                        UpdateDBForStatus(lEvent, "INPROGRESS");
                    }
                    else if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                    {
                        AddInDBForAdded(lEvent);
                    }
                    else if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                    {
                        AddInDBForRename(lEvent);
                    }

                }
                //Todo : if bRet is true update the DB here for every event.
                if (!bRet)
                {
                    RemoveIndexes.Add(events.IndexOf(lEvent));
                }
            }

            //foreach (int index in RemoveIndexes)
            RemoveIndexes.Sort();
            for(int n = RemoveIndexes.Count-1 ; n >=0 ; n--)
            {
                events.RemoveAt(RemoveIndexes[n]);
            }

            if (events.Count == 0)
            {
                isLocalEventInProgress = false;
                if (LocalEventList.Count != 0)
                    watcher_WatchCompletedEvent();
                return;
            }

            btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            
            ProcessLocalEvents();
            isLocalEventInProgress = false;

            ShowSyncMessage(events.Count > 0);
            
            btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");
        }

        private void ProcessLocalEvents()
        {
            string strUrl = "";
            pbSyncProgress.Visible = true;
            pbSyncProgress.Value = 0;
            pbSyncProgress.Maximum = events.Count;
            fileDownloadCount = 0;
            btnMoveFolder.Enabled = false;
            bool bRetConflicts = true;

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") +
                                            (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
            //frmParent.ShellNotifyIcon.StopNotifyIconBalloonText();
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.mezeosyncstatus_syncing.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") +
            //                                (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText"));

            foreach (LocalEvents lEvent in events)
            {
                if (!isLocalEventInProgress)
                {
                    StopLocalSync();
                    return;
                }

                fileDownloadCount++ ;
                showProgress();
                lblStatusL3.Text = lEvent.FullPath;// lEvent.FileName;

                FileAttributes attr = FileAttributes.Normal;

                bool isDirectory = false;
                bool isFile = File.Exists(lEvent.FullPath);
                if(!isFile)
                    isDirectory = Directory.Exists(lEvent.FullPath);
                if(isFile || isDirectory)
                    attr = File.GetAttributes(lEvent.FullPath);

                int nStatusCode = 0;
                bool bRet = true;
                switch (lEvent.EventType)
                {
                    case LocalEvents.EventsType.FILE_ACTION_ADDED:
                        {
                            string strParentURi = GetParentURI(lEvent.FileName);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {                                
                                string folderName = lEvent.FullPath.Substring((lEvent.FullPath.LastIndexOf("\\") + 1));
                                strUrl = cMezeoFileCloud.NewContainer(folderName, strParentURi, ref nStatusCode);
                                strUrl += "/contents";
                            }
                            else
                            {
                                strUrl = cMezeoFileCloud.UploadingFile(lEvent.FullPath, strParentURi, ref nStatusCode);
                                strUrl += "/content"; 
                            }

                            if ((strUrl.Trim().Length != 0) && (nStatusCode == 201))
                            {
                                bRet = true;
                                UpdateDBForAddedSuccess(strUrl, lEvent);
                            }
                            else
                            {
                                bRet = false;
                            }
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_MODIFIED:
                        {
                            string strContentURi =GetContentURI(lEvent.FileName);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                bRet = false;
                            else
                            {
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                                if (bRetConflicts)
                                {
                                    bRet = cMezeoFileCloud.OverWriteFile(lEvent.FullPath, strContentURi, ref nStatusCode);
                                    if (bRet)
                                        UpdateDBForModifiedSuccess(lEvent, strContentURi);
                                    else
                                    {
                                        ReportConflict(lEvent, IssueFound.ConflictType.CONFLICT_UPLOAD);
                                        bRet = false;
                                    }
                                }                               
                            }
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_REMOVED:
                        {
                            string strContentURi = GetContentURI(lEvent.FileName);
                            bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                            if (bRetConflicts)
                            {
                                if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                                    strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                                {
                                    strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                                }

                                bRet = cMezeoFileCloud.Delete(strContentURi, ref nStatusCode);
                                if (bRet)
                                    UpdateDBForRemoveSuccess(lEvent);
                            }
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_RENAMED:
                        {
                            string strContentURi = GetContentURI(lEvent.FileName);
                            string changedName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));
                            
                            bRetConflicts = CheckForConflicts(lEvent, strContentURi);

                            if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                               strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                            {
                                strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                bRet = cMezeoFileCloud.ContainerRename(strContentURi, changedName, ref nStatusCode);
                                if (bRet)
                                    UpdateDBForRenameSuccess(lEvent);
                            }
                            else
                            {
                                if (bRetConflicts)
                                {
                                    bool bPublic = dbHandler.GetBoolean(DbHandler.TABLE_NAME, DbHandler.PUBLIC, DbHandler.KEY + " = '" + lEvent.FileName + "'");
                                    string mimeType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.MIMIE_TYPE, DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                    bRet = cMezeoFileCloud.FileRename(strContentURi, changedName, mimeType, bPublic, ref nStatusCode);
                                    if (bRet)
                                        UpdateDBForRenameSuccess(lEvent);
                                }
                            }                            
                        }
                        break;
                }

            }

            isLocalEventInProgress = false;

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                        ToolTipIcon.None);
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
            //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

            events.Clear();

            if (LocalEventList.Count != 0)
                watcher_WatchCompletedEvent();
        }

        private void SetIssueFound(bool bIsIssueFound)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.btnIssuesFound.Visible = bIsIssueFound;
                });

            }
            else
            {
                this.btnIssuesFound.Visible = bIsIssueFound;
            }
        }

        //private  void  ShowHideIssueButton(bool value)
        //{
        //    btnIssuesFound.Visible = value;
        //    //if (bIsIssueFound)
        //    //{
        //    //    btnIssuesFound.Visible = true;
        //    //}
        //    //else
        //    //    btnIssuesFound.Visible = false;
        //}

        private void ReportConflict(LocalEvents lEvent , IssueFound.ConflictType cType)
        {
            FileInfo fInfo = new FileInfo(lEvent.FullPath);

            IssueFound iFound = new IssueFound();

            iFound.LocalFilePath = lEvent.FullPath;
            iFound.LocalIssueDT = fInfo.LastWriteTime;
            iFound.LocalSize = FormatSizeString(fInfo.Length);
            iFound.ConflictTimeStamp = DateTime.Now;
            iFound.cType = cType;

            string Description = AboutBox.AssemblyTitle;
            switch (cType)
            {
                case IssueFound.ConflictType.CONFLICT_MODIFIED:
                    {
                        iFound.IssueTitle = LanguageTranslator.GetValue("ConflictDetectedModified");

                        Description += LanguageTranslator.GetValue("ErrorBlurbConflict1");
                        Description += AboutBox.AssemblyProduct;
                        Description += LanguageTranslator.GetValue("ErrorBlurbConflict2") + "\n";
                        Description += LanguageTranslator.GetValue("ErrorBlurbConflict3") + "\n";
                        Description += LanguageTranslator.GetValue("ErrorBlurbConflict4");
                        Description += AboutBox.AssemblyProduct;
                        Description += LanguageTranslator.GetValue("ErrorBlurbConflict5");

                        iFound.IssueDescripation = Description;
                    }
                    break;
                case IssueFound.ConflictType.CONFLICT_UPLOAD:
                    {
                        iFound.IssueTitle = LanguageTranslator.GetValue("ConflictDetectedError");

                        Description += LanguageTranslator.GetValue("ErrorBlurbUpload1");
                        Description += AboutBox.AssemblyProduct;
                        Description += LanguageTranslator.GetValue("ErrorBlurbUpload2");
                        Description += LanguageTranslator.GetValue("ErrorBlurbUpload3");

                        iFound.IssueDescripation = Description;
                    }
                    break;
            }

            cMezeoFileCloud.AppEventViewer(AboutBox.AssemblyTitle, Description, 3);

            int nStatusCode = 0;
            string strContentURi = GetContentURI(lEvent.FileName);
            if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
               strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
            {
                strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
            }

             ItemDetails iDetails = cMezeoFileCloud.GetContinerResult(strContentURi, ref nStatusCode);

             iFound.ServerSize = FormatSizeString(iDetails.dblSizeInBytes);
             iFound.ServerIssueDT = iDetails.dtModified;
             iFound.ServerFileInfo = lEvent.FileName;

             frmIssuesFound.AddIssueToList(iFound);

             cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                         ToolTipIcon.None);

             cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

             frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
             //frmParent.me   

             //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText"));
             //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("SyncIssueFoundText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
             //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

             //frmParent.menuItem7.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");

            SetIssueFound(true);
        }

        private bool CheckForConflicts(LocalEvents lEvent, string strContentUrl)
        {
            int nStatusCode = 0;
            bool bRet = false;
            string strEtag;
            switch (lEvent.EventType)
            {
                case LocalEvents.EventsType.FILE_ACTION_MODIFIED:
                    {
                        strEtag = cMezeoFileCloud.GetETag(strContentUrl, ref nStatusCode);
                        string strDBETag = GetETag(lEvent.FileName);
                        if (strEtag.Trim().Length != 0)
                        {
                            if (strEtag != strDBETag)
                            {
                                ReportConflict(lEvent, IssueFound.ConflictType.CONFLICT_MODIFIED);
                                return true;
                            }
                            else
                            {
                                string URL = "";
                                if (strContentUrl.Substring(strContentUrl.Length - 9).Equals("/contents") ||
                                    strContentUrl.Substring(strContentUrl.Length - 8).Equals("/content"))
                                {
                                    URL = strContentUrl.Substring(0, strContentUrl.LastIndexOf("/"));
                                }

                                ItemDetails IDetails = cMezeoFileCloud.GetContinerResult(URL, ref nStatusCode);
                                string FileName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));
                                if (FileName != IDetails.strName)
                                {
                                    string FileReName = lEvent.FullPath.Substring(0,lEvent.FullPath.LastIndexOf("\\")+1);
                                    FileReName += IDetails.strName;
                                    
                                    bRet = cMezeoFileCloud.OverWriteFile(lEvent.FullPath, strContentUrl, ref nStatusCode);
                                    if (bRet)
                                    {
                                        UpdateDBForModifiedSuccess(lEvent, strContentUrl);
                                    }

                                    File.Move(lEvent.FullPath, FileReName);
                
                                    lEvent.OldFullPath = lEvent.FullPath;
                                    lEvent.FullPath = FileReName;
                                    lEvent.OldFileName = lEvent.FileName;
                                    lEvent.FileName = lEvent.FileName.Substring(0,lEvent.FileName.LastIndexOf("\\")+1);
                                    lEvent.FileName += IDetails.strName;

                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + " = '" + lEvent.FileName + "'", DbHandler.KEY + " = '" + lEvent.OldFileName + "'");
                                    UpdateDBForStatus(lEvent, "SUCCESS");
                                   
                                    return false;
                                }
                                else
                                    return true;
                            }
                        }
                        else
                        {
                            string strParentURL = GetParentURI(lEvent.FileName);
                            string strURL = cMezeoFileCloud.UploadingFile(lEvent.FullPath, strParentURL, ref nStatusCode);
                            string strEtagNew = cMezeoFileCloud.GetETag(strURL,ref nStatusCode);

                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL + " = '" + strURL + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");
                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + " = '" + strEtagNew + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

                            return false;
                        }
                    }
                    break;
                case LocalEvents.EventsType.FILE_ACTION_REMOVED:
                    {
                        strEtag = cMezeoFileCloud.GetETag(strContentUrl, ref nStatusCode);
                        string strDBETag = GetETag(lEvent.FileName);
                        string FileName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));

                        string URL = "";
                        if (strContentUrl.Substring(strContentUrl.Length - 9).Equals("/contents") ||
                            strContentUrl.Substring(strContentUrl.Length - 8).Equals("/content"))
                        {
                            URL = strContentUrl.Substring(0, strContentUrl.LastIndexOf("/"));
                        }

                        ItemDetails IDetails = cMezeoFileCloud.GetContinerResult(URL, ref nStatusCode);

                        if (strEtag.Trim().Length != 0)
                        {
                            if ((strEtag != strDBETag) || (FileName != IDetails.strName))
                            {
                                lEvent.FullPath = lEvent.FullPath.Substring(0, lEvent.FullPath.LastIndexOf("\\") + 1);
                                lEvent.FullPath += IDetails.strName;

                                bRet = cMezeoFileCloud.DownloadFile(strContentUrl + "/" + IDetails.strName, lEvent.FullPath, ref nStatusCode);

                                if (bRet)
                                {
                                    UpdateDBForRemoveSuccess(lEvent);
                                    if (FileName != IDetails.strName)
                                    {
                                        lEvent.FileName = lEvent.FileName.Substring(0, lEvent.FileName.LastIndexOf("\\") + 1);
                                        lEvent.FileName += IDetails.strName;  
                                    }
                                    AddInDBForAdded(lEvent);
                                    UpdateDBForAddedSuccess(strContentUrl, lEvent);
                             
                                    return false;
                                }
                                return true;
                            }   
                            return true;
                        }
                        else
                        {
                            UpdateDBForRemoveSuccess(lEvent);
                            return false;
                        }
                    }
                    break;
                case LocalEvents.EventsType.FILE_ACTION_RENAMED:
                    {
                        strEtag = cMezeoFileCloud.GetETag(strContentUrl, ref nStatusCode);
                        string strDBETag = GetETag(lEvent.FileName);
                        string FileName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));

                        string URL = "";
                        if (strContentUrl.Substring(strContentUrl.Length - 9).Equals("/contents") ||
                            strContentUrl.Substring(strContentUrl.Length - 8).Equals("/content"))
                        {
                            URL = strContentUrl.Substring(0, strContentUrl.LastIndexOf("/"));
                        }

                        ItemDetails IDetails = cMezeoFileCloud.GetContinerResult(URL, ref nStatusCode);

                        if (strEtag.Trim().Length != 0)
                        {
                            if(strEtag != strDBETag)
                            {
                                bRet = cMezeoFileCloud.DownloadFile(strContentUrl + "/" + lEvent.FileName, lEvent.FullPath, ref nStatusCode);
                                if (bRet)
                                {
                                    string strEtagNew = cMezeoFileCloud.GetETag(strContentUrl, ref nStatusCode);
                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + " = '" + strEtagNew + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                    FileInfo fInfo = new FileInfo(lEvent.FullPath);
                                    dbHandler.UpdateModifiedDate(fInfo.LastWriteTime, lEvent.FileName);
                                    return true;
                                }
                            }
                            return true;
                        }
                        else
                        {
                            string strParentUri = GetParentURI(lEvent.FileName);
                            string strUri = cMezeoFileCloud.UploadingFile(lEvent.FullPath, strParentUri, ref nStatusCode);
                            if (strUri.Trim().Length != 0)
                            {
                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY + " = '" + lEvent.FileName + "'", DbHandler.KEY + " = '" + lEvent.OldFileName + "'");
                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL + " = '" + strUri + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                string strEtagUpload = cMezeoFileCloud.GetETag(strUri, ref nStatusCode);

                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + " = '" + strEtagUpload + "'", DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                UpdateDBForStatus(lEvent, "SUCCESS");
                                return false;
                            }
                            return true;
                        }

                    }
                    break;
            }

            return true;
        }

        void watcher_WatchCompletedEvent()
        {
            if (!isLocalEventInProgress && !isSyncInProgress && BasicInfo.IsConnectedToInternet && LocalEventList.Count != 0)
            {
                lock (folderWatcherLockObject)
                {
                    if (LocalEventList.Count != 0)
                    {
                        if (events == null)
                            events = new List<LocalEvents>();

                        events.AddRange(LocalEventList);

                        LocalEventList.Clear();
                    }
                }
                HandleEvents();
            }
        }
            
        private void bwNQUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            int nStatusCode = 0;

            NQDetails[] pNQDetails = null;
            string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;

            int nNQLength = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
            if (nNQLength > 0)
                pNQDetails = cMezeoFileCloud.NQGetData(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, nNQLength, ref nStatusCode);

            if (pNQDetails != null)
            {

                for (int n = 0; n < nNQLength ; n++)
                {
                    if (bwNQUpdate.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    UpdateFromNQ(pNQDetails[n]);
                    cMezeoFileCloud.NQDeleteValue(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, 1, ref nStatusCode);
                    bwNQUpdate.ReportProgress(0);
                }

                
                isSyncInProgress = false;
            }
            
        }

        private void bwNQUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            bwNQUpdate.CancelAsync();
            ShowSyncMessage(isEventCanceled);
            isSyncInProgress = false;

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                        ToolTipIcon.None);

            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"));
            //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
            //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

            isEventCanceled = false;
        }

        private void bwNQUpdate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            showProgress();
            fileDownloadCount++;
            
        }

        private void btnIssuesFound_Click(object sender, EventArgs e)
        {
            ShowIssesFound();
        }

        private void ShowIssesFound()
        {
            frmIssuesFound.Show();            
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start( BasicInfo.ServiceUrl + "/help/sync");
        }
       
    }
}
