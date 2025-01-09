using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for file's record during sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{RelativeFileName,nq}")]
    public abstract class FileRecordBase : RecordBase
    {
        /// <summary>
        /// File's relative name.
        /// </summary>
        [DataMember]
        public string RelativeFileName { get; set; }

        /// <summary>
        /// Size of the file.
        /// </summary>
        [DataMember]
        public long FileSize { get; set; }

        /// <summary>
        /// Creation time of the file.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Last write time of the file.
        /// </summary>
        [DataMember]
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public FileRecordBase()
        {
            RelativeFileName = "";
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="id">Unique ID of the record.</param>
        /// <param name="file">File the record described.</param>
        /// <param name="relativeName">Relative name to target path.</param>
        protected FileRecordBase(FileBase file, string relativeName)
        {
            RelativeFileName = relativeName;
            FileSize = file.Size;
            CreationTime = file.CreationTime;
            LastWriteTime = file.LastWriteTime;
        }
    }
}
