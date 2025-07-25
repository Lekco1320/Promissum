﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for drive for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name,nq}, {Model,nq}, {DriveType}")]
    public abstract class DriveBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the drive.
        /// </summary>
        [DataMember]
        public string Name { get; protected set; }

        /// <summary>
        /// Model of the drive.
        /// </summary>
        [DataMember]
        public string Model { get; protected set; }

        /// <summary>
        /// Type of the drive.
        /// </summary>
        [DataMember]
        public DriveType DriveType { get; protected set; }

        /// <summary>
        /// File system type of the drive.
        /// </summary>
        [DataMember]
        public DriveFormat DriveFormat { get; protected set; }

        /// <summary>
        /// Root of the drive.
        /// </summary>
        public abstract string Root { get; }

        /// <summary>
        /// Root directory of the drive.
        /// </summary>
        public abstract DirectoryBase RootDirectory { get; }

        /// <summary>
        /// Root path of the drive.
        /// </summary>
        public abstract PathBase RootPath { get; }

        /// <summary>
        /// Unique ID of the drive.
        /// </summary>
        [DataMember]
        public string ID { get; protected set; }

        /// <summary>
        /// Whether the drive is ready to access.
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Available space of the drive.
        /// </summary>
        public abstract long AvailableSpace { get; }

        [DataMember]
        protected long _availableSpace;

        /// <summary>
        /// The size of the drive.
        /// </summary>
        public abstract long TotalSpace { get; }

        /// <summary>
        /// The total space of the drive.
        /// </summary>
        [DataMember]
        protected long _totalSpace;

        /// <summary>
        /// The used space of the drive.
        /// </summary>
        public virtual long UsedSpace => TotalSpace - AvailableSpace;

        /// <summary>
        /// Occurs when the readiness state changes.
        /// </summary>
        public abstract event EventHandler<bool>? IsReadyChanged;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        protected DriveBase()
        {
            Name = "";
            ID = "";
            Model = "Unknown";
        }

        /// <summary>
        /// Get full path of a path relative to this drive's root.
        /// </summary>
        /// <param name="path">Path relative to this drive's root.</param>
        /// <returns>Full path.</returns>
        protected virtual string GetFullPath(string path)
            => Root + (!path.StartsWith('\\') ? path : path[1..]);

        /// <summary>
        /// Get file system format enum with a given format string.
        /// </summary>
        /// <param name="format">Drive format string.</param>
        /// <returns>File system format enum.</returns>
        protected virtual DriveFormat GetDriveFormat(string format)
            => format.ToLower() switch
            {
                "ntfs" => DriveFormat.NTFS,
                "fat32" => DriveFormat.FAT32,
                "exfat" => DriveFormat.exFAT,
                "cdfs" => DriveFormat.CDFS,
                "refs" => DriveFormat.ReFS,
                "udf" => DriveFormat.UDF,
                "ext4" => DriveFormat.ext4,
                "XFS" => DriveFormat.XFS,
                "ZFS" => DriveFormat.ZFS,
                _ => DriveFormat.Unknown,
            };

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is DriveBase other)
            {
                return ID == other.ID && Model == other.Model;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(ID, Model);

        /// <summary>
        /// Check the drive whether is ready.
        /// </summary>
        /// <returns><see langword="true"/> if is ready; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool CheckIsReady();

        /// <summary>
        /// Get a file in the drive.
        /// </summary>
        /// <param name="path">The file's relative path to the drive.</param>
        /// <returns>The file.</returns>
        public abstract FileBase GetFile(string path);

        /// <summary>
        /// Get a directory in the drive.
        /// </summary>
        /// <param name="path">The directory's relative path to the drive.</param>
        /// <returns>The directory.</returns>
        public abstract DirectoryBase GetDirectory(string path);

        /// <summary>
        /// Open a specified file with default program.
        /// </summary>
        /// <param name="file">The specified file.</param>
        public abstract void OpenFile(FileBase file);

        /// <summary>
        /// Open a file system entity in explorer.
        /// </summary>
        /// <param name="entity">An entity in the drive.</param>
        public abstract void OpenInExplorer(FileSystemBase entity);

        /// <summary>
        /// Get given entity's relative path to the root of this drive.
        /// </summary>
        /// <param name="entity">Given entity of file system.</param>
        /// <returns>Relative path to the root of this drive.</returns>
        public abstract string GetRelativePath(FileSystemBase entity);
    }
}
