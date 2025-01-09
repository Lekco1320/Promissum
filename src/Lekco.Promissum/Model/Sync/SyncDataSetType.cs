using System;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Indicates the type of dataset of <see cref="SyncTask"/> database.
    /// </summary>
    [Flags]
    public enum SyncDataSetType
    {
        /// <summary>
        /// The type of dataset of <see cref="FileRecord"/>.
        /// </summary>
        FileRecordDataSet = 0x001,

        /// <summary>
        /// The type of dataset of <see cref="CleanUpRecord"/>.
        /// </summary>
        CleanUpDataSet = 0x010,

        /// <summary>
        /// The type of dataset of <see cref="ExecutionRecord"/>.
        /// </summary>
        ExecutionDataSet = 0x100,

        /// <summary>
        /// The type of all types.
        /// </summary>
        AllDataSets = FileRecordDataSet | CleanUpDataSet | ExecutionDataSet
    }
}
