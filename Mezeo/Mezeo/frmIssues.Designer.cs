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
            this.label1 = new System.Windows.Forms.Label();
            this.lvIssues = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblUpdateStatus = new System.Windows.Forms.Label();
            this.btnIgnoreConflict = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
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
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(504, 66);
            this.panel1.TabIndex = 0;
            // 
            // btnEventViewer
            // 
            this.btnEventViewer.Location = new System.Drawing.Point(411, 22);
            this.btnEventViewer.Name = "btnEventViewer";
            this.btnEventViewer.Size = new System.Drawing.Size(86, 23);
            this.btnEventViewer.TabIndex = 1;
            this.btnEventViewer.Text = "Event Viewer";
            this.btnEventViewer.UseVisualStyleBackColor = true;
            this.btnEventViewer.Click += new System.EventHandler(this.btnEventViewer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Calibri", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Sync Issues";
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 399);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Local file:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 415);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Modified:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 431);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Size:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 495);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Size:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 479);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Modified:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 463);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "File info";
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
            // 
            // lnkLocalFile
            // 
            this.lnkLocalFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkLocalFile.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkLocalFile.Location = new System.Drawing.Point(128, 399);
            this.lnkLocalFile.Name = "lnkLocalFile";
            this.lnkLocalFile.Size = new System.Drawing.Size(369, 13);
            this.lnkLocalFile.TabIndex = 15;
            this.lnkLocalFile.TabStop = true;
            this.lnkLocalFile.Text = "C:\\users\\mezeo file sync\\abc.txt";
            this.lnkLocalFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lnkFileInfo
            // 
            this.lnkFileInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkFileInfo.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkFileInfo.Location = new System.Drawing.Point(128, 463);
            this.lnkFileInfo.Name = "lnkFileInfo";
            this.lnkFileInfo.Size = new System.Drawing.Size(369, 13);
            this.lnkFileInfo.TabIndex = 16;
            this.lnkFileInfo.TabStop = true;
            this.lnkFileInfo.Text = "/sales/presentation/abc.txt";
            this.lnkFileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnIgnoreConflict);
            this.Controls.Add(this.lblUpdateStatus);
            this.Controls.Add(this.lvIssues);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Name = "frmIssues";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MezeoVault Sync Issues";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmIssues_FormClosing);
            this.Shown += new System.EventHandler(this.frmIssues_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEventViewer;
        private System.Windows.Forms.ListView lvIssues;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label lblUpdateStatus;
        private System.Windows.Forms.Button btnIgnoreConflict;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
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