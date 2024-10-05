using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// The class describes disk directory for MTP device.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName}")]
    public class MTPDirectory : DirectoryBase, IMTPDevice
    {
        /// <inheritdoc />
        public MediaDevice Device { get; protected set; }

        /// <inheritdoc />
        public override string Name => Path.GetFileName(FullName);

        /// <inheritdoc />
        public override bool Exists => Device.SafeDirectoryExists(FullName);

        /// <inheritdoc />
        public override DateTime LastWriteTime { get; protected set; }

        /// <inheritdoc />
        public override DirectoryBase Parent
        {
            get
            {
                if (info == null)
                    throw new DriveNotReadyException($"目录\"{FullName}\"的设备尚未就绪。");

                return new MTPDirectory(info.Parent, Device);
            }
        }

        /// <summary>
        /// A protected field for getting current info of the directory.
        /// </summary>
        protected MediaDirectoryInfo? info;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directoryInfo">Info of the directory.</param>
        /// <param name="device">Parent drive of the directory.</param>
        public MTPDirectory(MediaDirectoryInfo directoryInfo, MediaDevice device)
            : base(directoryInfo.FullName)
        {
            info = directoryInfo;
            Device = device;
            LastWriteTime = info.LastWriteTime ?? default;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directoryPath">Path of the directory.</param>
        /// <param name="device">Parent drive of the directory.</param>
        public MTPDirectory(string directoryPath, MediaDevice device)
            : base(directoryPath)
        {
            if (device.DirectoryExists(directoryPath))
            {
                info = device.GetDirectoryInfo(directoryPath);
            }
            Device = device;
        }

        /// <inheritdoc />
        public override IEnumerable<DirectoryBase> EnumerateDirectories(string searchPattern = "*")
        {
            if (info == null)
                throw new DriveNotReadyException($"目录\"{FullName}\"的设备尚未就绪。");

            return info.SafeEnumerateDirectories(searchPattern)
                       .Select(info => new MTPDirectory(info, Device));
        }

        /// <inheritdoc />
        public override IEnumerable<FileBase> EnumerateFiles(string searchPattern = "*")
        {
            if (info == null)
                throw new DriveNotReadyException($"目录\"{FullName}\"的设备尚未就绪。");
            
            return info.SafeEnumerateFiles(searchPattern)
                       .Select(info => new MTPFile(info, Device));
        }

        /// <inheritdoc />
        public override bool TryCreate([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                Device.SafeCreateDirectory(FullName);
                return true;
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Create, ExceptionType.DriveMissing, ex.Message);
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
        public override bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                Device.SafeDeleteDirectory(FullName, true);
                return true;
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.DriveMissing, ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.DirectoryNotFound, ex.Message);
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
