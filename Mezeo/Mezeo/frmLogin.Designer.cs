namespace Mezeo
{
    partial class frmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            this.btnLogin = new System.Windows.Forms.Button();
            this.cmSystemTrayLogin = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.loginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bwLogin = new System.ComponentModel.BackgroundWorker();
            this.cmSystemTraySyncMgr = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.msShowSyncMgr = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.msSyncMgrExit = new System.Windows.Forms.ToolStripMenuItem();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.labelError = new System.Windows.Forms.Label();
            this.cmLogin = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.cmSyncManager = new System.Windows.Forms.ContextMenu();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.niSystemTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.txtServerUrl = new Mezeo.CueTextBox();
            this.txtPasswrod = new Mezeo.CueTextBox();
            this.txtUserName = new Mezeo.CueTextBox();
            this.bwCheckServerStatus = new System.ComponentModel.BackgroundWorker();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.cmSystemTrayLogin.SuspendLayout();
            this.cmSystemTraySyncMgr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            resources.ApplyResources(this.btnLogin, "btnLogin");
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // cmSystemTrayLogin
            // 
            this.cmSystemTrayLogin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem6,
            this.loginToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.cmSystemTrayLogin.Name = "cmSystemTrayLogin";
            resources.ApplyResources(this.cmSystemTrayLogin, "cmSystemTrayLogin");
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Paint += new System.Windows.Forms.PaintEventHandler(this.toolStripMenuItem6_Paint);
            // 
            // loginToolStripMenuItem
            // 
            this.loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            resources.ApplyResources(this.loginToolStripMenuItem, "loginToolStripMenuItem");
            this.loginToolStripMenuItem.Click += new System.EventHandler(this.loginToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // bwLogin
            // 
            this.bwLogin.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwLogin_DoWork);
            this.bwLogin.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwLogin_RunWorkerCompleted);
            // 
            // cmSystemTraySyncMgr
            // 
            this.cmSystemTraySyncMgr.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.msShowSyncMgr,
            this.toolStripMenuItem4,
            this.toolStripSeparator2,
            this.toolStripMenuItem5,
            this.toolStripSeparator3,
            this.toolStripMenuItem3,
            this.toolStripSeparator4,
            this.msSyncMgrExit});
            this.cmSystemTraySyncMgr.Name = "cmSystemTrayLogin";
            resources.ApplyResources(this.cmSystemTraySyncMgr, "cmSystemTraySyncMgr");
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // msShowSyncMgr
            // 
            this.msShowSyncMgr.Name = "msShowSyncMgr";
            resources.ApplyResources(this.msShowSyncMgr, "msShowSyncMgr");
            this.msShowSyncMgr.Click += new System.EventHandler(this.msShowSyncMgr_Click);
            // 
            // toolStripMenuItem4
            // 
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // msSyncMgrExit
            // 
            this.msSyncMgrExit.Name = "msSyncMgrExit";
            resources.ApplyResources(this.msSyncMgrExit, "msSyncMgrExit");
            this.msSyncMgrExit.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // pbLogo
            // 
            this.pbLogo.Image = global::Mezeo.Properties.Resources.logo_medium_light;
            resources.ApplyResources(this.pbLogo, "pbLogo");
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.TabStop = false;
            // 
            // labelError
            // 
            resources.ApplyResources(this.labelError, "labelError");
            this.labelError.ForeColor = System.Drawing.Color.Red;
            this.labelError.Name = "labelError";
            // 
            // cmLogin
            // 
            this.cmLogin.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2,
            this.menuItem3});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            resources.ApplyResources(this.menuItem1, "menuItem1");
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            resources.ApplyResources(this.menuItem2, "menuItem2");
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            resources.ApplyResources(this.menuItem3, "menuItem3");
            this.menuItem3.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // cmSyncManager
            // 
            this.cmSyncManager.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuItem7,
            this.menuItem5,
            this.menuItem8,
            this.menuItem9,
            this.menuItem6});
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            resources.ApplyResources(this.menuItem4, "menuItem4");
            this.menuItem4.Click += new System.EventHandler(this.msShowSyncMgr_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 1;
            resources.ApplyResources(this.menuItem7, "menuItem7");
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 2;
            resources.ApplyResources(this.menuItem5, "menuItem5");
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 3;
            resources.ApplyResources(this.menuItem8, "menuItem8");
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 4;
            resources.ApplyResources(this.menuItem9, "menuItem9");
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 5;
            resources.ApplyResources(this.menuItem6, "menuItem6");
            this.menuItem6.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // niSystemTray
            // 
            resources.ApplyResources(this.niSystemTray, "niSystemTray");
            this.niSystemTray.MouseClick += new System.Windows.Forms.MouseEventHandler(this.niSystemTray_MouseClick);
            // 
            // txtServerUrl
            // 
            this.txtServerUrl.CueText = "";
            resources.ApplyResources(this.txtServerUrl, "txtServerUrl");
            this.txtServerUrl.Name = "txtServerUrl";
            this.txtServerUrl.TextChanged += new System.EventHandler(this.txtServerUrl_TextChanged);
            this.txtServerUrl.Leave += new System.EventHandler(this.txtServerUrl_Leave);
            // 
            // txtPasswrod
            // 
            this.txtPasswrod.CueText = "Password";
            resources.ApplyResources(this.txtPasswrod, "txtPasswrod");
            this.txtPasswrod.Name = "txtPasswrod";
            this.txtPasswrod.UseSystemPasswordChar = true;
            this.txtPasswrod.TextChanged += new System.EventHandler(this.txtPasswrod_TextChanged);
            // 
            // txtUserName
            // 
            this.txtUserName.CueText = "User Name";
            resources.ApplyResources(this.txtUserName, "txtUserName");
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.TextChanged += new System.EventHandler(this.txtUserName_TextChanged);
            // 
            // bwCheckServerStatus
            // 
            this.bwCheckServerStatus.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwCheckServerStatus_DoWork);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // frmLogin
            // 
            this.AcceptButton = this.btnLogin;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.txtServerUrl);
            this.Controls.Add(this.txtPasswrod);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.pbLogo);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmLogin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLogin_FormClosing);
            this.Load += new System.EventHandler(this.frmLogin_Load);
            this.Resize += new System.EventHandler(this.frmLogin_Resize);
            this.cmSystemTrayLogin.ResumeLayout(false);
            this.cmSystemTraySyncMgr.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Button btnLogin;
        private CueTextBox txtUserName;
        private CueTextBox txtPasswrod;
        private CueTextBox txtServerUrl;
        private System.Windows.Forms.ContextMenuStrip cmSystemTrayLogin;
        private System.Windows.Forms.ToolStripMenuItem loginToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bwLogin;
        private System.Windows.Forms.ContextMenuStrip cmSystemTraySyncMgr;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem msShowSyncMgr;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem msSyncMgrExit;
        private System.Windows.Forms.Label labelError;
        private System.Windows.Forms.ContextMenu cmLogin;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem menuItem9;
        private System.Windows.Forms.ContextMenu cmSyncManager;
        public System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.NotifyIcon niSystemTray;
        public System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.ComponentModel.BackgroundWorker bwCheckServerStatus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;

    }
}

