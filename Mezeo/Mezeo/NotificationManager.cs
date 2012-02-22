using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Mezeo
{
    public class NotificationManager
    {
        private const int BALLON_TIME_OUT = 100; // time in milli seconds

        private NotifyIcon cNotifyIcon;
        private string hoverText = "";
        private string ballonText = "";
        private string ballonTitleText = "";
        private Icon trayIcon = Properties.Resources.MezeoVault;

        public NotifyIcon NotificationHandler
        {
            get
            {
                return cNotifyIcon;
            }

            set
            {
                cNotifyIcon = value;
                cNotifyIcon.Text = hoverText;
                cNotifyIcon.BalloonTipText = ballonText;
                cNotifyIcon.BalloonTipTitle = ballonTitleText;
                cNotifyIcon.BalloonTipIcon = ToolTipIcon.None;
                cNotifyIcon.Icon = trayIcon;
            }
        }

        public string HoverText
        {
            get
            {
                return hoverText;
            }
            set
            {
                hoverText = value;
                cNotifyIcon.Text = hoverText;
            }
        }

        public string BallonText
        {
            get
            {
                return ballonText;
            }
            set
            {
                ballonText = value;
                cNotifyIcon.BalloonTipText = ballonText;
            }
        }

        public string BallonTipTitle
        {
            get
            {
                return ballonTitleText;
            }
            set
            {
                ballonTitleText = value;
                cNotifyIcon.BalloonTipTitle = ballonTitleText;
            }
        }

        public Icon NotifyIcon
        {
            get
            {
                return trayIcon;
            }
            set
            {
                trayIcon = value;
                cNotifyIcon.Icon = trayIcon;
            }
        }

        public void ShowBallon()
        {
            cNotifyIcon.ShowBalloonTip(BALLON_TIME_OUT);
        }
    }
}
