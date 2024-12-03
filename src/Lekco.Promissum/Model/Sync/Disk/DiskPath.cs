using Lekco.Promissum.Model.Sync.Base;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Disk
{
    /// <summary>
    /// The class describes path of a disk.
    /// </summary>
    [DataContract]
    public class DiskPath : PathBase
    {
        /// <inheritdoc />
        public override string FullPath => Path.Combine(Drive.Root, RelativePath);

        // <inheritdoc />
        public override DirectoryBase Directory => new DiskDirectory(FullPath);

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="drive">Drive of the path depends on.</param>
        /// <param name="relativePath">Path relative from the root of drive.</param>
        public DiskPath(DiskDrive drive, string relativePath)
            : base(drive, relativePath)
        {
        }

        // <inheritdoc />
        public override string GetRelativePath(FileSystemBase entity)
            => GetRelativePath(FullPath, entity.FullName);

        // <inheritdoc />
        public override FileBase GetFile(string relativePath)
            => new DiskFile(Path.Combine(FullPath, relativePath));

        // <inheritdoc />
        public override DirectoryBase GetDirectory(string relativePath)
            => new DiskDirectory(Path.Combine(FullPath, relativePath));
    }
}
