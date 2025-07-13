using Lekco.Promissum.Model.Sync.Base;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// The class describes path of a MTP drive.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullPath,nq}")]
    public class MTPPath : PathBase
    {
        /// <inheritdoc />
        public override string FullPath => RelativePath;

        /// <inheritdoc />
        public override DirectoryBase Directory => new MTPDirectory(FullPath, (MTPDrive)Drive);

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="drive">Drive of the path depends on.</param>
        /// <param name="relativePath">Path relative from the root of drive.</param>
        public MTPPath(MTPDrive drive, string relativePath)
            : base(drive, relativePath)
        {
        }

        /// <inheritdoc />
        public override string GetRelativePath(FileSystemBase entity)
            => GetRelativePath(entity.FullName);

        /// <inheritdoc />
        public override FileBase GetFile(string relativePath)
            => new MTPFile(Path.Combine(FullPath, relativePath), (MTPDrive)Drive);

        /// <inheritdoc />
        public override DirectoryBase GetDirectory(string relativePath)
            => new MTPDirectory(Path.Combine(FullPath, relativePath), (MTPDrive)Drive);
    }
}
