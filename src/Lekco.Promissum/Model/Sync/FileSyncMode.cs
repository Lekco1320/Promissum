using System.ComponentModel;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Indicates the mode when syncing files.
    /// </summary>
    public enum FileSyncMode
    {
        /// <summary>
        /// Sync always.
        /// </summary>
        [Description("总是同步")]
        Always,

        /// <summary>
        /// Sync the newer ones.
        /// </summary>
        [Description("同步较新的")]
        SyncNewer,

        /// <summary>
        /// Sync the older ones.
        /// </summary>
        [Description("同步较旧的")]
        SyncOlder,

        /// <summary>
        /// Sync the larger ones.
        /// </summary>
        [Description("同步较大的")]
        SyncLarger,

        /// <summary>
        /// Sync the smaller ones.
        /// </summary>
        [Description("同步较小的")]
        SyncSmaller,
    }
}
