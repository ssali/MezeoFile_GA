using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Mezeo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {//// {D19F100E-113F-4751-820C-FD5AF8D17A55}

            string guidString = AboutBox.AssemblyTitle + "-D19F100E-113F-4751-820C-FD5AF8D17A55";
            //Guid guid = new Guid(guidString);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            bool isAlreadyRunning=false;

          
            Mutex mutex=new Mutex(true,guidString, out isAlreadyRunning);

            if (!isAlreadyRunning)
            {
                BasicInfo.LoadRegistryValues();
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = @BasicInfo.SyncDirPath;
                proc.Start();
                               
                return;
            }
            
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
    }
}
