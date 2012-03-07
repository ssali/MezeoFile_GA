using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Mezeo
{
    public partial class BaseForm : Form
    {
        private ShellNotify shellNotify;//=new ShellNotify(IntPtr.Zero);
        static uint s_uTaskbarRestart;

        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;

        [DllImport("user32.dll", EntryPoint = "TrackPopupMenu")]
        private static extern int TrackPopupMenu(
            IntPtr hMenu,
            int wFlags,
            int x,
            int y,
            int nReserved,
            IntPtr hwnd,
            ref RECT lprc
            );

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }

        protected override void WndProc(ref Message msg)
        {
            if (shellNotify != null)
            {
                /* Listn tray notify messages only */
                if (msg.Msg == shellNotify.WM_NOTIFY_TRAY)
                {
                    if ((int)msg.WParam == shellNotify.uID)
                    {
                        MouseButtons mb = MouseButtons.None;
                        if ((int)msg.LParam == WM_LBUTTONDOWN)
                        {
                            /* left click */
                            if (shellNotify.contextMenuHwnd != IntPtr.Zero)
                            {
                                /* if connect menu exists create a rectangle and c */
                                RECT r = new RECT();
                                r.Left = Screen.PrimaryScreen.WorkingArea.Left;
                                r.Right = Screen.PrimaryScreen.WorkingArea.Right;
                                r.Top = Screen.PrimaryScreen.WorkingArea.Top;
                                r.Bottom = Screen.PrimaryScreen.WorkingArea.Right;

                                SetForegroundWindow(shellNotify.formHwnd);

                                TrackPopupMenu(
                                    shellNotify.contextMenuHwnd,
                                    2,
                                    Cursor.Position.X,
                                    Cursor.Position.Y,
                                    0,
                                    shellNotify.formHwnd,
                                    ref r
                                    );
                            }
                            else
                            {
                                mb = MouseButtons.Left;
                            }
                        }
                        else if ((int)msg.LParam == WM_MBUTTONDOWN)
                        {
                            /* middle click */
                            mb = MouseButtons.Middle;
                        }
                        else if ((int)msg.LParam == WM_RBUTTONDOWN)
                        {
                            /* right click */
                            if (shellNotify.contextMenuHwnd != IntPtr.Zero)
                            {
                                /* if connect menu exists create a rectangle and c */
                                RECT r = new RECT();
                                r.Left = Screen.PrimaryScreen.WorkingArea.Left;
                                r.Right = Screen.PrimaryScreen.WorkingArea.Right;
                                r.Top = Screen.PrimaryScreen.WorkingArea.Top;
                                r.Bottom = Screen.PrimaryScreen.WorkingArea.Right;

                                SetForegroundWindow(shellNotify.formHwnd);

                                TrackPopupMenu(
                                    shellNotify.contextMenuHwnd,
                                    2,
                                    Cursor.Position.X,
                                    Cursor.Position.Y,
                                    0,
                                    shellNotify.formHwnd,
                                    ref r
                                    );
                            }
                            else
                            {
                                // callback mousebuttons.right
                                mb = MouseButtons.Right;
                            }
                        }
                    }
                }
            }

            //switch (msg.Msg)
            //{
            //    case 0x0001:
            //        s_uTaskbarRestart = Win32Api.RegisterWindowMessage("TaskbarCreated");
            //        break;
            //    default:
            //        if (msg.Msg == s_uTaskbarRestart)
            //            ShellNotifyIcon.AddNotifyIcon();
            //        break;
            //}

           
            if (msg.Msg == s_uTaskbarRestart)
                ShellNotifyIcon.AddNotifyIcon();
           
            base.WndProc(ref msg);
        }

        public BaseForm()
        {
            s_uTaskbarRestart = Win32Api.RegisterWindowMessage("TaskbarCreated");
            InitializeComponent();
            shellNotify = new ShellNotify(this.Handle);
        }

        public ShellNotify ShellNotifyIcon
        {
            get
            {
                return this.shellNotify;
                
            }
        }
    }
}
