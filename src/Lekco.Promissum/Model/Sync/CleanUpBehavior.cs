using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Represents the clean up behavior configuration for files in destination path.
    /// </summary>
    [DataContract]
    public partial class CleanUpBehavior
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

        /// <summary>
        /// Extract version from file name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="version">Extracted version.</param>
        /// <returns>The file name without version mark.</returns>
        public static string ExtractVersion(string fileName, out int version)
        {
            version = 0;
            var regex = FileNameRegex();
            var match = regex.Match(fileName);
            if (match.Success)
            {
                version = int.Parse(match.Groups[1].Value[1..]);
            }
            return regex.Replace(fileName, "");
        }

        /// <summary>
        /// Indicates the given reserved file whether needs cleaning up.
        /// </summary>
        /// <param name="file">Given reserved file.</param>
        /// <param name="versionIndex">Index of the version which is 1-based.</param>
        /// <returns><see langword="true"/> if needs; otherwise, returns <see langword="false"/>.</returns>
        public bool NeedCleanUp(FileBase file, int versionIndex)
        {
            if (EnableRetentionPeriod && file.LastWriteTime < DateTime.Now - RetentionPeriod)
            {
                return true;
            }
            if (EnableVersioning && EnableMaxVersionRetention && versionIndex > MaxVersion)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Regex for matching file name marked with version.
        /// </summary>
        /// <returns>Compiled regex.</returns>
        [GeneratedRegex("(_\\d+)(?=\\.|$)")]
        private static partial Regex FileNameRegex();
    }
}
