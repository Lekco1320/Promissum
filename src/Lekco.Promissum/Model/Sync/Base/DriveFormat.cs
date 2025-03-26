namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// Defines constants for common drive file system on Windows.
    /// </summary>
    public enum DriveFormat
    {
        /// <summary>
        /// Unknown file system.
        /// </summary>
        Unknown,

        /// <summary>
        /// NTFS. Case-insensitive.
        /// </summary>
        NTFS,

        /// <summary>
        /// FAT32. Case-insensitive.
        /// </summary>
        FAT32,

        /// <summary>
        /// exFAT. Case-insensitive.
        /// </summary>
        exFAT,

        /// <summary>
        /// CDFS. Case-insensitive.
        /// </summary>
        CDFS,

        /// <summary>
        /// ReFS. Case-insensitive.
        /// </summary>
        ReFS,

        /// <summary>
        /// UDF. Case-sensitive.
        /// </summary>
        UDF,

        /// <summary>
        /// ext4. Case-sensitive.
        /// </summary>
        ext4,

        /// <summary>
        /// XFS. Case-sensitive.
        /// </summary>
        XFS,

        /// <summary>
        /// ZFS. Case-sensitive.
        /// </summary>
        ZFS,
    }
}
