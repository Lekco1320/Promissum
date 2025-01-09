namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Indicates the mode to compare two files.
    /// </summary>
    public enum FileCompareMode
    {
        /// <summary>
        /// Given files are absolutely different.
        /// </summary>
        None,

        /// <summary>
        /// Whether the file is newer.
        /// </summary>
        IsNewer,

        /// <summary>
        /// Whether the file is older.
        /// </summary>
        IsOlder,

        /// <summary>
        /// Whether the file is larger.
        /// </summary>
        IsLarger,

        /// <summary>
        /// Whether the file is smaller.
        /// </summary>
        IsSmaller,
    }
}
