using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mezeo
{
    public class Debugger
    {
        private static FileStream debugFile =null;// new FileStream(@"C:\MezeoFileLogs.txt", FileMode.Append, FileAccess.Write);

        public static Debugger calssInstance = null;

        public static Debugger Instance
        {
            get
            {
                if (calssInstance == null)
                {
                    calssInstance = new Debugger();
                    debugFile = new FileStream( Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "MezeoFileLogs.txt", FileMode.Append, FileAccess.Write);
                }

                return calssInstance;
            }
        }

        ~Debugger()
        {
            debugFile.Close();
        }

        public void logMessage(string tag, string message)
        {
            byte[] byteBuff = System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToString() + "\n");

            debugFile.Write(byteBuff, 0, byteBuff.Length);

            byteBuff = System.Text.Encoding.ASCII.GetBytes(tag + " - " + message + "\n");
            debugFile.Write(byteBuff, 0, byteBuff.Length);

            byteBuff = System.Text.Encoding.ASCII.GetBytes("---\n");
            debugFile.Write(byteBuff, 0, byteBuff.Length);

            debugFile.Flush();
        }
    }
}
