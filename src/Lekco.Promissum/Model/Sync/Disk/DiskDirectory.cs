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
    [DebuggerDisplay("{FullName}")]
    public class DiskDirectory : DirectoryBase
    {
        /// <inheritdoc />
        public override string Name => Path.GetFileName(FullName) ?? "";

        /// <inheritdoc />
        public override bool Exists => Directory.Exists(FullName);

        /// <inheritdoc />
        public override DateTime LastWriteTime { get; protected set; }

        /// <inheritdoc />
        public override DirectoryBase Parent
        {
            get
            {
                var info = new DirectoryInfo(FullName);
                var parent = info.Parent ?? info;
                return new DiskDirectory(parent);
            }
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the directory.</param>
        public DiskDirectory(string fullName)
            : base(fullName)
        {
            if (Exists)
            {
                LastWriteTime = Directory.GetLastWriteTime(FullName);
            }
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directoryInfo">Information of the directory.</param>
        public DiskDirectory(DirectoryInfo directoryInfo)
            : this(directoryInfo.FullName)
        {
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
            return Directory.EnumerateDirectories(FullName, searchPattern).Select(s => new DiskDirectory(s));
        }

        /// <inheritdoc />
        public override IEnumerable<FileBase> EnumerateFiles(string searchPattern = "*")
        {
            return Directory.EnumerateFiles(FullName, searchPattern).Select(s => new DiskFile(s));
        }
    }
}
