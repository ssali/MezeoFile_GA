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

        public frmLogin()
        {
            InitializeComponent();

            this.Icon = Properties.Resources.MezeoVault;
            
            notificationManager = new NotificationManager();
            notificationManager.NotificationHandler = this.niSystemTray;

            niSystemTray.ContextMenuStrip = cmSystemTrayLogin;
            //Debugger.ShowLogger();
            
            mezeoFileCloud = new MezeoFileSupport.MezeoFileCloud();

            LoadResources();
        }

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
                this.ShowInTaskbar = false;
            }
            else
            {
                this.ShowInTaskbar = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (syncManager != null && syncManager.isSyncInProgress)
            {
                DialogResult dResult = MessageBox.Show("A sync is currently in progress.\n Cancel sync and close Mezeo File Sync now?", "Mezeo File Sync", MessageBoxButtons.OKCancel);
                if (dResult == DialogResult.Cancel)
                    return;

                niSystemTray.Visible = false;
                syncManager.ApplicationExit();
            }
            else
            {
                niSystemTray.Visible = false;
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
            //if (BasicInfo.IsConnectedToInternet)
            {
                CheckForIllegalCrossThreadCalls = false;

                this.UseWaitCursor = true;

                this.txtUserName.Enabled = false;
                this.txtPasswrod.Enabled = false;
                this.txtServerUrl.Enabled = false;
                this.btnLogin.Enabled = false;

                //if (BasicInfo.IsConnectedToInternet)
                bwLogin.RunWorkerAsync();
            }
        }

        private string validateServiceUrl(string url)
        {
            if(!url.Substring(0,8).Equals("https://"))
	        {
                url = "https://" + url;
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
            //if (!(BasicInfo.IsCredentialsAvailable && BasicInfo.IsConnectedToInternet))
            //{
            //    showLogin = true;
            //}
        }

        private void bwLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            int referenceCode = 0;
            loginDetails = mezeoFileCloud.Login(txtUserName.Text, txtPasswrod.Text,validateServiceUrl(txtServerUrl.Text), ref referenceCode);
           
        }

        private void bwLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.UseWaitCursor = false;
            if (loginDetails == null)
            {
                ShowLoginError();
                
            }
            else
            {

                BasicInfo.UserName = txtUserName.Text;
                BasicInfo.Password = txtPasswrod.Text;
                BasicInfo.ServiceUrl = txtServerUrl.Text;
                
                isLoginSuccess = true;
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

            niSystemTray.ContextMenuStrip = cmSystemTraySyncMgr;
            CheckAndCreateSyncDirectory();
            syncManager = new frmSyncManager(mezeoFileCloud, loginDetails, notificationManager);
            syncManager.CreateControl();

            if (isLoginSuccess)
            {
                syncManager.InitializeSync();
            }
            else if (!BasicInfo.IsConnectedToInternet)
            {
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

        private void CheckAndCreateSyncDirectory()
        {
            DbHandler dbHandler = new DbHandler();
            bool isDbCreateNew = dbHandler.OpenConnection();

            bool isDirectoryExists = false;
            if (BasicInfo.SyncDirPath.Trim().Length != 0)
                isDirectoryExists = System.IO.Directory.Exists(BasicInfo.SyncDirPath);

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

                mezeoFileCloud.GetOverlayRegisteration();
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
                    if (loginDetails == null)
                    {
                        int referenceCode = 0;
                        loginDetails = mezeoFileCloud.Login(BasicInfo.UserName, BasicInfo.Password, validateServiceUrl(BasicInfo.ServiceUrl), ref referenceCode);
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
                        notificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOnlineText"), ToolTipIcon.None);
                        
                        notificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOnlineText");
                        notificationManager.NotifyIcon = Properties.Resources.MezeoVault;

                        syncManager.LoginDetail = loginDetails;
                        syncManager.EnableSyncManager();
                        syncManager.InitializeSync();
                    }
                }
                else
                {
                    notificationManager.NotificationHandler.ShowBalloonTip(5, LanguageTranslator.GetValue("TrayBalloonSyncStatusText"),
                                                                                LanguageTranslator.GetValue("TrayAppOfflineText"), ToolTipIcon.None);

                    notificationManager.HoverText = LanguageTranslator.GetValue("TrayAppOfflineText");
                    notificationManager.NotifyIcon = Properties.Resources.app_offline;

                    syncManager.DisableSyncManager();
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
                txtServerUrl.Text = "https://demo.mezeo.net";
            }
        }

    }
}
