using System;
using System.Runtime.InteropServices;

namespace NetShare
{
    internal static class Win32
    {
        internal const int WM_SYSCOMMAND = 0x112;
        internal const int MF_BYPOSITION = 0x400;
        internal const int MF_SEPARATOR = 0x800;
        internal const int SETTINGS_ITEM_ID = 1000;

        internal static Guid DownloadsFolderGuid => new Guid("374DE290-123F-4565-9164-39C4925E467B");

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIdNewItem, string lpNewItem);

        [DllImport("shell32", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern string SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken = 0);
    }
}
