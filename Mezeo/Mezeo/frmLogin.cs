using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Resources;
using MezeoFileSupport;

namespace Mezeo
{
    public partial class frmLogin : Form
    {
        NotificationManager notificationManager;
        MezeoFileSupport.MezeoFileCloud mezeoFileCloud;
        MezeoFileSupport.LoginDetails loginDetails;
        frmSyncManager syncManager;
        private bool internetConnection = false;

        public bool isLoginSuccess = false;
        public bool showLogin = false;

       // static uint s_uTaskbarRestart;

        public frmLogin()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.MezeoVault;

            //this.HandleCreated += new EventHandler(frmLogin_HandleCreated);
           // this.HandleDestroyed += new EventHandler(frmLogin_HandleDestroyed);
            
            notificationManager = new NotificationManager();
            notificationManager.NotificationHandler = this.niSystemTray;

            niSystemTray.ContextMenuStrip = cmSystemTrayLogin;
            //Debugger.ShowLogger();
            
            mezeoFileCloud = new MezeoFileSupport.MezeoFileCloud();

            LoadResources();
            
        }

        //void frmLogin_HandleDestroyed(object sender, EventArgs e)
        //{
        //    MessageBox.Show("Handle Destroyed");
        //}

        //void frmLogin_HandleCreated(object sender, EventArgs e)
        //{
        //    MessageBox.Show("Handle Created " + this.Handle);
        //}

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                niSystemTray.Visible = false;
            }
        }

        private void niSystemTray_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void frmLogin_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (syncManager != null && (syncManager.isSyncInProgress || syncManager.isLocalEventInProgress || syncManager.isOfflineWorking))
            {
                DialogResult dResult = MessageBox.Show(LanguageTranslator.GetValue("MezeoExitString1") + "\n" + LanguageTranslator.GetValue("MezeoExitString2"), AboutBox.AssemblyTitle, MessageBoxButtons.OKCancel);
                if (dResult == DialogResult.Cancel)
                    return;

                niSystemTray.Visible = false;
                syncManager.ApplicationExit();
                //Application.Exit();
                Environment.Exit(Environment.ExitCode);
               // ShellNotifyIcon.RemoveNotifyIcon();
            }
            else
            {
                niSystemTray.Visible = false;
                //ShellNotifyIcon.RemoveNotifyIcon();
                Application.Exit();
            }
        }


        private void niSystemTray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(niSystemTray, null);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }


        public void Login()
        {
           // if (BasicInfo.IsConnectedToInternet)
            {
                if (BasicInfo.IsConnectedToInternet)
                {
                    CheckForIllegalCrossThreadCalls = false;

                    this.UseWaitCursor = true;

                    this.txtUserName.Enabled = false;
                    this.txtPasswrod.Enabled = false;
                    this.txtServerUrl.Enabled = false;
                    this.btnLogin.Enabled = false;
                    this.labelError.Text = "";
                }
               //if (BasicInfo.IsConnectedToInternet)
                 bwLogin.RunWorkerAsync();
            }
        }

        private string validateServiceUrl(string url)
        {
            if(!url.Substring(0,8).Equals("https://"))
	        {
                url = "https://" + url;
                txtServerUrl.Text = url;
	        }


            if (!url.Substring(url.Length-3,3).Equals("/v2"))
	        {
		        url += "/v2";
	        }

            return url;
        }

        private void LoadResources()
        {
            LanguageTranslator.SetLanguage("en");
            this.Text = LanguageTranslator.GetValue("LoginFormTitle");
            this.txtUserName.CueText = LanguageTranslator.GetValue("UserIdCueText");
            this.txtPasswrod.CueText=LanguageTranslator.GetValue("PasswordCueText");
            this.txtServerUrl.CueText = LanguageTranslator.GetValue("ServerUrlCueText");
            this.txtServerUrl.Text = LanguageTranslator.GetValue("ServerUrlCueText");
            this.labelError.Text = "";

            if (!BasicInfo.LoadRegistryValues())
            {
                showLogin = true;
            }
            else
            {
                //if (BasicInfo.UserName.Trim().Length == 0 || BasicInfo.Password.Trim().Length == 0 || BasicInfo.ServiceUrl.Trim().Length == 0)
                if (!BasicInfo.IsCredentialsAvailable)
                {
                    showLogin = true;
                }
                else
                {
                    txtUserName.Text = BasicInfo.UserName;
                    txtUserName.Enabled = false;
   
                    txtPasswrod.Text = BasicInfo.Password;
                    
                    txtServerUrl.Text = BasicInfo.ServiceUrl;
                    txtServerUrl.Enabled = false;

                    showLogin = false;
                }
            }

            internetConnection = BasicInfo.IsConnectedToInternet;

            //ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
            //ShellNotifyIcon.AddNotifyIcon();
            //ShellNotifyIcon.ConnectMyMenu(cmLogin.Handle);
            //ShellNotifyIcon.ShowNotifyIcon(Properties.Resources.MezeoVault.Handle, "", "", "");
            //if (!(BasicInfo.IsCredentialsAvailable && BasicInfo.IsConnectedToInternet))
            //{
            //    showLogin = true;
            //}
        }

        private void bwLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            int referenceCode = 0;
            loginDetails = mezeoFileCloud.Login(txtUserName.Text, txtPasswrod.Text,validateServiceUrl(txtServerUrl.Text), ref referenceCode);
            e.Result = referenceCode;
           
        }

        private void ShowSyncManagerOfffline()
        {
            BasicInfo.UserName = txtUserName.Text;
            BasicInfo.Password = txtPasswrod.Text;
            BasicInfo.ServiceUrl = txtServerUrl.Text;

            isLoginSuccess = false;

            if (showLogin)
            {
                this.Close();
            }

            mezeoFileCloud.AppEventViewer(AboutBox.AssemblyTitle, LanguageTranslator.GetValue("TrayAppOfflineText"), 3);
            niSystemTray.ContextMenuStrip = cmSystemTraySyncMgr;

            syncManager = new frmSyncManager(mezeoFileCloud, loginDetails, notificationManager);

            syncManager.CreateControl();
            syncManager.ParentForm = this;

            syncManager.DisableSyncManager();
            syncManager.ShowOfflineAtStartUpSyncManager();

            notificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOfflineText"), ToolTipIcon.None);

            notificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOfflineText");
            notificationManager.NotifyIcon = Properties.Resources.app_offline;
            toolStripMenuItem4.Text = LanguageTranslator.GetValue("AppOfflineMenu");
        }

        private void bwLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.UseWaitCursor = false;
            if (loginDetails == null)
            {
                tmrConnectionCheck.Enabled = true;
                if (!BasicInfo.IsConnectedToInternet)
                {
                    ShowSyncManagerOfffline();
                }
                else
                {
                    ShowLoginError();
                }
                return;
                
            }
            else if (loginDetails.nAccountType == 0)
            {
                ShowGuestLoginError();
                return;
            }
            else
            {
                BasicInfo.UserName = txtUserName.Text;
                BasicInfo.Password = txtPasswrod.Text;
                BasicInfo.ServiceUrl = txtServerUrl.Text;

                isLoginSuccess = true;
                //ShellNotifyIcon.ConnectMyMenu(cmSyncManager.Handle);
                //niSystemTray.ContextMenuStrip = cmSystemTraySyncMgr;
                //CheckAndCreateSyncDirectory();
                //syncManager = new frmSyncManager(mezeoFileCloud, loginDetails, notificationManager);
                ////syncManager.CreateControl();
                ////syncManager.Show();
                //syncManager.CreateControl();

                //syncManager.InitializeSync();

                //if (showLogin)
                //{
                //    this.Close();
                //}
            }
             
            if (showLogin)
            {
                this.Close();
            }
            mezeoFileCloud.AppEventViewer(AboutBox.AssemblyTitle, LanguageTranslator.GetValue("LoginSuccess"), 3);
            niSystemTray.ContextMenuStrip = cmSystemTraySyncMgr;

            CheckAndCreateSyncDirectory();
            CheckAndCreateNotificationQueue();
            syncManager = new frmSyncManager(mezeoFileCloud, loginDetails, notificationManager);
            syncManager.CreateControl();
            syncManager.ParentForm = this;

            if (isLoginSuccess)
            {
                if (BasicInfo.IsInitialSync)
                {
                    syncManager.InitializeSync();
                }
                else
                {
                    syncManager.SetUpSync();
                    syncManager.SetUpSyncNowNotification();
                    syncManager.ProcessOfflineEvents();
                }
            }
            else if (!BasicInfo.IsConnectedToInternet)
            {
                //ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.app_offline.Handle);
                notificationManager.NotifyIcon = Properties.Resources.app_offline;
                syncManager.DisableSyncManager();
            }

            tmrConnectionCheck.Enabled = true;    

        }

        private void ShowLoginError()
        {
            this.labelError.Text = LanguageTranslator.GetValue("LoginErrorText");
            this.txtUserName.Enabled = true;
            this.txtPasswrod.Enabled = true;
            this.txtServerUrl.Enabled = true;

            this.btnLogin.Enabled = true;

            isLoginSuccess = false;
        }
        private void ShowGuestLoginError()
        {
            this.labelError.Text = LanguageTranslator.GetValue("LoginGuestAccMsgText");
            this.txtUserName.Enabled = true;
            this.txtPasswrod.Enabled = true;
            this.txtServerUrl.Enabled = true;

            this.btnLogin.Enabled = true;

            isLoginSuccess = false;
        }


        private void frmLogin_Load(object sender, EventArgs e)
        {
            
        }

        public void showSyncManager()
        {
            if (syncManager != null)
                syncManager.Show();
        }

        private void msShowSyncMgr_Click(object sender, EventArgs e)
        {
            showSyncManager();
        }

        private void CheckAndCreateNotificationQueue()
        {
            int nStatusCode = 0;
            string NQParentURI = mezeoFileCloud.NQParentUri(loginDetails.szManagementUri, ref nStatusCode);

            if (NQParentURI.Trim().Length != 0)
            {
                loginDetails.szNQParentUri = NQParentURI;
                NQDetails[] pNQDetails = null;
                string queueName = BasicInfo.GetMacAddress + "-" + BasicInfo.UserName;

                int nNQLength = mezeoFileCloud.NQGetLength(BasicInfo.ServiceUrl + loginDetails.szNQParentUri, queueName, ref nStatusCode);
                if (nNQLength > 0)
                    pNQDetails = mezeoFileCloud.NQGetData(BasicInfo.ServiceUrl + NQParentURI, queueName, nNQLength, ref nStatusCode);

                if (BasicInfo.IsInitialSync && pNQDetails!= null)
                    mezeoFileCloud.NQDeleteValue(BasicInfo.ServiceUrl + loginDetails.szNQParentUri, queueName, nNQLength, ref nStatusCode);

                if (nStatusCode == 404 && pNQDetails == null)
                {
                    bool bRet = mezeoFileCloud.NQCreate(BasicInfo.ServiceUrl + NQParentURI, queueName, NQParentURI, ref nStatusCode);
                }
            }
        }

        private void CheckAndCreateSyncDirectory()
        {
            DbHandler dbHandler = new DbHandler();
            

            bool isDirectoryExists = false;
            if (BasicInfo.SyncDirPath.Trim().Length != 0)
                isDirectoryExists = System.IO.Directory.Exists(BasicInfo.SyncDirPath);

            if (!isDirectoryExists)
            {
                dbHandler.DeleteDb();
            }

            bool isDbCreateNew = dbHandler.OpenConnection();

            if (!isDirectoryExists || isDbCreateNew)
            {
                //string userName = BasicInfo.UserName;

                //int atIndex = userName.IndexOf('@');

                //if (atIndex>0)
                //{
                //    userName = userName.Substring(0, atIndex-1);
                //}

                string dirName = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\" + AboutBox.AssemblyTitle ;
                
                if( System.IO.Directory.Exists(dirName))
                {
                    DateTime time = System.IO.Directory.GetCreationTime(dirName);

                    System.IO.Directory.Move(dirName, dirName + time.ToString("M-d-yyyy-h-mm-ss"));
                }

                System.IO.Directory.CreateDirectory(dirName);
                BasicInfo.IsInitialSync = true;
                BasicInfo.SyncDirPath = dirName;

                //mezeoFileCloud.GetOverlayRegisteration();
            }            
        }

        private void txtPasswrod_TextChanged(object sender, EventArgs e)
        {

        }


        private void tmrConnectionCheck_Tick(object sender, EventArgs e)
        {
            if (BasicInfo.IsConnectedToInternet != internetConnection)
            {
                tmrConnectionCheck.Enabled = false;
                internetConnection = BasicInfo.IsConnectedToInternet;

                if (BasicInfo.IsConnectedToInternet)
                {
                    if (loginDetails == null && BasicInfo.IsCredentialsAvailable)
                    {
                        int referenceCode = 0;
                        loginDetails = mezeoFileCloud.Login(BasicInfo.UserName, BasicInfo.Password, validateServiceUrl(BasicInfo.ServiceUrl), ref referenceCode);
                        if (loginDetails != null)
                        {
                            mezeoFileCloud.AppEventViewer(AboutBox.AssemblyTitle, LanguageTranslator.GetValue("LoginSuccess"), 3);
                            CheckAndCreateSyncDirectory();
                            CheckAndCreateNotificationQueue();
                        }
                    }

                    if (loginDetails == null)
                    {
                        this.labelError.Text = LanguageTranslator.GetValue("LoginErrorText");
                        this.txtUserName.Enabled = false;
                        this.txtPasswrod.Enabled = true;
                        this.txtServerUrl.Enabled = false;

                        this.btnLogin.Enabled = true;

                        isLoginSuccess = false;
                        this.Show();
                    }
                    else
                    {
                       
                        notificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOnlineText"), ToolTipIcon.None);

                        notificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOnlineText");
                        notificationManager.NotifyIcon = Properties.Resources.MezeoVault;

                       // ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.MezeoVault.Handle);
                        //ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayAppOnlineText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
                       // ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayAppOnlineText"));

                        if (syncManager != null)
                        {
                            syncManager.LoginDetail = loginDetails;
                            syncManager.EnableSyncManager();
                            syncManager.InitializeSync();
                        }
                    }
                }
                else
                {
                    notificationManager.NotificationHandler.ShowBalloonTip(1, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOfflineText"), ToolTipIcon.None);

                    notificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOfflineText");
                    notificationManager.NotifyIcon = Properties.Resources.app_offline;
                    toolStripMenuItem4.Text = LanguageTranslator.GetValue("AppOfflineMenu");
                    //ShellNotifyIcon.SetNotifyIconHandle(Properties.Resources.app_offline.Handle);
                    //ShellNotifyIcon.SetNotifyIconBalloonText(LanguageTranslator.GetValue("TrayAppOfflineText"), LanguageTranslator.GetValue("TrayBalloonSyncStatusText"));
                    //ShellNotifyIcon.SetNotifyIconToolTip(LanguageTranslator.GetValue("TrayAppOfflineText"));

                    if (syncManager != null)
                    {
                        syncManager.DisableSyncManager();
                    }
                }
                tmrConnectionCheck.Enabled = true;
            }
        }

        private void txtUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtServerUrl_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtServerUrl_Leave(object sender, EventArgs e)
        {
            if (txtServerUrl.Text.Trim().Length == 0)
            {
                txtServerUrl.Text = LanguageTranslator.GetValue("ServerUrlCueText"); ;
            }
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Paint(object sender, PaintEventArgs e)
        {
            //Point pt = new Point(0, 0);
            Rectangle rect=new Rectangle();
            rect.X=0;
            rect.Y=0;
            rect.Width=((ToolStripMenuItem)sender).Width;
            rect.Height=((ToolStripMenuItem)sender).Height;

            Image img = Properties.Resources.mezeo_menu_logo_large;
            //e.Graphics.DrawImage(img, pt);
            GraphicsUnit gu = new GraphicsUnit();
            e.Graphics.DrawImage(img, rect, img.GetBounds(ref gu), gu);
            //e.Graphics.DrawImageUnscaled(img, rect);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BasicInfo.ServiceUrl);
        }

        private void toolStripMenuItem6_Paint(object sender, PaintEventArgs e)
        {
            //Point pt = new Point(0, 0);
            Rectangle rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = ((ToolStripMenuItem)sender).Width;
            rect.Height = ((ToolStripMenuItem)sender).Height;

            Image img = Properties.Resources.meze_menu_logo_small;
            //e.Graphics.DrawImage(img, pt);
            GraphicsUnit gu = new GraphicsUnit();
            e.Graphics.DrawImage(img, rect, img.GetBounds(ref gu), gu);
            //e.Graphics.DrawImageUnscaled(img, rect);
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                
            }
        }

        //protected override void WndProc(ref Message msg)
        //{
        //    switch (msg.Msg)
        //    {
        //        case 0x0001:
        //            s_uTaskbarRestart = Win32Api.RegisterWindowMessage("TaskbarCreated");
        //            break;
        //        default:
        //            if (msg.Msg == s_uTaskbarRestart)
        //                ShellNotifyIcon.AddNotifyIcon();
        //            break;
        //    }
        //}

    }
}
