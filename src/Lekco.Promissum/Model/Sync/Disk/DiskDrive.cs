using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Disk
{
    /// <summary>
    /// The class describes a disk drive for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DriveType}, {Model}, {Name}")]
    public class DiskDrive : DriveBase
    {
        /// <inheritdoc />
        public override bool IsReady => isReady;

        /// <summary>
        /// Indicates whether the drive is ready.
        /// </summary>
        protected bool isReady;

        /// <inheritdoc />
        public override long AvailableSpace
        {
            get
            {
                if (IsReady)
                {
                    _availableSpace = info.AvailableFreeSpace;
                }
                return _availableSpace;
            }
        }

        /// <inheritdoc />
        public override long TotalSpace
        {
            get
            {
                if (IsReady)
                {
                    _totalSpace = info.TotalSize;
                }
                return _totalSpace;
            }
        }

        /// <inheritdoc />
        public override DirectoryBase RootDirectory => IsReady ? new DiskDirectory(info.RootDirectory)
                                                       : throw new DriveNotReadyException($"设备\"{Name}\"尚未就绪。", this);

        /// <inheritdoc />
        public override string Root => IsReady ? info.RootDirectory.FullName : "";

        /// <inheritdoc />
        public override PathBase RootPath => new DiskPath(this, Root);

        /// <summary>
        /// A protected field for getting current info of the drive.
        /// </summary>
        protected DriveInfo info;

        /// <summary>
        /// Letter of the drive.
        /// </summary>
        protected string driveLetter;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="driveInfo">DiveInfo of the hard disk.</param>
        public DiskDrive(DriveInfo driveInfo)
        {
            isReady = driveInfo.IsReady;
            Name = driveInfo.VolumeLabel;
            info = driveInfo;
            driveLetter = info.Name;
            GetSNAndModel(driveInfo, out var sn, out var model);
            ID = $"DISK_{sn}";
            Model = model;
            DriveType = (Base.DriveType)(int)info.DriveType;
        }

        /// <summary>
        /// Called after being deserialized to complete construction.
        /// </summary>
        /// <param name="context">Context of deserialization.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            SyncEngine.DrivesChanged += CheckIsReady;
            CheckIsReady(null, new EventArgs());
        }

        /// <summary>
        /// Get serial number and model of a logic drive.
        /// </summary>
        /// <param name="driveInfo">DiveInfo of the drive.</param>
        /// <param name="serialNumber">Serial number of the drive.</param>
        /// <param name="model">Model of the drive.</param>
        public static void GetSNAndModel(DriveInfo driveInfo, out string serialNumber, out string model)
        {
            string driveLetter = driveInfo.Name[..2];
            try
            {
                using var partitions = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE ResultClass=Win32_DiskPartition");
                foreach (var partition in partitions.Get())
                {
                    using var drives = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE ResultClass=Win32_DiskDrive");
                    foreach (var drive in drives.Get())
                    {
                        serialNumber = drive["SerialNumber"].ToString() ?? "Unknown";
                        model = drive["Model"].ToString() ?? "Unknown";
                        return;
                    }
                }
            }
            catch
            {
            }
            serialNumber = model = "Unknown";
        }

        /// <summary>
        /// Check the drive whether gets ready and get current volume letter of the drive.
        /// Invokes when any drive connects or disconnects.
        /// </summary>
        protected void CheckIsReady(object? sender, EventArgs e)
        {
            using var pDrives = new ManagementObjectSearcher($"SELECT * FROM Win32_DiskDrive WHERE Model = '{Model}'");
            foreach (var pDrive in pDrives.Get())
            {
                string deviceID = pDrive["DeviceID"].ToString() ?? "";
                string serialNumber = pDrive["SerialNumber"].ToString() ?? "Unknown";
                if (ID == $"DISK_{serialNumber}")
                {
                    using var partitionSearch = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceID}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition");
                    foreach (var partition in partitionSearch.Get())
                    {
                        using var lDrives = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition");
                        foreach (var lDrive in lDrives.Get())
                        {
                            string label = lDrive["VolumeName"].ToString() ?? string.Empty;
                            if (label == Name)
                            {
                                driveLetter = lDrive["Name"].ToString() ?? string.Empty;
                                info = new DriveInfo(driveLetter);
                                isReady = true;
                                return;
                            }
                        }
                    }
                }
            }
            isReady = false;
        }

        /// <inheritdoc />
        public override FileBase GetFile(string path)
            => new DiskFile(GetFullPath(path));

        /// <inheritdoc />
        public override DirectoryBase GetDirectory(string path)
            => new DiskDirectory(GetFullPath(path));

        /// <inheritdoc />
        public override void OpenFile(FileBase file)
        {
            if (file is not DiskFile)
                throw new InvalidOperationException($"文件\"{file.FullName}\"不在设备\"{Name}\"中。");

            if (!IsReady)
                throw new DriveNotReadyException($"设备\"{Name}\"尚未就绪。", this);

            if (!file.Exists)
                throw new FileNotFoundException($"文件\"{file.FullName}\"不存在。", file.FullName);

            Process.Start(new ProcessStartInfo(file.FullName) { UseShellExecute = true });
        }

        /// <inheritdoc />
        public override void OpenInExplorer(FileSystemBase entity)
        {
            if (entity is not (DiskFile or DiskDirectory))
                throw new InvalidOperationException($"文件(夹)\"{entity.FullName}\"不在设备\"{Name}\"中。");

            if (!IsReady)
                throw new DriveNotReadyException($"设备\"{Name}\"尚未就绪。", this);

            if (!entity.Exists)
                throw new FileNotFoundException($"文件(夹)\"{entity.FullName}\"不存在。", entity.FullName);

            Process.Start("explorer.exe", entity.FullName);
        }

        /// <inheritdoc />
        public override string GetRelativePath(FileSystemBase entity)
        {
            if (entity is not (DiskFile or DiskDirectory) || !entity.FullName.StartsWith(driveLetter))
                throw new InvalidOperationException($"文件(夹)\"{entity.FullName}\"不在设备\"{Name}\"中。");

            return entity.FullName.Length > 3 ? entity.FullName[3..] : "";
        }
    }
}
