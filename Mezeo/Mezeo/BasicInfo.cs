using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Mezeo
{
    public static class BasicInfo
    {
        private static string userName="";
        private static string password="";
        private static string serviceUrl="";
        private static DateTime lastSync;
        private static DateTime lastUpdateCheckAt;
        private static bool autoSync=true;
        private static bool loggingEnabled = false;
        private static string syncDirPath="";
        private static bool isInitailSync = true;

        private static RegistryHandler regHandler = new RegistryHandler();

        public static string UserName
        {
            get { return userName; }
            set 
            {
                userName = value;
                regHandler.Write("Basic1", userName, Microsoft.Win32.RegistryValueKind.Binary,true);
            }
        }

        public static string Password
        {
            get { return password; }
            set 
            {
                password = value;
                regHandler.Write("Basic2", password, Microsoft.Win32.RegistryValueKind.Binary, true);
            }
        }

        public static string ServiceUrl
        {
            get { return serviceUrl; }
            set 
            { 
                serviceUrl = value;
                regHandler.Write("Basic3", serviceUrl, Microsoft.Win32.RegistryValueKind.Binary,false);
            }
        }

        public static DateTime LastSyncAt
        {
            get { return lastSync; }
            set 
            { 
                lastSync = value;
                regHandler.Write("Basic5", lastSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            }
        }

        public static bool AutoSync
        {
            get { return autoSync; }
            set 
            { 
                autoSync = value;
                regHandler.Write("Basic6", autoSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            }
        }

        public static bool LoggingEnabled
        {
            get { return loggingEnabled; }
            set 
            {
                loggingEnabled = value;
                regHandler.Write("Basic8", loggingEnabled, Microsoft.Win32.RegistryValueKind.Binary, false);
            }
        }

        public static string SyncDirPath
        {
            get { return syncDirPath; }
            set 
            { 
                syncDirPath = value;
                regHandler.Write("Basic4", syncDirPath, Microsoft.Win32.RegistryValueKind.String, false);
            }
        }

        public static bool LoadRegistryValues()
        {             
            if (regHandler.isKeyExists())
            {
                ReadRegValue();
                return true;
            }
            else
            {
                WriteRegValue();
                return false;
            }
        }

        public static bool IsInitialSync
        {
            get
            {
                return isInitailSync;
            }
            set
            {
                isInitailSync = value;
                regHandler.Write("Basic7", isInitailSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            }
        }

        public static string MacAddress
        {
            get
            {
                return GetNinMacAddress();
            }
        }

        public static bool IsCredentialsAvailable
        {
            get
            {
                return userName.Trim().Length != 0 && password.Trim().Length != 0 && serviceUrl.Trim().Length != 0;
            }
        }

        private static void ReadRegValue()
        {
            userName = regHandler.Read("Basic1", Microsoft.Win32.RegistryValueKind.Binary, true);
            password = regHandler.Read("Basic2", Microsoft.Win32.RegistryValueKind.Binary, true);
            serviceUrl = regHandler.Read("Basic3", Microsoft.Win32.RegistryValueKind.Binary,false);
            syncDirPath = regHandler.Read("Basic4", Microsoft.Win32.RegistryValueKind.String, false);
            lastSync = DateTime.Parse(regHandler.Read("Basic5", Microsoft.Win32.RegistryValueKind.Binary, false));
            autoSync = Convert.ToBoolean(regHandler.Read("Basic6", Microsoft.Win32.RegistryValueKind.Binary, false));
            isInitailSync = Convert.ToBoolean(regHandler.Read("Basic7", Microsoft.Win32.RegistryValueKind.Binary, false));
            try
            {
                string regValue = regHandler.Read("Basic8", Microsoft.Win32.RegistryValueKind.Binary, false);
                if (null != regValue)
                    loggingEnabled = Convert.ToBoolean(regValue);
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("BasicInfo - ReadRegValue", "Caught exception: " + ex.Message);
            }
            try
            {
                lastSync = DateTime.Parse(regHandler.Read("Basic5", Microsoft.Win32.RegistryValueKind.Binary, false));
                string regValue = regHandler.Read("Basic9", Microsoft.Win32.RegistryValueKind.Binary, false);
                if (null != regValue)
                    lastUpdateCheckAt = DateTime.Parse(regValue);
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("BasicInfo - ReadRegValue", "Caught exception: " + ex.Message);
            }
        }

        private static void WriteRegValue()
        {
            regHandler.Write("Basic1", userName, Microsoft.Win32.RegistryValueKind.Binary, true);
            regHandler.Write("Basic2",password, Microsoft.Win32.RegistryValueKind.Binary,true);
            regHandler.Write("Basic3", serviceUrl, Microsoft.Win32.RegistryValueKind.Binary, false);
            regHandler.Write("Basic4", syncDirPath, Microsoft.Win32.RegistryValueKind.String, false);
            regHandler.Write("Basic5", lastSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            regHandler.Write("Basic6", autoSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            regHandler.Write("Basic7", isInitailSync, Microsoft.Win32.RegistryValueKind.Binary, false);
            regHandler.Write("Basic8", loggingEnabled, Microsoft.Win32.RegistryValueKind.Binary, false);
            regHandler.Write("Basic9", lastUpdateCheckAt, Microsoft.Win32.RegistryValueKind.Binary, false);
        }

        private static string GetNinMacAddress()
        {
            string macAddresses = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }

        public static string GetQueueName()
        {
            return MacAddress + "-" + UserName;
        }

        public static string GetUpdateURL()
        {
#if MEZEO32
            string osVersion = "32";
#else
            string osVersion = "64";
#endif
            return string.Format("{0}/update/sync/win/{1}/versioninfo.xml", ServiceUrl, osVersion);
        }

        public static DateTime LastUpdateCheckAt
        {
            get { return lastUpdateCheckAt; }
            set
            {
                lastUpdateCheckAt = value;
                regHandler.Write("Basic9", lastUpdateCheckAt, Microsoft.Win32.RegistryValueKind.Binary, false);
            }
        }
    }

    public static class LogWrapper
    {
        public static void LogMessage(string tag, string message)
        {
            if (BasicInfo.LoggingEnabled)
                Debugger.Instance.logMessage(tag, message);
        }
    }
}
