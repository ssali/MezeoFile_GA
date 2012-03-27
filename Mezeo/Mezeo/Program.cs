using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mezeo
{
    static class Program
    {
        static Mutex mutex;

        //[DllImport("user32.dll")]
        //private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        //[DllImport("user32.dll")]
        //private static extern int SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern int IsIconic(IntPtr hWnd);

        //const int SW_RESTORE = 3;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {//// {D19F100E-113F-4751-820C-FD5AF8D17A55}

            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name; 
            bool bCreatedNew;

            mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
            if (bCreatedNew)
                mutex.ReleaseMutex();
            else
            {
                BasicInfo.LoadRegistryValues();
                if (BasicInfo.SyncDirPath.Trim().Length != 0)
                {
                    string argument = BasicInfo.SyncDirPath;
                    System.Diagnostics.Process.Start(argument);
                }
                //else
                //{
                //    SwitchToCurrentInstance();
                //}
                return; 
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            frmLogin loginForm = new frmLogin();
            bool showLogin = loginForm.showLogin;
            
            if (showLogin)
            {
                Application.Run(loginForm);
            }
            else
            {
                loginForm.Login();
                Application.Run();
            }

        }

        //private static IntPtr GetCurrentInstanceWindowHandle()
        //{
        //    IntPtr hWnd = IntPtr.Zero;
        //    Process process = Process.GetCurrentProcess();
        //    Process[] processes = Process.GetProcessesByName(process.ProcessName);
        //    foreach (Process _process in processes)
        //    {
        //        // Get the first instance that is not this instance, has the
        //        // same process name and was started from the same file name
        //        // and location. Also check that the process has a valid
        //        // window handle in this session to filter out other user's
        //        // processes.
        //        if (_process.Id != process.Id &&
        //            _process.MainModule.FileName == process.MainModule.FileName &&
        //            _process.MainWindowHandle != IntPtr.Zero)
        //        {
        //            hWnd = _process.MainWindowHandle;
        //            break;
        //        }
        //    }
        //    return hWnd;
        //}

        //private static void SwitchToCurrentInstance()
        //{
        //    IntPtr hWnd = GetCurrentInstanceWindowHandle();
        //    if (hWnd != IntPtr.Zero)
        //    {
        //        // Restore window if minimised. Do not restore if already in
        //        // normal or maximised window state, since we don't want to
        //        // change the current state of the window.
        //        if (IsIconic(hWnd) != 0)
        //        {
        //            ShowWindow(hWnd, SW_RESTORE);
        //        }

        //        // Set foreground window.
        //        SetForegroundWindow(hWnd);
        //    }
        //}
    }
}
