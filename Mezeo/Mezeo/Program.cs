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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {//// {D19F100E-113F-4751-820C-FD5AF8D17A55}

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name; 
            bool bCreatedNew;

            LanguageTranslator.SetLanguage("en");

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

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debugger.Instance.logMessage("APP DOMAIN CRASH ", e.ToString());

            StackTrace trace = new StackTrace(true);

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame sf = trace.GetFrame(i);
                Debugger.Instance.logMessage("High up the call stack, Method: ",  sf.GetMethod().ToString());
                Debugger.Instance.logMessage("High up the call stack, Method: ", sf.GetFileLineNumber().ToString());
            }
        }
    }
}
