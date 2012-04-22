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
        //[DllImport("shell32.dll", CharSet = CharSet.Auto)]
        //static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
        //const int FO_DELETE = 3;
        //const int FOF_ALLOWUNDO = 0x40;
        //const int FOF_NOCONFIRMATION = 0x10;    //Don't prompt the user.; 
        //const int FOF_NOERRORUI = 0x0400;    //Don't prompt the user.; 

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //public static extern int GetLastError();

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        //public struct SHFILEOPSTRUCT
        //{
        //    public IntPtr hwnd;
        //    [MarshalAs(UnmanagedType.U4)]
        //    public int wFunc;
        //    public string pFrom;
        //    public string pTo;
        //    public short fFlags;
        //    [MarshalAs(UnmanagedType.Bool)]
        //    public bool fAnyOperationsAborted;
        //    public IntPtr hNameMappings;
        //    public string lpszProgressTitle;
        //}

        private static int NONE = -1;
        private static int SYNC_STARTED = 1;
        private static int PROCESS_LOCAL_EVENTS_STARTED = SYNC_STARTED + 1;
        private static int PROGRESS_CHANGED_WITH_FILE_NAME = PROCESS_LOCAL_EVENTS_STARTED + 1;
        private static int LOCAL_EVENTS_COMPLETED = PROGRESS_CHANGED_WITH_FILE_NAME + 1;
        private static int LOCAL_EVENTS_STOPPED = LOCAL_EVENTS_COMPLETED + 1;

        private static int ONE_SECOND = 1000;
        private static int ONE_MINUTE = ONE_SECOND * 60;

        private static int FIVE_SECONDS = ONE_SECOND * 5;
        private static int FIVE_MINUTES = ONE_MINUTE * 5;

        private static int INITIAL_NQ_SYNC = LOCAL_EVENTS_COMPLETED + 1;
        private static int UPDATE_NQ_PROGRESS = INITIAL_NQ_SYNC + 1;
        private static int UPDATE_NQ_CANCELED = UPDATE_NQ_PROGRESS + 1;
        private static int UPDATE_NQ_MAXIMUM = UPDATE_NQ_CANCELED + 1;

        private static int USER_CANCELLED = -3;
        private static int LOGIN_FAILED = -4;
        private static int SERVER_INACCESSIBLE = -5;

        private bool IsCalledByNextSyncTmr = false;

        private static string DB_STATUS_SUCCESS = "SUCCESS";
        private static string DB_STATUS_IN_PROGRESS = "INPROGRESS";

        NotificationManager cnotificationManager;

        #region Private Members

        //private MezeoFileCloud cMezeoFileCloud;
        private CloudService cMezeoFileCloud;
        private LoginDetails cLoginDetails;
        private frmLogin frmParent;
        private string[] statusMessages = new string[3];
        private int statusMessageCounter = 0;
        private bool isAnalysingStructure = false;
        private bool isAnalysisCompleted = false;
        public bool isOfflineWorking = false;
        public bool isSyncInProgress = false;
        public bool isLocalEventInProgress = false;
        public bool isEventCanceled = false;
        private FileDownloader fileDownloder;
        private bool isDownloadingFile = false;
        private StructureDownloader stDownloader;
        private int fileDownloadCount = 1;
        private DateTime lastSync;
        private OfflineWatcher offlineWatcher;
        private bool isDisabledByConnection = false;

        Queue<LocalItemDetails> queue;
        frmIssues frmIssuesFound;
        Watcher watcher;
        List<LocalEvents> LocalEventList;
        Object folderWatcherLockObject;

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
        public frmSyncManager(CloudService mezeoFileCloud, LoginDetails loginDetails, NotificationManager notificationManager)
        {
            InitializeComponent();
           

            cMezeoFileCloud = mezeoFileCloud;

            cMezeoFileCloud.fileCloud.downloadStoppedEvent += new MezeoFileCloud.FileDownloadStoppedEvent(cMezeoFileCloud_downloadStoppedEvent);

            cMezeoFileCloud.fileCloud.uploadStoppedEvent += new MezeoFileCloud.FileUploadStoppedEvent(cMezeoFileCloud_uploadStoppedEvent);

            cLoginDetails = loginDetails;
            cnotificationManager = notificationManager;

            LoadResources();

            LocalEventList = new List<LocalEvents>();
            folderWatcherLockObject = new Object();

            watcher = new Watcher(LocalEventList, lockObject, BasicInfo.SyncDirPath);
            watcher.WatchCompletedEvent += new Watcher.WatchCompleted(watcher_WatchCompletedEvent);
            CheckForIllegalCrossThreadCalls = false;

            watcher.StartMonitor();
            dbHandler = new DbHandler();
            dbHandler.OpenConnection();

            frmIssuesFound = new frmIssues(mezeoFileCloud);
            offlineWatcher = new OfflineWatcher(dbHandler);
        }

        void cMezeoFileCloud_uploadStoppedEvent(string szSourceFileName, string szContantURI)
        {
            DeleteUploadedIncompleteFile(szContantURI);
        }

        void DeleteUploadedIncompleteFile(string szContantURI)
        {
            if (szContantURI.Trim().Length != 0)
            {
                int nStatusCode = 0;
                bool bRet = cMezeoFileCloud.Delete(szContantURI, ref nStatusCode);
            }
        }

        void cMezeoFileCloud_downloadStoppedEvent(string fileName)
        {
            DeleteCurrentIncompleteFile(fileName);
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
            try
            {
                Panel pnl = sender as Panel;
                Rectangle bounds = new Rectangle(0, 0, pnl.Width, pnl.Height);
                Pen pen = new Pen(Color.FromArgb(206, 207, 188));

                DrawRoundedRectangle(e.Graphics, bounds, 10, pen, Color.Transparent);
            }
            catch (Exception ex)
            {
                //do nothing
            }

        }

        #endregion

        #region Form Events

        private void btnMoveFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            browserDialog.Description = LanguageTranslator.GetValue("SyncManagerMoveFolderDesc");
            
            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                System.IO.Directory.SetCurrentDirectory(exePath);
                watcher.StopMonitor();

                int index = BasicInfo.SyncDirPath.LastIndexOf("\\");
                string folderName = BasicInfo.SyncDirPath.Substring(index + 1);

               // Directory.Move(BasicInfo.SyncDirPath, browserDialog.SelectedPath + "\\" + folderName);
                Directory.CreateDirectory(browserDialog.SelectedPath + "\\" + folderName);
                DirectoryInfo rootDir = new DirectoryInfo(BasicInfo.SyncDirPath);
                WalkDirectoryTreeForMoveFolder(rootDir, browserDialog.SelectedPath + "\\" + folderName);
                Directory.Delete(BasicInfo.SyncDirPath);

                BasicInfo.SyncDirPath = browserDialog.SelectedPath + "\\" + folderName;

                lnkFolderPath.Text = BasicInfo.SyncDirPath;

                watcher = new Watcher(LocalEventList, lockObject, BasicInfo.SyncDirPath);
                watcher.WatchCompletedEvent += new Watcher.WatchCompleted(watcher_WatchCompletedEvent);
                watcher.StartMonitor();
                System.IO.Directory.SetCurrentDirectory(BasicInfo.SyncDirPath);

            }
        }

        private void WalkDirectoryTreeForMoveFolder(System.IO.DirectoryInfo root, string strMovePath)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            try
            {
                files = root.GetFiles("*.*");
            }
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
                    File.Copy(fi.FullName, strMovePath + "\\" + fi.Name);
                    File.Delete(fi.FullName);
                }

                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    Directory.CreateDirectory(strMovePath + "\\" + dirInfo.Name);
                    WalkDirectoryTreeForMoveFolder(dirInfo, strMovePath + "\\" + dirInfo.Name);
                    Directory.Delete(dirInfo.FullName);
                }
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }


        private void btnSyncNow_Click(object sender, EventArgs e)
        {
            //if (!BasicInfo.IsConnectedToInternet)
            //{
            //    frmParent.HandleConnectionState();
            //    return;
            //}

            isEventCanceled = false;
            if (!isLocalEventInProgress)
            {
                InitializeSync();
            }
            else
            {
                StopSync();
                //btnSyncNow.Enabled = false;
                //isLocalEventInProgress = false;
                //isEventCanceled = true;
                //bwLocalEvents.CancelAsync();

                //isOfflineWorking = false;
                //bwOfflineEvent.CancelAsync();
            }
        }

        private void tmrNextSync_Tick(object sender, EventArgs e)
        {
            if (BasicInfo.AutoSync)
            {
                if (!isLocalEventInProgress && !isSyncInProgress && !isOfflineWorking)
                {
                    IsCalledByNextSyncTmr = true;
                    tmrNextSync.Interval = FIVE_MINUTES;
                    //tmrNextSync.Enabled = false;
                    InitializeSync();
                }
                else if (isLocalEventInProgress)
                {
                    tmrNextSync.Interval = FIVE_SECONDS;
                }
            }
            else
            {
                if (isDisabledByConnection)
                {
                    int nStatusCode = 0;
                    string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;
                    try
                    {
                        NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
                        if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                        {
                            this.Hide();
                            frmParent.ShowLoginAgainFromSyncMgr();
                        }
                        else if (nStatusCode == ResponseCode.NQGETLENGTH)
                        {
                            BasicInfo.AutoSync = true;
                            EnableSyncManager();
                            BasicInfo.AutoSync = true;
                            InitializeSync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // order of these statement is important as it is triggering rbsyncoff button event
                        BasicInfo.AutoSync = true;
                        EnableSyncManager();
                        BasicInfo.AutoSync = true;
                        InitializeSync();
                    }
                }
            }
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
            if (!BasicInfo.AutoSync)
            {
                BasicInfo.AutoSync = true;

                //tmrNextSync.Interval = FIVE_MINUTES;
                //tmrNextSync.Enabled = true;

                if (!isSyncInProgress && !isLocalEventInProgress && !isOfflineWorking)
                {
                   InitializeSync();
                }
            }
        }

        private void rbSyncOff_CheckedChanged(object sender, EventArgs e)
        {
            if (BasicInfo.AutoSync)
            {
                BasicInfo.AutoSync = false;

                //tmrNextSync.Enabled = false;

                if (!isSyncInProgress && !isLocalEventInProgress && !isOfflineWorking)
                {
                    ShowSyncMessage();
                }
            }
        }

       


        #endregion

        #region Downloader Events

        void fileDownloder_fileDownloadCompleted()
        {
            isSyncInProgress = false;
            //if (BasicInfo.IsConnectedToInternet)
            //{
                BasicInfo.IsInitialSync = false;
                ShowSyncMessage();
                
                //watcher_WatchCompletedEvent();

                cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonInitialSyncText") + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText"),
                                                                        ToolTipIcon.None);
                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");

                //List<LocalEvents> offileEvents = offlineWatcher.PrepareStructureList();

                //if (offileEvents.Count > 0)
                //{
                //    events.AddRange(offileEvents);
                //    offileEvents.Clear();
                //    bwOfflineEvent.RunWorkerAsync();
                //}

                if (LocalEventList.Count > 0)
                {
                    watcher_WatchCompletedEvent();
                }
            //}
            //else
            //{
                
            //    DisableSyncManager();
            //}
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
                
                if (stDownloader.TotalFileCount > 0)
                {
                    pbSyncProgress.Maximum = stDownloader.TotalFileCount - (fileDownloadCount -1);
                }

                //pbSyncProgress.Maximum++;
                if (pbSyncProgress.Maximum > 0)
                {
                    fileDownloadCount = 1;
                    lblPercentDone.Text = "";
                    lblPercentDone.Visible = true;

                    showProgress();
                }
                //if (!BasicInfo.IsConnectedToInternet)
                //{
                //    DisableSyncManager();
                //}

                // MessageBox.Show(pbSyncProgress.Maximum.ToString());
                
            }
        }

        void fileDownloder_cancelDownloadEvent(CancelReason reason)
        {
            isDownloadingFile = false;

            fileDownloadCount = 0;
            OnThreadCancel(reason);

            if (reason == CancelReason.INSUFFICIENT_STORAGE)
            {
                ShowInsufficientStorageMessage();
            }
            
        }

        void ShowInsufficientStorageMessage()
        {
            lblStatusL1.Text = LanguageTranslator.GetValue("InsufficientStorageMessage");
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("InsufficientStorageMessage"),
                                                                        ToolTipIcon.None);
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("InsufficientStorageMessage");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");

        }

        void stDownloader_cancelDownloadEvent(CancelReason reason)
        {
            isAnalysingStructure = false;
            tmrSwapStatusMessage.Enabled = false;
            OnThreadCancel(reason);
        }

        public void ApplicationExit()
        {
            //if (lockObject != null)
            //    lockObject.ExitApplication = true;
            StopSync();
        }


        #endregion

        #region Functions and Methods

        private void OnThreadCancel(CancelReason reason)
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

                    if (reason == CancelReason.LOGIN_FAILED)
                    {
                        this.Hide();
                        frmParent.ShowLoginAgainFromSyncMgr();
                    }
                    else if (reason == CancelReason.SERVER_INACCESSIBLE)
                    {
                        DisableSyncManager();
                        ShowSyncManagerOffline();
                    }

                }
            }
        }

        private void LoadResources()
        {
            this.Text = AboutBox.AssemblyTitle; //LanguageTranslator.GetValue("SyncManagerTitle");
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

            if (cLoginDetails != null)
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

                this.lblUsageDetails.Text = usedSize + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + allocatedSize;
            }
            else
                this.lblUsageDetails.Text = LanguageTranslator.GetValue("UsageNotAvailable");

        }

        private void UpdateUsageLabel()
        {
            if (cLoginDetails != null)
            {


                if (!bwUpdateUsage.IsBusy)
                    bwUpdateUsage.RunWorkerAsync();

                //this.lblUsageDetails.Text = GetUsageString();
            }
            else
            {
                this.lblUsageDetails.Text = LanguageTranslator.GetValue("UsageNotAvailable");
            }
        }

        public void InitializeSync()
        {
            //if (!BasicInfo.IsConnectedToInternet)
            //{
            //    frmParent.HandleConnectionState();
            //    return;
            //}

            if (!isSyncInProgress)
            {
                SetUpSync();
                SyncNow();
            }
            else
            {
                isEventCanceled = true;
                StopSync();
            }
        }

        public void SetUpSync()
        {
            //ShowNextSyncLabel(false);
            lblPercentDone.Visible = true;
            lblPercentDone.Text = "";
            pbSyncProgress.Value = 0;
        }

        private void SetUpControlForSync()
        {
            SetIssueFound(false);
            btnSyncNow.Text = this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            btnSyncNow.Refresh();
            isAnalysingStructure = true;
            isDownloadingFile = true;
            isSyncInProgress = true;
            ShowNextSyncLabel(false);
            isAnalysisCompleted = false;
            //tmrNextSync.Enabled = false;
            pbSyncProgress.Visible = true;
            btnMoveFolder.Enabled = false;
        }
        public void StopSync()
        {
            if (isSyncInProgress || isLocalEventInProgress || isOfflineWorking)
            {
                tmrNextSync.Interval = FIVE_MINUTES;
                isLocalEventInProgress = false;
                isEventCanceled = true;
                isOfflineWorking = false;
                bwLocalEvents.CancelAsync();
                bwNQUpdate.CancelAsync();
                bwOfflineEvent.CancelAsync();
                //lblPercentDone.Text = "";
                //pbSyncProgress.Visible = false;
                ShowNextSyncLabel(true);
                btnSyncNow.Enabled = false;
                if (lockObject != null)
                    lockObject.StopThread = true;

                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    SetIssueFound(true);
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;

                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                         ToolTipIcon.None);

                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
                }
                else
                {
                    if(BasicInfo.AutoSync)
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    else
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                             LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                            ToolTipIcon.None);
                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                }

                //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
                //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncStopText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
                //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncStopText"));

                cMezeoFileCloud.StopSyncProcess();
                
            }
        }

        private void DeleteCurrentIncompleteFile(string fileName)
        {
            bool isFile = System.IO.File.Exists(fileName);
            
            if (isFile)
            {
                string key = fileName.Substring(BasicInfo.SyncDirPath.Length+1);
                string status = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.STATUS, new string[] { DbHandler.KEY }, new string[] { key }, new DbType[] { DbType.String });

                if (status != DB_STATUS_SUCCESS)
                {
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY ,key);
                    System.IO.File.Delete(fileName);
                }
                
            }
        }

        private void StopLocalSync()
        {
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            ShowNextSyncLabel(true);
            btnSyncNow.Enabled = true;

            if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
            {
                SetIssueFound(true);
                cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                         ToolTipIcon.None);

                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
            }
            else
            {
                if (BasicInfo.AutoSync)
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                else
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                         LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                        ToolTipIcon.None);
                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            }

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayBalloonSyncStopText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayBalloonSyncStopText"));
        }

        public void SetUpSyncNowNotification()
        {
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
        }

        public int CheckServerStatus()
        {
            int nStatusCode = 0;

            if (cLoginDetails == null)
            {
                cLoginDetails = frmParent.loginFromSyncManager();
                if (cLoginDetails == null)
                    return -1;
            }

            string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;
            NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
                return -1;
            }
            else if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                DisableSyncManager();
                ShowSyncManagerOffline();
                return -2;
            }

            return 1;
        }

        public void SyncNow()
        {
            
            int nServerStatus = CheckServerStatus();
            if (nServerStatus != 1)
                return;
            else
            {
                if (isDisabledByConnection)
                    EnableSyncManager();
            }

            SetUpSyncNowNotification();

            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
            //frmParent.ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.mezeosyncstatus_syncing.Handle);
            //frmParent.ShellNotifyIcon.SetNotifyIconToolTip(AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText"));
            
            //frmParent.ShellNotifyIcon.UpdateNotifyIcon();

            if (BasicInfo.IsInitialSync)
            {
                ShowNextSyncLabel(false);
                SetUpControlForSync();
                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedTitleText"),
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

                if ((events != null && events.Count > 0) || (LocalEventList != null && LocalEventList.Count > 0))
                {
                    if ((events != null && events.Count > 0))
                    {
                        ShowNextSyncLabel(false);
                        if (!bwLocalEvents.IsBusy)
                            bwLocalEvents.RunWorkerAsync();
                    }
                    else if ((LocalEventList != null && LocalEventList.Count > 0))
                    {
                        lock (folderWatcherLockObject)
                        {
                            if (events == null)
                                events = new List<LocalEvents>();

                            events.AddRange(LocalEventList);

                            LocalEventList.Clear();
                        }

                        if(!bwLocalEvents.IsBusy)
                            bwLocalEvents.RunWorkerAsync();
                    }
                }
                else
                {
                    UpdateNQ();
                }
              
                
            }
        }

        public void ProcessOfflineEvents()
        {
            List<LocalEvents> offileEvents = offlineWatcher.PrepareStructureList();

            if (offileEvents.Count > 0)
            {
                if (events == null)
                    events = new List<LocalEvents>();

                events.AddRange(offileEvents);
                offileEvents.Clear();

                if (!bwOfflineEvent.IsBusy)
                    bwOfflineEvent.RunWorkerAsync();
            }
            else
            {
                UpdateNQ();
            }

            
        }

        public void UpdateNQ()
        {
            Debugger.Instance.logMessage("frmSyncManager - UpdateNQ", "Enter");

            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerCheckingServer");
            lblStatusL3.Text = "";
            ShowNextSyncLabel(false);
            SetUpControlForSync();
            Application.DoEvents();
            int nStatusCode = 0;
            string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;
            //int nNQLength = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);

            NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
                return;
            }
            else if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                DisableSyncManager();
                ShowSyncManagerOffline();
                return;
            }
            else if (nqLengthRes == null)
                return;

            int nNQLength = 0;
            if (nqLengthRes.nEnd == -1 || nqLengthRes.nStart == -1)
                nNQLength = 0;
            else
                nNQLength = (nqLengthRes.nEnd + 1) - nqLengthRes.nStart;

            if (nNQLength > 0)
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateNQ", "nNQLength" + nNQLength.ToString());
                //ShowNextSyncLabel(false);
                if (!bwNQUpdate.IsBusy)
                {
                    Debugger.Instance.logMessage("frmSyncManager - UpdateNQ", "bwNQUpdate.RunWorkerAsync called");
                    bwNQUpdate.RunWorkerAsync(nNQLength);
                }
            }
            else
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateNQ", "nNQLength 0");

                isSyncInProgress = false;
                ShowSyncMessage();

                //if (BasicInfo.AutoSync)
                //{
                    tmrNextSync.Interval = FIVE_MINUTES;
                  //  tmrNextSync.Enabled = true;
                //}

                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    SetIssueFound(true);
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                         ToolTipIcon.None);

                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
                }
                else
                {
                    if (BasicInfo.AutoSync)
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    else
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                 LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                                ToolTipIcon.None);

                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
                }
            }

            Debugger.Instance.logMessage("frmSyncManager - UpdateNQ", "Leave");
        }

        private int UpdateFromNQ(NQDetails UpdateQ)
        {
            Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", "Enter"); 

            NQDetails nqDetail = UpdateQ;
            int nStatus = 0;

            int nStatusCode = 0;
            NSResult nsResult = null;
            nsResult = cMezeoFileCloud.GetNamespaceResult(cLoginDetails.szNamespaceUri + "/" +
                                                            nqDetail.StrMezeoExportedPath,
                                                            nqDetail.StrObjectType, ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                return nStatusCode;
            }
            else if (nStatusCode != ResponseCode.GETNAMESPACERESULT && nStatusCode != ResponseCode.NOTFOUND)
            {
                return nStatusCode;
            }
            
            if (nsResult == null && nqDetail.StrEvent != "cdmi_delete")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", "nsResult Null");

                nStatus = 1;
                return nStatus;
            }

            string strPath = nqDetail.StrParentUri.Substring(cLoginDetails.szNQParentUri.Length +1);
            string strKey = strPath.Replace("/" , "\\");
            
            if(nsResult == null)
                strKey += nqDetail.StrObjectName;
            else
                strKey += nsResult.StrName;

            strPath = BasicInfo.SyncDirPath + "\\" + strKey;
            lblStatusL3.Text = strPath;

            if (nqDetail.StrEvent == "cdmi_create_complete")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Enter"); 

                string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL }, new string[] { nsResult.StrContentsUri }, new DbType[] { DbType.String });
                if (strDBKey.Trim().Length == 0)
                {
                    nStatus = nqEventCdmiCreate(nqDetail, nsResult, strKey, strPath);
                    if (nStatus == ResponseCode.LOGINFAILED1 || nStatus == ResponseCode.LOGINFAILED2)
                    {
                        return nStatus;
                    }
                    else if (nStatus != ResponseCode.GETETAG && nStatus != ResponseCode.DOWNLOADFILE && nStatus != ResponseCode.DOWNLOADITEMDETAILS && nStatus != 1)
                    {
                        return nStatus;
                    }
                }
                else
                    nStatus = 1;

                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_modify_complete")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Enter"); 

                string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL }, new string[] { nsResult.StrContentsUri }, new DbType[] { DbType.String });
                string strDBEtag = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.E_TAG, new string[] { DbHandler.KEY }, new string[] { strDBKey }, new DbType[] { DbType.String });
                string strEtagCloud = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.GETETAG)
                {
                    return nStatusCode;
                }
                if (strDBEtag == strEtagCloud)
                {
                    if (strKey != strDBKey)
                    {
                        if (File.Exists(strPath))
                        {
                            nqEventCdmiDelete(BasicInfo.SyncDirPath + "\\" + strDBKey, strDBKey);
                        }
                        else
                        {
                            if(File.Exists(BasicInfo.SyncDirPath + "\\" + strDBKey))
                                File.Move(BasicInfo.SyncDirPath + "\\" + strDBKey, strPath);
                        }

                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , strKey , DbHandler.KEY , strDBKey );
                    }
                    nStatus = 1;
                }
                else
                {
                    bool bRet = cMezeoFileCloud.DownloadFile(nsResult.StrContentsUri + '/' + nsResult.StrName, strPath, nsResult.dblSizeInBytes,ref nStatusCode);
                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                    {
                        return nStatusCode;
                    }
                    else if (nStatusCode != ResponseCode.DOWNLOADFILE)
                    {
                        return nStatusCode;
                    }
                    if (bRet)
                    {
                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtagCloud , DbHandler.KEY , strKey);
                        bool isFileExist = File.Exists(strPath);
                        if (isFileExist)
                        {
                            FileInfo fInfo = new FileInfo(strPath);
                            dbHandler.UpdateModifiedDate(fInfo.LastWriteTime, strKey);
                        }
                        nStatus = 1;
                    }
                }

                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_delete")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Enter");

                nqEventCdmiDelete(strPath, strKey);

                nStatus = 1;

                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_rename")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Enter"); 

                string strDBKey = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL }, new string[] { nsResult.StrContentsUri }, new DbType[] { DbType.String });
                if (strDBKey.Trim().Length != 0 && strDBKey != strKey)
                {
                    if(Directory.Exists(strPath))
                    {
                        nqEventCdmiDelete(BasicInfo.SyncDirPath + "\\" + strDBKey, strDBKey);
                    }
                    else
                    {
                        Directory.Move(BasicInfo.SyncDirPath + "\\" + strDBKey, strPath);
                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , strKey , DbHandler.KEY , strDBKey );

                        DirectoryInfo rootDir = new DirectoryInfo(strPath);
                        WalkDirectoryTree(rootDir, BasicInfo.SyncDirPath + "\\" + strDBKey);
                    }
                }
                else if(strDBKey.Trim().Length == 0)
                {
                    nStatus = nqEventCdmiCreate(nqDetail, nsResult, strKey, strPath);
                    if (nStatus == ResponseCode.LOGINFAILED1 || nStatus == ResponseCode.LOGINFAILED2)
                    {
                        return nStatus;
                    }
                    else if (nStatus != ResponseCode.GETETAG && nStatus != ResponseCode.DOWNLOADFILE && nStatus != ResponseCode.DOWNLOADITEMDETAILS && nStatus != 1)
                    {
                        return nStatus;
                    }
                }
                nStatus = 1;
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_copy")
            {
                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Enter"); 

                if (strKey.LastIndexOf("\\") != -1)
                    CheckAndCreateForParentDir(strKey.Substring(0, strKey.LastIndexOf("\\")));

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
                fileFolderInfo.Status = DB_STATUS_SUCCESS;
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
                if (nqDetail.StrObjectType == "DIRECTORY")
                {
                    if (Directory.Exists(strPath))
                    {
                        //string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.KEY + "='" + strKey + "'");
                        string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.KEY }, new string[] { strKey }, new DbType[] { DbType.String });
                        if(strCheck == strKey)
                            bRet = false;
                        else
                            bRet = true;
                    }
                    else
                    {
                        Directory.CreateDirectory(strPath);
                        bRet = true;
                    }
                }
                else
                {
                    if (File.Exists(strPath))
                    {
                        bRet = false;
                    }
                    else
                    {
                        string strEtagCloud = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);
                        if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                        {
                            return nStatusCode;
                        }
                        else if (nStatusCode != ResponseCode.GETETAG)
                        {
                            return nStatusCode;
                        }
                        //string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.E_TAG + "='" + strEtagCloud + "'");
                        string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.E_TAG }, new string[] { strEtagCloud }, new DbType[] { DbType.String });
                        if (strCheck.Trim().Length != 0)
                        {
                            File.Copy(BasicInfo.SyncDirPath + "\\" + strCheck, strPath);
                            fileFolderInfo.ETag = strEtagCloud;
                            bRet = true;
                        }
                        else
                        {
                            bRet = cMezeoFileCloud.DownloadFile(nsResult.StrContentsUri + '/' + nsResult.StrName, strPath, nsResult.dblSizeInBytes, ref nStatusCode);
                            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                            {
                                return nStatusCode;
                            }
                            else if (nStatusCode != ResponseCode.DOWNLOADFILE)
                            {
                                return nStatusCode;
                            }
                        }
                    }
                }

                if (bRet)
                {
                    if (fileFolderInfo.ETag.Trim().Length == 0)
                        fileFolderInfo.ETag = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);

                    if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                    if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                    dbHandler.Write(fileFolderInfo);
                    nStatus = 1;
                }

                Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + "-" + strKey + "Leave"); 
            }

            Debugger.Instance.logMessage("frmSyncManager - UpdateFromNQ - ", "Leave");

            return nStatus;
        }

        private void nqEventCdmiDelete(string strPath, string strKey)
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

                    //SHFILEOPSTRUCT shOperation = new SHFILEOPSTRUCT();
                    //shOperation.wFunc = FO_DELETE;
                    //shOperation.pFrom = strPath;
                    //shOperation.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI;

                    //int nRet = SHFileOperation(ref shOperation);
                    //nRet = GetLastError();

                    SendToRecycleBin(strPath, false);
                    //Directory.Delete(strPath);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY, strKey);
                }
            }
            else
            {
                //SHFILEOPSTRUCT shOperation = new SHFILEOPSTRUCT();
                //shOperation.wFunc = FO_DELETE;
                //shOperation.pFrom = strPath;
                //shOperation.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI;

                //int nRet = SHFileOperation(ref shOperation);
                //nRet = GetLastError();

                SendToRecycleBin(strPath, true);
                //File.Delete(strPath);
                dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY, strKey);
            }
        }

        private int nqEventCdmiCreate(NQDetails nqDetail, NSResult nsResult, string strKey, string strPath)
        {
            //bool bIssuccess = false;
            int nStatus = 0;
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
            fileFolderInfo.Status = DB_STATUS_IN_PROGRESS;
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

            if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
            if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

            dbHandler.Write(fileFolderInfo);

            bool bRet = false;
            string strEtag = "";
            int refCode = 0;
            int nStatusCode = 0;

            if (nqDetail.StrObjectType == "FILE")
            {
                MarkParentsStatus(strPath, DB_STATUS_IN_PROGRESS);
                bRet = cMezeoFileCloud.DownloadFile(nsResult.StrContentsUri + '/' + nsResult.StrName, strPath,nsResult.dblSizeInBytes, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.DOWNLOADFILE)
                {
                    return nStatusCode;
                }
                strEtag = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref refCode);
                if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                {
                    return refCode;
                }
                else if (refCode != ResponseCode.GETETAG)
                {
                    return refCode;
                }
            }
            else
            {
                MarkParentsStatus(strPath, DB_STATUS_IN_PROGRESS);
                Directory.CreateDirectory(strPath);

                strEtag = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref refCode);
                if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                {
                    return refCode;
                }
                else if (refCode != ResponseCode.GETETAG)
                {
                    return refCode;
                }

                bRet = true;
            }

            if (bRet)
            {
                MarkParentsStatus(strPath, DB_STATUS_SUCCESS);
                if (nqDetail.StrObjectType == "DIRECTORY")
                {
                    DirectoryInfo dInfo = new DirectoryInfo(strPath);
                    dbHandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , fileFolderInfo.Key );
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS ,"SUCCESS", DbHandler.KEY ,fileFolderInfo.Key);
                }
                else
                {
                    FileInfo fInfo = new FileInfo(strPath);
                    dbHandler.UpdateModifiedDate(fInfo.LastWriteTime, fileFolderInfo.Key);
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , fileFolderInfo.Key );
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , "SUCCESS", DbHandler.KEY ,fileFolderInfo.Key );
                    nStatus = 1;
                }
            }

            if (nqDetail.StrObjectType == "DIRECTORY")
            {
                ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(nsResult.StrContentsUri, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                {
                    return nStatusCode;
                }
                if (iDetails != null)
                {
                    for (int num = 0; num < iDetails[0].nTotalItem; num++)
                    {
                        nStatus = DownloadFolderStructureForNQ(iDetails[num], strKey);
                        if (nStatus == ResponseCode.LOGINFAILED1 || nStatus == ResponseCode.LOGINFAILED2)
                        {
                            return nStatus;
                        }
                        else if (nStatus != ResponseCode.GETETAG && nStatus != ResponseCode.DOWNLOADFILE && nStatus != ResponseCode.DOWNLOADITEMDETAILS && nStatus != 1)
                        {
                            return nStatus;
                        }
                    }
                }
                else
                    nStatus = 1;
            }

            return nStatus;
        }

        private void CheckAndCreateForParentDir(string strKey)
        {
            string strpath = BasicInfo.SyncDirPath + "\\" + strKey;

            bool bIsDir = Directory.Exists(strpath);
            if (!bIsDir)
            {
                if (strKey.LastIndexOf("\\") != -1)
                    CheckAndCreateForParentDir(strKey.Substring(0, strKey.LastIndexOf("\\")));
                Directory.CreateDirectory(strpath);
            }

        }

        private int DownloadFolderStructureForNQ(ItemDetails iDetail,string strParentKey)
        {
            //bool bIssuccess = false;
            int nStatus = 0;
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
            fileFolderInfo.Status = DB_STATUS_IN_PROGRESS;
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

            if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
            if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

            dbHandler.Write(fileFolderInfo);

            string strEtag = "";
            int refCode = 0;

            if (iDetail.szItemType == "FILE")
            {
                MarkParentsStatus(strPath, DB_STATUS_IN_PROGRESS);
                bRet = cMezeoFileCloud.DownloadFile(iDetail.szContentUrl + '/' + iDetail.strName, strPath,iDetail.dblSizeInBytes, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.DOWNLOADFILE)
                {
                    return nStatusCode;
                }
                if (bRet)
                {
                    strEtag = cMezeoFileCloud.GetETag(iDetail.szContentUrl, ref refCode);
                    if (refCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                    {
                        return refCode;
                    }
                    else if (refCode != ResponseCode.GETETAG)
                    {
                        return refCode;
                    }
                    FileInfo fInfo = new FileInfo(strPath);
                    dbHandler.UpdateModifiedDate(fInfo.LastWriteTime, fileFolderInfo.Key);
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , fileFolderInfo.Key );
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , "SUCCESS", DbHandler.KEY ,fileFolderInfo.Key );
                }
                MarkParentsStatus(strPath, DB_STATUS_SUCCESS);
                nStatus = 1;
            }
            else
            {
                MarkParentsStatus(strPath, DB_STATUS_IN_PROGRESS);
                Directory.CreateDirectory(strPath);

                strEtag = cMezeoFileCloud.GetETag(iDetail.szContentUrl, ref refCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.GETETAG)
                {
                    return nStatusCode;
                }

                DirectoryInfo dInfo = new DirectoryInfo(strPath);
                dbHandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , fileFolderInfo.Key );
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , "SUCCESS", DbHandler.KEY ,fileFolderInfo.Key );

                MarkParentsStatus(strPath, DB_STATUS_SUCCESS);

                ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(iDetail.szContentUrl, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    return nStatusCode;
                }
                else if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                {
                    return nStatusCode;
                }
                if (iDetails != null)
                {
                    for (int num = 0; num < iDetails[0].nTotalItem; num++)
                    {
                        nStatus = DownloadFolderStructureForNQ(iDetails[num], fileFolderInfo.Key);
                        if (nStatus == ResponseCode.LOGINFAILED1 || nStatus == ResponseCode.LOGINFAILED2)
                        {
                            return nStatus;
                        }
                        else if (nStatus != ResponseCode.GETETAG && nStatus != ResponseCode.DOWNLOADFILE && nStatus != ResponseCode.DOWNLOADITEMDETAILS)
                        {
                            return nStatus;
                        }
                    }
                }
                else
                    nStatus = 1;
            }

            return nStatus;
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

            //if (this.InvokeRequired)
            //{

            //    this.Invoke((MethodInvoker)delegate
            //    {
            //        cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");

            //        if (fileDownloadCount <= pbSyncProgress.Maximum)
            //            pbSyncProgress.Value = fileDownloadCount;

            //        lblPercentDone.Text = (int)progress + "%";
            //        lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerDownloading") + " " + (fileDownloadCount) + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + pbSyncProgress.Maximum;
            //    });
            //}
            //else
            {
                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");

                if (fileDownloadCount <= pbSyncProgress.Maximum)
                    pbSyncProgress.Value = fileDownloadCount;

                lblPercentDone.Text = (int)progress + "%";
                lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerDownloading") + " " + (fileDownloadCount) + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + pbSyncProgress.Maximum;
            }
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

        private void ShowSyncMessage(bool IsStopped = false, bool IsLocalEvents = false)
        {
            lastSync = DateTime.Now;
            BasicInfo.LastSyncAt = lastSync;

            //if (BasicInfo.AutoSync)
            //{
            //    //tmrNextSync.Tick += new EventHandler(tmrNextSync_Tick);

            //    if (InvokeRequired)
            //    {
            //        this.Invoke((MethodInvoker)delegate
            //        {
            //            //tmrNextSync.Interval = FIVE_MINUTES;
            //            tmrNextSync.Enabled = true;
            //        });
            //    }
            //    else
            //    {
            //        //tmrNextSync.Interval = FIVE_MINUTES;
            //        tmrNextSync.Enabled = true;
            //    }

            //}

            UpdateUsageLabel();

            DisableProgress();
            this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");
            this.btnSyncNow.Enabled = true;

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

            if (IsStopped)
            {
                //cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    SetIssueFound(true);
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                              LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                             ToolTipIcon.None);

                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
                }
                else
                {
                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                             LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                            ToolTipIcon.None);
                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");

                    lblStatusL1.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                }
            }
            else
            {
                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    SetIssueFound(true);
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
                    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                              LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                             ToolTipIcon.None);

                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
                }
                else
                {
                    lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerStatusAllFilesInSyncLabel");
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
                    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
                }
            }

            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
           // System.Threading.Thread.Sleep(200);
            //label1.Visible = true;
            label1.Text = LanguageTranslator.GetValue("SyncManagerStatusNextSyncAtLabel") + " " + lastSync.AddMinutes(5).ToString("h:mm tt");
           // cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
            //frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");

            //if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
            //{
            //    SetIssueFound(true);
            //    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
            //    cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
            //                                                              LanguageTranslator.GetValue("SyncIssueFoundText"),
            //                                                             ToolTipIcon.None);

            //    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

            //    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
            //}
            //else
            //{
            //    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            //    cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
            //    frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
            //}
        }

        public void ShowOfflineAtStartUpSyncManager()
        {
            lastSync = BasicInfo.LastSyncAt;
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            label1.Text = LanguageTranslator.GetValue("SyncManagerResumeSync");
            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
        }

        private void ShowSyncDisabledMessage()
        {
            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");

            cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncManagerSyncDisabled") + ". " + LanguageTranslator.GetValue("SyncManagerTrayEnableOnText"),
                                                                         ToolTipIcon.None);

            cnotificationManager.HoverText = LanguageTranslator.GetValue("SyncManagerSyncDisabled") + ". " + LanguageTranslator.GetValue("SyncManagerTrayEnableOnText");
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

            lblStatusL1.Text = "";
            lblStatusL3.Text = "";

            pbSyncProgress.Visible = false;
            btnMoveFolder.Enabled = true;
            ShowNextSyncLabel(true);
        }

        private void EnableProgress()
        {
            lblPercentDone.Visible = true;
            pbSyncProgress.Visible = true;
            ShowNextSyncLabel(false);
            Application.DoEvents();
        }

        private void OpenFolder()
        {
            string argument = BasicInfo.SyncDirPath;
            System.Diagnostics.Process.Start(argument);
        }

        private bool ConnectedToInternet()
        {
            
            return false;
        }

        public void DisableSyncManager()
        {
            isDisabledByConnection = true;
            StopSync();

            pnlFileSyncOnOff.Enabled = false;
            rbSyncOff.Checked = true;

            btnMoveFolder.Enabled = false;
            btnSyncNow.Enabled = false;
            //tmrNextSync.Enabled = false;
            //lnkFolderPath.Enabled = false;

        }

        public void EnableSyncManager()
        {
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOnlineText"), ToolTipIcon.None);

            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOnlineText");
            cnotificationManager.NotifyIcon = Properties.Resources.MezeoVault;

            pnlFileSyncOnOff.Enabled = true;

            if (BasicInfo.AutoSync)
            {
                rbSyncOn.Checked = true;
            }
            else
            {
                rbSyncOff.Checked = true;
            }

            isDisabledByConnection = false;
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
            return dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { strPath }, new DbType[] { DbType.String });
        }

        public string GetETag(string strPath)
        {
            return dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.E_TAG, new string[] { DbHandler.KEY }, new string[] { strPath }, new DbType[] { DbType.String });
        }

        private int CheckForModifyEvent(LocalEvents lEvent)
        {
            string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
            if (strCheck.Trim().Length == 0)
                return 2;
            else
            {
                DateTime DBModTime = dbHandler.GetDateTime(DbHandler.TABLE_NAME, DbHandler.MODIFIED_DATE, DbHandler.KEY ,lEvent.FileName);

                DateTime ActualModTime = File.GetLastWriteTime(lEvent.FullPath);
                ActualModTime = ActualModTime.AddMilliseconds(-ActualModTime.Millisecond);
                TimeSpan diff = ActualModTime - DBModTime;

                if (diff >= TimeSpan.FromSeconds(1))
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
            fInfo.Status = DB_STATUS_IN_PROGRESS;
           

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
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , lEvent.FileName , DbHandler.KEY , lEvent.OldFileName);
            UpdateDBForStatus(lEvent, DB_STATUS_IN_PROGRESS);
        }

        private void UpdateDBForStatus(LocalEvents lEvent, string strStatus)
        {
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , strStatus , DbHandler.KEY , lEvent.FileName );
        }

        private void UpdateDBForAddedSuccess(string strContentUri, LocalEvents lEvent)
        {
            int nStatusCode = 0;
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL , strContentUri , DbHandler.KEY , lEvent.FileName);

            string strEtag = cMezeoFileCloud.GetETag(strContentUri, ref nStatusCode);
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , lEvent.FileName );

            UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
        }

        private void UpdateDBForRemoveSuccess(LocalEvents lEvent)
        {
            string strType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.TYPE, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
            dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , lEvent.FileName);

            if (strType == "DIRECTORY")
            {                
                UpdateDBForRemoveDir(lEvent.FileName);
            }
        }

        private void UpdateDBForRemoveDir(string strDir)
        {
            string strParentDir = strDir.Substring(strDir.LastIndexOf("\\") + 1);
            List<string> result = dbHandler.GetStringList(DbHandler.TABLE_NAME, DbHandler.KEY, DbHandler.PARENT_DIR , strParentDir);
            foreach (string path in result)
            {
                if ((path.Length > strDir.Length) && (path.Substring(0, strDir.Length) == strDir))
                {
                    string strType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.TYPE, new string[] { DbHandler.KEY }, new string[] { path }, new DbType[] { DbType.String });
                    if (strType == "DIRECTORY")
                    {
                        dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , path);
                        //string strpath = path.Substring(path.LastIndexOf("\\") + 1);
                        UpdateDBForRemoveDir(path);
                    }
                    else
                        dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , path);
                }
            }
        }

        private void UpdateDBForRenameSuccess(LocalEvents lEvent)
        {
            UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);

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

                    //SHFILEOPSTRUCT shOperation = new SHFILEOPSTRUCT();
                    //shOperation.wFunc = FO_DELETE;
                    //shOperation.pFrom = fi.FullName;
                    //shOperation.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI;

                    //int nRet = SHFileOperation(ref shOperation);
                    //nRet = GetLastError();
                    SendToRecycleBin(fi.FullName, true);
                   // File.Delete(fi.FullName);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , Key);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTreeForDelete(dirInfo);

                    string Key = dirInfo.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);

                    //SHFILEOPSTRUCT shOperation = new SHFILEOPSTRUCT();
                    //shOperation.wFunc = FO_DELETE;
                    //shOperation.pFrom = dirInfo.FullName;
                    //shOperation.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI;

                    //int nRet = SHFileOperation(ref shOperation);
                    //nRet = GetLastError();

                    SendToRecycleBin(dirInfo.FullName, false);
                    //Directory.Delete(dirInfo.FullName);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , Key);
                }
            }
        }

        private void WalkDirectoryTreeforAddFolder(System.IO.DirectoryInfo root, string lEventOldPath, ref List<LocalEvents> addEvents)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            bool bIsAlreadyAdded = false;
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
                    bIsAlreadyAdded = false;
                    foreach (LocalEvents id in events)
                    {
                        if (id.FileName == lEventOldPath + "\\" + fi.Name && id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                            bIsAlreadyAdded = true;
                    }

                    FileAttributes attr = File.GetAttributes(fi.FullName);
                    if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden || (attr & FileAttributes.Temporary) == FileAttributes.Temporary)
                        bIsAlreadyAdded = true;

                    if (!bIsAlreadyAdded)
                    {
                        LocalEvents lEvent = new LocalEvents();
                        lEvent.FileName = lEventOldPath + "\\" + fi.Name;
                        lEvent.FullPath = fi.FullName;
                        lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;

                        addEvents.Add(lEvent);
                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    bIsAlreadyAdded = false;
                    // Resursive call for each subdirectory.
                    foreach (LocalEvents id in events)
                    {
                        if (id.FileName == lEventOldPath + "\\" + dirInfo.Name && id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                            bIsAlreadyAdded = true;
                    }

                    if (!bIsAlreadyAdded)
                    {
                        LocalEvents lEvent = new LocalEvents();
                        lEvent.FileName = lEventOldPath + "\\" + dirInfo.Name;
                        lEvent.FullPath = dirInfo.FullName;
                        lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;

                        addEvents.Add(lEvent);
                    }

                    WalkDirectoryTreeforAddFolder(dirInfo, lEventOldPath + "\\" + dirInfo.Name , ref addEvents);
                }
            }
        }

        private void WalkDirectoryTreeForMove(System.IO.DirectoryInfo root,string lEventOldPath)
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

                   string strParentDir = lEventOldPath.Substring(lEventOldPath.LastIndexOf("\\") + 1);
                    
                   dbHandler.Update(DbHandler.TABLE_NAME,DbHandler.KEY , newKey ,DbHandler.KEY , oldKey );

                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTreeForMove(dirInfo, lEventOldPath + "\\" + dirInfo.Name);

                    string newKey = dirInfo.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);
                    string oldKey = lEventOldPath.Substring(BasicInfo.SyncDirPath.Length + 1);
                    oldKey += "\\" + dirInfo.Name;

                    string strParentDir = lEventOldPath.Substring(lEventOldPath.LastIndexOf("\\") + 1);

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , newKey , DbHandler.KEY , oldKey );
                }
            }
        }


        private void WalkDirectoryTree(System.IO.DirectoryInfo root, string lEventOldPath)
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

                    string strParentDir = lEventOldPath.Substring(lEventOldPath.LastIndexOf("\\") + 1);

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , newKey , DbHandler.KEY , oldKey );
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_DIR , strParentDir , DbHandler.KEY , newKey );

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

                    string strParentDir = lEventOldPath.Substring(lEventOldPath.LastIndexOf("\\") + 1);

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , newKey , DbHandler.KEY ,oldKey );
                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_DIR , strParentDir , DbHandler.KEY ,newKey );
                }
            }
        }

       private void UpdateDBForModifiedSuccess(LocalEvents lEvent, string strContentURi)
        {
            int nStatusCode = 0;
            string strEtag = cMezeoFileCloud.GetETag(strContentURi, ref nStatusCode);
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , lEvent.FileName );

            FileInfo fileInfo = new FileInfo(lEvent.FullPath);

            dbHandler.UpdateModifiedDate(fileInfo.LastWriteTime, lEvent.FileName);

            UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
        }

       private bool CheckModifiedEvent(string str, LocalEvents lEventMod, List<LocalEvents> eMoveList)
       {
           bool bRet = false;
           int nRet = 0;
           bool bIsMove = false;
           bool bIsBreak = false;

           while (!bIsBreak)
           {
               foreach (LocalEvents id in events)
               {
                   if (id.FileName == str && id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                   {
                       foreach (LocalEvents eMove in eMoveList)
                       {
                           if (eMove.FileName == str)
                               bIsMove = true;
                       }

                       if (!bIsMove)
                       {
                           lEventMod.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;
                           bRet = false;
                           bIsBreak = true;
                       }
                       else
                           bRet = false;

                       break;
                   }
                   else
                       bRet = false;
               }

               if (!bRet)
               {
                   nRet = str.LastIndexOf("\\");
                   if (nRet == -1)
                   {
                       bIsBreak = true;
                   }
                   else
                   {
                       //LocalEvents lEvent = new LocalEvents();
                       //lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;
                       //lEvent.FileName = str;
                       //lEvent.FullPath = BasicInfo.SyncDirPath + "\\" + str;

                       //events.Add(lEvent);
                       str = str.Substring(0, nRet);
                   }
               }
           }

           if (bRet)
               return true;
           else
               return false;
       }

       protected virtual bool IsFileLocked(FileInfo file)
       {
           FileStream stream = null;

           try
           {
               if (File.Exists(file.FullName))
                   stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
               else
                   return false;
           }
           catch (IOException)
           {
               //the file is unavailable because it is:
               //still being written to
               //or being processed by another thread
               //or does not exist (has already been processed)
               return true;
           }
           catch (Exception)
           {
               return File.Exists(file.FullName);
           }
           finally
           {
               if (stream != null)
                   stream.Close();
           }

           //file is not locked
           return false;
       }

        private int HandleEvents(BackgroundWorker caller)
        {
            Debugger.Instance.logMessage("frmSyncManager - HandleEvents", "Enter");

            //if (!BasicInfo.IsConnectedToInternet)
            //{
            //    //frmParent.HandleConnectionState();
            //    Debugger.Instance.logMessage("frmSyncManager - HandleEvents", "return with -2");
            //    return -2;
            //}

            isLocalEventInProgress = true;

            List<int> RemoveIndexes = new List<int>();

            List<LocalEvents> eModified = new List<LocalEvents>();

            List<LocalEvents> eMove = new List<LocalEvents>();

            List<LocalEvents> eAddEvents = new List<LocalEvents>();

            foreach (LocalEvents lEvent in events)
            {
                if (caller.CancellationPending)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents ", "Cancelled called");
                    caller.CancelAsync();
                    return USER_CANCELLED;
                }

                bool bRet = true;

                Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath);

                FileAttributes attr = FileAttributes.Normal ;

                bool isDirectory = false;
                bool isFile = File.Exists(lEvent.FullPath);
                if (!isFile)
                    isDirectory = Directory.Exists(lEvent.FullPath);
                if (isFile || isDirectory)
                    attr = File.GetAttributes(lEvent.FullPath);
                else
                {
                    if (lEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
                    {
                        if (!RemoveIndexes.Contains(events.IndexOf(lEvent)))
                            RemoveIndexes.Add(events.IndexOf(lEvent));
                        continue;
                    }
                }


                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Enter");

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
                            bRet = false;
                        }

                    }
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED || lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Enter");
                    //string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + lEvent.FileName + "' and " + DbHandler.STATUS + "='SUCCESS'");

                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY, DbHandler.STATUS }, new string[] { lEvent.FileName, DB_STATUS_SUCCESS }, new DbType[] { DbType.String, DbType.String });

                    if (strCheck.Trim().Length == 0)
                        bRet = true;
                    else
                        bRet = false;

                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED && bRet)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "- checking for move events - Enter");
                    string strNameAdd = lEvent.FileName.Substring(lEvent.FileName.LastIndexOf("\\") + 1);
                    foreach (LocalEvents id in events)
                    {
                        DateTime dtCreate = lEvent.EventTimeStamp.AddMilliseconds(-id.EventTimeStamp.Millisecond);
                        TimeSpan Diff = dtCreate - id.EventTimeStamp;
                       // dtCreate = dtCreate.AddMilliseconds(-dtCreate.Millisecond);
                        if (Diff <= TimeSpan.FromSeconds(1) && id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                        {
                           // if (id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                           // {
                            string strNameComp = id.FileName.Substring(id.FileName.LastIndexOf("\\") + 1);
                            if (strNameComp == strNameAdd)
                            {
                                if (!RemoveIndexes.Contains(events.IndexOf(id)))
                                    RemoveIndexes.Add(events.IndexOf(id));

                                bRet = false;

                                LocalEvents levent = new LocalEvents();
                                levent.FileName = lEvent.FileName;
                                levent.FullPath = lEvent.FullPath;
                                levent.OldFileName = id.FileName;
                                levent.OldFullPath = id.FullPath;
                                levent.EventType = LocalEvents.EventsType.FILE_ACTION_MOVE;

                                eMove.Add(levent);
                            }
                           // }
                        }
                    }
                    bool bIsMove = false;
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        foreach (LocalEvents eMoveEvent in eMove)
                        {
                            if (eMoveEvent.FileName == lEvent.FileName)
                                bIsMove = true;
                        }

                        if (!bIsMove)
                        {
                            WalkDirectoryTreeforAddFolder(new DirectoryInfo(lEvent.FullPath), lEvent.FileName, ref eAddEvents);
                        }
                    }

                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "- checking for move events - Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Enter");

                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.OldFileName }, new DbType[] { DbType.String });
                    if (strCheck.Trim().Length == 0)
                    {
                        int nIndex = lEvent.FullPath.LastIndexOf(".");
                        string strExtnsion = lEvent.FullPath.Substring(nIndex + 1);
                        if (strExtnsion == "doc" ||
                            strExtnsion == "docx" ||
                            strExtnsion == "xls" ||
                            strExtnsion == "xlsx" ||
                            strExtnsion == "ppt" ||
                            strExtnsion == "pptx" ||
                            strExtnsion == "rtf")
                        {
                            LocalEvents levent = new LocalEvents();
                            levent.FileName = lEvent.FileName;
                            levent.FullPath = lEvent.FullPath;
                            levent.EventType = LocalEvents.EventsType.FILE_ACTION_MODIFIED;

                            eModified.Add(levent);
                        }
                        else
                        {
                            string strCheck1 = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY, DbHandler.STATUS }, new string[] { lEvent.FileName, DB_STATUS_SUCCESS }, new DbType[] { DbType.String, DbType.String });
                            if (strCheck1.Trim().Length == 0)
                            {
                                //LocalEvents lNewEvent = new LocalEvents();
                                lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;
                                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                {
                                    WalkDirectoryTreeforAddFolder(new DirectoryInfo(lEvent.FullPath), lEvent.FileName, ref eAddEvents);
                                }
                                //eAddEvents.Add(lNewEvent);
                                //bRet = false;
                            }
                        }
                    }

                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Enter");

                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                    if (strCheck.Trim().Length == 0)
                        bRet = false;
                    else
                        bRet = true;

                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "-" + lEvent.EventType.ToString() + "Leave");
                }

               // if (lEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
              //  {
                    if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden || (attr & FileAttributes.Temporary) == FileAttributes.Temporary )
                        bRet = false;
               // }

                if (bRet)
                {
                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "AddinDB Enter");

                    if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                    {
                        UpdateDBForStatus(lEvent, DB_STATUS_IN_PROGRESS);
                    }
                    else if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                    {
                        AddInDBForAdded(lEvent);
                    }
                    else if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                    {
                        AddInDBForRename(lEvent);
                    }

                    Debugger.Instance.logMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "AddinDB Leave");
                }
              
                if (!bRet)
                {
                    if(!RemoveIndexes.Contains(events.IndexOf(lEvent)))
                        RemoveIndexes.Add(events.IndexOf(lEvent));
                }
            }

            RemoveIndexes.Sort();
            for(int n = RemoveIndexes.Count-1 ; n >=0 ; n--)
            {
                events.RemoveAt(RemoveIndexes[n]);
            }

            RemoveIndexes.Clear();

            if (eModified.Count != 0)
            {
                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eModifiedList -", eModified.Count.ToString() + " Enter");

                foreach (LocalEvents levent in eModified)
                {
                    UpdateDBForStatus(levent, DB_STATUS_IN_PROGRESS);
                }
                events.AddRange(eModified);
                eModified.Clear();

                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eModifiedList -", eModified.Count.ToString() + " Leave");
            }

            if (eAddEvents.Count != 0)
            {
                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eAddEventsList -", eAddEvents.Count.ToString() + " Enter");

                foreach (LocalEvents levent in eAddEvents)
                {
                    AddInDBForAdded(levent);
                }
                events.AddRange(eAddEvents);
                eAddEvents.Clear();

                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eAddEventsList -", eAddEvents.Count.ToString() + " Leave");
            }

            if (eMove.Count != 0)
            {
                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eMoveList -", eMove.Count.ToString() + " Enter");
                foreach (LocalEvents levent in eMove)
                {
                    UpdateKeyInDb(levent.OldFileName, levent.FileName);
                    UpdateDBForStatus(levent, DB_STATUS_IN_PROGRESS);

                    int sepIndex = levent.FileName.LastIndexOf("\\");
                    string newParentDir = levent.FileName.Substring(0, sepIndex);

                    sepIndex = newParentDir.LastIndexOf("\\");
                    newParentDir = newParentDir.Substring(sepIndex + 1);

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_DIR , newParentDir , DbHandler.KEY , levent.FileName);

                    MarkParentsStatus(levent.FullPath, DB_STATUS_IN_PROGRESS);

                    if (Directory.Exists(levent.FullPath))
                    {
                        WalkDirectoryTreeForMove(new DirectoryInfo(levent.FullPath), levent.OldFullPath);

                    }

                }
                events.AddRange(eMove);
                eMove.Clear();

                Debugger.Instance.logMessage("frmSyncManager - HandleEvents eMoveList -", eMove.Count.ToString() + " Leave");
            }
            int returnCode = 1;
            if (events.Count == 0)
            {
               
                Debugger.Instance.logMessage("frmSyncManager - HandleEvents" ," Events Count NUll");

                isLocalEventInProgress = false;
                if (LocalEventList.Count != 0)
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

                    returnCode = HandleEvents(caller);

                }
                return returnCode;
            }

            if (caller != null)
            {
                caller.ReportProgress(SYNC_STARTED);
            }

            Debugger.Instance.logMessage("frmSyncManager - HandleEvents", " ProcessLocalEvents Going");

            returnCode = ProcessLocalEvents(caller);

            Debugger.Instance.logMessage("frmSyncManager - HandleEvents", " ProcessLocalEvents Exit");

            isLocalEventInProgress = false;

            //if (caller != null && returnCode != 0)
            //{
            //    caller.ReportProgress(LOCAL_EVENTS_COMPLETED);
            //}
            //else if (returnCode == 0)
            //{
            //    caller.ReportProgress(LOCAL_EVENTS_STOPPED);
            //}
            
            Debugger.Instance.logMessage("frmSyncManager - HandleEvents", "Leave");
            return returnCode;
        }

        private void UpdateKeyInDb(string oldKey, string newKey)
        {
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , newKey , DbHandler.KEY , oldKey );
        }

        private void MarkParentsStatus(string path,string status)
        {
            string syncPath = path.Substring(BasicInfo.SyncDirPath.Length + 1);
            ChangeParentStatus(syncPath, status);
        }

        private void ChangeParentStatus(string syncPath, string status)
        {
            int sepIndex = syncPath.LastIndexOf("\\");

            if (sepIndex != -1)
            {
                string parentKey = syncPath.Substring(0, sepIndex);
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , status , DbHandler.KEY , parentKey );
                ChangeParentStatus(parentKey, status);
            }
        }

        private string CheckAndCreateForEventsParentDir(string strKeyEvent)
        {
            string strKey = strKeyEvent.Substring(0, strKeyEvent.LastIndexOf("\\"));
                   
            //bool bIsDir = Directory.Exists(strpath);
            string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { strKey }, new DbType[] { DbType.String });
            if (strCheck.Trim().Length == 0)
            {
                string strpath = BasicInfo.SyncDirPath + "\\" + strKey;     
                LocalEvents levent = new LocalEvents();
                levent.FullPath = strpath;
                levent.FileName = strKey;
                levent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;

                AddInDBForAdded(levent);
              
                string strUrl = "";
                int nStatusCode = 0;

                string strParentURi = GetParentURI(levent.FileName);
                string folderName = levent.FullPath.Substring((levent.FullPath.LastIndexOf("\\") + 1));
                strUrl = cMezeoFileCloud.NewContainer(folderName, strParentURi, ref nStatusCode);
                strUrl += "/contents";

                if ((strUrl.Trim().Length != 0) && (nStatusCode == 201))
                {
                    UpdateDBForAddedSuccess(strUrl, levent);

                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL , strUrl , DbHandler.KEY , strKeyEvent);
                }

                return strUrl;
            }

            return "";

        }

        private int ProcessLocalEvents(BackgroundWorker caller)
        {
            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Enter");
            string strUrl = "";
            bool bRetConflicts = true;

            if (caller != null)
            {
                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Calling ReportProgress with PROCESS_LOCAL_EVENTS_STARTED");
                caller.ReportProgress(PROCESS_LOCAL_EVENTS_STARTED,events.Count);
            }

            fileDownloadCount = 1;

            List<int> SuccessIndexes = new List<int>();
            //bool bOffline = false;

            foreach (LocalEvents lEvent in events)
            {
                if (caller.CancellationPending)
                {
                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Canceled Called");
                    caller.CancelAsync();
                    return USER_CANCELLED;
                }

                //if (!isLocalEventInProgress)
                //{
                //    StopLocalSync();
                //    return;
                //}

              //  fileDownloadCount++ ;

                if (caller != null)
                {
                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "ReportProgress Called with PROGRESS_CHANGED_WITH_FILE_NAME for " + lEvent.FullPath);
                    caller.ReportProgress(PROGRESS_CHANGED_WITH_FILE_NAME, lEvent.FullPath);
                }

                FileAttributes attr = FileAttributes.Normal;

                bool isDirectory = false;
                bool isFile = File.Exists(lEvent.FullPath);

                if (isFile && lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                {
                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Check for file lock - Enter");
                    FileInfo fInfo = new FileInfo(lEvent.FullPath);
                    bool IsLocked = IsFileLocked(fInfo);
                    while (IsLocked && fInfo.Exists)
                    {
                        IsLocked = IsFileLocked(fInfo);
                    }
                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Check for file lock - Leave");
                }

                isFile = File.Exists(lEvent.FullPath);
                if(!isFile)
                    isDirectory = Directory.Exists(lEvent.FullPath);
                if (isFile || isDirectory)
                    attr = File.GetAttributes(lEvent.FullPath);
                else
                {
                    if (lEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
                        continue;
                }               

                int nStatusCode = 0;
                bool bRet = true;
                
                switch (lEvent.EventType)
                {
                    case LocalEvents.EventsType.FILE_ACTION_MOVE:
                        {
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MOVE - Enter for file path " + lEvent.FullPath);
                             string strContentURi =GetContentURI(lEvent.FileName);
                             if (strContentURi.Trim().Length == 0)
                             {
                                 Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                 continue;
                             }

                            if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                                strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                            {
                                strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                            }

                            string strParentUri = GetParentURI(lEvent.FileName);
                            if (strParentUri.Trim().Length == 0)
                            {
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetParentURI for length ZERO");
                                continue;
                            }

                            if (strParentUri.Substring(strParentUri.Length - 9).Equals("/contents") ||
                                strParentUri.Substring(strParentUri.Length - 8).Equals("/content"))
                            {
                                strParentUri = strParentUri.Substring(0, strParentUri.LastIndexOf("/"));
                            }

                            string strName = lEvent.FullPath.Substring(lEvent.FullPath.LastIndexOf("\\") + 1);
                            ItemDetails iDetails = cMezeoFileCloud.GetContinerResult(strContentURi, ref nStatusCode);

                            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                            {
                                return LOGIN_FAILED;
                            }
                            else if (nStatusCode != ResponseCode.GETCONTINERRESULT)
                            {
                                return SERVER_INACCESSIBLE;
                            }

                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PUBLIC , iDetails.bPublic , DbHandler.KEY , lEvent.FileName );
                            string mimeType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.MIMIE_TYPE, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                bRet = cMezeoFileCloud.ContainerMove(strContentURi, strName, mimeType, iDetails.bPublic, strParentUri, ref nStatusCode);
                            }
                            else 
                            {
                                bRet = cMezeoFileCloud.FileMove(strContentURi, strName, mimeType, iDetails.bPublic, strParentUri, ref nStatusCode);
                            }

                            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                            {
                                return LOGIN_FAILED;
                            }
                            else if (nStatusCode != ResponseCode.CONTAINERMOVE)
                            {
                                return SERVER_INACCESSIBLE;
                            }
                            else
                            {
                                SuccessIndexes.Add(events.IndexOf(lEvent));
                                UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
                                MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                            }

                            //if (bRet && nStatusCode == 204)
                            //{
                            //    SuccessIndexes.Add(events.IndexOf(lEvent));
                            //    UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
                            //    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                            //}
                            //else if (nStatusCode != 204 && nStatusCode != 401 && nStatusCode != 403)
                            //{
                            //    bRet = false;
                            //    bOffline = true;

                            //}

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MOVE - Leave for file path " + lEvent.FullPath);

                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_ADDED:
                        {
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_ADDED - Enter for file path " + lEvent.FullPath);
                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);
                            string strParentURi = GetParentURI(lEvent.FileName);
                            if (strParentURi.Trim().Length == 0)
                            {
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetParentURI for length ZERO");
                                continue;
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {           
                                string folderName = lEvent.FullPath.Substring((lEvent.FullPath.LastIndexOf("\\") + 1));

                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Create new container for folder " + folderName);

                                // Defect 273 - If somehow, someway the same folder name exists locally and on the cloud,
                                //              do not create a new folder in the cloud with this name.  Just use the existing
                                //              container and upload files/containers into it.
                                ItemDetails[] itemDetails;
                                bool bCreateCloudContainer = true;
                                // Grab a list of files and containers for the parent.
                                itemDetails = cMezeoFileCloud.DownloadItemDetails(strParentURi, ref nStatusCode);
                                if (nStatusCode == 200)
                                {
                                    // Look through each item for a container with the same name.
                                    foreach (ItemDetails item in itemDetails)
                                    {
                                        if ("DIRECTORY" == item.szItemType)
                                        {
                                            if (folderName == item.strName)
                                            {
                                                // Don't create a new/duplicate folder.
                                                bCreateCloudContainer = false;
                                                // Populate the url with this folder.
                                                strUrl = item.szContentUrl;
                                            }
                                        }
                                    }
                                }
                                if (bCreateCloudContainer)
                                   strUrl = cMezeoFileCloud.NewContainer(folderName, strParentURi, ref nStatusCode);
                                    if (nStatusCode == 201)
                                        strUrl += "/contents";

                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Container URI for folder " + folderName + " is " + strUrl);
                            }
                            else
                            {
                                //if (strParentURi.Trim().Length == 0)
                                //    strParentURi = CheckAndCreateForEventsParentDir(lEvent.FileName);

                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Start uploading file for " + lEvent.FullPath + ", at parent URI " + strParentURi);

                                strUrl = cMezeoFileCloud.UploadingFile(lEvent.FullPath, strParentURi, ref nStatusCode);
                                if(nStatusCode == 201)
                                    strUrl += "/content"; 
                            }

                            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                            {
                                return LOGIN_FAILED;
                            }
                            else if (nStatusCode != ResponseCode.UPLOADINGFILE)
                            {
                                return SERVER_INACCESSIBLE;
                            }
                            else if ((strUrl.Trim().Length != 0) && (nStatusCode == ResponseCode.UPLOADINGFILE))
                            {
                                SuccessIndexes.Add(events.IndexOf(lEvent));
                                bRet = true;  

                                string strParent = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                if (strParent.Trim().Length == 0)
                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL , strParentURi , DbHandler.KEY , lEvent.FileName);

                                MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                UpdateDBForAddedSuccess(strUrl, lEvent);
                            }
                            //else if (nStatusCode != 201 && nStatusCode != 401 && nStatusCode != 403)
                            //{
                            //    bRet = false;
                            //    bOffline = true;
                            //}

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_ADDED - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_MODIFIED:
                        {
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MODIFIED - Enter for file path " + lEvent.FullPath);

                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);
                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                continue;
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                bRet = false;
                            else
                            {
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                                if (bRetConflicts)
                                {
                                    bRet = cMezeoFileCloud.OverWriteFile(lEvent.FullPath, strContentURi, ref nStatusCode);
                                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                    {
                                        return LOGIN_FAILED;
                                    }
                                    else if (nStatusCode != ResponseCode.OVERWRITEFILE)
                                    {
                                        return SERVER_INACCESSIBLE;
                                    }
                                    else
                                    {
                                        SuccessIndexes.Add(events.IndexOf(lEvent));
                                        MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                        UpdateDBForModifiedSuccess(lEvent, strContentURi);
                                    }
                                    //else if (nStatusCode != 204 && nStatusCode != 401 && nStatusCode != 403)
                                    //{
                                    //    bRet = false;
                                    //    bOffline = true;
                                    //}
                                    //else
                                    //{
                                    //    ReportConflict(lEvent, IssueFound.ConflictType.CONFLICT_UPLOAD);
                                    //    bRet = false;
                                    //}
                                }                               
                            }

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MODIFIED - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_REMOVED:
                        {
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_REMOVED - Enter for file path " + lEvent.FullPath);

                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                continue;
                            }

                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);

                            if (isFile)
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);

                            if (bRetConflicts)
                            {
                                if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                                    strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                                {
                                    strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                                }

                                bRet = cMezeoFileCloud.Delete(strContentURi, ref nStatusCode);
                                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                {
                                    return LOGIN_FAILED;
                                }
                                else if (nStatusCode != ResponseCode.DELETE)
                                {
                                    return SERVER_INACCESSIBLE;
                                }
                                else
                                {
                                    SuccessIndexes.Add(events.IndexOf(lEvent));
                                    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                    UpdateDBForRemoveSuccess(lEvent);
                                }
                                //else if (nStatusCode != 204 && nStatusCode != 401 && nStatusCode != 403)
                                //{
                                //    bRet = false;
                                //    bOffline = true;
                                //}
                            }

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_REMOVED - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_RENAMED:
                        {
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "case FILE_ACTION_RENAMED");

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetContentURI for " + lEvent.FileName);

                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                continue;
                            }

                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus DB_STATUS_IN_PROGRESS for " + lEvent.FullPath);
                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);

                            string changedName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));
                            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "changedName " + changedName);

                            if (isFile)
                            {
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "isFile bRetConflicts " + bRetConflicts.ToString());
                            }
                            if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                               strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                            {
                                strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                               
                                bRet = cMezeoFileCloud.ContainerRename(strContentURi, changedName, ref nStatusCode);

                                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Directory bRet " + bRet.ToString());
                                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                {
                                    return LOGIN_FAILED;
                                }
                                else if (nStatusCode != ResponseCode.CONTAINERRENAME)
                                {
                                    return SERVER_INACCESSIBLE;
                                }
                                else
                                {
                                    SuccessIndexes.Add(events.IndexOf(lEvent));
                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus DB_STATUS_SUCCESS for  " + lEvent.FullPath);
                                    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Calling for UpdateDBForRenameSuccess");
                                    UpdateDBForRenameSuccess(lEvent);
                                }
                                //else if (nStatusCode != 204 && nStatusCode != 401 && nStatusCode != 403)
                                //{
                                //    bRet = false;
                                //    bOffline = true;
                                //}
                            }
                            else
                            {
                                if (bRetConflicts)
                                {
                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "isFile bRetConflicts " + bRetConflicts.ToString());
                                    ItemDetails iDetails = cMezeoFileCloud.GetContinerResult(strContentURi, ref nStatusCode);

                                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                    {
                                        return LOGIN_FAILED;
                                    }
                                    else if (nStatusCode != ResponseCode.GETCONTINERRESULT)
                                    {
                                        return SERVER_INACCESSIBLE;
                                    }

                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "updating DB   DbHandler.PUBLIC to " + iDetails.bPublic + " for DbHandler.KEY " + lEvent.FileName);

                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PUBLIC , iDetails.bPublic , DbHandler.KEY , lEvent.FileName ); 
                                    //bool bPublic = dbHandler.GetBoolean(DbHandler.TABLE_NAME, DbHandler.PUBLIC, DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "getting mime type from DB");
                                    string mimeType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.MIMIE_TYPE, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "mime type " + mimeType);

                                    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Calling cMezeoFileCloud.FileRename for content uri " + strContentURi + " with new name " + changedName);
                                    bRet = cMezeoFileCloud.FileRename(strContentURi, changedName, mimeType, iDetails.bPublic, ref nStatusCode);
                                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                    {
                                        return LOGIN_FAILED;
                                    }
                                    else if (nStatusCode != ResponseCode.FILERENAME)
                                    {
                                        return SERVER_INACCESSIBLE;
                                    }
                                    else
                                    {
                                        SuccessIndexes.Add(events.IndexOf(lEvent));
                                        Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus " + lEvent.FullPath + " to DB_STATUS_SUCCESS");
                                        MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                        Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Calling UpdateDBForRenameSuccess");
                                        UpdateDBForRenameSuccess(lEvent);
                                    }
                                    //else if (nStatusCode != 204 && nStatusCode != 401 && nStatusCode != 403)
                                    //{
                                    //    bRet = false;
                                    //    bOffline = true;
                                    //}
                                }
                            }
                        }
                        break; 
                }

                //if (bOffline)
                //    break;

                fileDownloadCount++;
                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "file download count: " + fileDownloadCount);
            }

            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "clear events");
            if (events.Count == SuccessIndexes.Count)
                events.Clear();
            else
            {
                SuccessIndexes.Sort();
                for (int n = SuccessIndexes.Count - 1; n >= 0; n--)
                {
                    events.RemoveAt(SuccessIndexes[n]);
                }
            }

            //if (bOffline)
            //{
            //    Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "return with -2");
            //    return -2;
            //}
            int returnCode = 1;
            if (LocalEventList.Count != 0)
            {
                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "LocalEventList.Count NOT ZERO, locking folderWatcherLockObject");
                lock (folderWatcherLockObject)
                {
                    if (LocalEventList.Count != 0)
                    {
                        if (events == null)
                            events = new List<LocalEvents>();

                        Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "adding LocalEventList to events");
                        events.AddRange(LocalEventList);
                        Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "clear LocalEventList");
                        LocalEventList.Clear();
                    }
                }

                Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Calling HandleEvents");

                returnCode = HandleEvents(caller);
            }

            isLocalEventInProgress = false;

            Debugger.Instance.logMessage("SyncManager - ProcessLocalEvents", "Leave");

            return returnCode;

            //if (LocalEventList.Count != 0)
            //    watcher_WatchCompletedEvent();
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

        private void ReportConflict(LocalEvents lEvent , IssueFound.ConflictType cType)
        {
            Debugger.Instance.logMessage("SyncManager - ReportConflict", "Enter");
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
             iFound.ServerFileUri = iDetails.szContentUrl;

             frmIssuesFound.AddIssueToList(iFound);

             //cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
             //                                                             LanguageTranslator.GetValue("SyncIssueFoundText"),
             //                                                            ToolTipIcon.None);

             //cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

             //frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");

             //SetIssueFound(true);
             Debugger.Instance.logMessage("SyncManager - ReportConflict", "Leave");
        }

        private bool CheckForConflicts(LocalEvents lEvent, string strContentUrl)
        {
            Debugger.Instance.logMessage("SyncManager - CheckForConflicts", "Enter, content uri " + strContentUrl);
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

                                    if (File.Exists(lEvent.FullPath))
                                        File.Move(lEvent.FullPath, FileReName);
                
                                    lEvent.OldFullPath = lEvent.FullPath;
                                    lEvent.FullPath = FileReName;
                                    lEvent.OldFileName = lEvent.FileName;
                                    lEvent.FileName = lEvent.FileName.Substring(0,lEvent.FileName.LastIndexOf("\\")+1);
                                    lEvent.FileName += IDetails.strName;

                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , lEvent.FileName , DbHandler.KEY , lEvent.OldFileName );
                                    UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
                                   
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

                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL , strURL , DbHandler.KEY , lEvent.FileName);
                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtagNew , DbHandler.KEY , lEvent.FileName );

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

                                bRet = cMezeoFileCloud.DownloadFile(strContentUrl + "/" + IDetails.strName, lEvent.FullPath,IDetails.dblSizeInBytes, ref nStatusCode);

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
                                bRet = cMezeoFileCloud.DownloadFile(strContentUrl + "/" + lEvent.FileName, lEvent.FullPath,IDetails.dblSizeInBytes, ref nStatusCode);
                                if (bRet)
                                {
                                    string strEtagNew = cMezeoFileCloud.GetETag(strContentUrl, ref nStatusCode);
                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtagNew , DbHandler.KEY , lEvent.FileName);

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
                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.KEY , lEvent.FileName , DbHandler.KEY , lEvent.OldFileName);
                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL , strUri , DbHandler.KEY , lEvent.FileName );

                                string strEtagUpload = cMezeoFileCloud.GetETag(strUri, ref nStatusCode);

                                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtagUpload , DbHandler.KEY , lEvent.FileName);

                                UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
                                return false;
                            }
                            return true;
                        }

                    }
                    break;
            }
            
            Debugger.Instance.logMessage("SyncManager - CheckForConflicts", "Leave");
            return true;
        }

        void watcher_WatchCompletedEvent()
        {
            if (!isLocalEventInProgress && !isSyncInProgress && !isOfflineWorking /*&& BasicInfo.IsConnectedToInternet*/ && LocalEventList.Count != 0 && BasicInfo.AutoSync && !BasicInfo.IsInitialSync && !isDisabledByConnection)
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
              
                    if (!bwLocalEvents.IsBusy)
                    {
                        bwLocalEvents.RunWorkerAsync();
                    }
              
            }
        }
            
        private void bwNQUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "Enter");

            //isDisabledByConnection = false;
   
            fileDownloadCount = 1;

            int nStatusCode = 0;
            NQDetails[] pNQDetails = null;
            string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;

            NQLengthResult nqLengthRange = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                e.Result = CancelReason.LOGIN_FAILED;
                return;
            }
            else if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                e.Result = CancelReason.SERVER_INACCESSIBLE;
                return;
            }

            int nqRangeStart = nqLengthRange.nStart;
            int nqRangeEnd = nqLengthRange.nEnd;
            bool isBreak = false;

            int maxProgressValue = 0;

            if (BasicInfo.NQRangeStart == -1)
            {
                BasicInfo.NQRangeStart = nqRangeStart;
                BasicInfo.NQRangeEnd = nqRangeEnd;
            }

            int totalNQLength = (nqRangeEnd - nqRangeStart) + 1;
            maxProgressValue = totalNQLength;

            ((BackgroundWorker)sender).ReportProgress(INITIAL_NQ_SYNC, maxProgressValue);

            int nTempCount = 0;

            while (totalNQLength > 0)
            {
                int NQnumToRequest = 0;

                if (totalNQLength >= 10)
                {
                    NQnumToRequest = 10;
                }
                else
                {
                    NQnumToRequest = totalNQLength;
                }               

                pNQDetails = cMezeoFileCloud.NQGetData(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, NQnumToRequest, ref nStatusCode);
                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                {
                    e.Result = CancelReason.LOGIN_FAILED;
                    isBreak = true;
                    break;
                }
                else if (nStatusCode != ResponseCode.NQGETDATA)
                {
                    e.Result = CancelReason.SERVER_INACCESSIBLE;
                    isBreak = true;
                    break;
                }

                foreach (NQDetails nq in pNQDetails)
                {
                    if (bwNQUpdate.CancellationPending)
                    {
                        Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "bwNQUpdate.CancellationPending called inner");

                        //e.Cancel = true;
                        //bwNQUpdate.ReportProgress(UPDATE_NQ_CANCELED);
                        break;
                    }

                    int nStatus = UpdateFromNQ(nq);
                    if (nStatus == ResponseCode.LOGINFAILED1 || nStatus == ResponseCode.LOGINFAILED2)
                    {
                        e.Result = CancelReason.LOGIN_FAILED;
                        isBreak = true;
                        break;
                    }
                    else if (nStatus != ResponseCode.GETETAG && nStatus != ResponseCode.DOWNLOADFILE && nStatus != ResponseCode.DOWNLOADITEMDETAILS && nStatus != 1)
                    {
                        e.Result = CancelReason.SERVER_INACCESSIBLE;
                        isBreak = true;
                        break;
                    }
                    if (nStatus == 1)
                    {
                        Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork - ", nq.StrObjectName + " - Delete From NQ");
                        cMezeoFileCloud.NQDeleteValue(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, 1, ref nStatusCode);
                        if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                        {
                            e.Result = CancelReason.LOGIN_FAILED;
                            isBreak = true;
                            break;
                        }
                        else if (nStatusCode != ResponseCode.NQDELETEVALUE)
                        {
                            e.Result = CancelReason.SERVER_INACCESSIBLE;
                            isBreak = true;
                            break;
                        }
                    }

                    nTempCount++;
                    if (nTempCount < pNQDetails.Length)
                    {
                        bwNQUpdate.ReportProgress(UPDATE_NQ_PROGRESS);
                        fileDownloadCount++;
                    }
                }
                
                if (isBreak)
                    break;

                nTempCount = 0;

                if (bwNQUpdate.CancellationPending)
                {
                    Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "bwNQUpdate.CancellationPending called outer");

                    e.Cancel = true;
                    bwNQUpdate.ReportProgress(UPDATE_NQ_CANCELED);
                    break;
                }

                totalNQLength -= NQnumToRequest;

                bool gotUpdadtedValueOfNq = false;

                int nqRangeStartOriginal = nqRangeStart;
                while (!gotUpdadtedValueOfNq)
                {
                    nqLengthRange = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);
                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                    {
                        e.Result = CancelReason.LOGIN_FAILED;
                        isBreak = true;
                        break;
                    }
                    else if (nStatusCode != ResponseCode.NQGETLENGTH)
                    {
                        e.Result = CancelReason.SERVER_INACCESSIBLE;
                        isBreak = true;
                        break;
                    }
                    nqRangeStart = nqLengthRange.nStart;
                    nqRangeEnd = nqLengthRange.nEnd;

                    if (nqRangeStart == -1)
                    {
                        break;
                    }

                    if (nqRangeStartOriginal + NQnumToRequest < nqRangeStart)
                    {
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        gotUpdadtedValueOfNq = true;
                    }
                }

                if (isBreak)
                    break;

                BasicInfo.NQRangeStart = nqRangeStart;

                if (BasicInfo.NQRangeEnd <= nqRangeEnd)
                {
                    totalNQLength += nqRangeEnd - BasicInfo.NQRangeEnd;
                    maxProgressValue += nqRangeEnd - BasicInfo.NQRangeEnd;
                    ((BackgroundWorker)sender).ReportProgress(UPDATE_NQ_MAXIMUM, maxProgressValue);
                }

                BasicInfo.NQRangeEnd = nqRangeEnd;

            }

            //int nNQLength = (int)e.Argument; //cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, ref nStatusCode);

            //if (nNQLength > 0)
            //    pNQDetails = cMezeoFileCloud.NQGetData(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, nNQLength, ref nStatusCode);

            //if (pNQDetails != null)
            //{
            //    nNQLength = pNQDetails.Length;

            //    Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "nNQLength" + nNQLength.ToString());   

            //    ((BackgroundWorker)sender).ReportProgress(INITIAL_NQ_SYNC, nNQLength);

            //    for (int n = 0; n < nNQLength ; n++)
            //    { 
            //        if (bwNQUpdate.CancellationPending)
            //        {
            //            Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "bwNQUpdate.CancellationPending called"); 

            //            e.Cancel = true;
            //            bwNQUpdate.ReportProgress(UPDATE_NQ_CANCELED);
            //            break;
            //        }

            //        Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork - ", pNQDetails[n].StrObjectName + " - Enter"); 

            //        bool isSuccess = UpdateFromNQ(pNQDetails[n]);
            //        if (isSuccess)
            //        {
            //            Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork - ", pNQDetails[n].StrObjectName + " - Delete From NQ"); 
            //            cMezeoFileCloud.NQDeleteValue(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, queueName, 1, ref nStatusCode);
            //        }

            //        Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork - ", pNQDetails[n].StrObjectName + " - Leave"); 

            //        bwNQUpdate.ReportProgress(UPDATE_NQ_PROGRESS);
            //        fileDownloadCount++;
            //    }
                
            //    isSyncInProgress = false;
            //}

            isSyncInProgress = false;
            Debugger.Instance.logMessage("frmSyncManager - bwNQUpdate_DoWork", "Leave");   
        }

        private void bwNQUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowSyncMessage(isEventCanceled);
            tmrNextSync.Interval = FIVE_MINUTES;
            isSyncInProgress = false;
            isEventCanceled = false;
            try
            {
                if (e.Result != null && (CancelReason)e.Result == CancelReason.LOGIN_FAILED)
                {
                    this.Hide();
                    frmParent.ShowLoginAgainFromSyncMgr();
                }
                else if (e.Result != null && (CancelReason)e.Result == CancelReason.SERVER_INACCESSIBLE)
                {
                    DisableSyncManager();
                    ShowSyncManagerOffline();
                }
                else
                {
                    if (LocalEventList.Count > 0)
                    {
                        watcher_WatchCompletedEvent();
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void bwNQUpdate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == INITIAL_NQ_SYNC)
            {
                SetUpControlForSync();
                pbSyncProgress.Maximum = (int)e.UserState;
                showProgress();
                if (!pbSyncProgress.Visible)
                {
                    pbSyncProgress.Visible = true;
                    Application.DoEvents();
                }
            }
            else if (e.ProgressPercentage == UPDATE_NQ_PROGRESS)
            {
                showProgress();
            }
            else if (e.ProgressPercentage == UPDATE_NQ_CANCELED)
            {
                isSyncInProgress = false;
                if (isDisabledByConnection)
                {
                    DisableProgress();
                    ShowSyncDisabledMessage();
                    ShowSyncManagerOffline();
                }
                else
                {
                    ShowSyncMessage(true);
                }
            }
            else if (e.ProgressPercentage == UPDATE_NQ_MAXIMUM)
            {
                pbSyncProgress.Maximum = (int)e.UserState;
                showProgress();
            }
            
            
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

        private void lnkServerUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(BasicInfo.ServiceUrl);
        }

        private void bwOffilneEvent_DoWork(object sender, DoWorkEventArgs e)
        {
            isOfflineWorking = true;
            int statusCode = HandleEvents((BackgroundWorker)sender);
            e.Result = statusCode;
        }

        private void bwOffilneEvent_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isOfflineWorking = false;
            if ((int)e.Result == 1)
            {
                ShowLocalEventsCompletedMessage();

                if (!isEventCanceled)
                    UpdateNQ();
            }
            else if ((int)e.Result == USER_CANCELLED)
            {
                ShowSyncMessage(events.Count > 0);
            }
            else if ((int)e.Result == LOGIN_FAILED)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
            }
            else if ((int)e.Result == SERVER_INACCESSIBLE)
            {
                ShowLocalEventsCompletedMessage();
                DisableSyncManager();
                ShowSyncManagerOffline();
            }        
         
        }

        private void bwLocalEvents_DoWork(object sender, DoWorkEventArgs e)
        {
           // isDisabledByConnection = false;
            int statusCode = HandleEvents((BackgroundWorker)sender);
            e.Result = statusCode;
        }

        private void bwLocalEvents_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbSyncProgress.Visible = true;
            label1.Visible = false;
            pbSyncProgress.Refresh();

            if (!lblPercentDone.Visible)
            {
                lblPercentDone.Text = "";
                lblPercentDone.Visible = true;
                pbSyncProgress.Visible = true;
                Application.DoEvents();
            }

            if (e.ProgressPercentage == SYNC_STARTED)
            {
                ShowInitialSyncMessage();
            }
            else if (e.ProgressPercentage == PROCESS_LOCAL_EVENTS_STARTED)
            {
                pbSyncProgress.Maximum = (int)e.UserState;
                //pbSyncProgress.Visible = true;
                InitializeLocalEventsProcess();
            }
            else if (e.ProgressPercentage == PROGRESS_CHANGED_WITH_FILE_NAME)
            {
                showProgress();
                lblStatusL3.Text = e.UserState.ToString();  
            }
            else if (e.ProgressPercentage == LOCAL_EVENTS_COMPLETED)
            {
                if (isDisabledByConnection)
                {
                    lastSync = DateTime.Now;
                    BasicInfo.LastSyncAt = lastSync;

                    DisableProgress();
                    ShowSyncDisabledMessage();
                    ShowSyncManagerOffline();
                }
                else
                {
                    ShowLocalEventsCompletedMessage();
                }
            }
            else if (e.ProgressPercentage == LOCAL_EVENTS_STOPPED)
            {
                ShowSyncMessage(events.Count > 0);
            }
            
            //Application.DoEvents();
        }


        private void ShowInitialSyncMessage()
        {
            btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") +
                                            (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
        }

        private void InitializeLocalEventsProcess()
        {
            SetIssueFound(false);
            ShowNextSyncLabel(false);
            pbSyncProgress.Visible = true;
            pbSyncProgress.Value = 0;
            pbSyncProgress.Maximum = events.Count;
            fileDownloadCount = 1;
            btnMoveFolder.Enabled = false;
        }

        public void ShowSyncManagerOffline()
        {
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOfflineText"), ToolTipIcon.None);

            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOfflineText");
            cnotificationManager.NotifyIcon = Properties.Resources.app_offline;
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("AppOfflineMenu");

            lblStatusL1.Text = LanguageTranslator.GetValue("AppOfflineMenu");
            label1.Text = "";
            lblStatusL3.Text = lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
            btnMoveFolder.Enabled = false;
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
        }

        private void ShowLocalEventsCompletedMessage()
        {
            //btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            ShowSyncMessage(events.Count > 0);
            //btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");

            if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
            {
                SetIssueFound(true);
                cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;

                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                         ToolTipIcon.None);

                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
            }
            else
            {
                if (BasicInfo.AutoSync)
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                else
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                             LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                            ToolTipIcon.None);

                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
            }
        }

        private void bwUpdateUsage_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = GetUsageString();
        }

        private void bwUpdateUsage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblUsageDetails.Text = e.Result.ToString();
        }


        private void bwLocalEvents_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //if ((int)e.Result == -2)
            //{
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke((MethodInvoker)delegate
            //        {
            //            frmParent.HandleConnectionState();
            //        });

            //    }
            //    else
            //    {
            //        frmParent.HandleConnectionState();
            //    }
                
            //}

            if ((int)e.Result == 1)
            {
                ShowLocalEventsCompletedMessage();
                if (IsCalledByNextSyncTmr)
                {
                    IsCalledByNextSyncTmr = false;

                    if (!isEventCanceled)
                        UpdateNQ();
                }
            }
            else if ((int)e.Result == USER_CANCELLED)
            {
                ShowSyncMessage(events.Count > 0);
            }
            else if ((int)e.Result == LOGIN_FAILED)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
            }
            else if ((int)e.Result == SERVER_INACCESSIBLE)
            {
                ShowLocalEventsCompletedMessage();
                DisableSyncManager();
                ShowSyncManagerOffline();
            }
            //else if (IsCalledByNextSyncTmr)
            //{
            //    IsCalledByNextSyncTmr = false;

            //    if (!isEventCanceled)
            //        UpdateNQ();
            //}
          
            //else
            //{
            //    tmrNextSync.Interval = FIVE_MINUTES;
            //    tmrNextSync.Enabled = true;
            //}
        }


        private void SendToRecycleBin(string strPath, bool isFile)
        {
            try
            {
                if (isFile)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(strPath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                                                       Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                }
                else
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(strPath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                                                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
