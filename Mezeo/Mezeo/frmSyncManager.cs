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
using System.Net;
using System.Xml;
using System.Reflection;
using Mezeo;

namespace Mezeo
{
    public partial class frmSyncManager : Form
    {        
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

        private bool isCalledByNextSyncTmr = false;

        private static string DB_STATUS_SUCCESS = "SUCCESS";
        private static string DB_STATUS_IN_PROGRESS = "INPROGRESS";

        NotificationManager cnotificationManager;

        public MezeoFileSupport.CallbackIncrementProgress myDelegate;

        #region Private Members

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
        private int transferCount = 0;
        private int messageMax;
        private int messageValue;
        
        
        Queue<LocalItemDetails> queue;
        frmIssues frmIssuesFound;
        Watcher watcher;

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

            watcher = new Watcher(lockObject, BasicInfo.SyncDirPath);
            EventQueue.WatchCompletedEvent += new EventQueue.WatchCompleted(queue_WatchCompletedEvent);
            CheckForIllegalCrossThreadCalls = false;

            watcher.StartMonitor();
            dbHandler = new DbHandler();
            dbHandler.OpenConnection();

            frmIssuesFound = new frmIssues(mezeoFileCloud);
            offlineWatcher = new OfflineWatcher(dbHandler);
            
            myDelegate = new MezeoFileSupport.CallbackIncrementProgress(this.CallbackSyncProgress);
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
                bool bRet = cMezeoFileCloud.Delete(szContantURI, ref nStatusCode, "");
            }
        }

        void cMezeoFileCloud_downloadStoppedEvent(string fileName)
        {
            DeleteCurrentIncompleteFile(fileName);
        }

        public LoginDetails LoginDetail
        {
            get
            {
                return cLoginDetails;
            }
            set
            {
                cLoginDetails = value;
            }
        }

        #endregion

        #region Form Drawing Events
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

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
                LogWrapper.LogMessage("frmSyncManager - panel_Paint", "Caught exception: " + ex.Message);
            }
        }

        #endregion

        #region Form Events

        public bool IsSyncInProgress()
        {
            return isSyncInProgress;
        }

        public void SetIsSyncInProgress(bool syncInProgress)
        {
            isSyncInProgress = syncInProgress;
        }

        public bool IsOfflineWorking()
        {
            return isOfflineWorking;
        }

        public void SetIsOfflineWorking(bool isOfflineWorkingNew)
        {
            isOfflineWorking = isOfflineWorkingNew;
        }

        public bool IsEventCanceled()
        {
            return isEventCanceled;
        }

        public void SetIsEventCanceled(bool eventIsCanceled)
        {
            isEventCanceled = eventIsCanceled;
        }

        public bool IsLocalEventInProgress()
        {
            return isLocalEventInProgress;
        }

        public void SetIsLocalEventInProgress(bool localEventIsInProgress)
        {
            isLocalEventInProgress = localEventIsInProgress;
        }

        public bool IsCalledByNextSyncTmr()
        {
            return isCalledByNextSyncTmr;
        }

        public void SetIsCalledByNextSyncTmr(bool CalledByNextSyncTmr)
        {
            isCalledByNextSyncTmr = CalledByNextSyncTmr;
        }

        public bool IsDisabledByConnection()
        {
            return isDisabledByConnection;
        }

        public void SetIsDisabledByConnection(bool disabledByConnection)
        {
            isDisabledByConnection = disabledByConnection;
        }

        public bool IsInIdleState()
        {
            if (!IsSyncInProgress() && !IsLocalEventInProgress() && !IsOfflineWorking())
                return true;
            return false;
        }

        public bool WereItemsSynced()
        {
            return (transferCount > 0);
        }

        private void InitTransferCount()
        {
            transferCount = 0;
        }

        private void IncrementTransferCount()
        {
            transferCount++;
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
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForMoveFolder", "Caught exception (DirectoryNotFoundException): " + e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForMoveFolder", "Caught exception (DirectoryNotFoundException): " + e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    File.Copy(fi.FullName, strMovePath + "\\" + fi.Name);
                    File.Delete(fi.FullName);
                }

                subDirs = root.GetDirectories();

                if (subDirs != null)
                {
                    foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                    {
                        Directory.CreateDirectory(strMovePath + "\\" + dirInfo.Name);
                        WalkDirectoryTreeForMoveFolder(dirInfo, strMovePath + "\\" + dirInfo.Name);
                        Directory.Delete(dirInfo.FullName);
                    }
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
            SetIsEventCanceled(false);
            if (!IsLocalEventInProgress())
            {
                InitializeSync();
            }
            else
                StopSync();
        }

        // Read in the response for an HTTP request.
        private String OnGetResponseString(Stream responseStream)
        {
            StringBuilder responseString = new StringBuilder();
            byte[] buffer = new byte[1024 * 64];
            int bytes_read = 0;
            while ((bytes_read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                responseString.Append(Encoding.UTF8.GetString(buffer, 0, bytes_read));
            }
            responseStream.Close();
            return responseString.ToString();
        }

        private void tmrNextSync_Tick(object sender, EventArgs e)
        {
            // Only look for updates once a day.
            // TODO: Make the timespan (in hours) a string that can be part of branding or configuration.
            TimeSpan diff = DateTime.Now - BasicInfo.LastUpdateCheckAt;
            if (12 < diff.TotalHours)
            {
                // See if an update is available.
                string strURL = BasicInfo.GetUpdateURL();
                string strCurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string strNewVersion = "";

                // Remove the 4th field from the version since that doesn't exist in the sparkle version.
                string[] strSubVersion = strCurVersion.Split('.');
                strCurVersion = strSubVersion[0] + "." + strSubVersion[1] + "." + strSubVersion[2];

                // Check to see what versions are available.
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strURL);
                webRequest.Method = "GET";
                webRequest.KeepAlive = false;
                webRequest.Timeout = 60000;

                HttpWebResponse response = (HttpWebResponse)(webRequest.GetResponse());
                string strTemp = OnGetResponseString(response.GetResponseStream());

                XmlDocument m_xmlVersionList = new XmlDocument();
                m_xmlVersionList.LoadXml(strTemp);

                XmlNodeList nodes = m_xmlVersionList.SelectNodes("/rss/channel/item");
                if (null != nodes)
                {
                    foreach (XmlNode node in nodes)
                    {
                        if (node.HasChildNodes)
                        {
                            // See what the most recent version is and if it is newer than the current version.
                            XmlNode enclosure = node.SelectSingleNode("enclosure");
                            if (null != enclosure)
                            {
                                XmlNode xmlVersion = enclosure.Attributes.GetNamedItem("sparkle:version");
                                if (null != xmlVersion)
                                {
                                    if (-1 == strCurVersion.CompareTo(xmlVersion.Value))
                                        strNewVersion = xmlVersion.Value;

                                }
                            }
                        }
                    }
                }

                // If an update is available, the show a pop
                ShowUpdateAvailableBalloonMessage(strNewVersion);

                // Update the time we last checked for an update.
                BasicInfo.LastUpdateCheckAt = DateTime.Now;
            }

            // See if I need to kick off a sync action.
            if (BasicInfo.AutoSync)
            {
                if (IsInIdleState())
                {
                    SetIsCalledByNextSyncTmr(true);
                    tmrNextSync.Interval = FIVE_MINUTES;
                    //tmrNextSync.Enabled = false;
                    InitializeSync();
                }
                else if (IsLocalEventInProgress())
                {
                    tmrNextSync.Interval = FIVE_SECONDS;
                }
            }
            else
            {
                if (IsDisabledByConnection())
                {
                    int nStatusCode = 0;
                    try
                    {
                        NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), ref nStatusCode);
                        if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                        {
                            this.Hide();
                            frmParent.ShowLoginAgainFromSyncMgr();
                        }
                        else if (nStatusCode == ResponseCode.NQGETLENGTH)
                        {
                            if (IsSyncInProgress() == false)
                            {
                                BasicInfo.AutoSync = true;
                                EnableSyncManager();
                                BasicInfo.AutoSync = true;
                                InitializeSync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWrapper.LogMessage("frmSyncManager - tmrNextSync_Tick", "Caught exception: " + ex.Message);

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
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    statusMessageCounter++;

                    if (statusMessageCounter >= statusMessages.Length)
                    {
                        statusMessageCounter = 0;
                    }

                    lblStatusL1.Text = statusMessages[statusMessageCounter];
                });
            }
            else
            {
                statusMessageCounter++;

                if (statusMessageCounter >= statusMessages.Length)
                {
                    statusMessageCounter = 0;
                }

                lblStatusL1.Text = statusMessages[statusMessageCounter];
            }
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

                if (IsInIdleState())
                    InitializeSync();     
            }
        }

        private void rbSyncOff_Click(object sender, EventArgs e)
        {
            if (BasicInfo.AutoSync)
            {
                BasicInfo.AutoSync = false;

                if (IsInIdleState())
                    ShowSyncMessage();
            }
        }

        #endregion

        #region Downloader Events

        void fileDownloder_fileDownloadCompleted()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    SetIsSyncInProgress(false);

                    BasicInfo.IsInitialSync = false;
                    ShowSyncMessage();

                    InitialSyncUptodateMessage();

                    queue_WatchCompletedEvent();
                });
            }
            else
            {
                SetIsSyncInProgress(false);

                BasicInfo.IsInitialSync = false;
                ShowSyncMessage();

                InitialSyncUptodateMessage();

                queue_WatchCompletedEvent();
            }
        }

        void ShowNextSyncLabel(bool bIsShow)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Visible = bIsShow;
                    label1.BringToFront();
                });
            }
            else
            {
                label1.Visible = bIsShow;
                label1.BringToFront();
            }
        }

        void fileDownloder_downloadEvent(object sender, FileDownloaderEvents e)
        {
            if (isAnalysisCompleted)
            {
                showProgress();
            }
            fileDownloadCount++;
        }

        void stDownloader_downloadEvent(object sender, StructureDownloaderEvent e)
        {
            if (e.IsCompleted && !lockObject.StopThread)
            {
                isAnalysingStructure = false;
                tmrSwapStatusMessage.Enabled = false;

                isAnalysisCompleted = true;
                fileDownloder.IsAnalysisCompleted = true;
                lock (lockObject)
                {
                    Monitor.PulseAll(lockObject);
                }

                if (stDownloader.TotalFileCount > 0)
                {
                   messageMax = stDownloader.TotalFileCount - (fileDownloadCount -1);
                }

                if (pbSyncProgress.Maximum > 0)
                {
                    fileDownloadCount = 1;
                    lblPercentDone.Text = "";
                    lblPercentDone.Visible = true;

                    showProgress();
                }
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

        void stDownloader_cancelDownloadEvent(CancelReason reason)
        {
            isAnalysingStructure = false;
            tmrSwapStatusMessage.Enabled = false;
            OnThreadCancel(reason);
        }

        public void ApplicationExit()
        {
            if (lockObject != null)
                lockObject.ExitApplication = true;
            StopSync();
        }

        #endregion

        #region Functions and Methods

        private void OnThreadCancel(CancelReason reason)
        {
            if (!isAnalysingStructure && !isDownloadingFile)
            {
                if (lockObject.ExitApplication)
                    System.Environment.Exit(0);
                else
                {
                    SetIsSyncInProgress(false);
                    ShowSyncMessage(true);
                    btnSyncNow.Enabled = true;

                    if (reason == CancelReason.LOGIN_FAILED)
                    {
                        this.Hide();
                        frmParent.ShowLoginAgainFromSyncMgr();
                    }
                    else if (reason == CancelReason.SERVER_INACCESSIBLE)
                    {
                        // CheckServerStatus();TODO:check for offline (Modified for server status thread)
                    }
                }
            }
        }

        private void LoadResources()
        {
            this.Text = AboutBox.AssemblyTitle; 
            this.lblFileSync.Text = LanguageTranslator.GetValue("SyncManagerFileSyncLabel");
            this.rbSyncOff.Text = LanguageTranslator.GetValue("SyncManagerOffButtonText");
            this.rbSyncOn.Text = LanguageTranslator.GetValue("SyncManagerOnButtonText");
            this.lblFolder.Text = LanguageTranslator.GetValue("SyncManagerFolderLabel");
            this.lblStatus.Text = LanguageTranslator.GetValue("SyncManagerStatusLabel");
            this.lblUsage.Text = LanguageTranslator.GetValue("SyncManagerUsageLabel");

            // this.btnMoveFolder.Text = LanguageTranslator.GetValue("SyncManagerMoveFolderButtonText");
            // Commeted above line as move folder functinality disable 
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
            }
            else
            {
                this.lblUsageDetails.Text = LanguageTranslator.GetValue("UsageNotAvailable");
            }
        }

        public void InitializeSync()
        {
            if (!IsSyncInProgress())
            {
                SetUpSync();
                SyncNow();
            }
            else
            {
                SetIsEventCanceled(true);
                StopSync();
            }
        }

        public void SetUpSync()
        {
            lblPercentDone.Visible = true;
            lblPercentDone.Text = "";
           // pbSyncProgress.Value = 0;
            messageValue = 0;
        }

        private void SetUpControlForSync()
        {
            SetIssueFound(false);
            btnSyncNow.Text = this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            btnSyncNow.Refresh();
            isAnalysingStructure = true;
            isDownloadingFile = true;
            SetIsSyncInProgress(true);
            ShowNextSyncLabel(false);
            isAnalysisCompleted = false;
            // tmrNextSync.Enabled = false;
           // pbSyncProgress.Visible = true;
            // btnMoveFolder.Enabled = false;
            // Commeted above line as move folder functinality disable 
        }

        public void StopSync()
        {
            if (IsSyncInProgress() || IsLocalEventInProgress() || IsOfflineWorking())
            {
                tmrNextSync.Interval = FIVE_MINUTES;
                SetIsLocalEventInProgress(false);
                SetIsEventCanceled(true);
                SetIsOfflineWorking(false);
                bwLocalEvents.CancelAsync();
                bwNQUpdate.CancelAsync();
                bwOfflineEvent.CancelAsync();
                ShowNextSyncLabel(true);
                btnSyncNow.Enabled = false;
                if (lockObject != null)
                    lockObject.StopThread = true;

                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    IssueFoundBalloonMessage();
                }
                else
                {
                    if(BasicInfo.AutoSync)
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    else
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                    SyncStoppedBalloonMessage();
                }
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
            pbSyncProgress.Hide();
            ShowNextSyncLabel(true);
            btnSyncNow.Enabled = true;

            if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
            {
                IssueFoundBalloonMessage();
            }
            else
            {
                if (BasicInfo.AutoSync)
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                else
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                SyncStoppedBalloonMessage();
             }
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

            NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
                SetIsSyncInProgress(false);
                return -1;
            }
            else if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                DisableSyncManager();
                ShowSyncManagerOffline();
                SetIsSyncInProgress(false);
                return -2;
            }

            return 1;
        }

        public void SyncNow()
        {
            LogWrapper.LogMessage("frmSyncManager - SyncNow", "enter");
            InitTransferCount();

            // int nServerStatus = CheckServerStatus(); TODO:check for offline (Modified for server status thread)
           // if (nServerStatus != 1) 
           //if server status still offline return immediately  
           //     return;
           // else
           // {
                if (IsDisabledByConnection())
                    EnableSyncManager();
           // }

            SetUpSyncNowNotification();
            
            if (BasicInfo.IsInitialSync)
            {
                ShowNextSyncLabel(false);
                SetUpControlForSync();
                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedTitleText"),
                                                                        LanguageTranslator.GetValue("TrayBalloonInitialSyncStartedText"),
                                                                        ToolTipIcon.None);

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
                if (EventQueue.QueueNotEmpty())
                {
                    ShowNextSyncLabel(false);
                    if (!bwLocalEvents.IsBusy)
                        bwLocalEvents.RunWorkerAsync();                 
                }
                else
                {
                    UpdateNQ();
                }
            }
            LogWrapper.LogMessage("frmSyncManager - SyncNow", "leave");
        }

        public void ProcessOfflineEvents()
        {
            LogWrapper.LogMessage("frmSyncManager - ProcessOfflineEvents", "enter");
            // See if there are any offline events since the last time we ran.
            offlineWatcher.PrepareStructureList();

            if (EventQueue.QueueNotEmpty())
            {
                if (!bwOfflineEvent.IsBusy)
                    bwOfflineEvent.RunWorkerAsync();
            }
            else
            {
                UpdateNQ();
            }
            LogWrapper.LogMessage("frmSyncManager - ProcessOfflineEvents", "leave");
        }

        public void UpdateNQ()
        {
            LogWrapper.LogMessage("frmSyncManager - UpdateNQ", "Enter");

            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerCheckingServer");
            lblStatusL3.Text = "";
            ShowNextSyncLabel(false);
            SetUpControlForSync();
            Application.DoEvents();
            int nStatusCode = 0;

            // Added Code to fix issue [ 1680: Crash detecting internet connection ] 
            if (frmParent.checkReferenceCode() > 0)
            {
                if(cLoginDetails == null)
                    cLoginDetails = frmParent.loginFromSyncManager();
            }
            else
            {
                DisableSyncManager();
                ShowOfflineAtStartUpSyncManager();
                ShowSyncManagerOffline();
                SetIsSyncInProgress(false);
                SyncOfflineMessage();
                return;
            }


            NQLengthResult nqLengthRes = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), ref nStatusCode);
            if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
                return;
            }
            else if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                //CheckServerStatus(); TODO:check for offline (Modified for server status thread)
                //DisableSyncManager();
                //ShowSyncManagerOffline();
                return;
            }
            else if (nqLengthRes == null)
                return;

            int nNQLength = 0;
            if (nqLengthRes.nEnd == -1 || nqLengthRes.nStart == -1)
                nNQLength = 0;
            else
                nNQLength = (nqLengthRes.nEnd + 1) - nqLengthRes.nStart;

            messageMax = nNQLength;

            if (nNQLength > 0)
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateNQ", "nNQLength" + nNQLength.ToString());
                //ShowNextSyncLabel(false);
                if (!bwNQUpdate.IsBusy)
                {
                    LogWrapper.LogMessage("frmSyncManager - UpdateNQ", "bwNQUpdate.RunWorkerAsync called");
                    bwNQUpdate.RunWorkerAsync(nNQLength);
                }
            }
            else
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateNQ", "nNQLength 0");

                SetIsSyncInProgress(false);
                ShowSyncMessage();

                //if (BasicInfo.AutoSync)
                //{
                    tmrNextSync.Interval = FIVE_MINUTES;
                  //  tmrNextSync.Enabled = true;
                //}

                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    IssueFoundBalloonMessage();
                }
                else
                {
                    if (BasicInfo.AutoSync)
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                    else
                        cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;

                    SyncFolderUpToDateMessage();
                }
            }

            LogWrapper.LogMessage("frmSyncManager - UpdateNQ", "Leave");
        }

        private int UpdateFromNQ(NQDetails UpdateQ)
        {
            LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", "Enter"); 

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
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", "nsResult Null");

                nStatus = 1;
                return nStatus;
            }

            if (nqDetail.StrObjectName == "csp_recyclebin")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - skipping csp_recyclebin notification.", "csp_recyclebin notification skipped");
                nStatus = 1;
                return nStatus;
            }

            int startIndex = Math.Min(cLoginDetails.szNQParentUri.Length + 1, nqDetail.StrParentUri.Length);
            string strPath = nqDetail.StrParentUri.Substring(startIndex);
            string strKey = strPath.Replace("/" , "\\");
            
            if(nsResult == null)
                strKey += nqDetail.StrObjectName;
            else
                strKey += nsResult.StrName;

            strPath = BasicInfo.SyncDirPath + "\\" + strKey;
            //lblStatusL3.Text = strPath;

            if (nqDetail.StrEvent == "cdmi_create_complete")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Enter"); 

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

                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_modify_complete")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Enter"); 

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
                    }
                }
                nStatus = 1;
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_delete")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Enter");

                nqEventCdmiDelete(strPath, strKey);

                nStatus = 1;

                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_rename")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Enter"); 

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
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Leave"); 
            }
            else if (nqDetail.StrEvent == "cdmi_copy")
            {
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Enter"); 

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
                    if (fileFolderInfo.ETag == null)
                    {
                        fileFolderInfo.ETag = "";
                    }

                    if (fileFolderInfo.ETag.Trim().Length == 0)
                        fileFolderInfo.ETag = cMezeoFileCloud.GetETag(nsResult.StrContentsUri, ref nStatusCode);

                    if (fileFolderInfo.ETag == null)
                    {
                        fileFolderInfo.ETag = "";
                    }
                    if (fileFolderInfo.MimeType == null)
                    {
                        fileFolderInfo.MimeType = "";
                    }

                    dbHandler.Write(fileFolderInfo);
                }
                nStatus = 1;
                LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", nqDetail.StrEvent + " - " + strKey + " - Leave"); 
            }

            LogWrapper.LogMessage("frmSyncManager - UpdateFromNQ - ", "Leave");

            return nStatus;
        }

        private void nqEventCdmiDelete(string strPath, string strKey)
        {
            LogWrapper.LogMessage("frmSyncManager - nqEventCdmiDelete", "enter");
            bool isDirectory = false;
            bool isFile = File.Exists(strPath);
            if (!isFile)
            {
                isDirectory = Directory.Exists(strPath);
                if (isDirectory)
                {
                    DirectoryInfo rootDir = new DirectoryInfo(strPath);
                    WalkDirectoryTreeForDelete(rootDir);
                    SendToRecycleBin(strPath, false);
                    //Directory.Delete(strPath);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY, strKey);
                }
            }
            else
            {
           
                SendToRecycleBin(strPath, true);
                //File.Delete(strPath);
                dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY, strKey);
            }
            LogWrapper.LogMessage("frmSyncManager - nqEventCdmiDelete", "leave");
        }

        private int nqEventCdmiCreate(NQDetails nqDetail, NSResult nsResult, string strKey, string strPath)
        {
            LogWrapper.LogMessage("frmSyncManager - nqEventCdmiCreate", "enter");
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
                ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(nsResult.StrContentsUri, ref nStatusCode, null);
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

            LogWrapper.LogMessage("frmSyncManager - nqEventCdmiCreate", "leave");
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
            LogWrapper.LogMessage("frmSyncManager - DownloadFolderStructureForNQ", "enter");
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
                    if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
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
                if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                {
                    return refCode;
                }
                else if (refCode != ResponseCode.GETETAG)
                {
                    return refCode;
                }

                DirectoryInfo dInfo = new DirectoryInfo(strPath);
                dbHandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , strEtag , DbHandler.KEY , fileFolderInfo.Key );
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , "SUCCESS", DbHandler.KEY ,fileFolderInfo.Key );

                MarkParentsStatus(strPath, DB_STATUS_SUCCESS);

                ItemDetails[] iDetails = cMezeoFileCloud.DownloadItemDetails(iDetail.szContentUrl, ref nStatusCode, null);
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

            LogWrapper.LogMessage("frmSyncManager - DownloadFolderStructureForNQ", "leave");
            return nStatus;
        }

        void stDownloader_startDownloaderEvent(bool bStart)
        {
            LogWrapper.LogMessage("frmSyncManager - stDownloader_startDownloaderEvent", "enter");
            if (bStart)
                downloadingThread.Start();
            else
                fileDownloder.ForceComplete();
            LogWrapper.LogMessage("frmSyncManager - stDownloader_startDownloaderEvent", "leave");
        }

        private void showProgress()
        {
            LogWrapper.LogMessage("frmSyncManager - showProgress", "enter");
            double progress = 100.0;
            if (pbSyncProgress.Maximum > 0)
                progress = ((double)pbSyncProgress.Value / pbSyncProgress.Maximum) * 100.0;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");

            if (fileDownloadCount > messageMax)
                messageMax = fileDownloadCount;

            if(fileDownloadCount <= messageMax)  
                messageValue = fileDownloadCount;

            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerDownloading") + " " + (fileDownloadCount) + " " + LanguageTranslator.GetValue("SyncManagerUsageOfLabel") + " " + messageMax;
            LogWrapper.LogMessage("frmSyncManager - showProgress", "leave");
        }

        private void setUpControls()
        {
            lblStatusL1.Text = statusMessages[0];
            lblStatusL1.Visible = true;

            lblStatusL3.Text = "";
            lblStatusL3.Visible = true;

            tmrSwapStatusMessage.Enabled = true;
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
            LogWrapper.LogMessage("frmSyncManager - ShowSyncMessage", "enter");
            lastSync = DateTime.Now;
            BasicInfo.LastSyncAt = lastSync;

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
           
            LogWrapper.LogMessage("frmSyncManager - ShowSyncMessage", "leave");
        }

        #region Ballon message functions

        private void ShowUpdateAvailableBalloonMessage(string strNewVersion)
        {
            string strUpdate;

            if (null != strNewVersion)
                strUpdate = "Version " + strNewVersion + " of the sync application is now available.  Please exit the application and relaunch to install the update.";
            else
                strUpdate = "An update for the sync application is available.  Please exit the application and relaunch to install the update.";
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
            //cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
            //                                                          LanguageTranslator.GetValue("SyncIssueFoundText"),
            //                                                         ToolTipIcon.None);
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, "Update Available",
                                                                      strUpdate,
                                                                     ToolTipIcon.None);

            //cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + "Update Available";

            //frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
        }
 
        private void IssueFoundBalloonMessage()
        {
            SetIssueFound(true);
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_warning;
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                      LanguageTranslator.GetValue("SyncIssueFoundText"),
                                                                     ToolTipIcon.None);

            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("SyncIssueFoundText");

            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("SyncManagerMenuIssueFoundText");
        }

        private void SyncStoppedBalloonMessage()
        {
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                           LanguageTranslator.GetValue("TrayBalloonSyncStopText"),
                                                                          ToolTipIcon.None);
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncStopText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
        }

        private void InitialSyncBalloonMessage()
        {
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerStatusAllFilesInSyncLabel");
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText");
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

        private void InitialSyncUptodateMessage()
        {
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                        LanguageTranslator.GetValue("TrayBalloonInitialSyncText") + "\n" + LanguageTranslator.GetValue("TrayBalloonInitialSyncFilesUpToDateText"),
                                                                    ToolTipIcon.None);
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
        }

        public void SetUpSyncNowNotification()
        {
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
        }

        private void SyncFolderUpToDateMessage()
        {
            if (WereItemsSynced())
            {
                cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                        LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate"),
                                                                                       ToolTipIcon.None);

                cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayBalloonSyncFolderUpToDate");
                frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverInitialSyncUpToDateText");
            }
        }

        private void ShowInitialSyncMessage()
        {
            btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            cnotificationManager.NotificationHandler.Icon = Properties.Resources.mezeosyncstatus_syncing;
            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") +
                                            (int)0 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("TrayHoverSyncProgressText");
        }

        private void SyncOfflineMessage()
        {
            cnotificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                    LanguageTranslator.GetValue("TrayAppOfflineText"), ToolTipIcon.None);

            cnotificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOfflineText");
            cnotificationManager.NotifyIcon = Properties.Resources.app_offline;
            frmParent.toolStripMenuItem4.Text = LanguageTranslator.GetValue("AppOfflineMenu");

        }
        #endregion

        private void ShowAutoSyncMessage(bool IsStopped)
        {
            LogWrapper.LogMessage("frmSyncManager - ShowAutoSyncMessage", "enter");
            if (IsStopped)
            {
                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    IssueFoundBalloonMessage();
                }
                else
                {
                    SyncStoppedBalloonMessage();
                    lblStatusL1.Text = LanguageTranslator.GetValue("TrayBalloonSyncStopText");
                }
            }
            else
            {
                if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
                {
                    IssueFoundBalloonMessage();
                }
                else
                {
                    InitialSyncBalloonMessage();
                }
            }

            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
            label1.Text = LanguageTranslator.GetValue("SyncManagerStatusNextSyncAtLabel") + " " + lastSync.AddMinutes(5).ToString("h:mm tt");
            label1.BringToFront();
            label1.Visible = true;
            label1.Show();
            LogWrapper.LogMessage("frmSyncManager - ShowAutoSyncMessage", "leave");
        }

        public void ShowOfflineAtStartUpSyncManager()
        {
            LogWrapper.LogMessage("frmSyncManager - ShowOfflineAtStartUpSyncManager", "enter");
            lastSync = BasicInfo.LastSyncAt;
            lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            lblStatusL1.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            label1.Text = LanguageTranslator.GetValue("SyncManagerResumeSync");
            lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
            LogWrapper.LogMessage("frmSyncManager - ShowOfflineAtStartUpSyncManager", "leave");
        }

        private void ShowSyncDisabledMessage()
        {
            //frmParent.menuItem7.Text = LanguageTranslator.GetValue("SyncManagerSyncDisabled");
            LogWrapper.LogMessage("frmSyncManager - ShowSyncDisabledMessage", "enter");

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
            LogWrapper.LogMessage("frmSyncManager - ShowSyncDisabledMessage", "leave");
        }

        private void DisableProgress()
        {
            LogWrapper.LogMessage("frmSyncManager - DisableProgress", "enter");
            lblPercentDone.Visible = false;
            lblPercentDone.Text = "";

            lblStatusL1.Text = "";
            lblStatusL3.Text = "";
            pbSyncProgress.Visible = false;
           // btnMoveFolder.Enabled = true;
          // Commeted above line as move folder functinality disable 
            ShowNextSyncLabel(true);
            LogWrapper.LogMessage("frmSyncManager - DisableProgress", "leave");
        }

        private void EnableProgress()
        {
            LogWrapper.LogMessage("frmSyncManager - EnableProgress", "enter");
            ShowNextSyncLabel(false);
            Application.DoEvents();
            LogWrapper.LogMessage("frmSyncManager - EnableProgress", "leave");
        }

        private void OpenFolder()
        {
            string argument = BasicInfo.SyncDirPath;
            System.Diagnostics.Process.Start(argument);
        }

        public void DisableSyncManager()
        {
            LogWrapper.LogMessage("frmSyncManager - DisableSyncManager", "enter");
            SetIsDisabledByConnection(true);
            StopSync();
            pnlFileSyncOnOff.Enabled = false;
            rbSyncOff.Checked = true;
            //btnMoveFolder.Enabled = false;
            //Commeted above line as move folder functinality disable 
            btnSyncNow.Enabled = false;
            //tmrNextSync.Enabled = false;
            //lnkFolderPath.Enabled = false;
            LogWrapper.LogMessage("frmSyncManager - DisableSyncManager", "leave");
        }

        public void EnableSyncManager()
        {
            LogWrapper.LogMessage("frmSyncManager - EnableSyncManager", "enter");
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

            SetIsDisabledByConnection(false);
            //btnMoveFolder.Enabled = true;
            //Commeted above line as move folder functinality disable 
            btnSyncNow.Enabled = true;
            if(lockObject != null)
                lockObject.StopThread = false;
            //lnkFolderPath.Enabled = false;
            LogWrapper.LogMessage("frmSyncManager - EnableSyncManager", "leave");
        }
        #endregion

        public string GetParentURI(string strPath)
        {
            if (strPath.IndexOf("\\") == -1)
                return cLoginDetails.szContainerContentsUri;

            strPath = strPath.Substring(0, (strPath.LastIndexOf("\\")));
            return GetContentURI(strPath);
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
            LogWrapper.LogMessage("frmSyncManager - CheckForModifyEvent", "enter");
            string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
            if (strCheck.Trim().Length == 0)
            {
                LogWrapper.LogMessage("frmSyncManager - CheckForModifyEvent", "leave");
                return 2;
            }
            else
            {
                DateTime DBModTime = dbHandler.GetDateTime(DbHandler.TABLE_NAME, DbHandler.MODIFIED_DATE, DbHandler.KEY, lEvent.FileName);

                DateTime ActualModTime = File.GetLastWriteTime(lEvent.FullPath);
                ActualModTime = ActualModTime.AddMilliseconds(-ActualModTime.Millisecond);
                TimeSpan diff = ActualModTime - DBModTime;
                if (diff >= TimeSpan.FromSeconds(1) || diff.CompareTo(TimeSpan.Zero) < 0)
                {
                    LogWrapper.LogMessage("frmSyncManager - CheckForModifyEvent", "leave");
                    return 1;
                }
                else
                {
                    LogWrapper.LogMessage("frmSyncManager - CheckForModifyEvent", "leave");
                    return 0;
                }
            }
        }

        # region DB methods 

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

        private void UpdateDBForModifiedSuccess(LocalEvents lEvent, string strContentURi)
        {
            int nStatusCode = 0;
            string strEtag = cMezeoFileCloud.GetETag(strContentURi, ref nStatusCode);
            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG, strEtag, DbHandler.KEY, lEvent.FileName);

            FileInfo fileInfo = new FileInfo(lEvent.FullPath);

            dbHandler.UpdateModifiedDate(fileInfo.LastWriteTime, lEvent.FileName);

            UpdateDBForStatus(lEvent, DB_STATUS_SUCCESS);
        }

        #endregion

        # region Walk directory

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
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForDelete", "Caught exception (UnauthorizedAccessException): " + e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForDelete", "Caught exception (DirectoryNotFoundException): " + e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    string Key = fi.FullName.Substring(BasicInfo.SyncDirPath.Length + 1);
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
                    SendToRecycleBin(dirInfo.FullName, false);
                    //Directory.Delete(dirInfo.FullName);
                    dbHandler.Delete(DbHandler.TABLE_NAME, DbHandler.KEY , Key);
                }
            }
        }

        private void WalkDirectoryTreeforAddFolder(System.IO.DirectoryInfo root, string lEventOldPath, ref List<LocalEvents> addEvents, ref List<LocalEvents> events)
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
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeforAddFolder", "Caught exception (UnauthorizedAccessException): " + e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeforAddFolder", "Caught exception (DirectoryNotFoundException): " + e.Message);
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

                    try
                    {
                        FileAttributes attr = File.GetAttributes(fi.FullName);
                        if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden || (attr & FileAttributes.Temporary) == FileAttributes.Temporary)
                            bIsAlreadyAdded = true;
                    }
                    catch (Exception ex)
                    {
                        LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeforAddFolder", "Caught exception: " + ex.Message);
                        bIsAlreadyAdded = true;
                    }

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

                    WalkDirectoryTreeforAddFolder(dirInfo, lEventOldPath + "\\" + dirInfo.Name, ref addEvents, ref events);
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
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForMove", "Caught exception (UnauthorizedAccessException): " + e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTreeForMove", "Caught exception (DirectoryNotFoundException): " + e.Message);
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
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTree", "Caught exception (UnauthorizedAccessException): " + e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                LogWrapper.LogMessage("frmSyncManager - WalkDirectoryTree", "Caught exception (DirectoryNotFoundException): " + e.Message);
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

        # endregion

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
            LogWrapper.LogMessage("frmSyncManager - HandleEvents", "Enter");

            SetIsLocalEventInProgress(true);

            List<int> RemoveIndexes = new List<int>();
            List<LocalEvents> eModified = new List<LocalEvents>();
            List<LocalEvents> eMove = new List<LocalEvents>();
            List<LocalEvents> eAddEvents = new List<LocalEvents>();

            List<LocalEvents> events = EventQueue.GetCurrentQueue();

            foreach (LocalEvents lEvent in events)
            {
                if (caller.CancellationPending)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents ", "Cancelled called");
                    caller.CancelAsync();
                    return USER_CANCELLED;
                }

                bool bRet = true;

                LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath);

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
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Enter");

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
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED || lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Enter");
                    //string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, DbHandler.KEY + " = '" + lEvent.FileName + "' and " + DbHandler.STATUS + "='SUCCESS'");

                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY, DbHandler.STATUS }, new string[] { lEvent.FileName, DB_STATUS_SUCCESS }, new DbType[] { DbType.String, DbType.String });

                    if (strCheck.Trim().Length == 0)
                        bRet = true;
                    else
                        bRet = false;

                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED && bRet)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "- checking for move events - Enter");
                    string strNameAdd = lEvent.FileName.Substring(lEvent.FileName.LastIndexOf("\\") + 1);
                    foreach (LocalEvents id in events)
                    {
                        if (id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                        {
                            DateTime dtCreate = lEvent.EventTimeStamp.AddMilliseconds(-id.EventTimeStamp.Millisecond);
                            TimeSpan Diff = dtCreate - id.EventTimeStamp;
                            if (Diff <= TimeSpan.FromSeconds(1))
                            {
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
                            }
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
                            WalkDirectoryTreeforAddFolder(new DirectoryInfo(lEvent.FullPath), lEvent.FileName, ref eAddEvents, ref events);
                        }
                    }

                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + "- checking for move events - Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Enter");

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
                                    WalkDirectoryTreeforAddFolder(new DirectoryInfo(lEvent.FullPath), lEvent.FileName, ref eAddEvents, ref events);
                                }
                                //eAddEvents.Add(lNewEvent);
                                //bRet = false;
                            }
                        }
                    }

                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Leave");
                }

                if (lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Enter");

                    string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                    if (strCheck.Trim().Length == 0)
                        bRet = false;
                    else
                        bRet = true;

                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - " + lEvent.EventType.ToString() + " - Leave");
                }

               // if (lEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
              //  {
                    if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden || (attr & FileAttributes.Temporary) == FileAttributes.Temporary )
                        bRet = false;
               // }

                if (bRet)
                {
                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - AddinDB Enter");

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

                    LogWrapper.LogMessage("frmSyncManager - HandleEvents - lEvent - ", lEvent.FullPath + " - AddinDB Leave");
                }
              
                if (!bRet)
                {
                    if(!RemoveIndexes.Contains(events.IndexOf(lEvent)))
                        RemoveIndexes.Add(events.IndexOf(lEvent));
                }
            }

            RemoveIndexes.Sort();
            for (int n = RemoveIndexes.Count - 1; n >= 0; n--)
            {
                events.RemoveAt(RemoveIndexes[n]);
            }

            RemoveIndexes.Clear();

            messageMax = events.Count;

            if (eModified.Count != 0)
            {
                LogWrapper.LogMessage("frmSyncManager - HandleEvents eModifiedList - ", eModified.Count.ToString() + " - Enter");

                foreach (LocalEvents levent in eModified)
                {
                    UpdateDBForStatus(levent, DB_STATUS_IN_PROGRESS);
                }
                events.AddRange(eModified);
                eModified.Clear();

                LogWrapper.LogMessage("frmSyncManager - HandleEvents eModifiedList - ", eModified.Count.ToString() + " - Leave");
            }

            if (eAddEvents.Count != 0)
            {
                LogWrapper.LogMessage("frmSyncManager - HandleEvents eAddEventsList - ", eAddEvents.Count.ToString() + " - Enter");

                foreach (LocalEvents levent in eAddEvents)
                {
                    AddInDBForAdded(levent);
                }
                events.AddRange(eAddEvents);
                eAddEvents.Clear();

                LogWrapper.LogMessage("frmSyncManager - HandleEvents eAddEventsList - ", eAddEvents.Count.ToString() + " - Leave");
            }

            if (eMove.Count != 0)
            {
                LogWrapper.LogMessage("frmSyncManager - HandleEvents eMoveList - ", eMove.Count.ToString() + " - Enter");
                foreach (LocalEvents levent in eMove)
                {
                    UpdateKeyInDb(levent.OldFileName, levent.FileName);
                    UpdateDBForStatus(levent, DB_STATUS_IN_PROGRESS);

                    int sepIndex = levent.FileName.LastIndexOf("\\");
                    string newParentDir = "\\";
                    if (sepIndex >= 0)
                    {
                        newParentDir = levent.FileName.Substring(0, sepIndex);
                    }

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

                LogWrapper.LogMessage("frmSyncManager - HandleEvents eMoveList - ", eMove.Count.ToString() + " - Leave");
            }
            int returnCode = 1;

            if (caller != null)
            {
                caller.ReportProgress(SYNC_STARTED);
            }

            LogWrapper.LogMessage("frmSyncManager - HandleEvents", " ProcessLocalEvents Going");

            returnCode = ProcessLocalEvents(caller, ref events);

            LogWrapper.LogMessage("frmSyncManager - HandleEvents", " ProcessLocalEvents Exit");

            SetIsLocalEventInProgress(false);

            LogWrapper.LogMessage("frmSyncManager - HandleEvents", "Leave");
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
                dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS, status, DbHandler.KEY, parentKey);
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

        /* 25MB Need to keep this function for future reference */
        //Adding function to put limit on file upload 
        private bool checkFileTooLarge(string filePath)
        {
            //FileInfo iFileDetails = new FileInfo(filePath);
            //long fileSize;
            //if (File.Exists(filePath))
            //{
            //    fileSize = iFileDetails.Length;
            //    if (fileSize >= 25 * 1000 * 1000)
            //        return true;
            //}
            return false;
        }

        private int ProcessLocalEvents(BackgroundWorker caller, ref List<LocalEvents> events)
        {
            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Enter");
            string strUrl = "";
            bool bRetConflicts = true;

            if (caller != null)
            {
                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Calling ReportProgress with PROCESS_LOCAL_EVENTS_STARTED");
                caller.ReportProgress(PROCESS_LOCAL_EVENTS_STARTED, events.Count());
            }

            fileDownloadCount = 1;

            List<int> SuccessIndexes = new List<int>();

            foreach (LocalEvents lEvent in events)
            {
                IncrementTransferCount();
                if (caller.CancellationPending)
                {
                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Canceled Called");
                    caller.CancelAsync();
                    return USER_CANCELLED;
                }

                if (caller != null)
                {
                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "ReportProgress Called with PROGRESS_CHANGED_WITH_FILE_NAME for " + lEvent.FullPath);
                    caller.ReportProgress(PROGRESS_CHANGED_WITH_FILE_NAME, lEvent.FullPath);
                }

                FileAttributes attr = FileAttributes.Normal;

                bool isDirectory = false;
                bool isFile = File.Exists(lEvent.FullPath);

                if (isFile && lEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                {
                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Check for file lock - Enter");
                    FileInfo fInfo = new FileInfo(lEvent.FullPath);
                    bool IsLocked = IsFileLocked(fInfo);
                    while (IsLocked && fInfo.Exists)
                    {
                        IsLocked = IsFileLocked(fInfo);
                    }
                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Check for file lock - Leave");
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
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MOVE - Enter for file path " + lEvent.FullPath);
                             string strContentURi =GetContentURI(lEvent.FileName);
                             if (strContentURi.Trim().Length == 0)
                             {
                                 LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
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
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetParentURI for length ZERO");
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
                                if (!checkFileTooLarge(lEvent.FullPath))
                                    bRet = cMezeoFileCloud.FileMove(strContentURi, strName, mimeType, iDetails.bPublic, strParentUri, ref nStatusCode);
                                else
                                    nStatusCode = 200;
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

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MOVE - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_ADDED:
                        {
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_ADDED - Enter for file path " + lEvent.FullPath);
                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);
                            string strParentURi = GetParentURI(lEvent.FileName);
                            if (strParentURi.Trim().Length == 0)
                            {
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetParentURI for length ZERO");
                                continue;
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                string folderName = lEvent.FullPath.Substring((lEvent.FullPath.LastIndexOf("\\") + 1));

                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Create new container for folder " + folderName);

                                // Defect 273 - If somehow, someway the same folder name exists locally and on the cloud,
                                //              do not create a new folder in the cloud with this name.  Just use the existing
                                //              container and upload files/containers into it.
                                ItemDetails[] itemDetails;
                                bool bCreateCloudContainer = true;
                                // Grab a list of files and containers for the parent.
                                itemDetails = cMezeoFileCloud.DownloadItemDetails(strParentURi, ref nStatusCode, folderName);
                                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                {
                                    return LOGIN_FAILED;
                                }
                                else if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                                {
                                    return SERVER_INACCESSIBLE;
                                }
                                else if (nStatusCode == ResponseCode.DOWNLOADITEMDETAILS)
                                {
                                    // Look through each item for a container with the same name.
                                    if (itemDetails != null)
                                    {
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
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (bCreateCloudContainer)
                                {
                                   strUrl = cMezeoFileCloud.NewContainer(folderName, strParentURi, ref nStatusCode);
                                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                    {
                                        return LOGIN_FAILED;
                                    }
                                    else if (nStatusCode != ResponseCode.NEWCONTAINER)
                                    {
                                        return SERVER_INACCESSIBLE;
                                    }
                                    else if ((strUrl.Trim().Length != 0) && (nStatusCode == ResponseCode.NEWCONTAINER))
                                    {
                                        strUrl += "/contents";
                                        SuccessIndexes.Add(events.IndexOf(lEvent));
                                        bRet = true;

                                        string strParent = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                        if (strParent.Trim().Length == 0)
                                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, strParentURi, DbHandler.KEY, lEvent.FileName);

                                        MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                        UpdateDBForAddedSuccess(strUrl, lEvent);
                                    }
                                }
                                else
                                {
                                    SuccessIndexes.Add(events.IndexOf(lEvent));
                                    bRet = true;

                                    string strParent = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                    if (strParent.Trim().Length == 0)
                                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, strParentURi, DbHandler.KEY, lEvent.FileName);

                                    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                    UpdateDBForAddedSuccess(strUrl, lEvent);
                                }

                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Container URI for folder " + folderName + " is " + strUrl);
                            }
                            else
                            {
                                //if (strParentURi.Trim().Length == 0)
                                //    strParentURi = CheckAndCreateForEventsParentDir(lEvent.FileName);

                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Start uploading file for " + lEvent.FullPath + ", at parent URI " + strParentURi);

                                string fileName = lEvent.FullPath.Substring((lEvent.FullPath.LastIndexOf("\\") + 1));
                                ItemDetails[] itemDetailsfile;
                                bool buploadfileToCloud = true;
                                // Grab a list of files for the parent.
                                itemDetailsfile = cMezeoFileCloud.DownloadItemDetails(strParentURi, ref nStatusCode, fileName);
                                if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                {
                                    return LOGIN_FAILED;
                                }
                                else if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                                {
                                    return SERVER_INACCESSIBLE;
                                }
                                else if (nStatusCode == ResponseCode.DOWNLOADITEMDETAILS)
                                {
                                    // Look through each item for a file with the same name.
                                    if (itemDetailsfile != null)
                                    {
                                        foreach (ItemDetails item in itemDetailsfile)
                                        {
                                            if ("FILE" == item.szItemType)
                                            {
                                                if (fileName == item.strName)
                                                {
                                                    // Don't create a new/duplicate file.
                                                    buploadfileToCloud = false;
                                                    // Populate the url with this file.
                                                    strUrl = item.szContentUrl;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (buploadfileToCloud)
                                {
                                    if (!checkFileTooLarge(lEvent.FullPath))
                                        strUrl = cMezeoFileCloud.UploadingFile(lEvent.FullPath, strParentURi, ref nStatusCode);
                                    else
                                        nStatusCode = 201;

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
                                        strUrl += "/content";
                                        SuccessIndexes.Add(events.IndexOf(lEvent));
                                        bRet = true;

                                        string strParent = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                        if (strParent.Trim().Length == 0)
                                            dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, strParentURi, DbHandler.KEY, lEvent.FileName);

                                        MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                        UpdateDBForAddedSuccess(strUrl, lEvent);
                                    }
                                }
                                else
                                {
                                    SuccessIndexes.Add(events.IndexOf(lEvent));
                                    bRet = true;

                                    string strParent = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                    if (strParent.Trim().Length == 0)
                                        dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PARENT_URL, strParentURi, DbHandler.KEY, lEvent.FileName);

                                    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                    UpdateDBForAddedSuccess(strUrl, lEvent);
                                }
                            }

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_ADDED - Leave for file path " + lEvent.FullPath);
                        }
                        break;

                        case LocalEvents.EventsType.FILE_ACTION_MODIFIED:
                        {
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MODIFIED - Enter for file path " + lEvent.FullPath);

                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);
                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                continue;
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                bRet = false;
                            else
                            {
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                                if (bRetConflicts)
                                {
                                    if (!checkFileTooLarge(lEvent.FullPath))
                                        bRet = cMezeoFileCloud.OverWriteFile(lEvent.FullPath, strContentURi, ref nStatusCode);
                                    else
                                        nStatusCode = 200;

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
                                }
                            }

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_MODIFIED - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_REMOVED:
                        {
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_REMOVED - Enter for file path " + lEvent.FullPath);

                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
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

                                if (!checkFileTooLarge(lEvent.FullPath))
                                    bRet = cMezeoFileCloud.Delete(strContentURi, ref nStatusCode, lEvent.FullPath);
                                else
                                    nStatusCode = 200;

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
                       
                            }

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "FILE_ACTION_REMOVED - Leave for file path " + lEvent.FullPath);
                        }
                        break;
                    case LocalEvents.EventsType.FILE_ACTION_RENAMED:
                        {
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "case FILE_ACTION_RENAMED");

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetContentURI for " + lEvent.FileName);

                            string strContentURi = GetContentURI(lEvent.FileName);
                            if (strContentURi.Trim().Length == 0)
                            {
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "GetContentURI for length ZERO");
                                continue;
                            }

                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus DB_STATUS_IN_PROGRESS for " + lEvent.FullPath);
                            MarkParentsStatus(lEvent.FullPath, DB_STATUS_IN_PROGRESS);

                            string changedName = lEvent.FileName.Substring((lEvent.FileName.LastIndexOf("\\") + 1));
                            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "changedName " + changedName);

                            if (isFile)
                            {
                                bRetConflicts = CheckForConflicts(lEvent, strContentURi);
                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "isFile bRetConflicts " + bRetConflicts.ToString());
                            }
                            if (strContentURi.Substring(strContentURi.Length - 9).Equals("/contents") ||
                               strContentURi.Substring(strContentURi.Length - 8).Equals("/content"))
                            {
                                strContentURi = strContentURi.Substring(0, strContentURi.LastIndexOf("/"));
                            }

                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                bRet = cMezeoFileCloud.ContainerRename(strContentURi, changedName, ref nStatusCode);

                                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Directory bRet " + bRet.ToString());
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
                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus DB_STATUS_SUCCESS for  " + lEvent.FullPath);
                                    MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Calling for UpdateDBForRenameSuccess");
                                    UpdateDBForRenameSuccess(lEvent);
                                }
                   
                            }
                            else
                            {
                                if (bRetConflicts)
                                {
                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "isFile bRetConflicts " + bRetConflicts.ToString());
                                    ItemDetails iDetails = cMezeoFileCloud.GetContinerResult(strContentURi, ref nStatusCode);

                                    if (nStatusCode == ResponseCode.LOGINFAILED1 || nStatusCode == ResponseCode.LOGINFAILED2)
                                    {
                                        return LOGIN_FAILED;
                                    }
                                    else if (nStatusCode != ResponseCode.GETCONTINERRESULT)
                                    {
                                        return SERVER_INACCESSIBLE;
                                    }

                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "updating DB   DbHandler.PUBLIC to " + iDetails.bPublic + " for DbHandler.KEY " + lEvent.FileName);

                                    dbHandler.Update(DbHandler.TABLE_NAME, DbHandler.PUBLIC , iDetails.bPublic , DbHandler.KEY , lEvent.FileName ); 
                                    //bool bPublic = dbHandler.GetBoolean(DbHandler.TABLE_NAME, DbHandler.PUBLIC, DbHandler.KEY + " = '" + lEvent.FileName + "'");

                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "getting mime type from DB");
                                    string mimeType = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.MIMIE_TYPE, new string[] { DbHandler.KEY }, new string[] { lEvent.FileName }, new DbType[] { DbType.String });
                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "mime type " + mimeType);

                                    LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Calling cMezeoFileCloud.FileRename for content uri " + strContentURi + " with new name " + changedName);

                                    if (!checkFileTooLarge(lEvent.FullPath))
                                        bRet = cMezeoFileCloud.FileRename(strContentURi, changedName, mimeType, iDetails.bPublic, ref nStatusCode);
                                    else
                                        nStatusCode = 200;

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
                                        LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "MarkParentsStatus " + lEvent.FullPath + " to DB_STATUS_SUCCESS");
                                        MarkParentsStatus(lEvent.FullPath, DB_STATUS_SUCCESS);
                                        LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Calling UpdateDBForRenameSuccess");
                                        UpdateDBForRenameSuccess(lEvent);
                                    }
                
                                }
                            }
                        }
                        break; 
                }

                //if (bOffline)
                //    break;

                fileDownloadCount++;
                LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "file download count: " + fileDownloadCount);
            }

            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "clear events");
            //if (events.Count == SuccessIndexes.Count)
            if (EventQueue.QueueCount() == SuccessIndexes.Count)
                    events.Clear();
            else
            {
                SuccessIndexes.Sort();
                for (int n = SuccessIndexes.Count - 1; n >= 0; n--)
                {
                    events.RemoveAt(SuccessIndexes[n]);
                }
            }

           int returnCode = 1;
          
            SetIsLocalEventInProgress(false);

            LogWrapper.LogMessage("SyncManager - ProcessLocalEvents", "Leave");

            return returnCode;
        }

        private void SetIssueFound(bool bIsIssueFound)
        {
            LogWrapper.LogMessage("frmSyncManager - SetIssueFound", "enter");
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
            LogWrapper.LogMessage("frmSyncManager - SetIssueFound", "leave");
        }

        private void ReportConflict(LocalEvents lEvent , IssueFound.ConflictType cType)
        {
            LogWrapper.LogMessage("SyncManager - ReportConflict", "Enter");
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

            // Issue Fix for Conflicts 
             IssueFoundBalloonMessage();
            
             LogWrapper.LogMessage("SyncManager - ReportConflict", "Leave");
        }

        private bool CheckForConflicts(LocalEvents lEvent, string strContentUrl)
        {
            LogWrapper.LogMessage("SyncManager - CheckForConflicts", "Enter, content uri " + strContentUrl);
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
                   // break;
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
                  //  break;
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
                 //   break;
            }

            LogWrapper.LogMessage("SyncManager - CheckForConflicts", "Leave");
            return true;
        }

        void queue_WatchCompletedEvent()
        {
            LogWrapper.LogMessage("frmSyncManager - queue_WatchCompletedEvent", "enter");
            if (IsInIdleState() && EventQueue.QueueNotEmpty() && BasicInfo.AutoSync && !BasicInfo.IsInitialSync && !IsDisabledByConnection())
            {
                if (!bwLocalEvents.IsBusy)
                    bwLocalEvents.RunWorkerAsync();
            }
            LogWrapper.LogMessage("frmSyncManager - queue_WatchCompletedEvent", "leave");
        }

        private void bwNQUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_DoWork", "Enter");

            fileDownloadCount = 1;

            int nStatusCode = 0;
            NQDetails[] pNQDetails = null;

            NQLengthResult nqLengthRange = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), ref nStatusCode);
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

            int totalNQLength = (nqRangeEnd - nqRangeStart) + 1;
            maxProgressValue = totalNQLength;

            ((BackgroundWorker)sender).ReportProgress(INITIAL_NQ_SYNC, maxProgressValue);

            int nTempCount = 0;

            if (totalNQLength > 0)
            {
                ShowOtherProgressBar("");
            }

            while (totalNQLength > 0)
            {
                IncrementTransferCount();
                int NQnumToRequest = 0;

                if (totalNQLength >= 10)
                {
                    NQnumToRequest = 10;
                }
                else
                {
                    NQnumToRequest = totalNQLength;
                }

                pNQDetails = cMezeoFileCloud.NQGetData(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), NQnumToRequest, ref nStatusCode);
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

                if (pNQDetails != null)
                {
                    foreach (NQDetails nq in pNQDetails)
                    {
                        ShowOtherProgressBar(nq.StrObjectName);

                        if (bwNQUpdate.CancellationPending)
                        {
                            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_DoWork", "bwNQUpdate.CancellationPending called inner");

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
                            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_DoWork - ", nq.StrObjectName + " - Delete From NQ");
                            cMezeoFileCloud.NQDeleteValue(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), 1, ref nStatusCode);
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
                }
                
                if (isBreak)
                    break;

                nTempCount = 0;

                if (bwNQUpdate.CancellationPending)
                {
                    LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_DoWork", "bwNQUpdate.CancellationPending called outer");

                    e.Cancel = true;
                    bwNQUpdate.ReportProgress(UPDATE_NQ_CANCELED);
                    break;
                }

                totalNQLength -= NQnumToRequest;

                nqLengthRange = cMezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + cLoginDetails.szNQParentUri, BasicInfo.GetQueueName(), ref nStatusCode);
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

                if (!isBreak)
                {
                    if (nqRangeEnd <= nqLengthRange.nEnd)
                    {
                        int diff = nqLengthRange.nEnd - nqRangeEnd;
                        totalNQLength += diff;
                        maxProgressValue += diff;
                        messageMax += diff;
                        ((BackgroundWorker)sender).ReportProgress(UPDATE_NQ_MAXIMUM, maxProgressValue);
                    }
                }
                nqRangeStart = nqLengthRange.nStart;
                nqRangeEnd = nqLengthRange.nEnd;

                if (isBreak)
                    break;
            }

            SetIsSyncInProgress(false);
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_DoWork", "Leave");   
        }

        private void bwNQUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_RunWorkerCompleted", "enter");
            ShowSyncMessage(IsEventCanceled());
            tmrNextSync.Interval = FIVE_MINUTES;
            SetIsSyncInProgress(false);
            SetIsEventCanceled(false);
            try
            {
                if (e.Result != null && (CancelReason)e.Result == CancelReason.LOGIN_FAILED)
                {
                    this.Hide();
                    frmParent.ShowLoginAgainFromSyncMgr();
                }
                else if (e.Result != null && (CancelReason)e.Result == CancelReason.SERVER_INACCESSIBLE)
                {
                   // DisableSyncManager();
                   // ShowSyncManagerOffline();
                   // CheckServerStatus(); TODO:check for offline (Modified for server status thread)
                }
                else
                {
                    queue_WatchCompletedEvent();
                }
            }
            catch(Exception ex)
            {
                LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_RunWorkerCompleted", "Caught exception: " + ex.Message);
            }
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_RunWorkerCompleted", "leave");
        }

        private void bwNQUpdate_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_ProgressChanged", "enter");
            if (e.ProgressPercentage == INITIAL_NQ_SYNC)
            {
                SetUpControlForSync();
                showProgress();
                if (!pbSyncProgress.Visible)
                {
                    Application.DoEvents();
                }
            }
            else if (e.ProgressPercentage == UPDATE_NQ_PROGRESS)
            {
                showProgress();
            }
            else if (e.ProgressPercentage == UPDATE_NQ_CANCELED)
            {
                SetIsSyncInProgress(false);
                if (IsDisabledByConnection())
                {
                   // DisableProgress();
                   // ShowSyncDisabledMessage();
                    //ShowSyncManagerOffline();
                    // CheckServerStatus(); TODO:check for offline (Modified for server status thread)
                }
                else
                {
                    ShowSyncMessage(true);
                }
            }
            else if (e.ProgressPercentage == UPDATE_NQ_MAXIMUM)
            {
                showProgress();
            }
            LogWrapper.LogMessage("frmSyncManager - bwNQUpdate_ProgressChanged", "leave");
        }

        private void btnIssuesFound_Click(object sender, EventArgs e)
        {
            frmIssuesFound.Show();
            frmIssuesFound.BringToFront();
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
            LogWrapper.LogMessage("frmSyncManager - bwOffilneEvent_DoWork", "enter");
            SetIsOfflineWorking(true);
            int statusCode = HandleEvents((BackgroundWorker)sender);
            e.Result = statusCode;
            LogWrapper.LogMessage("frmSyncManager - bwOffilneEvent_DoWork", "leave");
        }

        private void bwOffilneEvent_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwOffilneEvent_RunWorkerCompleted", "enter");
            SetIsOfflineWorking(false);
            if ((int)e.Result == 1)
            {
                ShowLocalEventsCompletedMessage();

                if (!IsEventCanceled())
                    UpdateNQ();
            }
            else if ((int)e.Result == USER_CANCELLED)
            {
                ShowSyncMessage(EventQueue.QueueNotEmpty());
            }
            else if ((int)e.Result == LOGIN_FAILED)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
            }
            else if ((int)e.Result == SERVER_INACCESSIBLE)
            {
                ShowLocalEventsCompletedMessage();
               // DisableSyncManager();
               // ShowSyncManagerOffline();
                //  CheckServerStatus(); TODO:check for offline (Modified for server status thread)
            }
            LogWrapper.LogMessage("frmSyncManager - bwOffilneEvent_RunWorkerCompleted", "leave");
        }

        private void bwLocalEvents_DoWork(object sender, DoWorkEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_DoWork", "enter");
            // SetIsDisabledByConnection(false);
            int statusCode = HandleEvents((BackgroundWorker)sender);
            e.Result = statusCode;
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_DoWork", "leave");
        }

        private void bwLocalEvents_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_ProgressChanged", "enter");
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Visible = false;
                    pbSyncProgress.Refresh();

                    if (!lblPercentDone.Visible)
                    {
                        lblPercentDone.Text = "";
                        Application.DoEvents();
                    }

                    if (e.ProgressPercentage == SYNC_STARTED)
                    {
                        ShowInitialSyncMessage();
                    }
                    else if (e.ProgressPercentage == PROCESS_LOCAL_EVENTS_STARTED)
                    {
                        InitializeLocalEventsProcess((int)e.UserState);
                    }
                    else if (e.ProgressPercentage == PROGRESS_CHANGED_WITH_FILE_NAME)
                    {
                        showProgress();
                        lblStatusL3.Text = e.UserState.ToString();
                    }
                    else if (e.ProgressPercentage == LOCAL_EVENTS_COMPLETED)
                    {
                        if (IsDisabledByConnection())
                        {
                            lastSync = DateTime.Now;
                            BasicInfo.LastSyncAt = lastSync;

                            //  CheckServerStatus(); *** TODO:check for offline (Modified for server status thread)
                            // DisableProgress();
                            // ShowSyncDisabledMessage();
                            // ShowSyncManagerOffline();
                        }
                        else
                        {
                            ShowLocalEventsCompletedMessage();
                        }
                    }
                    else if (e.ProgressPercentage == LOCAL_EVENTS_STOPPED)
                    {
                        ShowSyncMessage(EventQueue.QueueNotEmpty());
                    }
                });
            }
            else
            {
                label1.Visible = false;
                pbSyncProgress.Refresh();

                if (!lblPercentDone.Visible)
                {
                    lblPercentDone.Text = "";
                    Application.DoEvents();
                }

                if (e.ProgressPercentage == SYNC_STARTED)
                {
                    ShowInitialSyncMessage();
                }
                else if (e.ProgressPercentage == PROCESS_LOCAL_EVENTS_STARTED)
                {
                    InitializeLocalEventsProcess((int)e.UserState);
                }
                else if (e.ProgressPercentage == PROGRESS_CHANGED_WITH_FILE_NAME)
                {
                    showProgress();
                    lblStatusL3.Text = e.UserState.ToString();
                }
                else if (e.ProgressPercentage == LOCAL_EVENTS_COMPLETED)
                {
                    if (IsDisabledByConnection())
                    {
                        lastSync = DateTime.Now;
                        BasicInfo.LastSyncAt = lastSync;

                        //  CheckServerStatus(); *** TODO:check for offline (Modified for server status thread)
                        // DisableProgress();
                        // ShowSyncDisabledMessage();
                        // ShowSyncManagerOffline();
                    }
                    else
                    {
                        ShowLocalEventsCompletedMessage();
                    }
                }
                else if (e.ProgressPercentage == LOCAL_EVENTS_STOPPED)
                {
                    ShowSyncMessage(EventQueue.QueueNotEmpty());
                }
            }
            
            //Application.DoEvents();
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_ProgressChanged", "leave");
        }

        public void SetMaxProgress(double fileSize, string fileName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    LogWrapper.LogMessage("frmSyncManager - SetMaxProgress", "enter");
                    lblStatusL3.Text = fileName;

                    pbSyncProgress.Maximum = (int)fileSize;
                    pbSyncProgress.Value = 0;

                    if (pbSyncProgress.Style != ProgressBarStyle.Continuous)
                        pbSyncProgress.Style = ProgressBarStyle.Continuous;

                    pbSyncProgress.BringToFront();
                    pbSyncProgress.Visible = true;
                    pbSyncProgress.Show();
                    lblPercentDone.Visible = true;
                    lblPercentDone.Show();
                    LogWrapper.LogMessage("frmSyncManager - SetMaxProgress", "leave");
                });
            }
            else
            {
                LogWrapper.LogMessage("frmSyncManager - SetMaxProgress", "enter");
                lblStatusL3.Text = fileName;

                pbSyncProgress.Maximum = (int)fileSize;
                pbSyncProgress.Value = 0;

                if (pbSyncProgress.Style != ProgressBarStyle.Continuous)
                    pbSyncProgress.Style = ProgressBarStyle.Continuous;

                pbSyncProgress.BringToFront();
                pbSyncProgress.Visible = true;
                pbSyncProgress.Show();
                lblPercentDone.Visible = true;
                lblPercentDone.Show();
                LogWrapper.LogMessage("frmSyncManager - SetMaxProgress", "leave");
            }
        }

        public void ShowOtherProgressBar(string fileName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    LogWrapper.LogMessage("frmSyncManager - ShowOtherProgressBar", "enter");
                    lblStatusL3.Text = fileName;
                    pbSyncProgress.Maximum = 1;
                    pbSyncProgress.Value = 0;

                    if (false == pbSyncProgress.Visible)
                    {
                        pbSyncProgress.Visible = true;
                        pbSyncProgress.Show();
                    }
                    pbSyncProgress.BringToFront();
                    pbSyncProgress.Refresh();
                    lblPercentDone.Text = "100%";
                    lblPercentDone.Visible = true;
                    lblPercentDone.Show();
                    LogWrapper.LogMessage("frmSyncManager - ShowOtherProgressBar", "leave");
                });
            }
            else
            {
                LogWrapper.LogMessage("frmSyncManager - ShowOtherProgressBar", "enter");
                lblStatusL3.Text = fileName;
                pbSyncProgress.Maximum = 1;
                pbSyncProgress.Value = 1;

                //if (pbSyncProgress.Style != ProgressBarStyle.Marquee)
                //{
                //    pbSyncProgress.Style = ProgressBarStyle.Marquee;
                //    pbSyncProgress.MarqueeAnimationSpeed = 200;
                //}

                if (false == pbSyncProgress.Visible)
                {
                    pbSyncProgress.Visible = true;
                    pbSyncProgress.Show();
                }
                pbSyncProgress.BringToFront();
                pbSyncProgress.Refresh();
                lblPercentDone.Text = "100%";
                lblPercentDone.Visible = true;
                lblPercentDone.Show();
                LogWrapper.LogMessage("frmSyncManager - ShowOtherProgressBar", "leave");
            }
        }

        //To increment progress bar this is a call back function 
        public void CallbackSyncProgress(double filesize)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    LogWrapper.LogMessage("frmSyncManager - CallbackSyncProgress", "enter");
                    try
                    {
                        if (pbSyncProgress.Value + filesize > pbSyncProgress.Maximum)
                            pbSyncProgress.Value = pbSyncProgress.Maximum;
                        else
                            pbSyncProgress.Value += (int)filesize;
                        if (0 == pbSyncProgress.Maximum)
                        {
                            // For 0 byte transfers and operations such as delete, copy, move, etc....
                            pbSyncProgress.Maximum = 1;
                            pbSyncProgress.Value = 1;
                        }
                        pbSyncProgress.Refresh();

                        if (pbSyncProgress.Maximum > 0)
                        {
                            double progress = ((double)pbSyncProgress.Value / pbSyncProgress.Maximum) * 100.0;
                            lblPercentDone.Text = Convert.ToString((int)progress) + "%";
                            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
                        }
                        else
                        {
                            //Progress bar will show 100% - making as string in resource file
                            lblPercentDone.Text = LanguageTranslator.GetValue("ProgressBarComplete");
                            //lblPercentDone.Text = "100%";
                            cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + 100 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWrapper.LogMessage("frmSyncManager - CallBackSyncProgress", "Caught exception: " + ex.Message);
                        LogWrapper.LogMessage("frmSyncManager - CallBackSyncProgress", "Caught exception Maximum and actual value is: " + pbSyncProgress.Maximum + " , " + pbSyncProgress.Value);
                    }
                    LogWrapper.LogMessage("frmSyncManager - CallbackSyncProgress", "leave");
                });
            }
            else
            {
                LogWrapper.LogMessage("frmSyncManager - CallbackSyncProgress", "enter");
                try
                {
                    if (pbSyncProgress.Value + filesize > pbSyncProgress.Maximum)
                        pbSyncProgress.Value = pbSyncProgress.Maximum;
                    else
                        pbSyncProgress.Value += (int)filesize;
                    if (0 == pbSyncProgress.Maximum)
                    {
                        // For 0 byte transfers and operations such as delete, copy, move, etc....
                        pbSyncProgress.Maximum = 1;
                        pbSyncProgress.Value = 1;
                    }
                    pbSyncProgress.Refresh();

                    if (pbSyncProgress.Maximum > 0)
                    {
                        double progress = ((double)pbSyncProgress.Value / pbSyncProgress.Maximum) * 100.0;
                        lblPercentDone.Text = Convert.ToString((int)progress) + "%";
                        cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + (int)progress + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
                    }
                    else
                    {
                        //Progress bar will show 100% - making as string in resource file
                        lblPercentDone.Text = LanguageTranslator.GetValue("ProgressBarComplete");
                        //lblPercentDone.Text = "100%";
                        cnotificationManager.HoverText = AboutBox.AssemblyTitle + "\n" + LanguageTranslator.GetValue("TrayHoverSyncProgressText") + 100 + LanguageTranslator.GetValue("TrayHoverSyncProgressInitialText");
                    }
                }
                catch (Exception ex)
                {
                    LogWrapper.LogMessage("frmSyncManager - CallBackSyncProgress", "Caught exception: " + ex.Message);
                    LogWrapper.LogMessage("frmSyncManager - CallBackSyncProgress", "Caught exception Maximum and actual value is: " + pbSyncProgress.Maximum + " , " + pbSyncProgress.Value);
                }

                LogWrapper.LogMessage("frmSyncManager - CallbackSyncProgress", "leave");
            }
        }

        private void InitializeLocalEventsProcess(int progressMax)
        {
            LogWrapper.LogMessage("frmSyncManager - InitializeLocalEventsProcess", "enter");
            messageValue = 0;
            fileDownloadCount = 1;

            SetIssueFound(false);
            ShowNextSyncLabel(false);
            LogWrapper.LogMessage("frmSyncManager - InitializeLocalEventsProcess", "leave");
        }

        public void ShowSyncManagerOffline()          
        {
            LogWrapper.LogMessage("frmSyncManager - ShowSyncManagerOffline", "enter");
            SyncOfflineMessage();
            lblStatusL1.Text = LanguageTranslator.GetValue("AppOfflineMenu");
            label1.Text = "";

            //Adding following line for fogbugzid: 1489
            if(lastSync.ToString("MMM d, yyyy h:mm tt") == "Jan 1, 0001 12:00 AM")
                lblStatusL3.Text = "";
            else  
                lblStatusL3.Text = lblStatusL3.Text = LanguageTranslator.GetValue("SyncManagerStatusLastSyncLabel") + " " + lastSync.ToString("MMM d, yyyy h:mm tt");
            
            //btnMoveFolder.Enabled = false;
            //Commeted above line as move folder functinality disable 
            
                lblPercentDone.Text = "";
            pbSyncProgress.Visible = false;
            pbSyncProgress.Hide();
            LogWrapper.LogMessage("frmSyncManager - ShowSyncManagerOffline", "leave");
        }

        private void ShowLocalEventsCompletedMessage()
        {
            LogWrapper.LogMessage("frmSyncManager - ShowLocalEventsCompletedMessage", "enter");
            //btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncStopButtonText");
            ShowSyncMessage(EventQueue.QueueNotEmpty());
            //btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");

            if (frmIssuesFound != null && frmIssuesFound.GetItemsInList() > 0)
            {
                IssueFoundBalloonMessage();
            }
            else
            {
                if (BasicInfo.AutoSync)
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.MezeoVault;
                else
                    cnotificationManager.NotificationHandler.Icon = Properties.Resources.app_icon_disabled;
               
                SyncFolderUpToDateMessage();
            }
            LogWrapper.LogMessage("frmSyncManager - ShowLocalEventsCompletedMessage", "leave");
        }

        private void bwUpdateUsage_DoWork(object sender, DoWorkEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwUpdateUsage_DoWork", "enter");
            e.Result = GetUsageString();
            LogWrapper.LogMessage("frmSyncManager - bwUpdateUsage_DoWork", "leave");
        }

        private void bwUpdateUsage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwUpdateUsage_RunWorkerCompleted", "enter");
            lblUsageDetails.Text = e.Result.ToString();
            LogWrapper.LogMessage("frmSyncManager - bwUpdateUsage_RunWorkerCompleted", "leave");
        }

        private void bwLocalEvents_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_RunWorkerCompleted", "enter");
            if ((int)e.Result == 1)
            {
                ShowLocalEventsCompletedMessage();
                if (IsCalledByNextSyncTmr())
                {
                    SetIsCalledByNextSyncTmr(false);

                    if (!IsEventCanceled())
                        UpdateNQ();
                }
            }
            else if ((int)e.Result == USER_CANCELLED)
            {
                ShowSyncMessage(EventQueue.QueueNotEmpty());
            }
            else if ((int)e.Result == LOGIN_FAILED)
            {
                this.Hide();
                frmParent.ShowLoginAgainFromSyncMgr();
            }
            else if ((int)e.Result == SERVER_INACCESSIBLE)
            {

                 if (frmParent.checkReferenceCode() > 0)
                 {
                     ShowLocalEventsCompletedMessage();

                 }
                 else
                 {
                     DisableSyncManager();
                     ShowOfflineAtStartUpSyncManager();
                     ShowSyncManagerOffline();
                     SetIsSyncInProgress(false);
                     SyncOfflineMessage();
                     return;
                 }
                
                // CheckServerStatus(); TODO:check for offline (Modified for server status thread)
                //DisableSyncManager();
                //ShowSyncManagerOffline();
            }
            LogWrapper.LogMessage("frmSyncManager - bwLocalEvents_RunWorkerCompleted", "leave");
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
                LogWrapper.LogMessage("frmSyncManager - SendToRecycleBin", "Caught exception: " + ex.Message);   
            }
        }
    }
}
