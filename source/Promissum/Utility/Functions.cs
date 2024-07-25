using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;

namespace Lekco.Promissum.Utility
{
    public static class Functions
    {
        public static string GetMD5(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found.");
            }
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var crypto = MD5.Create();
            return BitConverter.ToString(crypto.ComputeHash(stream));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetVolumeInformation(
            string lpRootPathName,
            string? lpVolumeNameBuffer,
            int nVolumeNameSize,
            ref int lpVolumeSerialNumber,
            int lpMaximumComponentLength,
            int lpFileSystemFlags,
            string? lpFileSystemNameBuffer,
            int nFileSystemNameSize
        );

        public static int HardDiskId(string driveName)
        {
            const int MAX_FILENAME_LEN = 256;
            int retVal = 0;
            int a = 0;
            int b = 0;
            string? str1 = null;
            string? str2 = null;
            _ = GetVolumeInformation(
                driveName,
                str1,
                MAX_FILENAME_LEN,
                ref retVal,
                a,
                b,
                str2,
                MAX_FILENAME_LEN
            );
            return retVal;
        }

        public static string RemoveDiskName(string fileName)
        {
            int id = fileName.IndexOf(@":\");
            if (id == -1)
            {
                return fileName;
            }
            return fileName[(id + 2)..];
        }

        public static bool TryGetDiskName(string fileName, [MaybeNullWhen(false)] out string diskName, out string removedName)
        {
            int id = fileName.IndexOf(@":\");
            if (id == -1 || id > 1)
            {
                diskName = null;
                removedName = fileName;
                return false;
            }
            diskName = fileName[..3];
            removedName = fileName[3..];
            return true;
        }

        public static string CombinePaths(DirectoryInfo directory, FileSystemInfo file)
        {
            string result = directory.FullName;
            if (!result.EndsWith('\\'))
            {
                result += '\\';
            }
            return result += file.Name;
        }

        public static T ReadFromFile<T>(string fileName)
        {
            using var stream = new FileStream(fileName, FileMode.Open);
            using var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
            var serializer = new DataContractSerializer(typeof(T));
            var result = (T?)serializer.ReadObject(reader, true) ?? throw new InvalidDataContractException("Invalid data or file has been destoryed.");
            return result;
        }

        public static void SaveAsFile<T>(T savingObject, string fileName)
        {
            using var stream = new FileStream(fileName, FileMode.Create);
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(stream, savingObject);
        }

        private static readonly string[] UNITS = { "B", "KB", "MB", "GB", "TB" };
        public static string FormatBytesLength(long bytesLength)
        {
            if (bytesLength == long.MaxValue)
            {
                return "∞";
            }
            if (bytesLength <= 0)
            {
                return "0";
            }
            if (bytesLength < 1024)
            {
                return $"{bytesLength} B";
            }
            double ans = bytesLength;
            int level = 0;
            while (ans >= 1024 && level < UNITS.Length - 1)
            {
                ans /= 1024;
                level++;
            }
            return $"{ans:F1} {UNITS[level]}";
        }

        public static void FormatBytesLength(long bytesLength, out double newLength, out string unit)
        {
            newLength = bytesLength;
            int level = 0;
            while (newLength >= 1024 && level < UNITS.Length - 1)
            {
                newLength /= 1024;
                level++;
            }
            unit = UNITS[level];
        }

        public static long FormatBytesLength(double newLength, string unit)
        {
            return unit switch
            {
                "B" => (long)newLength,
                "KB" => (long)(newLength * 1024),
                "MB" => (long)(newLength * 1024 * 1024),
                "GB" => (long)(newLength * 1024 * 1024 * 1024),
                "TB" => (long)(newLength * 1024 * 1024 * 1024 * 1024),
                _ => 0L
            };
        }

        public static T DeepCopy<T>(T obj)
        {
            object? ret;
            using var ms = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            ret = serializer.ReadObject(ms);
            return (T)ret!;
        }

        public static string GetEnumDescription(Enum @enum)
        {
            Type temType = @enum.GetType();
            MemberInfo[] memberInfos = temType.GetMember(@enum.ToString());
            if (memberInfos != null && memberInfos.Length > 0)
            {
                object[] objs = memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    return ((DescriptionAttribute)objs[0]).Description;
                }
            }
            return @enum.ToString();
        }

        public static IEnumerable<FileInfo> GetAllSubFiles(DirectoryInfo directoryInfo)
        {
            ConcurrentBag<FileInfo> ret;
            try
            {
                ret = new ConcurrentBag<FileInfo>(directoryInfo.GetFiles());
            }
            catch
            {
                return new List<FileInfo>();
            }
            Parallel.ForEach(directoryInfo.GetDirectories(), subdir =>
            {
                foreach (var fileInfo in GetAllSubFiles(subdir))
                {
                    ret.Add(fileInfo);
                }
            });
            return ret;
        }

        public static IntPtr GetWindowHandle(Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        //窗体置顶
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        //取消窗体置顶
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        //不调整窗体位置
        private const uint SWP_NOMOVE = 0x0002;

        //不调整窗体大小
        private const uint SWP_NOSIZE = 0x0001;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags
        );

        public static void SetTop(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }
    }
}
