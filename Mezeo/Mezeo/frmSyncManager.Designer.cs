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
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lnkFolderPath = new System.Windows.Forms.LinkLabel();
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblUsageDetails = new System.Windows.Forms.Label();
            this.lblUsage = new System.Windows.Forms.Label();
            this.btnIssuesFound = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPercentDone = new System.Windows.Forms.Label();
            this.pbSyncProgress = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSyncNow = new System.Windows.Forms.Button();
            this.lblStatusL3 = new System.Windows.Forms.Label();
            this.lblStatusL1 = new System.Windows.Forms.Label();
            this.imgStatus = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.pnlFileSyncOnOff.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            this.lblUserName.AutoEllipsis = true;
            this.lblUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.Location = new System.Drawing.Point(15, 76);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(189, 13);
            this.lblUserName.TabIndex = 4;
            this.lblUserName.Text = "User Name";
            // 
            // lnkServerUrl
            // 
            this.lnkServerUrl.AutoEllipsis = true;
            this.lnkServerUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkServerUrl.Location = new System.Drawing.Point(307, 72);
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
            this.lnkAbout.Location = new System.Drawing.Point(601, 343);
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
            this.lblFileSync.Location = new System.Drawing.Point(459, 22);
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
            this.pnlFileSyncOnOff.Size = new System.Drawing.Size(108, 26);
            this.pnlFileSyncOnOff.TabIndex = 1;
            // 
            // rbSyncOff
            // 
            this.rbSyncOff.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbSyncOff.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbSyncOff.Location = new System.Drawing.Point(53, 0);
            this.rbSyncOff.Name = "rbSyncOff";
            this.rbSyncOff.Size = new System.Drawing.Size(53, 26);
            this.rbSyncOff.TabIndex = 3;
            this.rbSyncOff.TabStop = true;
            this.rbSyncOff.Text = "Off";
            this.rbSyncOff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbSyncOff.UseVisualStyleBackColor = true;
            this.rbSyncOff.CheckedChanged += new System.EventHandler(this.rbSyncOff_CheckedChanged);
            // 
            // rbSyncOn
            // 
            this.rbSyncOn.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbSyncOn.Dock = System.Windows.Forms.DockStyle.Left;
            this.rbSyncOn.Location = new System.Drawing.Point(0, 0);
            this.rbSyncOn.Name = "rbSyncOn";
            this.rbSyncOn.Size = new System.Drawing.Size(53, 26);
            this.rbSyncOn.TabIndex = 1;
            this.rbSyncOn.TabStop = true;
            this.rbSyncOn.Text = "On";
            this.rbSyncOn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbSyncOn.UseVisualStyleBackColor = true;
            this.rbSyncOn.Click += new System.EventHandler(this.rbSyncOn_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::Mezeo.Properties.Resources.logo_small_light;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(224, 56);
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
            this.tmrNextSync.Enabled = true;
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
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(650, 365);
            this.shapeContainer1.TabIndex = 8;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.BorderColor = System.Drawing.SystemColors.AppWorkspace;
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = -1;
            this.lineShape1.X2 = 652;
            this.lineShape1.Y1 = 109;
            this.lineShape1.Y2 = 109;
            // 
            // lnkFolderPath
            // 
            this.lnkFolderPath.AutoEllipsis = true;
            this.lnkFolderPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkFolderPath.Location = new System.Drawing.Point(165, 141);
            this.lnkFolderPath.Name = "lnkFolderPath";
            this.lnkFolderPath.Size = new System.Drawing.Size(333, 23);
            this.lnkFolderPath.TabIndex = 11;
            this.lnkFolderPath.TabStop = true;
            this.lnkFolderPath.Text = "linkLabel1";
            this.lnkFolderPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFolderPath_LinkClicked);
            // 
            // lblFolder
            // 
            this.lblFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFolder.Location = new System.Drawing.Point(66, 141);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(75, 14);
            this.lblFolder.TabIndex = 10;
            this.lblFolder.Text = "Sync Folder";
            // 
            // lblUsageDetails
            // 
            this.lblUsageDetails.AutoSize = true;
            this.lblUsageDetails.Location = new System.Drawing.Point(165, 188);
            this.lblUsageDetails.Name = "lblUsageDetails";
            this.lblUsageDetails.Size = new System.Drawing.Size(35, 13);
            this.lblUsageDetails.TabIndex = 14;
            this.lblUsageDetails.Text = "label5";
            // 
            // lblUsage
            // 
            this.lblUsage.AutoSize = true;
            this.lblUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsage.Location = new System.Drawing.Point(98, 188);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(43, 13);
            this.lblUsage.TabIndex = 13;
            this.lblUsage.Text = "Usage";
            // 
            // btnIssuesFound
            // 
            this.btnIssuesFound.Location = new System.Drawing.Point(45, 275);
            this.btnIssuesFound.Name = "btnIssuesFound";
            this.btnIssuesFound.Size = new System.Drawing.Size(96, 23);
            this.btnIssuesFound.TabIndex = 24;
            this.btnIssuesFound.Text = "Issues Found";
            this.btnIssuesFound.UseVisualStyleBackColor = true;
            this.btnIssuesFound.Click += new System.EventHandler(this.btnIssuesFound_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(98, 233);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 13);
            this.lblStatus.TabIndex = 17;
            this.lblStatus.Text = "Status";
            // 
            // lblPercentDone
            // 
            this.lblPercentDone.AutoSize = true;
            this.lblPercentDone.Location = new System.Drawing.Point(472, 233);
            this.lblPercentDone.Name = "lblPercentDone";
            this.lblPercentDone.Size = new System.Drawing.Size(36, 13);
            this.lblPercentDone.TabIndex = 23;
            this.lblPercentDone.Text = "XXX%";
            this.lblPercentDone.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbSyncProgress
            // 
            this.pbSyncProgress.Location = new System.Drawing.Point(168, 249);
            this.pbSyncProgress.MarqueeAnimationSpeed = 50;
            this.pbSyncProgress.Name = "pbSyncProgress";
            this.pbSyncProgress.Size = new System.Drawing.Size(330, 23);
            this.pbSyncProgress.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(165, 254);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 21;
            // 
            // btnSyncNow
            // 
            this.btnSyncNow.Location = new System.Drawing.Point(538, 249);
            this.btnSyncNow.Name = "btnSyncNow";
            this.btnSyncNow.Size = new System.Drawing.Size(96, 23);
            this.btnSyncNow.TabIndex = 20;
            this.btnSyncNow.Text = "Sync Now";
            this.btnSyncNow.UseVisualStyleBackColor = true;
            this.btnSyncNow.Click += new System.EventHandler(this.btnSyncNow_Click);
            // 
            // lblStatusL3
            // 
            this.lblStatusL3.AutoEllipsis = true;
            this.lblStatusL3.Location = new System.Drawing.Point(165, 275);
            this.lblStatusL3.Name = "lblStatusL3";
            this.lblStatusL3.Size = new System.Drawing.Size(449, 13);
            this.lblStatusL3.TabIndex = 19;
            // 
            // lblStatusL1
            // 
            this.lblStatusL1.AutoEllipsis = true;
            this.lblStatusL1.Location = new System.Drawing.Point(165, 233);
            this.lblStatusL1.Name = "lblStatusL1";
            this.lblStatusL1.Size = new System.Drawing.Size(343, 13);
            this.lblStatusL1.TabIndex = 18;
            // 
            // imgStatus
            // 
            this.imgStatus.Image = global::Mezeo.Properties.Resources.ic_clock;
            this.imgStatus.Location = new System.Drawing.Point(60, 223);
            this.imgStatus.Name = "imgStatus";
            this.imgStatus.Size = new System.Drawing.Size(32, 35);
            this.imgStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.imgStatus.TabIndex = 16;
            this.imgStatus.TabStop = false;
            // 
            // frmSyncManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 365);
            this.Controls.Add(this.btnIssuesFound);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblPercentDone);
            this.Controls.Add(this.pbSyncProgress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSyncNow);
            this.Controls.Add(this.lblStatusL3);
            this.Controls.Add(this.lblStatusL1);
            this.Controls.Add(this.imgStatus);
            this.Controls.Add(this.lblUsageDetails);
            this.Controls.Add(this.lblUsage);
            this.Controls.Add(this.lnkFolderPath);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.lnkAbout);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.lnkServerUrl);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.shapeContainer1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSyncManager";
            this.Text = "frmSyncManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSyncManager_FormClosing);
            this.Load += new System.EventHandler(this.frmSyncManager_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlFileSyncOnOff.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlFileSyncOnOff;
        private System.Windows.Forms.RadioButton rbSyncOn;
        private System.Windows.Forms.Label lblFileSync;
        //private LabelEllipsis lblStatusL3;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.LinkLabel lnkServerUrl;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.LinkLabel lnkAbout;
        private System.Windows.Forms.Timer tmrSwapStatusMessage;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.Timer tmrNextSync;
        private System.ComponentModel.BackgroundWorker bwNQUpdate;
        private System.ComponentModel.BackgroundWorker bwOfflineEvent;
        private System.ComponentModel.BackgroundWorker bwLocalEvents;
        private System.ComponentModel.BackgroundWorker bwUpdateUsage;
        private System.Windows.Forms.RadioButton rbSyncOff;
       // private System.Windows.Forms.Button btnMoveFolder;
        private System.Windows.Forms.LinkLabel lnkFolderPath;
        private System.Windows.Forms.Label lblFolder;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private System.Windows.Forms.Button btnIssuesFound;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPercentDone;
        private System.Windows.Forms.ProgressBar pbSyncProgress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSyncNow;
        private System.Windows.Forms.Label lblStatusL3;
        private System.Windows.Forms.Label lblStatusL1;
        private System.Windows.Forms.PictureBox imgStatus;
        private System.Windows.Forms.Label lblUsageDetails;
        private System.Windows.Forms.Label lblUsage;
    }
}