using System.IO;
using Microsoft.Win32;

namespace oem_logo
{
    public static class SystemSettings
    {
        public static string OemIcon
        {
            get => Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation",
                    "Logo", "") as string;
            set => Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation",
                "Logo", value);
        }
        public static void SetOemIcon(FileInfo file)
        {
            
        }
    }
}