using MediaDevices;
using System.Collections.Generic;
using System.IO;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// Helper class for taking safe operations to MTP objects.
    /// </summary>
    public static class MTPHelper
    {
        /// <summary>
        /// An internal lock to ensure thread safety.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Delete a given file safely.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given file need deleting.</param>
        public static void SafeDeleteFile(this MediaDevice device, string mtpPath)
        {
            lock (_lock)
            {
                device.DeleteFile(mtpPath);
            }
        }

        /// <summary>
        /// Create a directory in a MTP device safely.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given directory need deleting.</param>
        public static void SafeCreateDirectory(this MediaDevice device, string mtpPath)
        {
            lock ( _lock)
            {
                device.CreateDirectory(mtpPath);
            }
        }

        /// <summary>
        /// Delete a given directory safely.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given directory need deleting.</param>
        /// <param name="recursive">Indicates whether delete the directory recursively.</param>
        public static void SafeDeleteDirectory(this MediaDevice device, string mtpPath, bool recursive = false)
        {
            lock (_lock)
            {
                device.DeleteDirectory(mtpPath, recursive);
            }
        }

        /// <summary>
        /// Download a file on MTP device to stream safely.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given file need deleting.</param>
        /// <param name="stream">Destination stream.</param>
        public static void SafeDownloadTo(this MediaDevice device, string mtpPath, Stream stream)
        {
            try
            {
                stream.Position = 0;
                lock (_lock)
                {
                    device.DownloadFile(mtpPath, stream);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Upload a file to MTP device from stream safely.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="stream">Source stream.</param>
        /// <param name="mtpPath">Path of given file need uploading.</param>
        public static void SafeUploadTo(this MediaDevice device, Stream stream, string mtpPath)
        {
            try
            {
                stream.Position = 0;
                lock (_lock)
                {
                    device.UploadFile(stream, mtpPath);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Check whether the file of the given path exists.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given file.</param>
        /// <returns><see langword="true"/> if file exists; otherwise, returns <see langword="false"/>.</returns>
        public static bool SafeFileExists(this MediaDevice device, string mtpPath)
        {
            lock (_lock)
            {
                return device.FileExists(mtpPath);
            }
        }

        /// <summary>
        /// Check whether the directory of the given path exists.
        /// </summary>
        /// <param name="device">Target MTP device.</param>
        /// <param name="mtpPath">Path of given file.</param>
        /// <returns><see langword="true"/> if directory exists; otherwise, returns <see langword="false"/>.</returns>
        public static bool SafeDirectoryExists(this MediaDevice device, string mtpPath)
        {
            lock (_lock)
            {
                return device.DirectoryExists(mtpPath);
            }
        }

        /// <summary>
        /// Enumerate files of a directory safely.
        /// </summary>
        /// <param name="directoryInfo">Information of target directory.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Files in the directory.</returns>
        public static IEnumerable<MediaFileInfo> SafeEnumerateFiles(this MediaDirectoryInfo directoryInfo, string pattern)
        {
            lock ( _lock)
            {
                return directoryInfo.EnumerateFiles(pattern);
            }
        }

        /// <summary>
        /// Enumerate directories of a directory safely.
        /// </summary>
        /// <param name="directoryInfo">Information of target directory.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Directories in the directory.</returns>
        public static IEnumerable<MediaDirectoryInfo> SafeEnumerateDirectories(this MediaDirectoryInfo directoryInfo, string pattern)
        {
            lock ( _lock)
            {
                return directoryInfo.EnumerateDirectories(pattern);
            }
        }
    }
}
