namespace Mezeo
{
    partial class frmIssues
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnEventViewer = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lvIssues = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.btnIgnoreConflict = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblLocalFileTitle = new System.Windows.Forms.Label();
            this.lblLocalModifiedTitle = new System.Windows.Forms.Label();
            this.lblLocalSizeTitle = new System.Windows.Forms.Label();
            this.lblServerSizeTitle = new System.Windows.Forms.Label();
            this.lblServerModifiedTitle = new System.Windows.Forms.Label();
            this.lblFileInfoTitle = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.lnkAbout = new System.Windows.Forms.LinkLabel();
            this.lnkLocalFile = new System.Windows.Forms.LinkLabel();
            this.lnkFileInfo = new System.Windows.Forms.LinkLabel();
            this.lblLocalModifiedDate = new System.Windows.Forms.Label();
            this.lblLocalFileSize = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblModified = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::Mezeo.Properties.Resources.patch_yellow1;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panel1.Controls.Add(this.btnEventViewer);
            this.panel1.Controls.Add(this.lblHeader);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(504, 66);
            this.panel1.TabIndex = 0;
            // 
            // btnEventViewer
            // 
            this.btnEventViewer.Enabled = false;
            this.btnEventViewer.Location = new System.Drawing.Point(411, 22);
            this.btnEventViewer.Name = "btnEventViewer";
            this.btnEventViewer.Size = new System.Drawing.Size(86, 23);
            this.btnEventViewer.TabIndex = 1;
            this.btnEventViewer.Text = "Event Viewer";
            this.btnEventViewer.UseVisualStyleBackColor = true;
            this.btnEventViewer.Visible = false;
            this.btnEventViewer.Click += new System.EventHandler(this.btnEventViewer_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.BackColor = System.Drawing.Color.Transparent;
            this.lblHeader.Font = new System.Drawing.Font("Calibri", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(3, 21);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(136, 24);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "File Conflicts";
            // 
            // lvIssues
            // 
            this.lvIssues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvIssues.FullRowSelect = true;
            this.lvIssues.GridLines = true;
            this.lvIssues.Location = new System.Drawing.Point(7, 72);
            this.lvIssues.MultiSelect = false;
            this.lvIssues.Name = "lvIssues";
            this.lvIssues.Size = new System.Drawing.Size(490, 178);
            this.lvIssues.TabIndex = 1;
            this.lvIssues.UseCompatibleStateImageBehavior = false;
            this.lvIssues.View = System.Windows.Forms.View.Details;
            this.lvIssues.SelectedIndexChanged += new System.EventHandler(this.lvIssues_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 268;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Issue";
            this.columnHeader2.Width = 115;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Date and Time";
            this.columnHeader3.Width = 95;
            // 
            // lblUpdateStatus
            // 
            this.lblUpdateStatus.AutoSize = true;
            this.lblUpdateStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateStatus.Location = new System.Drawing.Point(4, 262);
            this.lblUpdateStatus.Name = "lblUpdateStatus";
            this.lblUpdateStatus.Size = new System.Drawing.Size(86, 13);
            this.lblUpdateStatus.TabIndex = 2;
            this.lblUpdateStatus.Text = "Update Failed";
            // 
            // btnIgnoreConflict
            // 
            this.btnIgnoreConflict.Location = new System.Drawing.Point(411, 257);
            this.btnIgnoreConflict.Name = "btnIgnoreConflict";
            this.btnIgnoreConflict.Size = new System.Drawing.Size(86, 23);
            this.btnIgnoreConflict.TabIndex = 3;
            this.btnIgnoreConflict.Text = "Ignore Conflict";
            this.btnIgnoreConflict.UseVisualStyleBackColor = true;
            this.btnIgnoreConflict.Click += new System.EventHandler(this.btnIgnoreConflict_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(7, 283);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(490, 102);
            this.lblDescription.TabIndex = 4;
            this.lblDescription.Text = "This is a descriptive text of the event";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.BackgroundImage = global::Mezeo.Properties.Resources.horizontal_seperator;
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.Location = new System.Drawing.Point(7, 388);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(490, 2);
            this.panel2.TabIndex = 5;
            // 
            // lblLocalFileTitle
            // 
            this.lblLocalFileTitle.AutoSize = true;
            this.lblLocalFileTitle.Location = new System.Drawing.Point(7, 399);
            this.lblLocalFileTitle.Name = "lblLocalFileTitle";
            this.lblLocalFileTitle.Size = new System.Drawing.Size(55, 13);
            this.lblLocalFileTitle.TabIndex = 6;
            this.lblLocalFileTitle.Text = "Local File:";
            // 
            // lblLocalModifiedTitle
            // 
            this.lblLocalModifiedTitle.AutoSize = true;
            this.lblLocalModifiedTitle.Location = new System.Drawing.Point(7, 415);
            this.lblLocalModifiedTitle.Name = "lblLocalModifiedTitle";
            this.lblLocalModifiedTitle.Size = new System.Drawing.Size(50, 13);
            this.lblLocalModifiedTitle.TabIndex = 7;
            this.lblLocalModifiedTitle.Text = "Modified:";
            // 
            // lblLocalSizeTitle
            // 
            this.lblLocalSizeTitle.AutoSize = true;
            this.lblLocalSizeTitle.Location = new System.Drawing.Point(7, 431);
            this.lblLocalSizeTitle.Name = "lblLocalSizeTitle";
            this.lblLocalSizeTitle.Size = new System.Drawing.Size(30, 13);
            this.lblLocalSizeTitle.TabIndex = 8;
            this.lblLocalSizeTitle.Text = "Size:";
            // 
            // lblServerSizeTitle
            // 
            this.lblServerSizeTitle.AutoSize = true;
            this.lblServerSizeTitle.Location = new System.Drawing.Point(7, 495);
            this.lblServerSizeTitle.Name = "lblServerSizeTitle";
            this.lblServerSizeTitle.Size = new System.Drawing.Size(30, 13);
            this.lblServerSizeTitle.TabIndex = 11;
            this.lblServerSizeTitle.Text = "Size:";
            // 
            // lblServerModifiedTitle
            // 
            this.lblServerModifiedTitle.AutoSize = true;
            this.lblServerModifiedTitle.Location = new System.Drawing.Point(7, 479);
            this.lblServerModifiedTitle.Name = "lblServerModifiedTitle";
            this.lblServerModifiedTitle.Size = new System.Drawing.Size(50, 13);
            this.lblServerModifiedTitle.TabIndex = 10;
            this.lblServerModifiedTitle.Text = "Modified:";
            // 
            // lblFileInfoTitle
            // 
            this.lblFileInfoTitle.AutoSize = true;
            this.lblFileInfoTitle.Location = new System.Drawing.Point(7, 463);
            this.lblFileInfoTitle.Name = "lblFileInfoTitle";
            this.lblFileInfoTitle.Size = new System.Drawing.Size(81, 13);
            this.lblFileInfoTitle.TabIndex = 9;
            this.lblFileInfoTitle.Text = "Server File Info:";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Transparent;
            this.panel3.BackgroundImage = global::Mezeo.Properties.Resources.horizontal_seperator;
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.Location = new System.Drawing.Point(7, 520);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(490, 2);
            this.panel3.TabIndex = 12;
            // 
            // lnkHelp
            // 
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkHelp.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkHelp.Location = new System.Drawing.Point(7, 532);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(29, 13);
            this.lnkHelp.TabIndex = 13;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Help";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // lnkAbout
            // 
            this.lnkAbout.AutoSize = true;
            this.lnkAbout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkAbout.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkAbout.Location = new System.Drawing.Point(462, 532);
            this.lnkAbout.Name = "lnkAbout";
            this.lnkAbout.Size = new System.Drawing.Size(35, 13);
            this.lnkAbout.TabIndex = 14;
            this.lnkAbout.TabStop = true;
            this.lnkAbout.Text = "About";
            this.lnkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAbout_LinkClicked);
            // 
            // lnkLocalFile
            // 
            this.lnkLocalFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkLocalFile.Location = new System.Drawing.Point(128, 399);
            this.lnkLocalFile.Name = "lnkLocalFile";
            this.lnkLocalFile.Size = new System.Drawing.Size(369, 13);
            this.lnkLocalFile.TabIndex = 15;
            this.lnkLocalFile.TabStop = true;
            this.lnkLocalFile.Text = "C:\\users\\MezeoFile\\abc.txt";
            this.lnkLocalFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkLocalFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLocalFile_LinkClicked);
            // 
            // lnkFileInfo
            // 
            this.lnkFileInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkFileInfo.Location = new System.Drawing.Point(128, 463);
            this.lnkFileInfo.Name = "lnkFileInfo";
            this.lnkFileInfo.Size = new System.Drawing.Size(369, 13);
            this.lnkFileInfo.TabIndex = 16;
            this.lnkFileInfo.TabStop = true;
            this.lnkFileInfo.Text = "/sales/presentation/abc.txt";
            this.lnkFileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkFileInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFileInfo_LinkClicked);
            // 
            // lblLocalModifiedDate
            // 
            this.lblLocalModifiedDate.Location = new System.Drawing.Point(131, 415);
            this.lblLocalModifiedDate.Name = "lblLocalModifiedDate";
            this.lblLocalModifiedDate.Size = new System.Drawing.Size(366, 12);
            this.lblLocalModifiedDate.TabIndex = 17;
            this.lblLocalModifiedDate.Text = "01/11/7 4:00:02 PM";
            this.lblLocalModifiedDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLocalFileSize
            // 
            this.lblLocalFileSize.Location = new System.Drawing.Point(131, 431);
            this.lblLocalFileSize.Name = "lblLocalFileSize";
            this.lblLocalFileSize.Size = new System.Drawing.Size(366, 12);
            this.lblLocalFileSize.TabIndex = 18;
            this.lblLocalFileSize.Text = "365 KB";
            this.lblLocalFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFileSize
            // 
            this.lblFileSize.Location = new System.Drawing.Point(131, 495);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(366, 12);
            this.lblFileSize.TabIndex = 20;
            this.lblFileSize.Text = "365 KB";
            this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblModified
            // 
            this.lblModified.Location = new System.Drawing.Point(131, 479);
            this.lblModified.Name = "lblModified";
            this.lblModified.Size = new System.Drawing.Size(366, 12);
            this.lblModified.TabIndex = 19;
            this.lblModified.Text = "01/11/7 4:00:02 PM";
            this.lblModified.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frmIssues
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 557);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.lblModified);
            this.Controls.Add(this.lblLocalFileSize);
            this.Controls.Add(this.lblLocalModifiedDate);
            this.Controls.Add(this.lnkFileInfo);
            this.Controls.Add(this.lnkLocalFile);
            this.Controls.Add(this.lnkAbout);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.lblServerSizeTitle);
            this.Controls.Add(this.lblServerModifiedTitle);
            this.Controls.Add(this.lblFileInfoTitle);
            this.Controls.Add(this.lblLocalSizeTitle);
            this.Controls.Add(this.lblLocalModifiedTitle);
            this.Controls.Add(this.lblLocalFileTitle);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnIgnoreConflict);
            this.Controls.Add(this.lblUpdateStatus);
            this.Controls.Add(this.lvIssues);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmIssues";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MezeoFile";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmIssues_FormClosing);
            this.Load += new System.EventHandler(this.frmIssues_Load);
            this.Shown += new System.EventHandler(this.frmIssues_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Button btnEventViewer;
        private System.Windows.Forms.ListView lvIssues;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label lblUpdateStatus;
        private System.Windows.Forms.Button btnIgnoreConflict;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblLocalFileTitle;
        private System.Windows.Forms.Label lblLocalModifiedTitle;
        private System.Windows.Forms.Label lblLocalSizeTitle;
        private System.Windows.Forms.Label lblServerSizeTitle;
        private System.Windows.Forms.Label lblServerModifiedTitle;
        private System.Windows.Forms.Label lblFileInfoTitle;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.LinkLabel lnkAbout;
        private System.Windows.Forms.LinkLabel lnkLocalFile;
        private System.Windows.Forms.LinkLabel lnkFileInfo;
        private System.Windows.Forms.Label lblLocalModifiedDate;
        private System.Windows.Forms.Label lblLocalFileSize;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblModified;
    }
}