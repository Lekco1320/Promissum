using Org.BouncyCastle.Asn1.Tsp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lekco.Promissum.Utility
{
    /// <summary>
    /// A helper class for getting SHFileInfo.
    /// </summary>
    public static class SHFileInfoHelper
    {
        /// <summary>
        /// Indicate to get icon.
        /// </summary>
        private const uint SHGFI_ICON = 0x100;

        /// <summary>
        /// Indicate to get large icon (32x32).
        /// </summary>
        private const uint SHGFI_LARGEICON = 0x0;

        /// <summary>
        /// Indicate to get small icon (16x16).
        /// </summary>
        private const uint SHGFI_SMALLICON = 0x1;

        /// <summary>
        /// Indicate to get icon by extension.
        /// </summary>
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        /// <summary>
        /// Indicate to get typename by extension.
        /// </summary>
        public const uint SHGFI_TYPENAME = 0x000000400;

        private static readonly Dictionary<string, Icon> _largeIconCache = new Dictionary<string, Icon>();

        private static readonly Dictionary<string, Icon> _smallIconCache = new Dictionary<string, Icon>();

        private static readonly Dictionary<string, ImageSource> _largeIconImageCache = new Dictionary<string, ImageSource>();

        private static readonly Dictionary<string, ImageSource> _smallIconImageCache = new Dictionary<string, ImageSource>();

        private static readonly Dictionary<string, string> _typeInfoCache = new Dictionary<string, string>();

        public static readonly Icon LargeFolderIcon;

        public static readonly Icon SmallFolderIcon;

        public static readonly ImageSource LargeFolderIconImage;

        public static readonly ImageSource SmallFolderIconImage;

        public static readonly string FolderInfo = "文件夹";

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFileInfo
        {
            public IntPtr hIcon;

            public int iIcon;

            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFileInfo psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>
        /// Static constructer. Initialize all cache.
        /// </summary>
        static SHFileInfoHelper()
        {
            var largeSHInfo = new SHFileInfo();
            var smallSHInfo = new SHFileInfo();

            uint uFlags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            _ = SHGetFileInfo(Environment.SystemDirectory, SHGFI_USEFILEATTRIBUTES, ref largeSHInfo, (uint)Marshal.SizeOf(largeSHInfo), uFlags | SHGFI_LARGEICON);
            _ = SHGetFileInfo(Environment.SystemDirectory, SHGFI_USEFILEATTRIBUTES, ref smallSHInfo, (uint)Marshal.SizeOf(smallSHInfo), uFlags | SHGFI_SMALLICON);

            LargeFolderIcon = Icon.FromHandle(largeSHInfo.hIcon);
            SmallFolderIcon = Icon.FromHandle(smallSHInfo.hIcon);
            LargeFolderIconImage = ((Icon)LargeFolderIcon.Clone()).ToImageSource();
            SmallFolderIconImage = ((Icon)SmallFolderIcon.Clone()).ToImageSource();

            _ = DestroyIcon(largeSHInfo.hIcon);
            _ = DestroyIcon(smallSHInfo.hIcon);
        }

        /// <summary>
        /// Converts this Icon to an instance of <see cref="BitmapImage"/>.
        /// </summary>
        /// <param name="icon">Given Icon.</param>
        /// <returns>An instance of <see cref="BitmapImage"/> that represents the converted icon.</returns>
        public static ImageSource ToImageSource(this Icon icon)
            => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());

        /// <summary>
        /// Get the default icon of a given file.
        /// </summary>
        /// <param name="extension">The extension of the file.</param>
        /// <param name="largeIcon">Indicate whether to get large icon.</param>
        /// <returns>The default icon of the file.</returns>
        public static Icon GetFileIcon(string extension, bool largeIcon)
        {
            var cache = largeIcon ? _largeIconCache : _smallIconCache;
            if (cache.TryGetValue(extension, out var icon))
            {
                return icon;
            }

            var shinfo = new SHFileInfo();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            flags |= largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON;

            _ = SHGetFileInfo(extension, 0x80, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            cache.Add(extension, icon);
            _ = DestroyIcon(shinfo.hIcon);

            return icon;
        }

        /// <summary>
        /// Get image of the default icon of a given file.
        /// </summary>
        /// <param name="extension">The extension of the file.</param>
        /// <param name="largeIcon">Indicate whether to get large icon.</param>
        /// <returns>Image of the default icon of the file.</returns>
        public static ImageSource GetFileIconImage(string extension, bool largeIcon)
        {
            var cache = largeIcon ? _largeIconImageCache : _smallIconImageCache;
            if (cache.TryGetValue(extension, out var image))
            {
                return image;
            }

            var icon = GetFileIcon(extension, largeIcon);
            image = icon.ToImageSource();
            cache.Add(extension, image);
            return image;
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string? pszAssoc, string? pszExtra, [Out] StringBuilder? pszOut, [In][Out] ref uint pcchOut);

        [Flags]
        public enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        /// <summary>
        /// Associated string.
        /// </summary>
        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        /// <summary>
        /// Get extension info of file.
        /// </summary>
        /// <param name="extension">Extension of file.</param>
        /// <param name="assocStr">Associated string.</param>
        /// <returns>Extension info of file.</returns>
        public static string FileExtentionInfo(string extension, AssocStr assocStr)
        {
            uint pcchOut = 0;
            _ = AssocQueryString(AssocF.Verify, assocStr, extension, null, null, ref pcchOut);

            var pszOut = new StringBuilder((int)pcchOut);
            _ = AssocQueryString(AssocF.Verify, assocStr, extension, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }

        /// <summary>
        /// Get type name of a file by its extension.
        /// </summary>
        /// <param name="extension">The extension of the file.</param>
        /// <returns>Type name of the file.</returns>
        public static string GetTypeInfo(string extension)
        {
            if (_typeInfoCache.TryGetValue(extension, out var info))
            {
                return info;
            }

            var typename = FileExtentionInfo(extension, AssocStr.FriendlyDocName);
            _typeInfoCache.Add(extension, typename);
            return typename;
        }
    }
}
