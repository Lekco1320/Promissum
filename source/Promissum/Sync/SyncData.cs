using System.Collections.Concurrent;
using System.IO;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// The class storing data for sync execution.
    /// </summary>
    public class SyncData
    {
        /// <summary>
        /// The dictoionary storing information of files which need to be synced.
        /// </summary>
        public ConcurrentDictionary<FileInfo, string> NewFiles { get; set; }

        /// <summary>
        /// The bag storing information of directories which need to be created in destination path.
        /// </summary>
        public ConcurrentBag<DirectoryInfo> NewDirectories { get; set; }

        /// <summary>
        /// The bag storing information of directories which need to be deleted.
        /// </summary>
        public ConcurrentBag<DirectoryInfo> UnexpectedDirectories { get; set; }

        /// <summary>
        /// The bag storing information of files found unexpected in destination.
        /// </summary>
        public ConcurrentBag<FileInfo> UnexpectedFiles { get; set; }

        /// <summary>
        /// The bag storing information of files which have been moved.
        /// </summary>
        public ConcurrentBag<FileInfo> MovedFiles { get; set; }

        /// <summary>
        /// The bag storing information of files which have been deleted.
        /// </summary>
        public ConcurrentBag<FileInfo> DeletedFiles { get; set; }

        /// <summary>
        /// The bag storing information of out-dated files' records.
        /// </summary>
        public ConcurrentBag<DeletionFileRecord> DeletingFileRecords { get; set; }

        /// <summary>
        /// The bag storing information of directories which have been deleted.
        /// </summary>
        public ConcurrentBag<DirectoryInfo> DeletedDirectories { get; set; }

        /// <summary>
        /// The bag storing records of files and directories which failed when syncing.
        /// </summary>
        public ConcurrentBag<FailedSyncRecord> FailedSyncRecords { get; set; }

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        public SyncData()
        {
            NewFiles = new ConcurrentDictionary<FileInfo, string>();
            NewDirectories = new ConcurrentBag<DirectoryInfo>();
            UnexpectedDirectories = new ConcurrentBag<DirectoryInfo>();
            UnexpectedFiles = new ConcurrentBag<FileInfo>();
            DeletingFileRecords = new ConcurrentBag<DeletionFileRecord>();
            MovedFiles = new ConcurrentBag<FileInfo>();
            DeletedFiles = new ConcurrentBag<FileInfo>();
            DeletedDirectories = new ConcurrentBag<DirectoryInfo>();
            FailedSyncRecords = new ConcurrentBag<FailedSyncRecord>();
        }
    }
}
