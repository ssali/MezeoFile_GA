using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    public static class Debugger
    {
        private static frmConsole console = new frmConsole();

        public static void ShowLogger()
        {
            console.Show();
        }

        public static void hideLogger()
        {
            console.Hide();
        }

        public static void logMessage(string tag, string message)
        {
            console.LogMessage(tag, message);
        }
    }
}
