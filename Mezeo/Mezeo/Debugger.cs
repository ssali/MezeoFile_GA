using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mezeo
{
    public class Debugger
    {
       // private FileStream debugFile=new FileStream(@"C:\MezeoFileLogs.txt",FileMode.Append,FileAccess.Write);

      //  public static Debugger Instance { get; private set; }

        public Debugger()
        {
          //  Instance = new Debugger();
        }

        ~Debugger()
        {
            //debugFile.Close();
        }

        public void logMessage(string tag, string message)
        {
            //if (!File.Exists(@"C:\MezeoFileLogs.txt"))
            //{
            //    using (StreamWriter sw = File.CreateText(@"C:\MezeoFileLogs.txt"))
            //    {
            //        sw.WriteLine(DateTime.Now.ToString());
            //        sw.WriteLine(tag);
            //        sw.WriteLine(message);
            //        sw.WriteLine("\n\n");
            //        sw.Close();
            //    }
            //}
            //else
            //{
            //    using (StreamWriter sw = File.AppendText(@"C:\MezeoFileLogs.txt"))
            //    {
            //        sw.WriteLine(DateTime.Now.ToString());
            //        sw.WriteLine(tag);
            //        sw.WriteLine(message);
            //        sw.WriteLine("\n\n");
            //        sw.Close();
            //    }
            //}

            //byte[] byteBuff = System.Text.Encoding.ASCII.GetBytes(DateTime.Now.ToString() + "\n");

            //debugFile.Write(byteBuff, 0, byteBuff.Length);

            //byteBuff = System.Text.Encoding.ASCII.GetBytes(tag + "\n");
            //debugFile.Write(byteBuff, 0, byteBuff.Length);

            //byteBuff = System.Text.Encoding.ASCII.GetBytes(message + "\n");
            //debugFile.Write(byteBuff, 0, byteBuff.Length);

            //debugFile.Flush();
        }
    }
}
