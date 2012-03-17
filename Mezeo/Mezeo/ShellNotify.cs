using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Mezeo
{
    public class ShellNotify
    {
       // #region Base Declarations

       //// private readonly InnerClass.FormTmp formTmp = null;
       // private readonly IntPtr formTmpHwnd = IntPtr.Zero;
       // private bool forgetDelNotifyBox = false;
       // internal IntPtr formHwnd = IntPtr.Zero;
       // internal IntPtr contextMenuHwnd = IntPtr.Zero;
       // private NotifyIconData notifyIconData;
       // #endregion

        

       // #region Declarations/structure for shellapi notify icon
        
       // /* declarations for ShellApi Variables */
       // internal readonly int WM_NOTIFY_TRAY = 0x0400 + 2001;
       // internal readonly int uID = 1;

       // private const int NIF_MESSAGE = 0x01;
       // private const int NIF_ICON = 0x02;
       // private const int NIF_TIP = 0x04;
       // private const int NIF_STATE = 0x08;
       // private const int NIF_INFO = 0x10;
       // private const int NIIF_USER = 0x4;

       // private const int NIM_ADD = 0x00;
       // private const int NIM_MODIFY = 0x01;
       // private const int NIM_DELETE = 0x02;
        

       // [DllImport("shell32.dll", EntryPoint="Shell_NotifyIcon")]
       // private static extern bool Shell_NotifyIcon ( int dwMessage, ref NotifyIconData lpData );

		
       // [DllImport("user32.dll", EntryPoint="SetForegroundWindow")]
       // public static extern int SetForegroundWindow ( IntPtr hwnd	);


       // /* define structure of shell notifyicon data */
       // [StructLayout(LayoutKind.Sequential)]
       // private struct NotifyIconData 
       // {
       //     internal int cbSize;
       //     internal IntPtr hwnd;
       //     internal int uID;
       //     internal int uFlags;
       //     internal int uCallbackMessage;
       //     internal IntPtr hIcon;
       //     [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x80)]
       //     internal string szTip;
       //     internal int dwState;
       //     internal int dwStateMask;
       //     [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0xFF)]
       //     internal string szInfo;
       //     internal int uTimeoutAndVersion;
       //     [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
       //     internal string szInfoTitle;
       //     internal int dwInfoFlags;
       // }


       // #endregion



       // #region properties of Shell Notify structure
       // /*create properties of new Shell Notify structure here*/
       // private NotifyIconData GetNOTIFYICONDATA(IntPtr iconHwnd, string sTip, string boxTitle, string boxText) {
       //     NotifyIconData nData = new NotifyIconData();

       //     nData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(nData); // struct size
       //     nData.hwnd = formTmpHwnd; 
       //     nData.uID = uID; 
       //     nData.uFlags = NIF_MESSAGE; 
       //     nData.uCallbackMessage = WM_NOTIFY_TRAY;
       //     nData.hIcon = iconHwnd; 
       //     nData.uTimeoutAndVersion = 100; 
       //     nData.uFlags |= NIF_INFO;
       //     nData.dwInfoFlags = NIIF_USER; 

       //     nData.szTip = sTip; 
       //     nData.szInfoTitle = boxTitle; 
       //     nData.szInfo = boxText; 

       //     return nData;
       // }
       // #endregion



        //#region methods to add/update/delete notifications

        //public void StopNotifyIconBalloonText()
        //{
        //    notifyIconData.uFlags |= NIIF_USER;
        //    notifyIconData.dwInfoFlags = NIF_INFO;

        //    notifyIconData.szInfo = "";
        //    notifyIconData.szInfoTitle = "";
        //}

        //public void SetNotifyIconBalloonText(string strBalloonText, string strBalloonTitle)
        //{
        //    if (strBalloonText.Length > 0 && strBalloonText.Length < 200)
        //    {
        //        notifyIconData.szInfo = strBalloonText;
        //        notifyIconData.dwInfoFlags = NIIF_USER;
        //    }

        //    if(strBalloonTitle.Length > 0 && strBalloonTitle.Length < 64)
        //        notifyIconData.szInfoTitle = strBalloonTitle;
        //}

        //public void SetNotifyIconToolTip(string strToolTip)
        //{
        //    notifyIconData.szTip = strToolTip;
        //    notifyIconData.uFlags |= NIF_TIP; 
        //}

        //public void SetNotifyIconHandle(IntPtr hIcon)
        //{
        //    notifyIconData.hIcon = hIcon;
        //    notifyIconData.uFlags |= NIF_ICON;

        //   UpdateNotifyIcon();
        //}

        //public void AddNotifyIcon()
        //{
        //    Shell_NotifyIcon(NIM_ADD, ref notifyIconData);
        //}

        //public void RemoveNotifyIcon()
        //{
        //    Shell_NotifyIcon(NIM_DELETE, ref notifyIconData);
        //}

        //public void UpdateNotifyIcon()
        //{
        //    Shell_NotifyIcon(NIM_MODIFY, ref notifyIconData);
        //}

        //public int ShowNotifyIcon(IntPtr iconHwnd, string sTip, string boxTitle, string boxText) 
        //{
        //    //NotifyIconData nData = GetNOTIFYICONDATA(iconHwnd, sTip, boxTitle, boxText);

        //    if (Shell_NotifyIcon(NIM_ADD, ref notifyIconData)) 
        //    {
        //        this.forgetDelNotifyBox = true;
        //        return 1;
        //    }
        //    else 
        //    {
        //        return 0;
        //    }
        //}

        ///* This method will be called to modify existing Shell Notify Tool tip */
        //public int ModifyNotifyIcon(IntPtr iconHwnd, string sTip, string boxTitle, string boxText)
        //{
        //    NotifyIconData nData = GetNOTIFYICONDATA(iconHwnd, sTip, boxTitle, boxText);
        //    nData.uFlags |= NIF_INFO;
        //    nData.dwInfoFlags = NIIF_USER;

        //    if (Shell_NotifyIcon(NIM_MODIFY, ref nData))
        //    {
        //        this.forgetDelNotifyBox = true;
        //        return 1;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        ///* This method will be called to delete existing Shell Notify Tool tip*/
        //public int DelNotifyIcon() 
        //{
        //    NotifyIconData nData = GetNOTIFYICONDATA(IntPtr.Zero, null, null, null);
        //    if (Shell_NotifyIcon(NIM_DELETE,ref nData)) {
        //        this.forgetDelNotifyBox = false;
        //        return 1;
        //    }
        //    else 
        //    {
        //        return 0;
        //    }
        //}

        //#endregion  
        

        //#region random functions
        //public ShellNotify(IntPtr handle)
        //{
        //    formTmpHwnd = handle;
        //    formHwnd = handle;
        //    notifyIconData = new NotifyIconData();

        //    notifyIconData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(notifyIconData); // struct size
        //    notifyIconData.hwnd = formTmpHwnd;
        //    notifyIconData.uID = uID;
        //    notifyIconData.uFlags = NIF_MESSAGE | NIF_INFO;
        //    notifyIconData.uCallbackMessage = WM_NOTIFY_TRAY;
        //    notifyIconData.hIcon = IntPtr.Zero;
        //    notifyIconData.uTimeoutAndVersion = 1;
        //    notifyIconData.dwInfoFlags = NIIF_USER;

        //    notifyIconData.szTip = "";
        //    notifyIconData.szInfoTitle = "";
        //    notifyIconData.szInfo = ""; 
        //}

        //~ShellNotify()
        //{
        //    if (forgetDelNotifyBox) this.DelNotifyIcon();
        //}

        ///* code for connecting shell notification to contextMenu */
        //public void ConnectMyMenu(IntPtr _contextMenuHwnd)
        //{
        //    contextMenuHwnd = _contextMenuHwnd;
        //}

        //public void Dispose()
        //{

        //}
        //#endregion
    }
}
