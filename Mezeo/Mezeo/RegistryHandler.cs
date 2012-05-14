using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using MezeoFileSupport;

namespace Mezeo
{
    public class RegistryHandler
    {
        private bool showError = true;
        
        public bool ShowError
        {
            get { return showError; }
            set { showError = value; }
        }

        private string subKey = "SOFTWARE\\" + AboutBox.AssemblyTitle + "\\Basic Info";
        public string SubKey
        {
            get { return subKey; }
            set { subKey = value; }
        }

        private RegistryKey baseRegistryKey = Registry.CurrentUser;
        public RegistryKey BaseRegistryKey
        {
            get { return baseRegistryKey; }
            set { baseRegistryKey = value; }
        }

      
        public string Read(string KeyName, RegistryValueKind valueKind,bool isEncrypted)
        {
            RegistryKey rk = baseRegistryKey;
            RegistryKey sk1 = rk.OpenSubKey(subKey);
            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    if (valueKind == RegistryValueKind.Binary)
                    {
                        Byte[] bytes = (Byte[])sk1.GetValue(KeyName.ToUpper());

                        if (isEncrypted)
                        {
                            return MezeoFileCloud.Decrypt(bytes);
                        }
                        else
                        {
                            return System.Text.Encoding.Default.GetString(bytes);
                        }
                    }
                    else
                    {
                        return (string)sk1.GetValue(KeyName.ToUpper());
                    }
                }
                catch (Exception e)
                {
                    //ShowErrorMessage(e, "Reading registry " + KeyName.ToUpper());
                    return null;
                }
            }
        }

      
        public bool Write(string KeyName, object Value, RegistryValueKind valueKind,bool isEncrypted)
        {
            try
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                
                if (valueKind == RegistryValueKind.Binary)
                {
                    if (isEncrypted)
                    {
                        sk1.SetValue(KeyName.ToUpper(),MezeoFileCloud.Encrypt(Value.ToString()), valueKind);
                    }
                    else
                    {
                        sk1.SetValue(KeyName.ToUpper(), Encoding.ASCII.GetBytes(Value.ToString()), valueKind);
                    }
                }
                else
                {
                    sk1.SetValue(KeyName.ToUpper(), Value.ToString(), valueKind);
                }

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e, "Writing registry " + KeyName.ToUpper());
                return false;
            }
        }

       
        public bool DeleteKey(string KeyName)
        {
            try
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                if (sk1 == null)
                    return true;
                else
                    sk1.DeleteValue(KeyName);

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

        
        public bool DeleteSubKeyTree()
        {
            try
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e, "Deleting SubKey " + subKey);
                return false;
            }
        }

      
        public int SubKeyCount()
        {
            try
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    return sk1.SubKeyCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e, "Retriving subkeys of " + subKey);
                return 0;
            }
        }

        
        public int ValueCount()
        {
            try
            {
                RegistryKey rk = baseRegistryKey;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    return sk1.ValueCount;
                else
                    return 0;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e, "Retriving keys of " + subKey);
                return 0;
            }
        }

        private void ShowErrorMessage(Exception e, string Title)
        {
            if (showError == true)
                MessageBox.Show(e.Message,
                                Title
                                , MessageBoxButtons.OK
                                , MessageBoxIcon.Error);
        }

        public bool isKeyExists()
        {
            RegistryKey subRegKey = baseRegistryKey.OpenSubKey(subKey);
            if (subRegKey == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
