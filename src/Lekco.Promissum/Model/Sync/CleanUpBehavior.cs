using System;
using System.Runtime.Serialization;
using Lekco.Promissum.Model.Sync.Base;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Represents the clean up behavior configuration for files in destination path.
    /// </summary>
    [DataContract]
    public class CleanUpBehavior
    {
        /// <summary>
        /// Indicates whether to enable reservation of files during clean up.
        /// </summary>
        [DataMember]
        public bool EnableReserve { get; set; }

        /// <summary>
        /// The path to be reserved for cleaned up files.
        /// </summary>
        [DataMember]
        public PathBase? ReservedPath { get; set; }

        /// <summary>
        /// Indicates whether to enable the retention period for reserved files.
        /// </summary>
        [DataMember]
        public bool EnableRetentionPeriod { get; set; }

        /// <summary>
        /// The retention period for reserved files.
        /// </summary>
        [DataMember]
        public TimeSpan RetentionPeriod { get; set; }

        /// <summary>
        /// Indicates whether file versioning is enabled.
        /// </summary>
        [DataMember]
        public bool EnableVersioning { get; set; }

        /// <summary>
        /// Indicates whether to enable retention of the maximum number of file versions.
        /// </summary>
        [DataMember]
        public bool EnableMaxVersionRetention { get; set; }

        /// <summary>
        /// The maximum number of file versions to retain.
        /// </summary>
        [DataMember]
        public int MaxVersion { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public CleanUpBehavior()
        {
            RetentionPeriod = new TimeSpan(7, 0, 0, 0);
            MaxVersion = 3;
        }
    }
}
