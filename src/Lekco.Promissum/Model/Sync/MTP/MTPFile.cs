using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Disk;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Promissum.Utility;
using MediaDevices;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Security;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// The class describes a file for MTP device.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName}")]
    public class MTPFile : FileBase, IMTPDevice
    {
        /// <summary>
        /// Parent device of the directory.
        /// </summary>
        public MediaDevice Device { get; protected set; }

        /// <inheritdoc />
        public override string Name => Path.GetFileName(FullName);

        /// <inheritdoc />
        public override string Extension => Path.GetExtension(Name);

        /// <inheritdoc />
        public override bool Exists => Device.SafeFileExists(FullName);

        /// <inheritdoc />
        public override DateTime LastWriteTime { get; protected set; }

        /// <inheritdoc />
        public override FileStream Stream => (FileStream)info!.OpenRead();

        /// <inheritdoc />
        public override string NameWithoutExtension => Path.GetFileNameWithoutExtension(Name);

        /// <inheritdoc />
        public override DirectoryBase Parent
        {
            get
            {
                if (info == null)
                    throw new DriveNotReadyException($"Parent drive of \"{FullName}\" is not ready.");

                return new MTPDirectory(info.Directory, Device);
            }
        }

        /// <summary>
        /// A protected field for getting current info of the file.
        /// </summary>
        protected MediaFileInfo? info;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fileInfo">Info of the file.</param>
        /// <param name="device">Parent drive of the file.</param>
        public MTPFile(MediaFileInfo fileInfo, MediaDevice device)
            : base(fileInfo.FullName)
        {
            info = fileInfo;
            Device = device;
            Size = (long)info.Length;
            CreationTime = info.CreationTime ?? default;
            LastWriteTime = info.LastWriteTime ?? default;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="filePath">Path of the file.</param>
        /// <param name="device">Parent drive of the file.</param>
        public MTPFile(string filePath, MediaDevice device)
            : base(filePath)
        {
            if (device.FileExists(filePath))
            {
                info = device.GetFileInfo(filePath);
            }
            Device = device;
        }

        /// <inheritdoc />
        public override bool TryCopyTo(FileBase newFile, [MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                switch (newFile)
                {
                case DiskFile:
                    using (var stream = new FileStream(newFile.FullName, FileMode.Create))
                    {
                        Device.SafeDownloadTo(FullName, stream);
                    }
                    return true;

                case MTPFile:
                    // If available memory > 256MB and size < 128MB, use MemoryStream to optimize performance.
                    ulong available = ComputerHelper.GetAvailableMemory();
                    if (available > (1 << 28) && Size < (1 << 27))
                    {
                        using var stream = new MemoryStream();
                        Device.SafeDownloadTo(FullName, stream);
                        Device.SafeUploadTo(stream, newFile.FullName);
                    }
                    else
                    {
                        var tmpName = Path.GetTempFileName();
                        using var stream = new FileStream(tmpName, FileMode.Create);
                        Device.SafeDownloadTo(FullName, stream);
                        Device.SafeUploadTo(stream, newFile.FullName);
                        File.Delete(tmpName);
                    }
                    return true;

                default:
                    throw new ArgumentException("Unknown file type.", nameof(newFile));
                }
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.DriveMissing, ex.Message);
            }
            catch (SecurityException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.NoPermission, ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.DirectoryNotFound, ex.Message);
            }
            catch (PathTooLongException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.PathTooLong, ex.Message);
            }
            catch (IOException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.FileOccupied, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.UnauthorizedAccess, ex.Message);
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Copy, ExceptionType.Unknown, ex.Message);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                Device.DeleteFile(FullName);
                return true;
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.DriveMissing, ex.Message);
            }
            catch (ArgumentException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.InvalidPath, ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.FileNotFound, ex.Message);
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.Unknown, ex.Message);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryMoveTo(FileBase newFile, bool overwrite, [MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                switch (newFile)
                {
                case DiskFile:
                    using (var stream = new FileStream(newFile.FullName, FileMode.Create))
                    {
                        Device.SafeDownloadTo(FullName, stream);
                        Device.SafeDeleteFile(FullName);
                    }
                    return true;

                case MTPFile:
                    // If available memory > 256MB and size < 128MB, use MemoryStream to optimize performance.
                    ulong available = ComputerHelper.GetAvailableMemory();
                    if (available > (1 << 28) && Size < (1 << 27))
                    {
                        using var stream = new MemoryStream();
                        Device.SafeDownloadTo(FullName, stream);
                        Device.SafeUploadTo(stream, newFile.FullName);
                        Device.SafeDeleteFile(FullName);
                    }
                    else
                    {
                        string tmpName = Path.GetTempFileName();
                        using var stream = new FileStream(tmpName, FileMode.Create);
                        Device.SafeDownloadTo(FullName, stream);
                        Device.SafeUploadTo(stream, newFile.FullName);
                        File.Delete(tmpName);
                        Device.SafeDeleteFile(FullName);
                    }
                    return true;

                default:
                    throw new ArgumentException("Unknown file type.", nameof(newFile));
                }
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.DriveMissing, ex.Message);
            }
            catch (IOException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.InvalidPath, ex.Message);
            }
            catch (SecurityException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.NoPermission, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.UnauthorizedAccess, ex.Message);
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.Unknown, ex.Message);
            }

            return false;
        }
    }
}
