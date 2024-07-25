using Lekco.Promissum.Utility;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncPath : INotifyPropertyChanged
    {
        [DataMember]
        public string RelativePath
        {
            get => _relativePath;
            protected set
            {
                _relativePath = value;
                OnPropertyChanged(nameof(RelativePath));
            }
        }
        private string _relativePath;

        [DataMember]
        public int DriveId
        {
            get => _driveId;
            protected set
            {
                _driveId = value;
                OnPropertyChanged(nameof(DriveId));
            }
        }
        private int _driveId;

        [DataMember]
        public string DriveName
        {
            get => _driveName;
            protected set
            {
                _driveName = value;
                OnPropertyChanged(nameof(DriveName));
            }
        }
        private string _driveName;

        [DataMember]
        public long FreeSpace
        {
            get => _freeSpace;
            protected set
            {
                _freeSpace = value;
                OnPropertyChanged(nameof(FreeSpace));
            }
        }
        private long _freeSpace;

        [DataMember]
        public long TotalSpace
        {
            get => _totalSpace;
            protected set
            {
                _totalSpace = value;
                OnPropertyChanged(nameof(TotalSpace));
            }
        }
        private long _totalSpace;

        public long UsedSpace { get => TotalSpace - FreeSpace; }

        public double UsedSpacePercentage { get => (double)UsedSpace / TotalSpace * 100d; }

        public string FullPath { get => DriveName + RelativePath; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SyncPath(string path)
        {
            if (!Functions.TryGetDiskName(path, out string? oriDiskName, out string oriFileName))
            {
                throw new InvalidDataException("Path is invalid.");
            }
            _relativePath = oriFileName;
            var driveInfo = new DriveInfo(oriDiskName);
            _driveId = Functions.HardDiskId(driveInfo.Name);
            _driveName = driveInfo.Name;
            _totalSpace = driveInfo.IsReady ? driveInfo.TotalSize : 0L;
            _freeSpace = driveInfo.IsReady ? driveInfo.TotalFreeSpace : 0L;
        }

        public bool TryMatch()
        {
            foreach (var driveInfo in DriveInfo.GetDrives())
            {
                if (Functions.HardDiskId(driveInfo.Name) == DriveId && Directory.Exists(FullPath))
                {
                    DriveName = driveInfo.Name;
                    TotalSpace = driveInfo.TotalSize;
                    FreeSpace = driveInfo.TotalFreeSpace;
                    return true;
                }
            }
            return false;
        }

        public static bool IsSubPath(SyncPath fatherPath, SyncPath subPath)
        {
            return subPath.FullPath.StartsWith(fatherPath.FullPath);
        }

        public DirectoryInfo GetPathInfo()
        {
            return new DirectoryInfo(FullPath);
        }

        public static bool operator ==(SyncPath? pathA, SyncPath? pathB)
        {
            if (pathA is null && pathB is not null || pathA is not null && pathB is null)
            {
                return false;
            }
            if (pathA is null && pathB is null)
            {
                return true;
            }
            return pathA!.FullPath == pathB!.FullPath && (pathA.DriveId == pathB.DriveId || pathA.DriveId * pathB.DriveId == 0);
        }

        public static bool operator !=(SyncPath? pathA, SyncPath? pathB)
        {
            if (pathA is null && pathB is not null || pathA is not null && pathB is null)
            {
                return true;
            }
            if (pathA is null && pathB is null)
            {
                return false;
            }
            return pathA!.FullPath != pathB!.FullPath || (pathA.DriveId != pathB.DriveId && pathA.DriveId * pathB.DriveId != 0);
        }

        public override bool Equals(object? obj)
        {
            return this == (SyncPath?)obj;
        }

        public override int GetHashCode()
        {
            return (DriveId + FullPath).GetHashCode();
        }

        public override string ToString()
        {
            return FullPath;
        }
    }
}
