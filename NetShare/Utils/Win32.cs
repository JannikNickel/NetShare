using System;
using System.Runtime.InteropServices;

namespace NetShare
{
    public static class Win32
    {
        public const int WM_SYSCOMMAND = 0x112;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_SEPARATOR = 0x800;
        public const int SETTINGS_ITEM_ID = 1000;

        public static Guid DownloadsFolderGuid => new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool InsertMenu(IntPtr hMenu, int wPostiion, int wFlags, int wIdNewItem, string lpNewItem);

        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        public static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken = 0);
    }
}
