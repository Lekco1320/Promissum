using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;

namespace Lekco.Promissum.Model.Sync.Execution
{
    /// <summary>
    /// Stores info of reserved file.
    /// </summary>
    public class ReservedFileInfo
    {
        /// <summary>
        /// Related file.
        /// </summary>
        public FileBase File { get; }

        /// <summary>
        /// Related <see cref="Record.CleanUpRecord"/> of the reserved file.
        /// </summary>
        public CleanUpRecord CleanUpRecord { get; }

        /// <summary>
        /// Index of version of the file.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="record">Related <see cref="Record.CleanUpRecord"/> of the reserved file.</param>
        /// <param name="version">Version of the file.</param>
        /// <param name="file">Related file.</param>
        public ReservedFileInfo(FileBase file, CleanUpRecord record, int version)
        {
            CleanUpRecord = record;
            Version = version;
            File = file;
        }
    }
}
