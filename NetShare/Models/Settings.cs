using System;
using System.IO;

namespace NetShare.Models
{
    public class Settings
    {
        public string? DisplayName { get; set; }
        public string? DownloadPath { get; set; }
        public int BroadcastPort { get; set; }
        public int TransferPort { get; set; }

        public static Settings Default => BuildDefaultSettings();

        private static Settings BuildDefaultSettings()
        {
            Settings settings = new Settings();
            settings.DisplayName = Environment.MachineName;
            settings.DownloadPath = Path.Combine(Win32.SHGetKnownFolderPath(Win32.DownloadsFolderGuid, 0), "NetShareDownloads");
            settings.BroadcastPort = 37335;
            settings.TransferPort = 37336;
            return settings;
        }

        public static bool IsValidDownloadPath(string? path)
        {
            bool valid;
            try
            {
                path = Path.GetFullPath(path!);
                valid = Path.IsPathRooted(path);
            }
            catch
            {
                valid = false;
            }
            return valid;
        }

        public static bool PrepDownloadPath(ref string path)
        {
            try
            {
                path = Path.GetFullPath(path);
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using FileStream fs = File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                fs.Dispose();
                return true;
            }
            catch { }
            return false;
        }

        public static bool IsValidPort(int port)
        {
            return port >= 0 && port <= 65535;
        }
    }
}
