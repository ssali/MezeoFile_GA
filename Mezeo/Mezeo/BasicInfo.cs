﻿using System;
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
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        private static string userName="";
        private static string password="";
        private static string serviceUrl="";
        private static DateTime lastSync;
        private static bool autoSync=true;
        private static string syncDirPath="";
        private static bool isInitailSync = true;

        private static int nQRangeStart = -1;
        private static int nQRangeEnd = -1;
        private static int nQProcessed = -1;

        private static RegistryHandler regHandler = new RegistryHandler();

        public static int NQProcessed
        {
            get { return nQProcessed; }
            set
            {
                nQProcessed = value;
                regHandler.Write("Basic10", nQProcessed, Microsoft.Win32.RegistryValueKind.String, false);
            }
        }

        public static int NQRangeEnd
        {
            get { return nQRangeEnd; }
            set
            {
                nQRangeEnd = value;
                regHandler.Write("Basic9", nQRangeEnd, Microsoft.Win32.RegistryValueKind.String, false);
            }
        }

        public static int NQRangeStart
        {
            get { return nQRangeStart; }
            set
            {
                nQRangeStart = value;
                regHandler.Write("Basic8", nQRangeStart, Microsoft.Win32.RegistryValueKind.String, false);
            }
        }

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

        public static string GetMacAddress
        {
            get
            {
                return GetNinMacAddress();
            }
        }

        //public static bool IsConnectedToInternet
        //{
        //    get
        //    {
        //        return ConnectedToInternet();
        //    }
        //}

        public static bool IsCredentialsAvailable
        {
            get
            {
                return userName.Trim().Length != 0 && password.Trim().Length != 0 && serviceUrl.Trim().Length != 0;
            }
        }

        private static void ReadRegValue()
        {
            userName = regHandler.Read("Basic1", Microsoft.Win32.RegistryValueKind.Binary,true);
            password = regHandler.Read("Basic2", Microsoft.Win32.RegistryValueKind.Binary, true);
            serviceUrl = regHandler.Read("Basic3", Microsoft.Win32.RegistryValueKind.Binary,false);
            syncDirPath = regHandler.Read("Basic4", Microsoft.Win32.RegistryValueKind.String, false);
            lastSync = DateTime.Parse(regHandler.Read("Basic5", Microsoft.Win32.RegistryValueKind.Binary, false));
            autoSync = Convert.ToBoolean(regHandler.Read("Basic6", Microsoft.Win32.RegistryValueKind.Binary, false));
            isInitailSync = Convert.ToBoolean(regHandler.Read("Basic7", Microsoft.Win32.RegistryValueKind.Binary, false));
            
            nQRangeStart = Convert.ToInt32(regHandler.Read("Basic8", Microsoft.Win32.RegistryValueKind.String, false));
            nQRangeEnd = Convert.ToInt32(regHandler.Read("Basic9", Microsoft.Win32.RegistryValueKind.String, false));
            nQProcessed = Convert.ToInt32(regHandler.Read("Basic10", Microsoft.Win32.RegistryValueKind.String, false));
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
            regHandler.Write("Basic8", nQRangeStart, Microsoft.Win32.RegistryValueKind.String, false);
            regHandler.Write("Basic9", nQRangeEnd, Microsoft.Win32.RegistryValueKind.String, false);
            regHandler.Write("Basic10", nQProcessed, Microsoft.Win32.RegistryValueKind.String, false);
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

        private static bool ConnectedToInternet()
        {
            //int Desc = 0x1 | 0x2;
            //return InternetGetConnectedState(out Desc, 0);
            PingReply pReply;
            try
            {
                string pingUrl = "";

                if (serviceUrl.Contains("https://") || serviceUrl.Contains("http://"))
                {
                    int sepIndex = serviceUrl.IndexOf("//") + 2;
                    pingUrl = serviceUrl.Substring(sepIndex);
                }
                else
                {
                    pingUrl = serviceUrl;
                }


                if (pingUrl.Substring(pingUrl.Length - 3) == "/v2")
                {
                    pingUrl = pingUrl.Substring(0, pingUrl.Length - 3);
                }



                System.Net.NetworkInformation.Ping ping = new Ping();
                pReply = ping.Send(pingUrl);

                int retryCount = 0;

                while ((pReply.Status != IPStatus.Success) && retryCount < 3)
                {
                    pReply = ping.Send(pingUrl);
                    retryCount++;
                }

                if (pReply.Status == IPStatus.Success)
                {
                    ping.Dispose();
                    return true;
                }
            }
            catch
            {
                string sUrl = ServiceUrl;
                if (sUrl.Trim().Length == 0)
                {
                    int Desc = 0x1 | 0x2;
                    return InternetGetConnectedState(out Desc, 0);
                }
            }

            
            return false;
            
        }
    }
}
