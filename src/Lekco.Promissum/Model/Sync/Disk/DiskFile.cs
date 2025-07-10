using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.MTP;
using Lekco.Promissum.Model.Sync.Record;
using MediaDevices;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Security;

namespace Lekco.Promissum.Model.Sync.Disk
{
    /// <summary>
    /// The class describes a disk file for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName,nq}")]
    public class DiskFile : FileBase
    {
        /// <inheritdoc />
        public override string Name => Path.GetFileName(FullName);

        /// <inheritdoc />
        public override string Extension => Path.GetExtension(FullName);

        /// <inheritdoc />
        public override bool Exists => File.Exists(FullName);

        /// <inheritdoc />
        public override DateTime LastWriteTime { get; protected set; }

        /// <inheritdoc />
        public override FileStream Stream => new FileStream(FullName, FileMode.Open, FileAccess.Read);

        /// <inheritdoc />
        public override string NameWithoutExtension => Path.GetFileNameWithoutExtension(FullName);

        /// <inheritdoc />
        public override DirectoryBase Parent => new DiskDirectory(Path.GetDirectoryName(FullName)!);

        /// <summary>
        /// A protected field for getting current info of the directory.
        /// </summary>
        protected FileInfo? info;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the file.</param>
        public DiskFile(string fullName)
            : this(new FileInfo(fullName))
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fileInfo">Information of the file.</param>
        public DiskFile(FileInfo fileInfo)
            : base(fileInfo.FullName)
        {
            info = fileInfo;
            if (info.Exists)
            {
                Size = info.Length;
                CreationTime = info.CreationTime;
                LastWriteTime = info.LastWriteTime;
            }
        }

        public bool TryDeleteReadOnly([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            info ??= new FileInfo(FullName);
            try
            {
                info.IsReadOnly = false;
                info.Delete();
                return true;
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Delete, ExceptionType.UnauthorizedAccess, ex.Message);
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
                    File.Move(FullName, newFile.FullName, overwrite);
                    return true;

                case MTPFile mtpFile:
                    using (var stream = new FileStream(FullName, FileMode.Open))
                    {
                        mtpFile.Device.SafeUploadTo(stream, newFile.FullName);
                    }
                    File.Delete(FullName);
                    return true;

                default:
                    throw new ArgumentException("Unknown file type.", nameof(newFile));
                }
            }
            catch (NotConnectedException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.DriveMissing, ex.Message);
            }
            catch (SecurityException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.NoPermission, ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return TryDeleteReadOnly(out exRecord);                
            }
            catch (FileNotFoundException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.FileNotFound, ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.DirectoryNotFound, ex.Message);
            }
            catch (PathTooLongException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.PathTooLong, ex.Message);
            }
            catch (IOException ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.FileOccupied, ex.Message);
            }
            catch (Exception ex)
            {
                exRecord = new ExceptionRecord(FullName, OperationType.Move, ExceptionType.Unknown, ex.Message);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                File.Delete(FullName);
                return true;
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
        public override bool TryCopyTo(FileBase newFile, [MaybeNullWhen(true)] out ExceptionRecord exRecord)
        {
            exRecord = null;
            try
            {
                switch (newFile)
                {
                case DiskFile:
                    File.Copy(FullName, newFile.FullName, true);
                    newFile = new DiskFile(FullName);
                    return true;

                case MTPFile mtpFile:
                    using (var stream = new FileStream(FullName, FileMode.Open))
                    {
                        mtpFile.Device.SafeUploadTo(stream, newFile.FullName);
                    }
                    return true;

                default:
                    throw new ArgumentException("Unknown file type.", nameof(newFile));
                }
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
    }
}
