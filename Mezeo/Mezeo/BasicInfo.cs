using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    public static class BasicInfo
    {
        private static string userName="";
        private static string password="";
        private static string serviceUrl="";
        private static DateTime lastSync;
        private static bool autoSync=true;
        private static string syncDirPath="";
        private static bool isInitailSync = true;

        private static RegistryHandler regHandler = new RegistryHandler();

        public static string UserName
        {
            get { return userName; }
            set 
            {
                userName = value;
                regHandler.Write("Basic1", userName, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        public static string Password
        {
            get { return password; }
            set 
            {
                password = value;
                regHandler.Write("Basic2", password, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        public static string ServiceUrl
        {
            get { return serviceUrl; }
            set 
            { 
                serviceUrl = value;
                regHandler.Write("Basic3", serviceUrl, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        public static DateTime LastSyncAt
        {
            get { return lastSync; }
            set 
            { 
                lastSync = value;
                regHandler.Write("Basic5", lastSync, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        public static bool AutoSync
        {
            get { return autoSync; }
            set 
            { 
                autoSync = value;
                regHandler.Write("Basic6", autoSync, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        public static string SyncDirPath
        {
            get { return syncDirPath; }
            set 
            { 
                syncDirPath = value;
                regHandler.Write("Basic4", syncDirPath, Microsoft.Win32.RegistryValueKind.String);
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
                regHandler.Write("Basic7", isInitailSync, Microsoft.Win32.RegistryValueKind.Binary);
            }
        }

        private static void ReadRegValue()
        {
            userName = regHandler.Read("Basic1", Microsoft.Win32.RegistryValueKind.Binary);
            password = regHandler.Read("Basic2", Microsoft.Win32.RegistryValueKind.Binary);
            serviceUrl = regHandler.Read("Basic3", Microsoft.Win32.RegistryValueKind.Binary);
            syncDirPath = regHandler.Read("Basic4", Microsoft.Win32.RegistryValueKind.String);
            lastSync = DateTime.Parse(regHandler.Read("Basic5", Microsoft.Win32.RegistryValueKind.Binary));
            autoSync = Convert.ToBoolean(regHandler.Read("Basic6", Microsoft.Win32.RegistryValueKind.Binary));
            isInitailSync = Convert.ToBoolean(regHandler.Read("Basic7", Microsoft.Win32.RegistryValueKind.Binary));
        }

        private static void WriteRegValue()
        {
            regHandler.Write("Basic1",userName, Microsoft.Win32.RegistryValueKind.Binary);
            regHandler.Write("Basic2",password, Microsoft.Win32.RegistryValueKind.Binary);
            regHandler.Write("Basic3",serviceUrl, Microsoft.Win32.RegistryValueKind.Binary);
            regHandler.Write("Basic4", syncDirPath, Microsoft.Win32.RegistryValueKind.String);
            regHandler.Write("Basic5",lastSync, Microsoft.Win32.RegistryValueKind.Binary);
            regHandler.Write("Basic6",autoSync, Microsoft.Win32.RegistryValueKind.Binary);
            regHandler.Write("Basic7", isInitailSync, Microsoft.Win32.RegistryValueKind.Binary);
        }
    }
}
