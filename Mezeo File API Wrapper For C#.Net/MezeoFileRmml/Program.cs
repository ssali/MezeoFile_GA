using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace MezeoFileRmml
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "-mezrebtsu")
            {
                Process.Start("taskkill.exe", "/f /im explorer.exe");
                Thread.Sleep(1000);
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe");
            }
            else if (args[0] == "-cuunmnt")
            {
                Process.Start("taskkill.exe", "/f /im MezeoFileSync.exe");
            }
            else if (args[0] == "-tcsmnt")
            {
                string szLocalAppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                File.Delete(szLocalAppPath + "\\Mezeo File Sync\\mezeoDb.s3db");
                Directory.Delete(szLocalAppPath + "\\Mezeo File Sync");                
            }
        }
    }
}
