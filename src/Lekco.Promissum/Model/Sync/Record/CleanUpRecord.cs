using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        /// Versions of reserved files.
        /// </summary>
        [DataMember]
        public string ReservedVersions { get; protected set; }

        /// <summary>
        /// Versions of reserved files in list form.
        /// </summary>
        public List<int> ReservedVersionList
        {
            get
            {
                string[] splitted = ReservedVersions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return splitted.Length > 0 ? splitted.Select(int.Parse).ToList() : new List<int>();
            }
            set => ReservedVersions = string.Join(',', value);
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public CleanUpRecord()
        {
            ReservedVersions = "";
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
            ReservedVersions = "";
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
    }
}
