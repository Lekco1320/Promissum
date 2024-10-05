using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lekco.Promissum.Model.Sync.Base;

namespace Lekco.Promissum.Model.Sync.Record
{
    /// <summary>
    /// The class describes clean up record of a file.
    /// </summary>
    [DataContract]
    public class CleanUpRecord : FileRecordBase
    {
        /// <summary>
        /// Last operate time to the file.
        /// </summary>
        [DataMember]
        public DateTime LastOperateTime { get; set; }

        /// <summary>
        /// Versions of remained cleaned up files.
        /// </summary>
        [DataMember]
        public List<int> ReservedVersions { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public CleanUpRecord()
        {
            ReservedVersions = new List<int>();
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="id">Unique ID of the record.</param>
        /// <param name="file">File the record described.</param>
        /// <param name="relativeName">Relative name to target path.</param>
        public CleanUpRecord(FileBase file, string relativeName)
            : base(file, relativeName)
        {
            ReservedVersions = new List<int>() { 1 };
            LastOperateTime = DateTime.Now;
        }

        /// <summary>
        /// Update data of file.
        /// </summary>
        /// <param name="file">File the record described.</param>
        public void Update(FileBase file)
        {
            FileSize = file.Size;
            CreationTime = file.CreationTime;
            LastWriteTime = file.LastWriteTime;
        }

        /// <summary>
        /// Add a new version of the reserved file.
        /// </summary>
        /// <param name="version">Latest verion.</param>
        public void AddVersion(int version)
        {
            ReservedVersions.Add(version);
            LastOperateTime = DateTime.Now;
        }

        /// <summary>
        /// Remove a version whose index is given.
        /// </summary>
        /// <param name="versionIndex">Given version's index.</param>
        public void RemoveVersion(int versionIndex)
        {
            ReservedVersions.RemoveAt(versionIndex);
            LastOperateTime = DateTime.Now;
        }
    }
}
