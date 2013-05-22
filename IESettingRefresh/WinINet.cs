using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
//using System.Windows.Interop;


namespace IESettingRefresh
{
    internal class WinINet
    {
        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int dwBufferLength);

        public static void Notify_OptionsSettingsChanges()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}
