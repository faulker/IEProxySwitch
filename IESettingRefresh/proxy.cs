using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace IESettingRefresh
{
    class proxy
    {
        /// <summary>
        /// Check to see if the proxy is enabled or not.
        /// </summary>
        /// <returns>Proxy Status as bool</returns>
        public bool isProxyEnabled()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings");
            if (key != null)
            {
                object value = key.GetValue("ProxyEnable", 1, RegistryValueOptions.None);
                key.Close();
                return Convert.ToBoolean(value);
            }
            else
            {
                key.Close();
                return false;
            }
        }

        /// <summary>
        /// Change the status
        /// </summary>
        public void changeProxyState()
        {
            bool value = isProxyEnabled();

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            if (value)
            {
                key.SetValue("ProxyEnable", 0);
            }
            else
            {
                key.SetValue("ProxyEnable", 1);
            }

            key.Close();

            WinINet.Notify_OptionsSettingsChanges();
        }

        
    }
}
