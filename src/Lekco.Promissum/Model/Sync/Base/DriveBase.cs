using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for drive for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DriveType}, {Model}, {Name}")]
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

    /// <summary>
    /// Defines constants for drive types.
    /// </summary>
    public enum DriveType
    {
        /// <summary>
        /// Unkown type.
        /// </summary>
        [Description("未知驱动器类型")]
        Unknown = 0,

        /// <summary>
        /// Drive without root directory.
        /// </summary>
        [Description("无根目录的驱动器")]
        NoRootDirectory = 1,

        /// <summary>
        /// Removable drive.
        /// </summary>
        [Description("可移动驱动器")]
        Removable = 2,

        /// <summary>
        /// Fixed drive.
        /// </summary>
        [Description("固定驱动器")]
        Fixed = 3,

        /// <summary>
        /// Network drive.
        /// </summary>
        [Description("网络驱动器")]
        Network = 4,

        /// <summary>
        /// CD-ROM drive.
        /// </summary>
        [Description("光盘驱动器")]
        CDRom = 5,

        /// <summary>
        /// RAM drive.
        /// </summary>
        [Description("RAM磁盘驱动器")]
        Ram = 6,

        /// <summary>
        /// Generic MTP device.
        /// </summary>
        [Description("通用MTP设备")]
        GenericMTPDevice = 7,

        /// <summary>
        /// Camera device.
        /// </summary>
        [Description("照相机")]
        Camera = 8,

        /// <summary>
        /// Media player device.
        /// </summary>
        [Description("媒体播放器")]
        MediaPlayer = 9,

        /// <summary>
        /// Phone device.
        /// </summary>
        [Description("手机")]
        Phone = 10,

        /// <summary>
        /// A video device.
        /// </summary>
        [Description("视频设备")]
        Video = 11,

        /// <summary>
        /// Personal information manager device.
        /// </summary>
        [Description("个人信息管理器")]
        PersonalInformationManager = 12,

        /// <summary>
        /// Audio recorder.
        /// </summary>
        [Description("音频录音设备")]
        AudioRecorder = 13,
    }
}
