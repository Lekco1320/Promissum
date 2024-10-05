using System;
using System.Runtime.Serialization;
using Lekco.Promissum.Model.Sync.Base;

namespace Lekco.Promissum.Model.Sync.Record
{
    /// <summary>
    /// The class describes sync record of a file.
    /// </summary>
    [DataContract]
    public class FileRecord : FileRecordBase
    {
        /// <summary>
        /// The count of sync.
        /// </summary>
        [DataMember]
        public int SyncCount { get; set; }

        /// <summary>
        /// Last sync time of the file.
        /// </summary>
        [DataMember]
        public DateTime LastSyncTime { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public FileRecord()
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="id">Unique ID of the record.</param>
        /// <param name="file">File the record described.</param>
        /// <param name="relativeName">Relative name to target path.</param>
        public FileRecord(FileBase file, string relativeName)
            : base(file, relativeName)
        {
            SyncCount = 1;
            LastSyncTime = DateTime.Now;
        }

        /// <summary>
        /// Update information of file.
        /// </summary>
        /// <param name="file">The record's file.</param>
        public void SyncAndUpdate(FileBase file)
        {
            FileSize = file.Size;
            CreationTime = file.CreationTime;
            LastWriteTime = file.LastWriteTime;
            SyncCount += 1;
        }
    }
}
