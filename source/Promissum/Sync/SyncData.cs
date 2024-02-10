using System.Collections.Concurrent;
using System.IO;

namespace Lekco.Promissum.Sync
{
    public class SyncData
    {
        public ConcurrentDictionary<FileInfo, string> NewFiles { get; set; }
        public ConcurrentBag<DirectoryInfo> NewDirectories { get; set; }
        public ConcurrentBag<DirectoryInfo> UnexpectedDirectories { get; set; }
        public ConcurrentBag<FileInfo> UnexpectedFiles { get; set; }
        public ConcurrentBag<FileInfo> DeletingFiles { get; set; }
        public ConcurrentBag<FileInfo> MovedFiles { get; set; }
        public ConcurrentBag<FileInfo> DeletedFiles { get; set; }
        public ConcurrentBag<DirectoryInfo> DeletedDirectories { get; set; }
        public ConcurrentBag<FailedSyncRecord> FailedSyncRecords { get; set; }

        public SyncData()
        {
            NewFiles = new ConcurrentDictionary<FileInfo, string>();
            NewDirectories = new ConcurrentBag<DirectoryInfo>();
            UnexpectedDirectories = new ConcurrentBag<DirectoryInfo>();
            UnexpectedFiles = new ConcurrentBag<FileInfo>();
            DeletingFiles = new ConcurrentBag<FileInfo>();
            MovedFiles = new ConcurrentBag<FileInfo>();
            DeletedFiles = new ConcurrentBag<FileInfo>();
            DeletedDirectories = new ConcurrentBag<DirectoryInfo>();
            FailedSyncRecords = new ConcurrentBag<FailedSyncRecord>();
        }
    }
}
