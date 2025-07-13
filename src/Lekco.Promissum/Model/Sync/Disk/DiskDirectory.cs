using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;

namespace Lekco.Promissum.Model.Sync.Disk
{
    /// <summary>
    /// The class describes disk directory for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName,nq}")]
    public class DiskDirectory : DirectoryBase
    {
        /// <inheritdoc />
        public override string Name => Path.GetFileName(FullName) ?? "";

        /// <inheritdoc />
        public override bool Exists => Drive.IsReady && Directory.Exists(FullName);

        /// <inheritdoc />
        public override DateTime LastWriteTime { get; protected set; }

        /// <inheritdoc />
        public override DirectoryBase Parent
        {
            get
            {
                info ??= new DirectoryInfo(FullName);
                var parent = info.Parent ?? info;
                return new DiskDirectory(parent, (DiskDrive)Drive);
            }
        }

        /// <inheritdoc />
        public override DriveBase Drive { get; }

        /// <summary>
        /// A protected field for getting current info of the directory.
        /// </summary>
        protected DirectoryInfo? info;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the directory.</param>
        /// <param name="drive">Parent drive of the directory.</param>
        public DiskDirectory(string fullName, DiskDrive drive)
            : this(new DirectoryInfo(fullName), drive)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directoryInfo">Information of the directory.</param>
        /// <param name="drive">Parent drive of the directory.</param>
        public DiskDirectory(DirectoryInfo directoryInfo, DiskDrive drive)
            : base(directoryInfo.FullName)
        {
            Drive = drive;
            info = directoryInfo;
            if (info.Exists)
            {
                LastWriteTime = info.LastWriteTime;
            }
        }

        /// <inheritdoc />
        public override bool TryCreate([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                Directory.CreateDirectory(FullName);
                return true;
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Create, ExceptionType.NoPermission, ex.Message);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                Directory.Delete(FullName, true);
                return true;
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

        /// <inheritdoc />
        public override IEnumerable<DirectoryBase> EnumerateDirectories(string searchPattern = "*")
        {
            info ??= new DirectoryInfo(FullName);
            return info.EnumerateDirectories(searchPattern)
                       .Select(i => new DiskDirectory(i, (DiskDrive)Drive));
        }

        /// <inheritdoc />
        public override IEnumerable<FileBase> EnumerateFiles(string searchPattern = "*")
        {
            info ??= new DirectoryInfo(FullName);
            return info.EnumerateFiles(searchPattern)
                       .Select(i => new DiskFile(i, (DiskDrive)Drive));
        }
    }
}
