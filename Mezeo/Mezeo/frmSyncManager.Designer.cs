namespace Mezeo
{
    partial class frmSyncManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSyncManager));
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.btnIssuesFound = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPercentDone = new System.Windows.Forms.Label();
            this.pbSyncProgress = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSyncNow = new System.Windows.Forms.Button();
            this.lblStatusL3 = new System.Windows.Forms.Label();
            this.lblStatusL1 = new System.Windows.Forms.Label();
            this.imgStatus = new System.Windows.Forms.PictureBox();
            this.pnlUsage = new System.Windows.Forms.Panel();
            this.lblUsageDetails = new System.Windows.Forms.Label();
            this.lblUsage = new System.Windows.Forms.Label();
            this.imgUsgae = new System.Windows.Forms.PictureBox();
            this.pnlFolder = new System.Windows.Forms.Panel();
            this.btnMoveFolder = new System.Windows.Forms.Button();
            this.lnkFolderPath = new System.Windows.Forms.LinkLabel();
            this.lblFolder = new System.Windows.Forms.Label();
            this.imgFolder = new System.Windows.Forms.PictureBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lnkServerUrl = new System.Windows.Forms.LinkLabel();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.lnkAbout = new System.Windows.Forms.LinkLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFileSync = new System.Windows.Forms.Label();
            this.pnlFileSyncOnOff = new System.Windows.Forms.Panel();
            this.rbSyncOff = new System.Windows.Forms.RadioButton();
            this.rbSyncOn = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tmrSwapStatusMessage = new System.Windows.Forms.Timer(this.components);
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.tmrNextSync = new System.Windows.Forms.Timer(this.components);
            this.bwNQUpdate = new System.ComponentModel.BackgroundWorker();
            this.bwOfflineEvent = new System.ComponentModel.BackgroundWorker();
            this.bwLocalEvents = new System.ComponentModel.BackgroundWorker();
            this.bwUpdateUsage = new System.ComponentModel.BackgroundWorker();
            this.pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatus)).BeginInit();
            this.pnlUsage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgUsgae)).BeginInit();
            this.pnlFolder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFolder)).BeginInit();
            this.panel1.SuspendLayout();
            this.pnlFileSyncOnOff.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.btnIssuesFound);
            this.pnlStatus.Controls.Add(this.lblStatus);
            this.pnlStatus.Controls.Add(this.lblPercentDone);
            this.pnlStatus.Controls.Add(this.pbSyncProgress);
            this.pnlStatus.Controls.Add(this.label1);
            this.pnlStatus.Controls.Add(this.btnSyncNow);
            this.pnlStatus.Controls.Add(this.lblStatusL3);
            this.pnlStatus.Controls.Add(this.lblStatusL1);
            this.pnlStatus.Controls.Add(this.imgStatus);
            this.pnlStatus.Location = new System.Drawing.Point(18, 249);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(614, 80);
            this.pnlStatus.TabIndex = 1;
            this.pnlStatus.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Paint);
            // 
            // btnIssuesFound
            // 
            this.btnIssuesFound.Location = new System.Drawing.Point(19, 55);
            this.btnIssuesFound.Name = "btnIssuesFound";
            this.btnIssuesFound.Size = new System.Drawing.Size(96, 23);
            this.btnIssuesFound.TabIndex = 15;
            this.btnIssuesFound.Text = "Issues Found";
            this.btnIssuesFound.UseVisualStyleBackColor = true;
            this.btnIssuesFound.Click += new System.EventHandler(this.btnIssuesFound_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(80, 18);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status";
            // 
            // lblPercentDone
            // 
            this.lblPercentDone.AutoSize = true;
            this.lblPercentDone.Location = new System.Drawing.Point(445, 13);
            this.lblPercentDone.Name = "lblPercentDone";
            this.lblPercentDone.Size = new System.Drawing.Size(36, 13);
            this.lblPercentDone.TabIndex = 14;
            this.lblPercentDone.Text = "XXX%";
            this.lblPercentDone.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbSyncProgress
            // 
            this.pbSyncProgress.Location = new System.Drawing.Point(150, 29);
            this.pbSyncProgress.MarqueeAnimationSpeed = 50;
            this.pbSyncProgress.Name = "pbSyncProgress";
            this.pbSyncProgress.Size = new System.Drawing.Size(330, 23);
            this.pbSyncProgress.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 10;
            // 
            // btnSyncNow
            // 
            this.btnSyncNow.Location = new System.Drawing.Point(500, 29);
            this.btnSyncNow.Name = "btnSyncNow";
            this.btnSyncNow.Size = new System.Drawing.Size(96, 23);
            this.btnSyncNow.TabIndex = 8;
            this.btnSyncNow.Text = "Sync Now";
            this.btnSyncNow.UseVisualStyleBackColor = true;
            this.btnSyncNow.Click += new System.EventHandler(this.btnSyncNow_Click);
            // 
            // lblStatusL3
            // 
            this.lblStatusL3.AutoEllipsis = true;
            this.lblStatusL3.Location = new System.Drawing.Point(147, 55);
            this.lblStatusL3.Name = "lblStatusL3";
            this.lblStatusL3.Size = new System.Drawing.Size(449, 13);
            this.lblStatusL3.TabIndex = 7;
            this.lblStatusL3.Text = "label8";
            // 
            // lblStatusL1
            // 
            this.lblStatusL1.AutoEllipsis = true;
            this.lblStatusL1.Location = new System.Drawing.Point(147, 13);
            this.lblStatusL1.Name = "lblStatusL1";
            this.lblStatusL1.Size = new System.Drawing.Size(287, 13);
            this.lblStatusL1.TabIndex = 5;
            this.lblStatusL1.Text = "label6";
            // 
            // imgStatus
            // 
            this.imgStatus.Image = global::Mezeo.Properties.Resources.ic_clock;
            this.imgStatus.Location = new System.Drawing.Point(19, 9);
            this.imgStatus.Name = "imgStatus";
            this.imgStatus.Size = new System.Drawing.Size(41, 44);
            this.imgStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgStatus.TabIndex = 2;
            this.imgStatus.TabStop = false;
            // 
            // pnlUsage
            // 
            this.pnlUsage.Controls.Add(this.lblUsageDetails);
            this.pnlUsage.Controls.Add(this.lblUsage);
            this.pnlUsage.Controls.Add(this.imgUsgae);
            this.pnlUsage.Location = new System.Drawing.Point(18, 194);
            this.pnlUsage.Name = "pnlUsage";
            this.pnlUsage.Size = new System.Drawing.Size(614, 56);
            this.pnlUsage.TabIndex = 2;
            this.pnlUsage.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Paint);
            // 
            // lblUsageDetails
            // 
            this.lblUsageDetails.AutoSize = true;
            this.lblUsageDetails.Location = new System.Drawing.Point(147, 22);
            this.lblUsageDetails.Name = "lblUsageDetails";
            this.lblUsageDetails.Size = new System.Drawing.Size(35, 13);
            this.lblUsageDetails.TabIndex = 3;
            this.lblUsageDetails.Text = "label5";
            // 
            // lblUsage
            // 
            this.lblUsage.AutoSize = true;
            this.lblUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsage.Location = new System.Drawing.Point(80, 22);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(43, 13);
            this.lblUsage.TabIndex = 2;
            this.lblUsage.Text = "Usage";
            // 
            // imgUsgae
            // 
            this.imgUsgae.Image = global::Mezeo.Properties.Resources.ic_usage;
            this.imgUsgae.Location = new System.Drawing.Point(19, 11);
            this.imgUsgae.Name = "imgUsgae";
            this.imgUsgae.Size = new System.Drawing.Size(41, 34);
            this.imgUsgae.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgUsgae.TabIndex = 1;
            this.imgUsgae.TabStop = false;
            // 
            // pnlFolder
            // 
            this.pnlFolder.Controls.Add(this.btnMoveFolder);
            this.pnlFolder.Controls.Add(this.lnkFolderPath);
            this.pnlFolder.Controls.Add(this.lblFolder);
            this.pnlFolder.Controls.Add(this.imgFolder);
            this.pnlFolder.Location = new System.Drawing.Point(18, 139);
            this.pnlFolder.Name = "pnlFolder";
            this.pnlFolder.Size = new System.Drawing.Size(614, 56);
            this.pnlFolder.TabIndex = 3;
            this.pnlFolder.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Paint);
            // 
            // btnMoveFolder
            // 
            this.btnMoveFolder.Location = new System.Drawing.Point(500, 17);
            this.btnMoveFolder.Name = "btnMoveFolder";
            this.btnMoveFolder.Size = new System.Drawing.Size(96, 23);
            this.btnMoveFolder.TabIndex = 9;
            this.btnMoveFolder.Text = "Move Folder";
            this.btnMoveFolder.UseVisualStyleBackColor = true;
            this.btnMoveFolder.Click += new System.EventHandler(this.btnMoveFolder_Click);
            // 
            // lnkFolderPath
            // 
            this.lnkFolderPath.AutoEllipsis = true;
            this.lnkFolderPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkFolderPath.Location = new System.Drawing.Point(147, 22);
            this.lnkFolderPath.Name = "lnkFolderPath";
            this.lnkFolderPath.Size = new System.Drawing.Size(333, 23);
            this.lnkFolderPath.TabIndex = 2;
            this.lnkFolderPath.TabStop = true;
            this.lnkFolderPath.Text = "linkLabel1";
            this.lnkFolderPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFolderPath_LinkClicked);
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolder.Location = new System.Drawing.Point(80, 22);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(42, 13);
            this.lblFolder.TabIndex = 1;
            this.lblFolder.Text = "Folder";
            // 
            // imgFolder
            // 
            this.imgFolder.Image = global::Mezeo.Properties.Resources.ic_logo;
            this.imgFolder.Location = new System.Drawing.Point(19, 11);
            this.imgFolder.Name = "imgFolder";
            this.imgFolder.Size = new System.Drawing.Size(41, 34);
            this.imgFolder.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgFolder.TabIndex = 0;
            this.imgFolder.TabStop = false;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoEllipsis = true;
            this.lblUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.Location = new System.Drawing.Point(98, 99);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(189, 13);
            this.lblUserName.TabIndex = 4;
            this.lblUserName.Text = "User Name";
            // 
            // lnkServerUrl
            // 
            this.lnkServerUrl.AutoEllipsis = true;
            this.lnkServerUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkServerUrl.Location = new System.Drawing.Point(303, 95);
            this.lnkServerUrl.Name = "lnkServerUrl";
            this.lnkServerUrl.Size = new System.Drawing.Size(329, 20);
            this.lnkServerUrl.TabIndex = 5;
            this.lnkServerUrl.TabStop = true;
            this.lnkServerUrl.Text = "https://41.mezeo.net";
            this.lnkServerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkServerUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkServerUrl_LinkClicked);
            // 
            // lnkHelp
            // 
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.Location = new System.Drawing.Point(15, 343);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(29, 13);
            this.lnkHelp.TabIndex = 6;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Help";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // lnkAbout
            // 
            this.lnkAbout.AutoSize = true;
            this.lnkAbout.Location = new System.Drawing.Point(597, 343);
            this.lnkAbout.Name = "lnkAbout";
            this.lnkAbout.Size = new System.Drawing.Size(35, 13);
            this.lnkAbout.TabIndex = 7;
            this.lnkAbout.TabStop = true;
            this.lnkAbout.Text = "About";
            this.lnkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::Mezeo.Properties.Resources.patch_green;
            this.panel1.Controls.Add(this.lblFileSync);
            this.panel1.Controls.Add(this.pnlFileSyncOnOff);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(650, 62);
            this.panel1.TabIndex = 0;
            // 
            // lblFileSync
            // 
            this.lblFileSync.AutoSize = true;
            this.lblFileSync.BackColor = System.Drawing.Color.Transparent;
            this.lblFileSync.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileSync.Location = new System.Drawing.Point(463, 25);
            this.lblFileSync.Name = "lblFileSync";
            this.lblFileSync.Size = new System.Drawing.Size(63, 13);
            this.lblFileSync.TabIndex = 2;
            this.lblFileSync.Text = "File Sync:";
            // 
            // pnlFileSyncOnOff
            // 
            this.pnlFileSyncOnOff.BackColor = System.Drawing.Color.Transparent;
            this.pnlFileSyncOnOff.Controls.Add(this.rbSyncOff);
            this.pnlFileSyncOnOff.Controls.Add(this.rbSyncOn);
            this.pnlFileSyncOnOff.Location = new System.Drawing.Point(528, 15);
            this.pnlFileSyncOnOff.Name = "pnlFileSyncOnOff";
            this.pnlFileSyncOnOff.Size = new System.Drawing.Size(108, 33);
            this.pnlFileSyncOnOff.TabIndex = 1;
            // 
            // rbSyncOff
            // 
            this.rbSyncOff.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbSyncOff.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbSyncOff.Location = new System.Drawing.Point(53, 0);
            this.rbSyncOff.Name = "rbSyncOff";
            this.rbSyncOff.Size = new System.Drawing.Size(53, 33);
            this.rbSyncOff.TabIndex = 2;
            this.rbSyncOff.TabStop = true;
            this.rbSyncOff.Text = "Off";
            this.rbSyncOff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbSyncOff.UseVisualStyleBackColor = true;
            this.rbSyncOff.Click += new System.EventHandler(this.rbSyncOff_CheckedChanged);
            // 
            // rbSyncOn
            // 
            this.rbSyncOn.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbSyncOn.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbSyncOn.Location = new System.Drawing.Point(0, 0);
            this.rbSyncOn.Name = "rbSyncOn";
            this.rbSyncOn.Size = new System.Drawing.Size(53, 33);
            this.rbSyncOn.TabIndex = 1;
            this.rbSyncOn.TabStop = true;
            this.rbSyncOn.Text = "On";
            this.rbSyncOn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbSyncOn.UseVisualStyleBackColor = true;
            this.rbSyncOn.CheckedChanged += new System.EventHandler(this.rbSyncOn_CheckedChanged);
            this.rbSyncOn.Click += new System.EventHandler(this.rbSyncOn_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::Mezeo.Properties.Resources.logo_small_light;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(284, 56);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // tmrSwapStatusMessage
            // 
            this.tmrSwapStatusMessage.Interval = 10000;
            this.tmrSwapStatusMessage.Tick += new System.EventHandler(this.tmrSwapStatusMessage_Tick);
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // tmrNextSync
            // 
            this.tmrNextSync.Interval = 300000;
            this.tmrNextSync.Tick += new System.EventHandler(this.tmrNextSync_Tick);
            // 
            // bwNQUpdate
            // 
            this.bwNQUpdate.WorkerReportsProgress = true;
            this.bwNQUpdate.WorkerSupportsCancellation = true;
            this.bwNQUpdate.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwNQUpdate_DoWork);
            this.bwNQUpdate.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwNQUpdate_ProgressChanged);
            this.bwNQUpdate.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwNQUpdate_RunWorkerCompleted);
            // 
            // bwOfflineEvent
            // 
            this.bwOfflineEvent.WorkerReportsProgress = true;
            this.bwOfflineEvent.WorkerSupportsCancellation = true;
            this.bwOfflineEvent.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwOffilneEvent_DoWork);
            this.bwOfflineEvent.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwLocalEvents_ProgressChanged);
            this.bwOfflineEvent.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwOffilneEvent_RunWorkerCompleted);
            // 
            // bwLocalEvents
            // 
            this.bwLocalEvents.WorkerReportsProgress = true;
            this.bwLocalEvents.WorkerSupportsCancellation = true;
            this.bwLocalEvents.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwLocalEvents_DoWork);
            this.bwLocalEvents.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwLocalEvents_ProgressChanged);
            this.bwLocalEvents.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwLocalEvents_RunWorkerCompleted);
            // 
            // bwUpdateUsage
            // 
            this.bwUpdateUsage.WorkerReportsProgress = true;
            this.bwUpdateUsage.WorkerSupportsCancellation = true;
            this.bwUpdateUsage.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwUpdateUsage_DoWork);
            this.bwUpdateUsage.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwUpdateUsage_RunWorkerCompleted);
            // 
            // frmSyncManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 365);
            this.Controls.Add(this.lnkAbout);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.lnkServerUrl);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.pnlFolder);
            this.Controls.Add(this.pnlUsage);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSyncManager";
            this.Text = "frmSyncManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSyncManager_FormClosing);
            this.Load += new System.EventHandler(this.frmSyncManager_Load);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatus)).EndInit();
            this.pnlUsage.ResumeLayout(false);
            this.pnlUsage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgUsgae)).EndInit();
            this.pnlFolder.ResumeLayout(false);
            this.pnlFolder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgFolder)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlFileSyncOnOff.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlFileSyncOnOff;
        private System.Windows.Forms.RadioButton rbSyncOff;
        private System.Windows.Forms.RadioButton rbSyncOn;
        private System.Windows.Forms.Label lblFileSync;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Panel pnlUsage;
        private System.Windows.Forms.Panel pnlFolder;
        private System.Windows.Forms.PictureBox imgStatus;
        private System.Windows.Forms.PictureBox imgUsgae;
        private System.Windows.Forms.PictureBox imgFolder;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblUsage;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Button btnSyncNow;
        private System.Windows.Forms.Label lblStatusL3;
        private System.Windows.Forms.Label lblStatusL1;
        private System.Windows.Forms.Label lblUsageDetails;
        private System.Windows.Forms.Button btnMoveFolder;
        private System.Windows.Forms.LinkLabel lnkFolderPath;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.LinkLabel lnkServerUrl;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.LinkLabel lnkAbout;
        private System.Windows.Forms.Timer tmrSwapStatusMessage;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer tmrNextSync;
        private System.Windows.Forms.Label lblPercentDone;
        private System.Windows.Forms.ProgressBar pbSyncProgress;
        private System.ComponentModel.BackgroundWorker bwNQUpdate;
        private System.Windows.Forms.Button btnIssuesFound;
        private System.ComponentModel.BackgroundWorker bwOfflineEvent;
        private System.ComponentModel.BackgroundWorker bwLocalEvents;
        private System.ComponentModel.BackgroundWorker bwUpdateUsage;
    }
}