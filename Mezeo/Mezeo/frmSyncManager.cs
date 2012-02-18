using System;
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

namespace Mezeo
{
    public partial class frmSyncManager : Form
    {
        private MezeoFileCloud cMezeoFileCloud;
        private LoginDetails cLoginDetails;



        public frmSyncManager()
        {
            InitializeComponent();
            LoadResources();
        }

        public frmSyncManager(MezeoFileCloud mezeoFileCloud, LoginDetails loginDetails)
        {
            InitializeComponent();
            LoadResources();

            cMezeoFileCloud = mezeoFileCloud;
            cLoginDetails = loginDetails;
        }

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
            Panel pnl = sender as Panel;
            Rectangle bounds = new Rectangle(0, 0, pnl.Width, pnl.Height);
            Pen pen = new Pen(Color.FromArgb(206,207,188));

            DrawRoundedRectangle(e.Graphics, bounds, 10, pen, Color.Transparent);
        }
                
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void LoadResources()
        {
            this.Text = LanguageTranslator.GetValue("SyncManagerTitle");
            this.lblFileSync.Text = LanguageTranslator.GetValue("SyncManagerFileSyncLabel");
            this.rbSyncOff.Text = LanguageTranslator.GetValue("SyncManagerOffButtonText");
            this.rbSyncOn.Text = LanguageTranslator.GetValue("SyncManagerOnButtonText");
            this.lblFolder.Text = LanguageTranslator.GetValue("SyncManagerFolderLabel");
            this.lblStatus.Text = LanguageTranslator.GetValue("SyncManagerStatusLabel");
            this.lblUsage.Text = LanguageTranslator.GetValue("SyncManagerUsageLabel");

            this.btnMoveFolder.Text = LanguageTranslator.GetValue("SyncManagerMoveFolderButtonText");
            this.btnSyncNow.Text = LanguageTranslator.GetValue("SyncManagerSyncNowButtonText");

            this.lnkAbout.Text = LanguageTranslator.GetValue("SyncManagerAboutLinkText");
            this.lnkHelp.Text = LanguageTranslator.GetValue("SyncManagerHelpLinkText");

            this.lblUserName.Text = BasicInfo.UserName;
            this.lnkServerUrl.Text = BasicInfo.ServiceUrl;
            this.lnkFolderPath.Text = BasicInfo.SyncDirPath;
        }

        

        private void btnSyncNow_Click(object sender, EventArgs e)
        {
            SyncNow();
        }

        public void SyncNow()
        {
            Queue<LocalItemDetails> queue = new Queue<LocalItemDetails>();
            Object lockObject = new Object();

            StructureDownloader stDownloader = new StructureDownloader(queue, lockObject, cLoginDetails.szContainerContentsUrl, cMezeoFileCloud);
            //stDownloader.startAnalyseItemDetails();

            FileDownloader fileDownloder = new FileDownloader(queue, lockObject, cMezeoFileCloud);
            //fileDownloder.consume();

            Thread analyseThread = new Thread(stDownloader.startAnalyseItemDetails);
            Thread downloadingThread = new Thread(fileDownloder.consume);

            analyseThread.Start();
            downloadingThread.Start();
        }
    }
}
