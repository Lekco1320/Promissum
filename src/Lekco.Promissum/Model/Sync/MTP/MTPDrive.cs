using Lekco.Promissum.Control;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Disk;
using Lekco.Wpf.Utility.Helper;
using MediaDevices;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// The class describes a MTP drive for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name,nq}, {Model,nq}, {DriveType}")]
    public class MTPDrive : DriveBase, IMTPDevice
    {
        /// <summary>
        /// A protected field for getting current info of the device.
        /// </summary>
        public MediaDevice Device { get; protected set; }

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
                    var queryStr = Device.FunctionalObjects(FunctionalCategory.Storage);
                    var info = Device.GetStorageInfo(queryStr.First());
                    _availableSpace = (long)info.FreeSpaceInBytes;
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
                    var queryStr = Device.FunctionalObjects(FunctionalCategory.Storage);
                    var info = Device.GetStorageInfo(queryStr.First());
                    _totalSpace = (long)info.Capacity;
                }
                return _totalSpace;
            }
        }

        /// <inheritdoc />
        public override MTPDirectory RootDirectory
        {
            get
            {
                if (!IsReady)
                    throw new DriveNotReadyException($"Drive \"{Name}\" is not ready.", this);

                return new MTPDirectory(Device.GetRootDirectory(), Device);
            }
        }

        /// <inheritdoc />
        public override string Root => RootDirectory.FullName;

        /// <inheritdoc />
        public override PathBase RootPath => new MTPPath(this, Root);

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="mediaDevice">A media device.</param>
        public MTPDrive(MediaDevice mediaDevice)
        {
            Device = mediaDevice;
            mediaDevice.Connect();
            isReady = mediaDevice.IsConnected;
            Name = Model = Device.Model;
            DriveType = (Base.DriveType)((int)Device.DeviceType + 7);
            ID = $"MTP_{Device.SerialNumber}";
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
        /// Check the drive whether gets ready and get the right device.
        /// Invokes when any drive connects or disconnects.
        /// </summary>
        private void CheckIsReady(object? sender, EventArgs e)
        {
            foreach (var device in MediaDevice.GetDevices())
            {
                string? model = null, sn = null;
                try
                {
                    device.Connect();
                    model = device.Model;
                    sn = $"MTP_{device.SerialNumber}";
                }
                finally
                {
                    model ??= "Unknown";
                    sn ??= $"MTP_{model.GetHashCode()}";
                    device.Disconnect();
                }
                if (Model == model && ID == sn)
                {
                    Device = device;
                    isReady = true;
                    device.DeviceRemoved += CheckIsReady;
                    device.DeviceReset += CheckIsReady;
                    device.Connect();
                    return;
                }
            }
        }

        /// <inheritdoc />
        public override DirectoryBase GetDirectory(string path)
            => new MTPDirectory(Device.GetDirectoryInfo(path), Device);

        /// <inheritdoc />
        public override FileBase GetFile(string path)
            => new MTPFile(Device.GetFileInfo(path), Device);

        /// <inheritdoc />
        public override void OpenFile(FileBase file)
        {
            if (file is not MTPFile mtpFile || mtpFile.Device != Device)
                throw new InvalidOperationException($"文件(夹)\"{file.FullName}\"不在设备\"{Name}\"中。");

            var diskInfo = new FileInfo(App.Promissum.TempDir + '\\' + file.Name);
            var diskFile = new DiskFile(diskInfo);
            if (mtpFile.TryCopyTo(diskFile, out var exRecord))
            {
                diskInfo.IsReadOnly = true;
                try
                {
                    Process.Start(new ProcessStartInfo(diskFile.FullName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowError($"文件\"{file.FullName}\"打开失败：{ex.Message}。");
                }
            }
            else
            {
                DialogHelper.ShowError($"文件\"{file.FullName}\"打开失败：{exRecord.ExceptionType.GetDiscription()}。");
            }
        }

        /// <inheritdoc />
        public override void OpenInExplorer(FileSystemBase entity)
        {
            if (entity is not IMTPDevice device || device.Device != Device)
                throw new InvalidOperationException($"文件(夹)\"{entity.FullName}\"不在设备\"{Name}\"中。");

            if (!IsReady)
                throw new DriveNotReadyException($"设备\"{Name}\"尚未就绪。", this);

            if (!entity.Exists)
                throw new FileNotFoundException($"文件(夹)\"{entity.FullName}\"不存在。", entity.FullName);

            var directory = entity is MTPFile file ? file.Parent : (DirectoryBase)entity;
            new ExplorerWindow(this, directory).Show();
        }

        /// <inheritdoc />
        public override string GetRelativePath(FileSystemBase entity)
        {
            if (entity is not (MTPFile or MTPDirectory))
                throw new InvalidOperationException($"文件(夹)\"{entity.FullName}\"不在设备\"{Name}\"中。");

            return entity.FullName;
        }
    }
}
