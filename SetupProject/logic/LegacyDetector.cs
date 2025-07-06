using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace WixSharp.logic
{
    public class LegacyDetector
    {
        private static readonly string EXPECTED_DISPLAY_NAME = "Media Extractor";
        private const string LEGACY_GUID = "{D1DF90C3-1CE8-4D17-9A39-C1D1568D508F}}_is1";

        // Both 64-bit and 32-bit-on-64-bit hives
        private static readonly string[] REGISTRY_PATHS = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        /// <summary>
        /// Returns true if an Inno Setup install with our old AppId is present.
        /// </summary>
        public static bool HasLegacyInstallation()
        {
            foreach (string path in REGISTRY_PATHS)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"{path}\{LEGACY_GUID}"))
                {
                    string name = key?.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(name) && name.ToLower().Contains(EXPECTED_DISPLAY_NAME.ToLower()))
                    {
                        return true;
                    }
                }
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"{path}\{LEGACY_GUID}"))
                {
                    string name = key?.GetValue("DisplayName") as string;
                    if (!string.IsNullOrEmpty(name) && name.ToLower().Contains(EXPECTED_DISPLAY_NAME.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Reads the UninstallString from the old Inno Setup key, if it exists.
        /// </summary>
        public static string GetUninstallString()
        {
            foreach (string path in REGISTRY_PATHS)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"{path}\{LEGACY_GUID}"))
                {
                    string uninstallString = key?.GetValue("UninstallString") as string;
                    if (!string.IsNullOrEmpty(uninstallString))
                        return uninstallString;
                }
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey($@"{path}\{LEGACY_GUID}"))
                {
                    string uninstallString = key?.GetValue("UninstallString") as string;
                    if (!string.IsNullOrEmpty(uninstallString))
                        return uninstallString;
                }
            }
            return null;
        }

        /// <summary>
        /// If a legacy UninstallString was found, launches it silently and waits for it to finish.
        /// </summary>
        public static void RemoveLegacyInstallation()
        {
            string uninstallPath = GetUninstallString();
            if (string.IsNullOrEmpty(uninstallPath))
                return;

            // Inno Setup’s uninstallers typically accept /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = uninstallPath,
                Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }
            }
            catch
            {
                // swallow or log any errors—installer can decide what to do
            }
        }
    }
}
