using System;
using System.Runtime.InteropServices;

namespace Mezeo
{
	/// <summary>
	/// Wrapper class for WritePrivateProfileString Win32 API function.
	/// </summary>
	public class IniWriter
	{
         //using the WritePrivateProfileString
        // Win32 API function 

        [DllImport("kernel32")] 
        private static extern int WritePrivateProfileString(
                string iniSection, 
                string iniKey, 
                string iniValue, 
                string iniFilePath);		
        
             public static void WriteValue(string iniSection, 
                                     string iniKey, 
                                     string iniValue,
                                     string iniFilePath)
        {
            WritePrivateProfileString(iniSection, iniKey, iniValue, iniFilePath);
        }
        
	}
}
