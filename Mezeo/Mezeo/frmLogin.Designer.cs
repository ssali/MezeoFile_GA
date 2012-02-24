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
            this.niSystemTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmSystemTrayLogin = new System.Windows.Forms.ContextMenuStrip(this.components);
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

            this.txtServerUrl = new Mezeo.CueTextBox();
            this.txtPasswrod = new Mezeo.CueTextBox();
            this.txtUserName = new Mezeo.CueTextBox();
            this.tmrConnectionCheck = new System.Windows.Forms.Timer(this.components);
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
            // niSystemTray
            // 
            resources.ApplyResources(this.niSystemTray, "niSystemTray");
            this.niSystemTray.DoubleClick += new System.EventHandler(this.niSystemTray_DoubleClick);
            this.niSystemTray.MouseClick += new System.Windows.Forms.MouseEventHandler(this.niSystemTray_MouseClick);
            // 
            // cmSystemTrayLogin
            // 
            this.cmSystemTrayLogin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loginToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.cmSystemTrayLogin.Name = "cmSystemTrayLogin";
            resources.ApplyResources(this.cmSystemTrayLogin, "cmSystemTrayLogin");
            // 
            // loginToolStripMenuItem
            // 
            this.loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            resources.ApplyResources(this.loginToolStripMenuItem, "loginToolStripMenuItem");
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
            this.msSyncMgrExit});
            this.cmSystemTraySyncMgr.Name = "cmSystemTrayLogin";
            resources.ApplyResources(this.cmSystemTraySyncMgr, "cmSystemTraySyncMgr");
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripMenuItem2.BackgroundImage = global::Mezeo.Properties.Resources.logo_horizontal_right_click;
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
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

            // txtServerUrl
            // 
            this.txtServerUrl.CueText = "https://demo.mezeo.net";
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
            // tmrConnectionCheck
            // 
            this.tmrConnectionCheck.Interval = 5000;
            this.tmrConnectionCheck.Tick += new System.EventHandler(this.tmrConnectionCheck_Tick);
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
        private System.Windows.Forms.NotifyIcon niSystemTray;
        private System.Windows.Forms.ContextMenuStrip cmSystemTrayLogin;
        private System.Windows.Forms.ToolStripMenuItem loginToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bwLogin;
        private System.Windows.Forms.ContextMenuStrip cmSystemTraySyncMgr;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem msShowSyncMgr;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem msSyncMgrExit;
        private System.Windows.Forms.Timer tmrConnectionCheck;
        private System.Windows.Forms.Label labelError;

    }
}

